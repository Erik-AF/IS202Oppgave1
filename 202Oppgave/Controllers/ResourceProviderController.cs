using _202Oppgave.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _202Oppgave.Controllers;

[Authorize(Roles = RoleNames.ResourceProvider)]
public class ResourceProviderController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Resources()
    {
        return View();
    }
    
    [HttpGet]
    public IActionResult Profile()
    {
        return View();
    }
}

