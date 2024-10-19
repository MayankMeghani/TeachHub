using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Models;
using TeachHub.Services;

namespace TeachHub.Controllers.teacher
{
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly TeachHubContext _context;
        private readonly FirebaseService _firebaseService;
        public ProfileController(ILogger<ProfileController> logger, UserManager<User> userManager, TeachHubContext context,FirebaseService firebaseService)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _firebaseService = firebaseService;
        }

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users
                .Include(u => u.Teacher)  
                .Include(u => u.Learner)  
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> CreateTeacher(Teacher model, IFormFile profilePicture)
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
                    string profilePictureUrl = null;

                    // Upload the picture to Firebase and get the URL
                    if (profilePicture != null && profilePicture.Length > 0)
                    {
                        // Create a unique file name (optional, can use profilePicture.FileName directly)
                        var fileName = $"{user.Id}_{profilePicture.FileName}";

                        // Convert IFormFile to Stream and upload
                        using (var fileStream = profilePicture.OpenReadStream())
                        {
                            profilePictureUrl = await _firebaseService.UploadProfilePicture(fileStream, fileName);
                        }
                    }

                    var teacher = new Teacher
                    {
                        TeacherId = user.Id,
                        Bio = model.Bio,
                        Name = model.Name,
                        ProfilePicture = profilePictureUrl, // Store the URL from Firebase
                        User = user
                    };

                    _context.Teachers.Add(teacher);
                    _logger.LogInformation("Teacher added to context");

                    user.IsProfileComplete = true;
                    await _userManager.UpdateAsync(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Profile marked as complete and changes saved for user {UserId}", user.Id);
                    TempData["SuccessMessage"] = "Profile Completed successfully!";

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

            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(string id, [Bind("TeacherId,Bio,Name,ProfilePicture")] Teacher teacher, IFormFile ProfilePicture)
        {
            if (id != teacher.TeacherId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ProfilePicture != null && ProfilePicture.Length > 0)
                    {
                        using (var stream = ProfilePicture.OpenReadStream())
                        {
                            var fileName = $"{id}_{ProfilePicture.FileName}"; // Create a unique file name

                            teacher.ProfilePicture = await _firebaseService.UploadProfilePicture(stream, fileName);
                        }
                    }

                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Profile Updated successfully!";

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

            return View(teacher);
        }


        // GET: Learners/Create
        public async Task<IActionResult> CreateLearner( )
        {
            var user = await _userManager.GetUserAsync(User);

            var learner = new Learner { LearnerId = user.Id }; 
            return View(learner);
        }

        // POST: Learners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLearner([Bind("LearnerId,Name,ProfilePicture")] Learner learner, IFormFile ProfilePicture)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("User not found");
                    return NotFound();
                }

                string profilePictureUrl = null;
                if (ProfilePicture != null && ProfilePicture.Length > 0)
                {
                    var fileName = $"{user.Id}_{ProfilePicture.FileName}";

                    using (var stream = ProfilePicture.OpenReadStream())
                    {
                        profilePictureUrl = await _firebaseService.UploadProfilePicture(stream, fileName);
                    }
                }

                learner.LearnerId = user.Id; 
                learner.ProfilePicture = profilePictureUrl;

                _context.Add(learner);
                await _context.SaveChangesAsync();

                user.IsProfileComplete = true;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile Completed successfully!";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Model state is invalid for learner creation. Errors:");
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    _logger.LogError(error.ErrorMessage);
                }
            }

            _logger.LogInformation("Invalid Learner Data: {@Learner}", learner);
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
            return View(learner);
        }

        // POST: Learners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLearner(string id, [Bind("LearnerId,Name,ProfilePicture")] Learner learner, IFormFile ProfilePicture)
        {
            if (id != learner.LearnerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ProfilePicture != null && ProfilePicture.Length > 0)
                    {
                        var fileName = $"{learner.LearnerId}_{ProfilePicture.FileName}";

                        using (var stream = ProfilePicture.OpenReadStream())
                        {
                            learner.ProfilePicture = await _firebaseService.UploadProfilePicture(stream, fileName);
                        }
                    }

                    _context.Update(learner);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Profile Updated successfully!";

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

            return View(learner);
        }


    }
}