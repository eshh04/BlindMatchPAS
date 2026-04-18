// ============================================================================
// File: Controllers/HomeController.cs
// Purpose: Handles public-facing pages (Home, Privacy) and shared error display.
// Pattern: MVC Controller with default ASP.NET Core error handling.
// ============================================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Controllers;

/// <summary>
/// Controller for public pages and error handling.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>GET: / — Landing page.</summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>GET: /Home/Privacy — Privacy policy page.</summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>Shared error handler — displays request ID for debugging.</summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
