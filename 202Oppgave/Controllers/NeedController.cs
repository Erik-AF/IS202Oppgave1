using Microsoft.AspNetCore.Mvc;

using _202Oppgave.ViewModels;

namespace _202Oppgave.Controllers
{
    public class NeedController : Controller
    {
        private static readonly List<NeedViewModel> _needs = new();

        [HttpGet]
        public IActionResult Index()
        {
            return View(_needs);
        }

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

            _needs.Add(model);

            return View("Confirmation", model);
        }
    }
}
