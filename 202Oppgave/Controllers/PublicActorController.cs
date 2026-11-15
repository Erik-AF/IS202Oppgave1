using _202Oppgave.Models.Authorization;
using _202Oppgave.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace _202Oppgave.Controllers;

[Authorize(Roles = RoleNames.PublicActor)]
public class PublicActorController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public PublicActorController(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        ViewBag.FullName = user.FullName;

        return View();
    }

    [HttpGet]
    public IActionResult Needs()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Profile()
    {
        return View();
    }
}