using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CardCompass.Web.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly IRecommendationService _recommendationService;
        private readonly ICardService _cardService;

        public RecommendationController(
            IRecommendationService recommendationService,
            ICardService cardService)
        {
            _recommendationService = recommendationService;
            _cardService = cardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] RecommendationWizardViewModel? model)
        {
            var vm = model ?? new RecommendationWizardViewModel();
            var allCards = await _cardService.GetAllActiveCardsAsync();
            ViewBag.AllCards = allCards;

            // If user clicked submit or came with params
            if (Request.Query.ContainsKey("MonthlyIncome") || Request.Query.ContainsKey("Spending.OnlineShopping"))
            {
                vm.Recommendations = await _recommendationService.GetRecommendationsAsync(vm);
                vm.HasCalculated = true;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(RecommendationWizardViewModel model)
        {
            var allCards = await _cardService.GetAllActiveCardsAsync();
            ViewBag.AllCards = allCards;

            model.Recommendations = await _recommendationService.GetRecommendationsAsync(model);
            model.HasCalculated = true;

            return View("Index", model);
        }
    }
}
