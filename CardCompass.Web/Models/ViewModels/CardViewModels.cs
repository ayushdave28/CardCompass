using CardCompass.Web.Models.Entities;

namespace CardCompass.Web.Models.ViewModels
{
    public class CardFilterViewModel
    {
        public string? SearchQuery { get; set; }
        public int? BankId { get; set; }
        public CardCategory? Category { get; set; }
        public CardNetwork? Network { get; set; }
        public string? FeeType { get; set; } // "all", "free", "under1000", "under5000", "premium"
        public bool RuPayOnly { get; set; }
        public bool LoungeOnly { get; set; }
        public string SortBy { get; set; } = "popular"; // popular, fee_asc, fee_desc, rewards_desc
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 9;
    }

    public class CardCatalogViewModel
    {
        public List<CreditCard> Cards { get; set; } = new();
        public List<Bank> AvailableBanks { get; set; } = new();
        public CardFilterViewModel Filter { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / Filter.PageSize);
        public List<int> UserFavoriteCardIds { get; set; } = new();
    }

    public class CardDetailViewModel
    {
        public CreditCard Card { get; set; } = null!;
        public List<CreditCard> SimilarCards { get; set; } = new();
        public bool IsUserFavorite { get; set; }
        public RewardCalculationResult EstimatedAnnualRewardsDefault { get; set; } = null!;
    }

    public class CardComparisonViewModel
    {
        public List<CreditCard> Cards { get; set; } = new();
        public ComparisonSpendingProfile SpendingProfile { get; set; } = new();
        public Dictionary<int, RewardCalculationResult> CardCalculations { get; set; } = new();
        public Dictionary<string, int> CategoryWinners { get; set; } = new(); // e.g. "Online Shopping" -> CardId
    }

    public class ComparisonSpendingProfile
    {
        public decimal OnlineShoppingMonthly { get; set; } = 15000;
        public decimal GroceryMonthly { get; set; } = 8000;
        public decimal DiningMonthly { get; set; } = 5000;
        public decimal TravelMonthly { get; set; } = 5000;
        public decimal FuelMonthly { get; set; } = 4000;
        public decimal UtilitiesMonthly { get; set; } = 3000;
        public decimal GeneralMonthly { get; set; } = 10000;
        public decimal TotalMonthlySpend => OnlineShoppingMonthly + GroceryMonthly + DiningMonthly + TravelMonthly + FuelMonthly + UtilitiesMonthly + GeneralMonthly;
    }
}
