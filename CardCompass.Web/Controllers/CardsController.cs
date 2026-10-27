using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CardCompass.Web.Controllers
{
    public class CardsController : Controller
    {
        private readonly ICardService _cardService;
        private readonly IRewardCalculatorService _calculatorService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CardsController(
            ICardService cardService,
            IRewardCalculatorService calculatorService,
            UserManager<ApplicationUser> userManager)
        {
            _cardService = cardService;
            _calculatorService = calculatorService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index([FromQuery] CardFilterViewModel filter)
        {
            var userId = _userManager.GetUserId(User);
            var model = await _cardService.GetCatalogAsync(filter, userId);
            return View(model);
        }

        public async Task<IActionResult> Details(string? id, string? slug)
        {
            var identifier = !string.IsNullOrWhiteSpace(slug) ? slug : id;
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return NotFound();
            }

            CreditCard? card = null;
            if (int.TryParse(identifier, out int cardId))
            {
                card = await _cardService.GetCardByIdAsync(cardId);
            }

            if (card == null)
            {
                card = await _cardService.GetCardBySlugAsync(identifier);
            }

            if (card == null)
            {
                return NotFound();
            }

            // Estimate default rewards on standard spending profile (₹50k/mo)
            var defaultSpend = new SpendingInputModel
            {
                OnlineShopping = 15000,
                Grocery = 8000,
                Dining = 5000,
                Travel = 5000,
                Fuel = 4000,
                Utilities = 3000,
                GeneralSpends = 10000
            };

            var calc = _calculatorService.CalculateRewards(card, defaultSpend);

            // Fetch 3 similar cards in same category
            var allActive = await _cardService.GetAllActiveCardsAsync();
            var similar = allActive
                .Where(c => c.Id != card.Id && (c.Category == card.Category || c.BankId == card.BankId))
                .Take(3)
                .ToList();

            var userId = _userManager.GetUserId(User);
            bool isFav = false;
            if (!string.IsNullOrEmpty(userId))
            {
                var catalog = await _cardService.GetCatalogAsync(new CardFilterViewModel(), userId);
                isFav = catalog.UserFavoriteCardIds.Contains(card.Id);
            }

            var model = new CardDetailViewModel
            {
                Card = card,
                SimilarCards = similar,
                IsUserFavorite = isFav,
                EstimatedAnnualRewardsDefault = calc
            };

            return View(model);
        }
    }
}
