using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;

namespace TeachHub.Controllers.learner
{

    [Authorize]
    public class MyReviewsController : Controller
    {
        private readonly TeachHubContext _context;

        private readonly UserManager<User> _userManager;

        public MyReviewsController(TeachHubContext context,UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MyReviews
        public async Task<IActionResult> Index()
        {
            // Await the result to get the current user
            var user = await _userManager.GetUserAsync(User);

            var teachHubContext = _context.Reviews
                .Include(r => r.Course) // Include the related Course
                    .ThenInclude(c => c.Teacher) // Then include the related Teacher for each Course
                .Where(r => r.LearnerId == user.Id); // Filter by the LearnerId (current user)

            return View(await teachHubContext.ToListAsync());
        }

        // GET: MyReviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Course)
                .Include(r => r.Learner)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: MyReviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Description", review.CourseId);
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "LearnerId", review.LearnerId);
            return View(review);
        }

        // POST: MyReviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,Content,Rating,CreatedAt,CourseId,LearnerId")] Review review)
        {
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync(); 
                    TempData["SuccessMessage"] = "Review updated successfully!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ReviewId))
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
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Description", review.CourseId);
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "LearnerId", review.LearnerId);
            return View(review);
        }

        // GET: MyReviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Course)
                .Include(r => r.Learner)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: MyReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review); 
                TempData["SuccessMessage"] = "Review deleted successfully!";

            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }
    }
}
