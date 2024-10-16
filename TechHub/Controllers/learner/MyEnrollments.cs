using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                                  Teacher = t.Name, // Fetch the Teacher's name from the Teachers table
                                  EnrollDate = e.TransactionDate, // Enrollment Date
                                  Amount = e.Amount // Optionally display the amount paid
                              };

            // Fetch the list asynchronously
            var enrollmentList = await enrollments.ToListAsync();

            return View(enrollmentList); // Pass the list of enrollments to the view
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
        }        

    }
}
