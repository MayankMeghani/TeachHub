using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TeachHub.Data;
using TeachHub.Models;
using TeachHub.Services;
using TeachHub.ViewModels;

namespace TeachHub.Controllers.learner
{
    [Authorize]
    public class AvailableCoursesController : Controller
    {
        private readonly TeachHubContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly StripeService _stripeService;

        public AvailableCoursesController(TeachHubContext context,UserManager<User> userManager,StripeService stripeService,IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _stripeService = stripeService;
        }

        // GET: AvailableCourses
        public async Task<IActionResult> Index(string searchTerm)
        {
            var user = await _userManager.GetUserAsync(User);

            // Check if the user is logged in
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch available courses (that the user is not enrolled in)
            var availableCoursesQuery = _context.Courses
                .Include(c => c.Teacher) // Include related Teacher entity
                .GroupJoin(
                    _context.Enrollments.Where(e => e.LearnerId == user.Id), // Get only enrollments by this user
                    course => course.CourseId,
                    enrollment => enrollment.CourseId,
                    (course, enrollments) => new
                    {
                        Course = course,
                        IsEnrolled = enrollments.Any() // Determine if the user is enrolled
                    }
                )
                .Where(c => !c.IsEnrolled); // Filter out courses where the user is already enrolled

            // If a search term is provided, filter the courses
            if (!string.IsNullOrEmpty(searchTerm))
            {
                availableCoursesQuery = availableCoursesQuery.Where(c =>
                    c.Course.Title.Contains(searchTerm) ||
                    (c.Course.Teacher != null && c.Course.Teacher.Name.Contains(searchTerm))
                );
            }

            // Select the course objects
            var availableCourses = await availableCoursesQuery
                .Select(c => c.Course)
                .ToListAsync();

            return View(availableCourses); // Pass the available courses to the view
        }


        // GET: AvailableCourses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Teacher) // Include the related Teacher entity
                .Include(c => c.Reviews) // Include related Reviews
                    .ThenInclude(r => r.Learner) // Include the Learner associated with each Review
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
        // GET: AvailableCourses/Enroll
        public async Task<IActionResult> Enroll(int CourseId)
        {
            var user = await _userManager.GetUserAsync(User);

            // Check if the user's profile is complete
            if (user == null || !user.IsProfileComplete)
            {
                // Add an error message to TempData
                TempData["ProfileIncompleteError"] = "Your profile is incomplete. Please complete your profile before enrolling in a course.";

                TempData["ErrorMessage"] = "Your profile is incomplete. Please complete your profile before enrolling in a course.";
                // Redirect to the Index page or Profile completion page
                return RedirectToAction("Details", "AvailableCourses", new { id = CourseId });
            }


             var course = _context.Courses.Find(CourseId);
            if (!course.IsActive)
            {
                TempData["ErrorMessage"] = "This course is currently inactive and not open for new enrollments.";
                return RedirectToAction("Details", new { id = CourseId });
            }
            ViewBag.Course = course;
            ViewBag.LearnerId = user.Id;
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(EnrollmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload course details if validation fails
                var course = _context.Courses.Find(model.CourseId);
                if (course == null)
                {
                    return NotFound();
                }
                ViewBag.Course = course;

                return View(model);
            }

            try
            {
                // Process the payment using the Stripe service
                var course = _context.Courses.Find(model.CourseId);
                string chargeId = await _stripeService.CreateCharge(model.StripeToken, course.Price);

                // Payment successful, proceed with enrollment
                var enrollment = new Enrollment
                {
                    CourseId = model.CourseId,
                    LearnerId = model.LearnerId,
                    TransactionId = chargeId, 
                    Amount = course.Price, 
                    TransactionDate = DateTime.Now 
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Enrolled in Course successfully!";
                return RedirectToAction(nameof(Confirmation), new { CourseId = model.CourseId, Amount = course.Price });
            }
            catch (Exception ex)
            {
                // Payment failed, show an error on the view
                ViewBag.PaymentError = "Payment failed: " + ex.Message;

                var course = _context.Courses.Find(model.CourseId);
                ViewBag.Course = course;

                return View(model); 
            }
        }
        // GET: AvailableCourses/Confirmation
        public IActionResult Confirmation(int CourseId, decimal Amount)
        {
            var course = _context.Courses.Find(CourseId);

            if (course == null)
            {
                return NotFound();
            }

            ViewData["CourseTitle"] = course.Title;
            ViewData["Amount"] = Amount;

            return View();
        }



    }
}
