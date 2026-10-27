using CardCompass.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace CardCompass.Web.Models.ViewModels
{
    public class UserDashboardViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<UserCard> UserCards { get; set; } = new();
        public List<Expense> RecentExpenses { get; set; } = new();
        public List<SavedComparison> SavedComparisons { get; set; } = new();
        public List<CreditCard> FavoriteCards { get; set; } = new();
        public decimal TotalRewardPointsValue { get; set; }
        public decimal TotalMonthlyExpenses { get; set; }
        public decimal TotalMonthlyRewardsEarned { get; set; }
        public Dictionary<SpendingCategory, decimal> SpendingByCategory { get; set; } = new();
    }

    public class AddUserCardViewModel
    {
        [Required]
        public int CreditCardId { get; set; }

        [Required, MaxLength(50)]
        public string Nickname { get; set; } = "Primary Card";

        [Required, MaxLength(4), MinLength(4)]
        public string Last4Digits { get; set; } = "1234";

        [Range(1, 31)]
        public int BillingCycleDay { get; set; } = 15;

        [Range(1, 31)]
        public int PaymentDueDay { get; set; } = 5;

        public decimal? CreditLimit { get; set; }
    }

    public class AddExpenseViewModel
    {
        public int? UserCardId { get; set; }

        [Required, Range(1, 10000000)]
        public decimal Amount { get; set; }

        public SpendingCategory Category { get; set; } = SpendingCategory.OnlineShopping;

        [Required, MaxLength(100)]
        public string MerchantName { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Today;

        [MaxLength(200)]
        public string? Notes { get; set; }
    }

    public class UpdateRewardBalanceViewModel
    {
        [Required]
        public int UserCardId { get; set; }

        [Required, MaxLength(100)]
        public string ProgramName { get; set; } = "Reward Points";

        [Required, Range(0, 100000000)]
        public decimal PointsBalance { get; set; }

        [Range(0, 10000000)]
        public decimal EstimatedRupeeValue { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}
