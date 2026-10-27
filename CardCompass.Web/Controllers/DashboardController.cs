using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CardCompass.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUserPortfolioService _portfolioService;
        private readonly ICardService _cardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            IUserPortfolioService portfolioService,
            ICardService cardService,
            UserManager<ApplicationUser> userManager)
        {
            _portfolioService = portfolioService;
            _cardService = cardService;
            _userManager = userManager;
        }

        // Overview
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var vm = await _portfolioService.GetDashboardDataAsync(userId);
            ViewBag.AvailableCards = await _cardService.GetAllActiveCardsAsync();
            return View(vm);
        }

        // Wallet Cards View
        public async Task<IActionResult> Wallet()
        {
            var userId = _userManager.GetUserId(User)!;
            var vm = await _portfolioService.GetDashboardDataAsync(userId);
            ViewBag.AvailableCards = await _cardService.GetAllActiveCardsAsync();
            return View(vm);
        }

        // Expenses Ledger View
        public async Task<IActionResult> Expenses()
        {
            var userId = _userManager.GetUserId(User)!;
            var vm = await _portfolioService.GetDashboardDataAsync(userId);
            return View(vm);
        }

        // Add Card
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCard(AddUserCardViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;
            if (ModelState.IsValid)
            {
                var result = await _portfolioService.AddUserCardAsync(userId, model);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Credit card added to your personal wallet.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add card. Card was not found in directory.";
                }
            }
            return RedirectToAction(nameof(Wallet));
        }

        // Remove Card
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCard(int userCardId)
        {
            var userId = _userManager.GetUserId(User)!;
            var success = await _portfolioService.RemoveUserCardAsync(userId, userCardId);
            if (success)
            {
                TempData["SuccessMessage"] = "Card removed from your wallet.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not remove card or permission denied.";
            }
            return RedirectToAction(nameof(Wallet));
        }

        // Add Expense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(AddExpenseViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;
            if (ModelState.IsValid)
            {
                var expense = await _portfolioService.AddExpenseAsync(userId, model);
                if (expense != null)
                {
                    TempData["SuccessMessage"] = $"Expense of ₹{expense.Amount:N0} recorded! Estimated rewards earned: ₹{expense.EstimatedRewardEarned:N2}";
                }
            }
            return RedirectToAction(nameof(Expenses));
        }

        // Remove Expense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveExpense(int expenseId)
        {
            var userId = _userManager.GetUserId(User)!;
            var success = await _portfolioService.RemoveExpenseAsync(userId, expenseId);
            if (success)
            {
                TempData["SuccessMessage"] = "Expense record deleted.";
            }
            return RedirectToAction(nameof(Expenses));
        }

        // Update Reward Balance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBalance(UpdateRewardBalanceViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;
            if (ModelState.IsValid)
            {
                await _portfolioService.UpdateRewardBalanceAsync(userId, model);
                TempData["SuccessMessage"] = "Reward balance updated successfully.";
            }
            return RedirectToAction(nameof(Wallet));
        }

        // Remove Saved Comparison
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveComparison(int comparisonId)
        {
            var userId = _userManager.GetUserId(User)!;
            await _portfolioService.RemoveComparisonAsync(userId, comparisonId);
            TempData["SuccessMessage"] = "Saved comparison removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
