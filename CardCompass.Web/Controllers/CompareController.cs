using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CardCompass.Web.Controllers
{
    public class CompareController : Controller
    {
        private readonly ICardService _cardService;
        private readonly IRewardCalculatorService _calculatorService;
        private readonly IUserPortfolioService _portfolioService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompareController(
            ICardService cardService,
            IRewardCalculatorService calculatorService,
            IUserPortfolioService portfolioService,
            UserManager<ApplicationUser> userManager)
        {
            _cardService = cardService;
            _calculatorService = calculatorService;
            _portfolioService = portfolioService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index([FromQuery] string? ids, [FromQuery] ComparisonSpendingProfile? profile)
        {
            var cardIds = new List<int>();

            if (!string.IsNullOrWhiteSpace(ids))
            {
                cardIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id > 0)
                    .Take(3)
                    .ToList();
            }

            // Default to 3 prominent cards if none specified
            if (cardIds.Count < 2)
            {
                var featured = await _cardService.GetFeaturedCardsAsync(3);
                cardIds = featured.Select(c => c.Id).Take(3).ToList();
            }

            var cards = await _cardService.GetCardsByIdsAsync(cardIds);
            var spendingProfile = profile ?? new ComparisonSpendingProfile();

            var spendModel = new SpendingInputModel
            {
                OnlineShopping = spendingProfile.OnlineShoppingMonthly,
                Grocery = spendingProfile.GroceryMonthly,
                Dining = spendingProfile.DiningMonthly,
                Travel = spendingProfile.TravelMonthly,
                Fuel = spendingProfile.FuelMonthly,
                Utilities = spendingProfile.UtilitiesMonthly,
                GeneralSpends = spendingProfile.GeneralMonthly
            };

            var calculations = new Dictionary<int, RewardCalculationResult>();
            foreach (var card in cards)
            {
                calculations[card.Id] = _calculatorService.CalculateRewards(card, spendModel);
            }

            // Determine category winners
            var winners = new Dictionary<string, int>();

            if (cards.Any())
            {
                // Winner: Highest Net Benefit
                var bestNet = calculations.OrderByDescending(kv => kv.Value.NetRenewalYearBenefit).FirstOrDefault();
                if (bestNet.Key > 0) winners["Net Financial Benefit"] = bestNet.Key;

                // Winner: Lowest Annual Fee
                var lowestFee = cards.OrderBy(c => c.AnnualFee).FirstOrDefault();
                if (lowestFee != null) winners["Lowest Annual Fee"] = lowestFee.Id;

                // Winner: Best Lounge Access
                var bestLounge = cards.OrderByDescending(c => c.LoungeAccessDomestic + c.LoungeAccessInternational).FirstOrDefault();
                if (bestLounge != null && (bestLounge.LoungeAccessDomestic > 0 || bestLounge.LoungeAccessInternational > 0))
                {
                    winners["Airport Lounge Access"] = bestLounge.Id;
                }

                // Winner: Lowest Forex Markup
                var bestForex = cards.OrderBy(c => c.ForexMarkupPercent).FirstOrDefault();
                if (bestForex != null) winners["Lowest Forex Markup"] = bestForex.Id;
            }

            var allActive = await _cardService.GetAllActiveCardsAsync();
            ViewBag.AllCards = allActive;

            var model = new CardComparisonViewModel
            {
                Cards = cards,
                SpendingProfile = spendingProfile,
                CardCalculations = calculations,
                CategoryWinners = winners
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveComparison(string title, string cardIds)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var ids = cardIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var i) ? i : 0)
                .Where(i => i > 0)
                .ToList();

            if (ids.Any())
            {
                await _portfolioService.SaveComparisonAsync(userId, title ?? "My Card Comparison", ids);
                TempData["SuccessMessage"] = "Comparison successfully saved to your dashboard!";
            }

            return RedirectToAction(nameof(Index), new { ids = cardIds });
        }
    }
}
