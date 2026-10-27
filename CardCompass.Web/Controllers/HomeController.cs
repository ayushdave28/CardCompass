using System.Diagnostics;
using CardCompass.Web.Data;
using CardCompass.Web.Models;
using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CardCompass.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICardService _cardService;
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            ICardService cardService,
            IFeedbackService feedbackService,
            ILogger<HomeController> logger)
        {
            _context = context;
            _cardService = cardService;
            _feedbackService = feedbackService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var featured = await _cardService.GetFeaturedCardsAsync(6);
            var totalCards = await _context.CreditCards.CountAsync(c => c.IsActive);
            var totalBanks = await _context.Banks.CountAsync();
            var ltfCount = await _context.CreditCards.CountAsync(c => c.IsActive && c.AnnualFee == 0);
            var rupayCount = await _context.CreditCards.CountAsync(c => c.IsActive && c.RuPaySupported);

            var topCashback = await _context.CreditCards
                .Include(c => c.Bank)
                .Include(c => c.RewardRules)
                .Where(c => c.IsActive && c.Category == CardCategory.Cashback)
                .Take(3)
                .ToListAsync();

            var topTravel = await _context.CreditCards
                .Include(c => c.Bank)
                .Include(c => c.RewardRules)
                .Where(c => c.IsActive && c.Category == CardCategory.Travel)
                .Take(3)
                .ToListAsync();

            var topRuPay = await _context.CreditCards
                .Include(c => c.Bank)
                .Include(c => c.RewardRules)
                .Where(c => c.IsActive && c.RuPaySupported)
                .Take(3)
                .ToListAsync();

            var banks = await _context.Banks.OrderBy(b => b.Name).ToListAsync();

            var model = new HomeViewModel
            {
                FeaturedCards = featured,
                TotalCardsCount = totalCards,
                TotalBanksCount = totalBanks,
                LifetimeFreeCount = ltfCount,
                RuPayCount = rupayCount,
                TopCashbackCards = topCashback,
                TopTravelCards = topTravel,
                TopRuPayCards = topRuPay,
                SupportedBanks = banks
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View(new Feedback());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                await _feedbackService.SubmitFeedbackAsync(feedback);
                TempData["SuccessMessage"] = "Thank you! Your feedback and inquiry have been received by the CardCompass editorial team.";
                return RedirectToAction(nameof(Contact));
            }

            return View(feedback);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
