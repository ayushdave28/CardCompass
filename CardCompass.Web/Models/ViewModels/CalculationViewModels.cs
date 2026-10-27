using CardCompass.Web.Models.Entities;

namespace CardCompass.Web.Models.ViewModels
{
    public class CategoryRewardDetail
    {
        public SpendingCategory Category { get; set; }
        public decimal MonthlySpend { get; set; }
        public decimal MonthlyEarnedRupees { get; set; }
        public decimal AnnualEarnedRupees => MonthlyEarnedRupees * 12;
        public decimal EffectiveRatePercent { get; set; }
        public bool CapReached { get; set; }
        public decimal? CapAmountRupees { get; set; }
        public string RuleApplied { get; set; } = string.Empty;
    }

    public class RewardCalculationResult
    {
        public int CreditCardId { get; set; }
        public string CardName { get; set; } = string.Empty;
        public decimal MonthlySpendTotal { get; set; }
        public decimal AnnualSpendTotal => MonthlySpendTotal * 12;
        public decimal MonthlyRewardsRupees { get; set; }
        public decimal AnnualRewardsRupees => MonthlyRewardsRupees * 12;
        public decimal EffectiveAnnualFee { get; set; }
        public bool IsFeeWaived { get; set; }
        public decimal WelcomeBonusValue { get; set; }
        public decimal JoiningFee { get; set; }
        public decimal NetFirstYearBenefit { get; set; }
        public decimal NetRenewalYearBenefit { get; set; }
        public List<CategoryRewardDetail> CategoryDetails { get; set; } = new();
        public List<string> AppliedCaps { get; set; } = new();
        public List<string> ExclusionsNotes { get; set; } = new();
        public List<string> AssumptionsAndDisclosures { get; set; } = new();
    }

    public class SpendingInputModel
    {
        public int CardId { get; set; }
        public decimal OnlineShopping { get; set; } = 0;
        public decimal Grocery { get; set; } = 0;
        public decimal Dining { get; set; } = 0;
        public decimal Travel { get; set; } = 0;
        public decimal Fuel { get; set; } = 0;
        public decimal Utilities { get; set; } = 0;
        public decimal Entertainment { get; set; } = 0;
        public decimal UPI { get; set; } = 0;
        public decimal GeneralSpends { get; set; } = 0;
        public decimal International { get; set; } = 0;

        public decimal TotalMonthly => OnlineShopping + Grocery + Dining + Travel + Fuel + Utilities + Entertainment + UPI + GeneralSpends + International;
    }

    public class CalculatorPageViewModel
    {
        public List<CreditCard> AvailableCards { get; set; } = new();
        public CreditCard? SelectedCard { get; set; }
        public SpendingInputModel SpendInput { get; set; } = new();
        public RewardCalculationResult? Result { get; set; }
    }
}
