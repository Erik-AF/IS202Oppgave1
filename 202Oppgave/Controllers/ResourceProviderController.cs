using _202Oppgave.Models.Authorization;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _202Oppgave.DataAccess.Repositories;

namespace _202Oppgave.Controllers;

[Authorize(Roles = RoleNames.ResourceProvider)]
public class ResourceProviderController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IResourceRepository _resourceRepository;

    public ResourceProviderController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IResourceRepository resourceRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _resourceRepository = resourceRepository;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        var resources = await _resourceRepository.GetByUserIdAsync(user.Id);
        return View(resources.Take(4).ToList());
    }
    
    [HttpGet]
    public async Task<IActionResult> Resources()
    {
        // Henter ID-en til den innloggede brukeren
        var userId = _userManager.GetUserId(User);

        if (userId == null)
            return Challenge();

        // Viser bare ressursene denne brukeren har registrert
        var resources = await _resourceRepository.GetByUserIdAsync(userId);

        return View(resources);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        var model = new ProfileViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Email = user.Email ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        var email = model.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null && existingUser.Id != user.Id)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "E-postadressen er allerede i bruk.");

            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber.Trim();

        // Innloggingen bruker e-postadresse som brukernavn.
        user.Email = email;
        user.UserName = email;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Profilen din er oppdatert.";

        return RedirectToAction(nameof(Profile));
    }
}


