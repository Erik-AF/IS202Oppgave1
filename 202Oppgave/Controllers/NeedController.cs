using Microsoft.AspNetCore.Mvc;

using _202Oppgave.ViewModels;

namespace _202Oppgave.Controllers
{
    public class NeedController : Controller
    {
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NeedViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return View("Confirmation", model);
        }
    }
}
