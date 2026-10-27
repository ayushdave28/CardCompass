using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CardCompass.Web.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly ICardService _cardService;
        private readonly IRewardCalculatorService _calculatorService;

        public CalculatorController(
            ICardService cardService,
            IRewardCalculatorService calculatorService)
        {
            _cardService = cardService;
            _calculatorService = calculatorService;
        }

        public async Task<IActionResult> Index([FromQuery] int? cardId, [FromQuery] SpendingInputModel? spend)
        {
            var availableCards = await _cardService.GetAllActiveCardsAsync();
            var targetCard = cardId.HasValue
                ? availableCards.FirstOrDefault(c => c.Id == cardId.Value)
                : availableCards.FirstOrDefault(c => c.IsFeatured) ?? availableCards.FirstOrDefault();

            var spendModel = spend ?? new SpendingInputModel();
            RewardCalculationResult? result = null;

            if (targetCard != null)
            {
                result = _calculatorService.CalculateRewards(targetCard, spendModel);
            }

            var vm = new CalculatorPageViewModel
            {
                AvailableCards = availableCards,
                SelectedCard = targetCard,
                SpendInput = spendModel,
                Result = result
            };

            return View(vm);
        }
    }
}
