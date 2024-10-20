using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TeachHub.Models;
using TeachHub.ViewModels;

namespace TeachHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger; 
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
        public async Task<IActionResult> Login(string message = null)
        {
            await _signInManager.SignOutAsync();
            if (!string.IsNullOrEmpty(message))
            {
                TempData["Message"] = message;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Attempting login for user: {Email}", model.Email);

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Login successful for user: {Email}", model.Email);

                    TempData["SuccessMessage"] = "You have successfully logged in!";


                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    _logger.LogWarning("Invalid login attempt for user: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt."); // Adds error to ModelState
                    return View(model); // Redisplay the form with error
                }
            }

            return View(model);
        }
        // GET: /Account/Register
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await _signInManager.SignOutAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    IsProfileComplete = false 
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    }
                    await _userManager.AddToRoleAsync(user, model.Role);

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["SuccessMessage"] = "Registration successful! Welcome!";

                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }


        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out"); 
            TempData["SuccessMessage"] = "You have logged out successfully!";
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl, string action = null)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home", new { notify = action });
            }
        }
    }
}
