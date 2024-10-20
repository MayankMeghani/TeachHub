using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;
using TeachHub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace TeachHub.Controllers.Teachers
{

    [Authorize]
    public class MyCoursesController : Controller
    {
        private readonly TeachHubContext _context;
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<MyCoursesController> _logger;
        private readonly UserManager<User> _userManager; 

        public MyCoursesController(TeachHubContext context, FirebaseService firebaseService, ILogger<MyCoursesController> logger, UserManager<User> userManager)
        {
            _context = context;
            _firebaseService = firebaseService;
            _logger = logger;
            _userManager = userManager; 
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
                .Include(c => c.Videos)
                .Include(c => c.Reviews)
                .ThenInclude(r => r.Learner)  // Include the learner details for each review
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: MyCourses
        public async Task<IActionResult> Index()
        {
            var teacherId = _userManager.GetUserId(User);
            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacherId) 
                .Include(c => c.Teacher)
                .ToListAsync();

            return View(courses);
        }

        // GET: MyCourses/Create
        public async Task<IActionResult> Create()
        {
            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            // Check if the user's profile is complete
            if (user == null || !user.IsProfileComplete)
            {
                // Add an error message to TempData
                TempData["ProfileIncompleteError"] = "Your profile is incomplete. Please complete your profile before creating a course.";

                TempData["ErrorMessage"] = "Your profile is incomplete. Please complete your profile before creating a course.";
                
                return RedirectToAction("Index"); 
            }

            // Create a new course object
            var course = new Course
            {
                TeacherId = user.Id,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            // Return the Create view
            return View(course);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,Title,Description,Price,CreatedAt,TeacherId")] Course course, List<IFormFile> videoFiles)
        {
            // Check if a course with the same title exists for the teacher
            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.Title == course.Title && c.TeacherId == course.TeacherId);

            if (existingCourse != null)
            {
                ModelState.AddModelError("Title", "A course with the same title already exists.");
                return View(course); // Return to view if duplicate course exists
            }

            // Ensure at least one video is uploaded
            if (videoFiles == null || !videoFiles.Any())
            {
                ModelState.AddModelError("", "Please upload at least one video.");
                return View(course); // Return to view if no video is uploaded
            }

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
                                    var videoUrl = await _firebaseService.UploadVideo(stream, file.FileName);

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
                        _context.Videos.AddRange(uploadedVideos);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"All videos processed and saved for course '{course.Title}'.");
                        TempData["SuccessMessage"] = "Course created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while creating the course: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the course. Please try again.");
                    TempData["ErrorMessage"] = "An error occurred while creating the course.";

                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                _logger.LogWarning($"Model validation error: {error.ErrorMessage}");
            }

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
            if (course == null || course.TeacherId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: MyCourses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,Title,Description,Price,IsActive,CreatedAt,TeacherId")] Course course, List<IFormFile> videoFiles, List<int> videoIdsToRemove)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.Title == course.Title && c.TeacherId == course.TeacherId && c.CourseId != course.CourseId);

            if (existingCourse != null)
            {
                ModelState.AddModelError("Title", "A course with the same title already exists.");
                var courseWithVideos = await _context.Courses
                    .Include(c => c.Videos)
                    .FirstOrDefaultAsync(c => c.CourseId == id);
                return View(courseWithVideos);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();

                    if (videoIdsToRemove != null && videoIdsToRemove.Any())
                    {
                        var videosToDelete = _context.Videos.Where(v => videoIdsToRemove.Contains(v.VideoId));
                        _context.Videos.RemoveRange(videosToDelete);
                        await _context.SaveChangesAsync();
                    }

                    foreach (var file in videoFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            using (var stream = file.OpenReadStream())
                            {
                                var videoUrl = await _firebaseService.UploadVideo(stream, file.FileName);

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

                TempData["SuccessMessage"] = "Course updated successfully!";
                return RedirectToAction(nameof(Index));
            }

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
            // Find the course by ID and include its enrollments to check if any exist
            var course = await _context.Courses
                .Include(c => c.Enrollments) // Include enrollments to check if any students are enrolled
                .FirstOrDefaultAsync(c => c.CourseId == id);

            // Check if the course exists and if the current user is the teacher of the course
            if (course != null && course.TeacherId == _userManager.GetUserId(User))
            {
                // If there are any enrollments, restrict deletion
                if (course.Enrollments.Any())
                {
                    // Prevent deletion and prompt the teacher to deactivate the course instead
                    TempData["ErrorMessage"] = "Cannot delete this course because students are enrolled. You can deactivate it instead.";
                    return RedirectToAction(nameof(Index));
                }

                // If no enrollments exist, allow deletion
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Course deleted successfully!";
            }
            else
            {
                // If the course doesn't exist or the teacher is not the owner, show an error message
                TempData["ErrorMessage"] = "Failed to delete the course. Either the course doesn't exist or you don't have permission to delete it.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
