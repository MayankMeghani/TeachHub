using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;
using TeachHub.Services;
using Microsoft.AspNetCore.Identity; // Add this for UserManager

namespace TeachHub.Controllers.Teachers
{
    public class MyCoursesController : Controller
    {
        private readonly TeachHubContext _context;
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<MyCoursesController> _logger;
        private readonly UserManager<User> _userManager; // Inject UserManager

        public MyCoursesController(TeachHubContext context, FirebaseService firebaseService, ILogger<MyCoursesController> logger, UserManager<User> userManager)
        {
            _context = context;
            _firebaseService = firebaseService;
            _logger = logger;
            _userManager = userManager; // Assign UserManager
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Videos)  // Include the videos related to this course
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }        // GET: Courses/Create

        // GET: MyCourses
        public async Task<IActionResult> Index()
        {
            var teacherId = _userManager.GetUserId(User); // Get the logged-in teacher's ID
            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacherId) // Filter courses by teacher ID
                .Include(c => c.Teacher)
                .ToListAsync();

            return View(courses);
        }

        // GET: MyCourses/Create
        public async Task<IActionResult> Create()
        {
            var user= await _userManager.GetUserAsync(User);

            // Pass the TeacherId to the view
            var course = new Course
            {
                TeacherId = user.Id, // Set the TeacherId for the course
                CreatedAt = DateTime.UtcNow,
            };

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,Title,Description,Price,CreatedAt,TeacherId")] Course course, List<IFormFile> videoFiles)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    _context.Add(course);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Course '{course.Title}' created with ID {course.CourseId}.");

                    var uploadedVideos = new List<Video>();

                    foreach (var file in videoFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            _logger.LogInformation($"Uploading video file '{file.FileName}'.");
                            try
                            {
                                using (var stream = file.OpenReadStream())
                                {
                                    var videoUrl = await _firebaseService.UploadToFirebase(stream, file.FileName);

                                    if (!string.IsNullOrEmpty(videoUrl))
                                    {
                                        var video = new Video
                                        {
                                            Title = file.FileName,
                                            VideoUrl = videoUrl,
                                            UploadedAt = DateTime.Now,
                                            CourseId = course.CourseId
                                        };
                                        uploadedVideos.Add(video);
                                        _logger.LogInformation($"Video '{file.FileName}' uploaded successfully. URL: {videoUrl}");
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Upload succeeded but no URL returned for file '{file.FileName}'.");
                                        ModelState.AddModelError("", $"Failed to get URL for uploaded video '{file.FileName}'. Please try again later.");
                                    }
                                }
                            }
                            catch (FirebaseStorageException ex)
                            {
                                _logger.LogError(ex, $"Firebase Storage Exception occurred while uploading '{file.FileName}'.");
                                ModelState.AddModelError("", $"Failed to upload video '{file.FileName}'. Please try again later.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Unexpected error occurred while uploading '{file.FileName}'.");
                                ModelState.AddModelError("", $"An unexpected error occurred while uploading '{file.FileName}'. Please try again.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"No valid video file provided for upload.");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        // Add all successfully uploaded videos to the context
                        _context.Videos.AddRange(uploadedVideos);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"All videos processed and saved for course '{course.Title}'.");
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while creating the course: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the course. Please try again.");
                }
            }
            else
            {
                // Log the invalid model state with the errors for debugging purposes
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    _logger.LogWarning($"Model validation error: {error.ErrorMessage}");
                }
            }

            // If the model is invalid, return to the same view with the course object
            return View(course);
        }

        // GET: MyCourses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var course = await _context.Courses
                          .Include(c => c.Videos)  // Include existing videos
                          .FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null || course.TeacherId != _userManager.GetUserId(User)) // Check if the course belongs to the logged-in teacher
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: MyCourses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,Title,Description,Price,CreatedAt,TeacherId")] Course course, List<IFormFile> videoFiles, List<int> videoIdsToRemove)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
            
                    _context.Update(course);
                    await _context.SaveChangesAsync();

                    // Handle removal of existing videos
                    if (videoIdsToRemove != null && videoIdsToRemove.Any())
                    {
                        var videosToDelete = _context.Videos.Where(v => videoIdsToRemove.Contains(v.VideoId));
                        _context.Videos.RemoveRange(videosToDelete);
                    }

                    // Handle new video uploads
                    foreach (var file in videoFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            using (var stream = file.OpenReadStream())
                            {
                                var videoUrl = await _firebaseService.UploadToFirebase(stream, file.FileName);

                                if (!string.IsNullOrEmpty(videoUrl))
                                {
                                    var video = new Video
                                    {
                                        Title = file.FileName,
                                        VideoUrl = videoUrl,
                                        UploadedAt = DateTime.Now,
                                        CourseId = course.CourseId
                                    };

                                    _context.Videos.Add(video);
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "Name", course.TeacherId);
            return View(course);
        }

        // GET: MyCourses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.CourseId == id);

            // Check ownership
            if (course == null || course.TeacherId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: MyCourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null && course.TeacherId == _userManager.GetUserId(User)) // Check ownership
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
