using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCompass.Web.Models.Entities
{
    public class RewardRule
    {
        public int Id { get; set; }

        public int CreditCardId { get; set; }
        public CreditCard? CreditCard { get; set; }

        public SpendingCategory Category { get; set; }

        public RewardType RewardType { get; set; } = RewardType.Cashback;

        [Column(TypeName = "decimal(6,2)")]
        public decimal EarningRatePercent { get; set; } // Effective cashback percentage or points yield

        [Column(TypeName = "decimal(8,2)")]
        public decimal PointsPer100Rupees { get; set; } // Reward points earned per ₹100

        [Column(TypeName = "decimal(8,4)")]
        public decimal PointRupeeValue { get; set; } = 1.0m; // Value of 1 point in INR (e.g. 0.20, 0.25, 1.00)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyCapRupees { get; set; } // Monthly cashback or reward cap in INR

        public int? MonthlyCapPoints { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumSpendAmount { get; set; } // Minimum transaction amount to qualify

        [MaxLength(500)]
        public string ExcludedTransactions { get; set; } = string.Empty; // Rent, Fuel, Wallet load, EMI, etc.

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = true;
    }
}
