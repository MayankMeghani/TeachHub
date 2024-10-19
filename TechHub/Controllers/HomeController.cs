/*using Microsoft.AspNetCore.Mvc;
using System;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Message"] = "Welcome to TeachHub! Learn about various courses.";
        return View();
    }

    [Route("/Home/HandleError/{statusCode}")]
    public IActionResult HandleError(int statusCode)
    {
        var viewName = "Error";
        var errorMessage = "An unexpected error occurred.";

        switch (statusCode)
        {
            case 404:
                viewName = "NotFound";
                errorMessage = "Sorry, the page you requested could not be found.";
                break;
                // Add more cases for other status codes if needed
        }

        ViewBag.ErrorMessage = errorMessage;
        return View(viewName);
    }

    // This method can be removed if you want all errors to go through HandleError
    // public IActionResult Error()
    // {
    //     ViewBag.ErrorMessage = "An unexpected error occurred.";
    //     return View("Error");
    // }

    // This method can also be removed as it's now handled by HandleError
    // [Route("Home/NotFound")]
    // public IActionResult NotFound()
    // {
    //     ViewBag.ErrorMessage = "Sorry, the page you requested could not be found.";
    //     return View("NotFound");
    // }
}*/

using Microsoft.AspNetCore.Mvc;
using System;

public class HomeController : Controller
{
    // Default action for Home page
    public IActionResult Index()
    {
        ViewData["Message"] = "Welcome to TeachHub! Learn about various courses.";
        return View();
    }

    // Error handling action
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
