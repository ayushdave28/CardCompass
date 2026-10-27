using CardCompass.Web.Data;
using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CardCompass.Web.Services
{
    public class UserPortfolioService : IUserPortfolioService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRewardCalculatorService _calculatorService;

        public UserPortfolioService(ApplicationDbContext context, IRewardCalculatorService calculatorService)
        {
            _context = context;
            _calculatorService = calculatorService;
        }

        public async Task<UserDashboardViewModel> GetDashboardDataAsync(string userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");

            var userCards = await _context.UserCards
                .Include(uc => uc.CreditCard)
                    .ThenInclude(c => c!.Bank)
                .Include(uc => uc.CreditCard)
                    .ThenInclude(c => c!.RewardRules)
                .Include(uc => uc.RewardBalances)
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            var recentExpenses = await _context.Expenses
                .Include(e => e.UserCard)
                    .ThenInclude(uc => uc!.CreditCard)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(15)
                .ToListAsync();

            var savedComparisons = await _context.SavedComparisons
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var favoriteCards = await _context.UserFavorites
                .Include(f => f.CreditCard)
                    .ThenInclude(c => c!.Bank)
                .Where(f => f.UserId == userId && f.CreditCard != null)
                .Select(f => f.CreditCard!)
                .ToListAsync();

            // Total reward points value across cards
            decimal totalPointsValue = userCards
                .SelectMany(uc => uc.RewardBalances)
                .Sum(rb => rb.EstimatedRupeeValue);

            // Monthly expenses total (current month)
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var nextMonth = startOfMonth.AddMonths(1);
            var currentMonthExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startOfMonth && e.Date < nextMonth)
                .ToListAsync();

            var monthlyExpensesTotal = currentMonthExpenses.Sum(e => e.Amount);
            var monthlyRewardsEarned = currentMonthExpenses.Sum(e => e.EstimatedRewardEarned);

            // Spending by category
            var spendingByCategory = currentMonthExpenses
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            return new UserDashboardViewModel
            {
                User = user,
                UserCards = userCards,
                RecentExpenses = recentExpenses,
                SavedComparisons = savedComparisons,
                FavoriteCards = favoriteCards,
                TotalRewardPointsValue = totalPointsValue,
                TotalMonthlyExpenses = monthlyExpensesTotal,
                TotalMonthlyRewardsEarned = monthlyRewardsEarned,
                SpendingByCategory = spendingByCategory
            };
        }

        public async Task<bool> ToggleFavoriteAsync(string userId, int cardId)
        {
            var existing = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.CreditCardId == cardId);

            if (existing != null)
            {
                _context.UserFavorites.Remove(existing);
                await _context.SaveChangesAsync();
                return false; // Removed
            }
            else
            {
                _context.UserFavorites.Add(new UserFavorite
                {
                    UserId = userId,
                    CreditCardId = cardId,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return true; // Added
            }
        }

        public async Task<UserCard?> AddUserCardAsync(string userId, AddUserCardViewModel model)
        {
            var card = await _context.CreditCards.FindAsync(model.CreditCardId);
            if (card == null) return null;

            var userCard = new UserCard
            {
                UserId = userId,
                CreditCardId = model.CreditCardId,
                Nickname = model.Nickname,
                Last4Digits = model.Last4Digits,
                BillingCycleDay = model.BillingCycleDay,
                PaymentDueDay = model.PaymentDueDay,
                CreditLimit = model.CreditLimit,
                AddedDate = DateTime.UtcNow
            };

            _context.UserCards.Add(userCard);
            await _context.SaveChangesAsync();

            // Initialize default reward balance entry
            _context.RewardBalances.Add(new RewardBalance
            {
                UserId = userId,
                UserCardId = userCard.Id,
                ProgramName = $"{card.Name} Rewards",
                PointsBalance = 0,
                EstimatedRupeeValue = 0,
                LastUpdated = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return userCard;
        }

        public async Task<bool> RemoveUserCardAsync(string userId, int userCardId)
        {
            var userCard = await _context.UserCards
                .FirstOrDefaultAsync(uc => uc.Id == userCardId && uc.UserId == userId);

            if (userCard == null) return false;

            _context.UserCards.Remove(userCard);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Expense?> AddExpenseAsync(string userId, AddExpenseViewModel model)
        {
            decimal estimatedReward = 0;

            if (model.UserCardId.HasValue)
            {
                var userCard = await _context.UserCards
                    .Include(uc => uc.CreditCard)
                        .ThenInclude(c => c!.RewardRules)
                    .FirstOrDefaultAsync(uc => uc.Id == model.UserCardId.Value && uc.UserId == userId);

                if (userCard?.CreditCard != null)
                {
                    var rule = userCard.CreditCard.RewardRules.FirstOrDefault(r => r.Category == model.Category)
                               ?? userCard.CreditCard.RewardRules.FirstOrDefault(r => r.Category == SpendingCategory.GeneralSpends);

                    if (rule != null)
                    {
                        if (rule.PointsPer100Rupees > 0)
                        {
                            var points = (model.Amount / 100m) * rule.PointsPer100Rupees;
                            estimatedReward = points * rule.PointRupeeValue;
                        }
                        else
                        {
                            estimatedReward = model.Amount * (rule.EarningRatePercent / 100m);
                        }

                        if (rule.MonthlyCapRupees.HasValue && estimatedReward > rule.MonthlyCapRupees.Value)
                        {
                            estimatedReward = rule.MonthlyCapRupees.Value;
                        }
                    }
                }
            }

            var expense = new Expense
            {
                UserId = userId,
                UserCardId = model.UserCardId,
                Amount = model.Amount,
                Category = model.Category,
                MerchantName = model.MerchantName,
                Date = model.Date,
                Notes = model.Notes ?? string.Empty,
                EstimatedRewardEarned = Math.Round(estimatedReward, 2)
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<bool> RemoveExpenseAsync(string userId, int expenseId)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId);

            if (expense == null) return false;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateRewardBalanceAsync(string userId, UpdateRewardBalanceViewModel model)
        {
            var userCard = await _context.UserCards
                .Include(uc => uc.CreditCard)
                    .ThenInclude(c => c!.RewardRules)
                .FirstOrDefaultAsync(uc => uc.Id == model.UserCardId && uc.UserId == userId);

            if (userCard == null) return false;

            var existingBalance = await _context.RewardBalances
                .FirstOrDefaultAsync(rb => rb.UserCardId == model.UserCardId && rb.UserId == userId);

            // Compute rupee value if not provided
            var pointValue = userCard.CreditCard?.RewardRules.FirstOrDefault()?.PointRupeeValue ?? 0.25m;
            var rupeeValue = model.EstimatedRupeeValue > 0 ? model.EstimatedRupeeValue : model.PointsBalance * pointValue;

            if (existingBalance != null)
            {
                existingBalance.ProgramName = model.ProgramName;
                existingBalance.PointsBalance = model.PointsBalance;
                existingBalance.EstimatedRupeeValue = rupeeValue;
                existingBalance.ExpiryDate = model.ExpiryDate;
                existingBalance.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                _context.RewardBalances.Add(new RewardBalance
                {
                    UserId = userId,
                    UserCardId = model.UserCardId,
                    ProgramName = model.ProgramName,
                    PointsBalance = model.PointsBalance,
                    EstimatedRupeeValue = rupeeValue,
                    ExpiryDate = model.ExpiryDate,
                    LastUpdated = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SavedComparison> SaveComparisonAsync(string userId, string title, List<int> cardIds)
        {
            var comparison = new SavedComparison
            {
                UserId = userId,
                Title = title,
                CardIdsJson = JsonSerializer.Serialize(cardIds),
                CreatedAt = DateTime.UtcNow
            };

            _context.SavedComparisons.Add(comparison);
            await _context.SaveChangesAsync();
            return comparison;
        }

        public async Task<bool> RemoveComparisonAsync(string userId, int comparisonId)
        {
            var comparison = await _context.SavedComparisons
                .FirstOrDefaultAsync(c => c.Id == comparisonId && c.UserId == userId);

            if (comparison == null) return false;

            _context.SavedComparisons.Remove(comparison);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
