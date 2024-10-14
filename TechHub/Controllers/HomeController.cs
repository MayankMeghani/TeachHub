using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Message"] = "Welcome to TechHub! Learn about various courses.";
        return View();
    }
}
