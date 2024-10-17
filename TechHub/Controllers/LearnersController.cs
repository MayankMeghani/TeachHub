using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;

namespace TeachHub.Controllers
{
    public class LearnersController : Controller
    {
        private readonly TeachHubContext _context;

        public LearnersController(TeachHubContext context)
        {
            _context = context;
        }

        // GET: Learners
        public async Task<IActionResult> Index()
        {
            var teachHubContext = _context.Learners.Include(l => l.User);
            return View(await teachHubContext.ToListAsync());
        }

        // GET: Learners/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var learner = await _context.Learners
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LearnerId == id);
            if (learner == null)
            {
                return NotFound();
            }

            return View(learner);
        }

        // GET: Learners/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Set<User>(), "Id", "Id");
            return View();
        }

        // POST: Learners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ProfilePicture")] Learner learner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(learner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Set<User>(), "LearnerId", "LearnerId", learner.LearnerId);
            return View(learner);
        }

        // GET: Learners/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var learner = await _context.Learners.FindAsync(id);
            if (learner == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Set<User>(), "LearnerId", "LearnerId", learner.LearnerId);
            return View(learner);
        }

        // POST: Learners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,ProfilePicture")] Learner learner)
        {
            if (id != learner.LearnerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(learner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LearnerExists(learner.LearnerId))
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
            ViewData["Id"] = new SelectList(_context.Set<User>(), "LearnerId", "LearnerId", learner.LearnerId);
            return View(learner);
        }

        // GET: Learners/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var learner = await _context.Learners
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LearnerId == id);
            if (learner == null)
            {
                return NotFound();
            }

            return View(learner);
        }

        // POST: Learners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var learner = await _context.Learners.FindAsync(id);
            if (learner != null)
            {
                _context.Learners.Remove(learner);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LearnerExists(string id)
        {
            return _context.Learners.Any(e => e.LearnerId == id);
        }
    }
}
