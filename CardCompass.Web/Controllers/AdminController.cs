using CardCompass.Web.Data;
using CardCompass.Web.Models.Entities;
using CardCompass.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CardCompass.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICardService _cardService;
        private readonly IFeedbackService _feedbackService;

        public AdminController(
            ApplicationDbContext context,
            ICardService cardService,
            IFeedbackService feedbackService)
        {
            _context = context;
            _cardService = cardService;
            _feedbackService = feedbackService;
        }

        // Admin Overview
        public async Task<IActionResult> Index()
        {
            var totalCards = await _context.CreditCards.CountAsync();
            var activeCards = await _context.CreditCards.CountAsync(c => c.IsActive);
            var totalBanks = await _context.Banks.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var feedbacksCount = await _context.Feedbacks.CountAsync();
            var pendingFeedbacks = await _feedbackService.GetPendingCountAsync();

            var cards = await _context.CreditCards
                .Include(c => c.Bank)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.TotalCards = totalCards;
            ViewBag.ActiveCards = activeCards;
            ViewBag.TotalBanks = totalBanks;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.FeedbacksCount = feedbacksCount;
            ViewBag.PendingFeedbacks = pendingFeedbacks;

            return View(cards);
        }

        // Create Card (GET & POST)
        [HttpGet]
        public async Task<IActionResult> CreateCard()
        {
            ViewBag.Banks = await _context.Banks.OrderBy(b => b.Name).ToListAsync();
            return View(new CreditCard
            {
                CardGradient = "linear-gradient(135deg, #1e3a8a 0%, #0f172a 100%)",
                ChipColor = "gold"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCard(CreditCard card)
        {
            if (ModelState.IsValid)
            {
                // Auto generate slug if empty
                if (string.IsNullOrWhiteSpace(card.Slug))
                {
                    card.Slug = card.Name.ToLower().Replace(" ", "-").Replace("&", "and");
                }

                card.CreatedAt = DateTime.UtcNow;
                card.SourceVerifiedDate = DateTime.UtcNow;

                await _cardService.CreateCardAsync(card);
                TempData["SuccessMessage"] = $"Credit card '{card.Name}' successfully added to database.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Banks = await _context.Banks.OrderBy(b => b.Name).ToListAsync();
            return View(card);
        }

        // Edit Card (GET & POST)
        [HttpGet]
        public async Task<IActionResult> EditCard(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card == null) return NotFound();

            ViewBag.Banks = await _context.Banks.OrderBy(b => b.Name).ToListAsync();
            return View(card);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCard(CreditCard card)
        {
            if (ModelState.IsValid)
            {
                card.SourceVerifiedDate = DateTime.UtcNow;
                await _cardService.UpdateCardAsync(card);
                TempData["SuccessMessage"] = $"Credit card '{card.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Banks = await _context.Banks.OrderBy(b => b.Name).ToListAsync();
            return View(card);
        }

        // Toggle Active
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            await _cardService.ToggleCardActiveAsync(id);
            TempData["SuccessMessage"] = "Card status toggled successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Delete Card
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCard(int id)
        {
            await _cardService.DeleteCardAsync(id);
            TempData["SuccessMessage"] = "Card deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Banks Management
        [HttpGet]
        public async Task<IActionResult> Banks()
        {
            var banks = await _context.Banks.Include(b => b.Cards).OrderBy(b => b.Name).ToListAsync();
            return View(banks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBank(Bank bank)
        {
            if (ModelState.IsValid)
            {
                _context.Banks.Add(bank);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Bank '{bank.Name}' created successfully.";
            }
            return RedirectToAction(nameof(Banks));
        }

        // Feedback Review
        [HttpGet]
        public async Task<IActionResult> Feedbacks()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
            return View(feedbacks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFeedbackReviewed(int id)
        {
            await _feedbackService.MarkReviewedAsync(id);
            TempData["SuccessMessage"] = "Feedback marked as reviewed.";
            return RedirectToAction(nameof(Feedbacks));
        }
    }
}
