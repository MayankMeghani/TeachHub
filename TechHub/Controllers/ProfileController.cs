using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;
using TeachHub.ViewModels;

namespace TeachHub.Controllers.teacher
{
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly TeachHubContext _context; // Assuming you have your DbContext injected

        public ProfileController(ILogger<ProfileController> logger, UserManager<User> userManager, TeachHubContext context)
        {
            _logger = logger;

            _userManager = userManager;
            _context = context;
        }

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users
                .Include(u => u.Teacher)  // Eager load Teacher profile
                .Include(u => u.Learner)  // Eager load Learner profile
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null)
            {
                return NotFound();
            }

            // Pass the user to the view with profile details
            return View(user);
        }


        // GET: Profile/Create
        public async Task<IActionResult> CreateTeacher()
        {
            var user = await _userManager.GetUserAsync(User);

            // Create a new Teacher instance or use a ViewModel
            var model = new Teacher
            {
                TeacherId = user.Id,
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(Teacher model)
        {
            _logger.LogInformation("CreateTeacher method called");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model is valid. Teacher data: {@Teacher}", model);

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("User not found");
                    return NotFound();
                }

                try
                {
                    var teacher = new Teacher
                    {
                        TeacherId = user.Id,
                        Bio = model.Bio,
                        Name = model.Name,
                        ProfilePicture = model.ProfilePicture,
                        User = user
                    };

                    _context.Teachers.Add(teacher);
                    _logger.LogInformation("Teacher added to context");

                    user.IsProfileComplete = true;
                    await _userManager.UpdateAsync(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Profile marked as complete and changes saved for user {UserId}", user.Id);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating Teacher profile");
                    throw;
                }
            }
            else
            {
                // Log model state errors with actual error messages
                var errorMessages = ModelState
                    .SelectMany(x => x.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Model state is invalid. Errors: {Errors}", string.Join(", ", errorMessages));

                // Log the model details with field values
                _logger.LogInformation("Invalid Teacher data: {Id}, {Bio}, {Name}, {ProfilePicture}",
                                        model.TeacherId, model.Bio, model.Name, model.ProfilePicture);
            }

            return View(model);
        }
        // GET: Teachers/Edit/5
        public async Task<IActionResult> EditTeacher(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Set<User>(), "TeacherId", "TeacherId", teacher.TeacherId);
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(string id, [Bind("Id,Bio,Name,ProfilePicture")] Teacher teacher)
        {
            if (id != teacher.TeacherId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Teachers.Any(e => e.TeacherId == teacher.TeacherId))
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
            ViewData["Id"] = new SelectList(_context.Set<User>(), "TeacherId", "TeacherId", teacher.TeacherId);
            return View(teacher);
        }
        // GET: Learners/Create
        public async Task<IActionResult> CreateLearner( )
        {
            var user = await _userManager.GetUserAsync(User);

            var learner = new Learner { LearnerId = user.Id }; // Initialize Learner with Id
            return View(learner);
        }

        // POST: Learners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLearner([Bind("Id,Name,ProfilePicture")] Learner learner)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("User not found");
                    return NotFound();
                }

                _context.Add(learner);
                await _context.SaveChangesAsync();
                user.IsProfileComplete = true;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Log the invalid model state
            _logger.LogWarning("Model state is invalid for learner creation. Errors:");

            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    _logger.LogError(error.ErrorMessage);
                }
            }

            // Log the learner details
            _logger.LogInformation("Invalid Learner Data: {@Learner}", learner);

            // If we reach this point, something went wrong
            return View(learner);
        }


        // GET: Learners/Edit/5
        public async Task<IActionResult> EditLearner(string id)
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
        public async Task<IActionResult> EditLearner(string id, [Bind("Id,Name,ProfilePicture")] Learner learner)
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
                    if (!_context.Learners.Any(e => e.LearnerId == learner.LearnerId))
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

    }
}