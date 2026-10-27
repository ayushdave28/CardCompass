using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;

namespace CardCompass.Web.Services
{
    public interface IUserPortfolioService
    {
        Task<UserDashboardViewModel> GetDashboardDataAsync(string userId);
        Task<bool> ToggleFavoriteAsync(string userId, int cardId);
        Task<UserCard?> AddUserCardAsync(string userId, AddUserCardViewModel model);
        Task<bool> RemoveUserCardAsync(string userId, int userCardId);
        Task<Expense?> AddExpenseAsync(string userId, AddExpenseViewModel model);
        Task<bool> RemoveExpenseAsync(string userId, int expenseId);
        Task<bool> UpdateRewardBalanceAsync(string userId, UpdateRewardBalanceViewModel model);
        Task<SavedComparison> SaveComparisonAsync(string userId, string title, List<int> cardIds);
        Task<bool> RemoveComparisonAsync(string userId, int comparisonId);
    }
}
