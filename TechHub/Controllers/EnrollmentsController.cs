using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TeachHub.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly TeachHubContext _context;

        public EnrollmentsController(TeachHubContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var teachHubContext = _context.Enrollments.Include(e => e.Course).Include(e => e.Learner);
            return View(await teachHubContext.ToListAsync());
        }

        // GET: Enrollments/Details
        public async Task<IActionResult> Details(int courseId, int learnerId)
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
            return View();
        }

        // POST: Enrollments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,LearnerId")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title", enrollment.CourseId);
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "Name", enrollment.LearnerId);
            return View(enrollment);
        }

        // GET: Enrollments/Edit
        [HttpGet("Edit/{courseId}/{learnerId}")]
        public async Task<IActionResult> Edit(int courseId, int learnerId)
        {
            var enrollment = await _context.Enrollments.FindAsync(courseId, learnerId);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title", enrollment.CourseId);
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "Name", enrollment.LearnerId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit
        [HttpPost]
        [Route("Enrollments/Edit/{courseId}/{learnerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int courseId, int learnerId, [Bind("CourseId,LearnerId")] Enrollment enrollment)
        {
            if (courseId != enrollment.CourseId || learnerId != enrollment.LearnerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.CourseId, enrollment.LearnerId))
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
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title", enrollment.CourseId);
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "Name", enrollment.LearnerId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete
        public async Task<IActionResult> Delete(int courseId, int learnerId)
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
        public async Task<IActionResult> DeleteConfirmed(int courseId, int learnerId)
        {
            var enrollment = await _context.Enrollments.FindAsync(courseId, learnerId);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int courseId, int learnerId)
        {
            return _context.Enrollments.Any(e => e.CourseId == courseId && e.LearnerId == learnerId);
        }
    }
}
