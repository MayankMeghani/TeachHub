using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeachHub.Services;
using TeachHub.ViewModels;

namespace TeachHub.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly TeachHubContext _context;
        private readonly IConfiguration _configuration;
        private readonly StripeService _stripeService;
        private readonly ILogger<CoursesController> _logger; // Add logger

        public EnrollmentsController(TeachHubContext context,StripeService stripeService,IConfiguration configuration,ILogger<CoursesController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _stripeService = stripeService;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var teachHubContext = _context.Enrollments.Include(e => e.Course).Include(e => e.Learner);
            return View(await teachHubContext.ToListAsync());
        }

        // GET: Enrollments/Details
        public async Task<IActionResult> Details(int courseId,string learnerId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Learner)
                .FirstOrDefaultAsync(m => m.CourseId == courseId && m.LearnerId == learnerId);

            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }
        // GET: Enrollments/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title");
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "Name");
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EnrollmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload the dropdowns if validation fails
                ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title");
                ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "Name");
                return View(model);
            }

            try
            {
                // Process the payment using the Stripe service
                string chargeId = await _stripeService.CreateCharge(model.StripeToken, model.Amount);

                // Payment successful, proceed with enrollment
                var enrollment = new Enrollment
                {
                    CourseId = model.CourseId,
                    LearnerId = model.LearnerId,
                    TransactionId = chargeId, // Store charge ID
                    Amount = 100, // Assuming a fixed amount for demonstration
                    TransactionDate = DateTime.Now // Assign the current date and time

                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // On success, redirect to the index or a confirmation page
            }
            catch (Exception ex)
            {
                // Payment failed, show an error on the view
                ViewBag.PaymentError = "Payment failed: " + ex.Message;

                // Reload the dropdowns
                ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title");
                ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "Name");

                return View(model); // Show the form again with the error message
            }
        }

        // GET: Enrollments/Delete
        public async Task<IActionResult> Delete(int courseId, string learnerId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Learner)
                .FirstOrDefaultAsync(m => m.CourseId == courseId && m.LearnerId == learnerId);

            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int courseId, string learnerId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Learner)
                .FirstOrDefaultAsync(m => m.CourseId == courseId && m.LearnerId == learnerId);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int courseId, string learnerId)
        {
            return _context.Enrollments.Any(e => e.CourseId == courseId && e.LearnerId == learnerId);
        }
    }
}
