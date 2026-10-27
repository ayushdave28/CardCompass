using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCompass.Web.Models.Entities
{
    public class CreditCard
    {
        public int Id { get; set; }

        public int BankId { get; set; }
        public Bank? Bank { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Slug { get; set; } = string.Empty;

        public CardNetwork Network { get; set; } = CardNetwork.Visa;

        public CardCategory Category { get; set; } = CardCategory.Cashback;

        [Column(TypeName = "decimal(18,2)")]
        public decimal JoiningFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FeeWaiverSpendThreshold { get; set; }

        [MaxLength(255)]
        public string FeeWaiverDescription { get; set; } = string.Empty;

        public bool RuPaySupported { get; set; }

        public int LoungeAccessDomestic { get; set; } // Visits per calendar year or quarter description

        public int LoungeAccessInternational { get; set; }

        [MaxLength(255)]
        public string LoungeDetails { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal ForexMarkupPercent { get; set; } = 3.5m;

        [MaxLength(255)]
        public string WelcomeBonus { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal WelcomeBonusMonetaryValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumIncomeMonthly { get; set; }

        [MaxLength(500)]
        public string PublishedEligibility { get; set; } = string.Empty;

        public string Pros { get; set; } = string.Empty; // Newline-delimited

        public string Cons { get; set; } = string.Empty; // Newline-delimited

        [MaxLength(500)]
        public string OfficialProductUrl { get; set; } = string.Empty;

        public DateTime SourceVerifiedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        [MaxLength(255)]
        public string CardGradient { get; set; } = "linear-gradient(135deg, #1e3a8a 0%, #0f172a 100%)";

        [MaxLength(20)]
        public string ChipColor { get; set; } = "gold"; // gold or silver

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<RewardRule> RewardRules { get; set; } = new List<RewardRule>();
        public ICollection<CardBenefit> Benefits { get; set; } = new List<CardBenefit>();
        public EligibilityRule? Eligibility { get; set; }
        public ICollection<CardSource> Sources { get; set; } = new List<CardSource>();
        public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
        public ICollection<UserCard> UserCards { get; set; } = new List<UserCard>();
    }
}
