using Microsoft.AspNetCore.Mvc;
using System;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Message"] = "Welcome to TeachHub! Learn about various courses.";
        return View();
    }

    [Route("/Home/HandleError/{statusCode}")]
    public IActionResult Error(int code)
    {
        if (code == 404)
        {
            return View("404");
        }
        return View("Error");

    }

}
