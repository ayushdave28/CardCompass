using CardCompass.Web.Data;
using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CardCompass.Web.Services
{
    public class CardService : ICardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CardService> _logger;

        public CardService(ApplicationDbContext context, ILogger<CardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CardCatalogViewModel> GetCatalogAsync(CardFilterViewModel filter, string? userId = null)
        {
            var query = _context.CreditCards
                .Include(c => c.Bank)
                .Include(c => c.RewardRules)
                .Include(c => c.Benefits)
                .Where(c => c.IsActive)
                .AsQueryable();

            // Search query filter
            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                var term = filter.SearchQuery.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(term) ||
                                         (c.Bank != null && c.Bank.Name.ToLower().Contains(term)) ||
                                         c.PublishedEligibility.ToLower().Contains(term));
            }

            // Bank filter
            if (filter.BankId.HasValue && filter.BankId.Value > 0)
            {
                query = query.Where(c => c.BankId == filter.BankId.Value);
            }

            // Category filter
            if (filter.Category.HasValue)
            {
                query = query.Where(c => c.Category == filter.Category.Value);
            }

            // Network filter
            if (filter.Network.HasValue)
            {
                query = query.Where(c => c.Network == filter.Network.Value);
            }

            // RuPay/UPI filter
            if (filter.RuPayOnly)
            {
                query = query.Where(c => c.RuPaySupported);
            }

            // Lounge filter
            if (filter.LoungeOnly)
            {
                query = query.Where(c => c.LoungeAccessDomestic > 0 || c.LoungeAccessInternational > 0);
            }

            // Fee Type filter
            if (!string.IsNullOrWhiteSpace(filter.FeeType) && filter.FeeType != "all")
            {
                switch (filter.FeeType.ToLower())
                {
                    case "free":
                        query = query.Where(c => c.AnnualFee == 0);
                        break;
                    case "under1000":
                        query = query.Where(c => c.AnnualFee > 0 && c.AnnualFee <= 1000);
                        break;
                    case "under5000":
                        query = query.Where(c => c.AnnualFee > 1000 && c.AnnualFee <= 5000);
                        break;
                    case "premium":
                        query = query.Where(c => c.AnnualFee > 5000);
                        break;
                }
            }

            // Sorting
            query = filter.SortBy.ToLower() switch
            {
                "fee_asc" => query.OrderBy(c => (double)c.AnnualFee),
                "fee_desc" => query.OrderByDescending(c => (double)c.AnnualFee),
                "rewards_desc" => query.OrderByDescending(c => c.RewardRules.Max(r => (double?)r.EarningRatePercent) ?? 0.0),
                _ => query.OrderByDescending(c => c.IsFeatured).ThenBy(c => (double)c.JoiningFee)
            };

            var totalItems = await query.CountAsync();

            var cards = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var availableBanks = await _context.Banks.OrderBy(b => b.Name).ToListAsync();

            var userFavorites = new List<int>();
            if (!string.IsNullOrEmpty(userId))
            {
                userFavorites = await _context.UserFavorites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.CreditCardId)
                    .ToListAsync();
            }

            return new CardCatalogViewModel
            {
                Cards = cards,
                AvailableBanks = availableBanks,
                Filter = filter,
                TotalItems = totalItems,
                UserFavoriteCardIds = userFavorites
            };
        }

        public async Task<CreditCard?> GetCardBySlugAsync(string slug)
        {
            return await _context.CreditCards
                .Include(c => c.Bank)
                .Include(c => c.RewardRules)
                .Include(c => c.Benefits)
                .Include(c => c.Eligibility)
                .Include(c => c.Sources)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
        }

        public async Task<CreditCard?> GetCardByIdAsync(int id)
        {
            return await _context.CreditCards
                .Include(c => c.Bank)
                .Include(c => c.RewardRules)
                .Include(c => c.Benefits)
                .Include(c => c.Eligibility)
                .Include(c => c.Sources)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<CreditCard>> GetFeaturedCardsAsync(int count = 6)
        {
            return await _context.CreditCards
                .Include(c => c.Bank)
                .Include(c => c.RewardRules)
                .Where(c => c.IsActive && c.IsFeatured)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<CreditCard>> GetCardsByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.CreditCards
                .Include(c => c.Bank)
                .Include(c => c.RewardRules)
                .Include(c => c.Benefits)
                .Include(c => c.Eligibility)
                .Include(c => c.Sources)
                .Where(c => ids.Contains(c.Id) && c.IsActive)
                .ToListAsync();
        }

        public async Task<List<Bank>> GetAllBanksAsync()
        {
            return await _context.Banks.OrderBy(b => b.Name).ToListAsync();
        }

        public async Task<List<CreditCard>> GetAllActiveCardsAsync()
        {
            return await _context.CreditCards
                .Include(c => c.Bank)
                .Include(c => c.RewardRules)
                .Include(c => c.Benefits)
                .Include(c => c.Eligibility)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<int> CreateCardAsync(CreditCard card)
        {
            _context.CreditCards.Add(card);
            await _context.SaveChangesAsync();
            return card.Id;
        }

        public async Task UpdateCardAsync(CreditCard card)
        {
            _context.CreditCards.Update(card);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleCardActiveAsync(int id)
        {
            var card = await _context.CreditCards.FindAsync(id);
            if (card != null)
            {
                card.IsActive = !card.IsActive;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteCardAsync(int id)
        {
            var card = await _context.CreditCards.FindAsync(id);
            if (card != null)
            {
                _context.CreditCards.Remove(card);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalCardsCountAsync()
        {
            return await _context.CreditCards.CountAsync();
        }

        public async Task<int> GetActiveCardsCountAsync()
        {
            return await _context.CreditCards.CountAsync(c => c.IsActive);
        }
    }
}
