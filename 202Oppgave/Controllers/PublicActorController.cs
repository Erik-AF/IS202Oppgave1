
using System.Security.Claims;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Authorization;
using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace _202Oppgave.Controllers;

[Authorize(Roles = RoleNames.PublicActor + "," + RoleNames.Admin)]
public class PublicActorController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly INeedRepository _needRepository;

    public PublicActorController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        INeedRepository needRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _needRepository = needRepository;
    }

   
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        ViewBag.FullName = user.FullName;

        var allNeeds = await _needRepository.GetAllAsync();

        var myNeeds = allNeeds
            .Where(need => need.UserId == user.Id)
            .ToList();

        return View(myNeeds);
    }


    [HttpGet]
    public async Task<IActionResult> Needs()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var allNeeds = await _needRepository.GetAllAsync();

        var myNeeds = allNeeds
            .Where(need => need.UserId == userId)
            .ToList();

        return View(myNeeds);
    }
    
    [HttpGet]
    public async Task<IActionResult> EditNeed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var need = await _needRepository.GetByIdAsync(id);

        if (need == null || need.UserId != userId)
            return NotFound();

        var model = new NeedViewModel
        {
            Type = need.Type,
            Location = need.Location,
            Priority = need.Priority,
            Description = need.Description,
            Deadline = need.Deadline,
            Latitude = need.Latitude,
            Longitude = need.Longitude,
            GeometryJson = need.GeometryJson
        };

        ViewBag.NeedId = id;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditNeed(int id, NeedViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var need = await _needRepository.GetByIdAsync(id);

        if (need == null || need.UserId != userId)
            return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.NeedId = id;
            return View(model);
        }

        need.Type = model.Type;
        need.Location = model.Location;
        need.Priority = model.Priority!.Value;
        need.Description = model.Description;
        need.Deadline = model.Deadline!.Value;
        need.Latitude = model.Latitude!.Value;
        need.Longitude = model.Longitude!.Value;

        await _needRepository.UpdateAsync(need);

        TempData["Success"] = "Behovet er oppdatert.";

        return RedirectToAction(nameof(Needs));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNeed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var need = await _needRepository.GetByIdAsync(id);

        if (need == null || need.UserId != userId)
            return NotFound();

        await _needRepository.DeleteAsync(id);

        TempData["Success"] = "Behovet er slettet.";

        return RedirectToAction(nameof(Needs));
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
