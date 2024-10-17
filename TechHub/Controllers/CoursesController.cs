using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;
using TeachHub.Services;

namespace TeachHub.Controllers
{
    public class CoursesController : Controller
    {
        private readonly TeachHubContext _context;
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<CoursesController> _logger; // Add logger

        public CoursesController(TeachHubContext context, FirebaseService firebaseService, ILogger<CoursesController> logger)
        {
            _context = context;
            _firebaseService = firebaseService;
            _logger = logger; // Assign logger
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var teachHubContext = _context.Courses.Include(c => c.Teacher);
            return View(await teachHubContext.ToListAsync());
        }

        // GET: Courses/Details/5
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
        public IActionResult Create()
        {
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "Name");
            return View();
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

            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "Bio", course.TeacherId);
            return View(course);
        }
        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                                       .Include(c => c.Videos)  // Include existing videos
                                       .FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "Name", course.TeacherId);
            return View(course);
        }

        // POST: Courses/Edit/5
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
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "Bio", course.TeacherId);
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
