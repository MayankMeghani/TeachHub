using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TeachHub.Models;
using TeachHub.ViewModels;

namespace TeachHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger; // Inject logger
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }
        // GET: /Account/Login
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            await _signInManager.SignOutAsync();
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Attempting login for user: {Email}", model.Email);

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password,false ,lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Login successful for user: {Email}", model.Email);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    _logger.LogWarning("Invalid login attempt for user: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt."); // Adds error to ModelState
                    return View(model); // Redisplays the form with error
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await _signInManager.SignOutAsync();

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create the user with only email and password
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    IsProfileComplete = false // Initially set profile as incomplete
                };

                // Register the user using ASP.NET Identity
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign the role (Teacher or Learner) to the user
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }
                    await _userManager.AddToRoleAsync(user, model.Role);

                    // Log the user in
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect the user to a page to complete their profile
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }


        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // Helper method
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
