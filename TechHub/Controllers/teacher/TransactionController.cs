using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;
using TeachHub.ViewModels;

namespace TeachHub.Controllers.teacher
{

    [Authorize]
    public class TransactionController : Controller
    {
        private readonly TeachHubContext _context;
        private readonly UserManager<User> _userManager;

        public TransactionController(TeachHubContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transacction
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var coursesWithEnrollments = await _context.Courses
    .Where(c => c.TeacherId == user.Id) // Fetch courses created by the teacher
    .GroupJoin(
        _context.Enrollments,
        course => course.CourseId,
        enrollment => enrollment.CourseId,
        (course, enrollments) => new
        {
            Course = course,
            StudentCount = enrollments.Count() // Count enrollments for each course
        }
    )
    .ToListAsync();


            // Pass the result to the view (you'll need to create a ViewModel for this)
            var viewModel = coursesWithEnrollments.Select(c => new CourseEnrollmentViewModel
            {
                CourseId = c.Course.CourseId,
                Title = c.Course.Title,
                Description = c.Course.Description,
                StudentCount = c.StudentCount
            }).ToList();

            return View(viewModel);
        }

        // GET: Transacction/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Check if the course exists
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            // Fetch all enrollment transactions for the specific course
            var enrollments = await _context.Enrollments
                .Include(e => e.Learner)
                .Where(e => e.CourseId == id)
                .ToListAsync();

            // Map to ViewModel for display
            var viewModel = new CourseDetailsViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Enrollments = enrollments.Select(e => new EnrollmentTransactionViewModel
                {
                    LearnerName = e.Learner.Name,
                    TransactionId = e.TransactionId,
                    Amount = e.Amount,
                    EnrollmentDate = e.TransactionDate
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
