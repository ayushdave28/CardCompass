using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCompass.Web.Models.Entities
{
    public class Expense
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int? UserCardId { get; set; }
        public UserCard? UserCard { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public SpendingCategory Category { get; set; } = SpendingCategory.GeneralSpends;

        [Required, MaxLength(150)]
        public string MerchantName { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string Notes { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedRewardEarned { get; set; }
    }
}
