using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CardCompass.Web.Controllers
{
    public class PurchaseOptimizerController : Controller
    {
        private readonly IPurchaseOptimizerService _optimizerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchaseOptimizerController(
            IPurchaseOptimizerService optimizerService,
            UserManager<ApplicationUser> userManager)
        {
            _optimizerService = optimizerService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] PurchaseOptimizerViewModel? model)
        {
            var vm = model ?? new PurchaseOptimizerViewModel();
            var userId = _userManager.GetUserId(User);

            if (Request.Query.ContainsKey("Amount") || Request.Query.ContainsKey("Category"))
            {
                vm = await _optimizerService.OptimizePurchaseAsync(vm, userId);
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Optimize(PurchaseOptimizerViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _optimizerService.OptimizePurchaseAsync(model, userId);
            return View("Index", result);
        }
    }
}
