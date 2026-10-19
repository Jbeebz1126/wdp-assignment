using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WDP_Assignment.Data;
using WDP_Assignment.Models;

namespace WDP_Assignment.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        List<AIImage> images;
        try
        {
            images = await _context.AIImages
                .AsNoTracking()
                .OrderByDescending(i => i.Like)
                .ThenByDescending(i => i.UploadDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // If the database can't be read, still show the page without the gallery
            _logger.LogError(ex, "Could not load AI images for the home page.");
            images = new List<AIImage>();
        }

        return View(images);
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
