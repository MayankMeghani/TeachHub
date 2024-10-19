using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;
using TeachHub.Services;
using TeachHub.ViewModels;

namespace TeachHub.Controllers.learner
{
    public class MyEnrollments : Controller
    {
        private readonly TeachHubContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public MyEnrollments(TeachHubContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            // Await the result to get the user object
            var user = await _userManager.GetUserAsync(User);

            var enrollments = from e in _context.Enrollments
                              join c in _context.Courses on e.CourseId equals c.CourseId
                              join t in _context.Teachers on c.TeacherId equals t.TeacherId // Join the Teachers table
                              where e.LearnerId == user.Id // Correctly compare the LearnerId
                              select new MyEnrollmentViewModel
                              {
                                  CourseId = c.CourseId,
                                  Title = c.Title,
                                  Description = c.Description,
                                  Teacher = t.Name, 
                                  EnrollDate = e.TransactionDate, 
                                  Amount = e.Amount 
                              };

            var enrollmentList = await enrollments.ToListAsync();

            return View(enrollmentList);
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
                .Include(c => c.Videos)  
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
        public async Task<IActionResult> Review(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.CourseId == id && r.LearnerId == user.Id);

            if (existingReview != null)
            {
                // If the review already exists, return an error or redirect to an edit page
                TempData["ErrorMessage"] = "You have already reviewed this course.";
                TempData["ReviewExistsError"] = "You have already reviewed this course.";
                return RedirectToAction("Index");
            }

            // If no review exists, create a new Review object
            var review = new Review
            {
                CourseId = id,
                LearnerId = user.Id, // Set the current user's ID
                CreatedAt = DateTime.Now,
            };

            return View(review); // Pass the review object to the view
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review([Bind("ReviewId,Content,Rating,CreatedAt,CourseId,LearnerId")] Review review)
        {
            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Review Added successfully!";

                return RedirectToAction(nameof(Index));
            }

            return View(review);
        }

    }
}
