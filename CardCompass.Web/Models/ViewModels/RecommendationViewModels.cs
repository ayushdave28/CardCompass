using CardCompass.Web.Models.Entities;

namespace CardCompass.Web.Models.ViewModels
{
    public class RecommendationWizardViewModel
    {
        public decimal MonthlyIncome { get; set; } = 75000;
        public EmploymentType EmploymentType { get; set; } = EmploymentType.Salaried;
        public string FeePreference { get; set; } = "low_or_none"; // "none", "low_or_none", "any"
        public bool RequiresLoungeAccess { get; set; } = true;
        public bool PrefersRuPayUPI { get; set; } = false;
        public bool InternationalTraveler { get; set; } = false;

        public SpendingInputModel Spending { get; set; } = new();

        public List<int> ExistingCardIds { get; set; } = new();

        public List<CardRecommendationResult> Recommendations { get; set; } = new();

        public bool HasCalculated { get; set; } = false;
    }

    public class CardRecommendationResult
    {
        public CreditCard Card { get; set; } = null!;
        public int MatchScore { get; set; } // 0 - 100
        public string EligibilityStatus { get; set; } = "Likely Eligible"; // Likely Eligible, Uncertain, Possible Income Mismatch
        public RewardCalculationResult Calculation { get; set; } = null!;
        public List<string> MatchReasons { get; set; } = new();
        public List<string> Limitations { get; set; } = new();
        public string ComparisonAdvantage { get; set; } = string.Empty;
    }

    public class PurchaseOptimizerViewModel
    {
        public decimal Amount { get; set; } = 3500;
        public SpendingCategory Category { get; set; } = SpendingCategory.OnlineShopping;
        public string MerchantName { get; set; } = "Amazon";
        public string SelectionMode { get; set; } = "all"; // "all" or "wallet"
        
        public PurchaseOptionResult? BestCardOption { get; set; }
        public List<PurchaseOptionResult> AlternativeOptions { get; set; } = new();
        public bool HasCalculated { get; set; } = false;
    }

    public class PurchaseOptionResult
    {
        public CreditCard Card { get; set; } = null!;
        public decimal EstimatedReturnRupees { get; set; }
        public decimal EffectiveRatePercent { get; set; }
        public string RewardTypeDescription { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string WarningOrCapNotice { get; set; } = string.Empty;
    }
}
