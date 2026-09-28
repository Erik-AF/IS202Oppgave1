
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using _202Oppgave.Models;
using _202Oppgave.ViewModels;

namespace _202Oppgave.Controllers;

public class ResourceController : Controller
{
    
    private static readonly List<Resource> _resources = new();

    public IActionResult Index()
    {
        return View(_resources);
    }

    public IActionResult Register()
    {
        return View(new ResourceViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(ResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resource = new Resource
        {
            Id = _resources.Count + 1,
            Type = model.Type,
            Description = model.Description,
            Area = model.Area,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            AvailableFrom = model.AvailableFrom,
            AvailableTo = model.AvailableTo,
            ContactName = model.ContactName,
            ContactPhone = model.ContactPhone,
            ContactEmail = model.ContactEmail,
            Status = model.Status,
            OwnerName = model.OwnerName
        };

        _resources.Add(resource);

        return RedirectToAction(nameof(Details), new { id = resource.Id });
    }

    public IActionResult Details(int id)
    {
        var resource = _resources.FirstOrDefault(r => r.Id == id);
        if (resource is null)
        {
            return NotFound();
        }

        return View(resource);
    }
}