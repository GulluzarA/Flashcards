using Flashcards.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flashcards.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    [ViewData]
    public string Title { get; set; } = "Home";
    
    public IActionResult Index()
    {
        try 
        {
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError("{FormatException}",
                ErrorHandling.FormatException(ControllerContext, "Internal server error.", ex));
            return StatusCode(500);
        }

    }
}