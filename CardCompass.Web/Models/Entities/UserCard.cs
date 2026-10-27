using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCompass.Web.Models.Entities
{
    public class UserCard
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int CreditCardId { get; set; }
        public CreditCard? CreditCard { get; set; }

        [Required, MaxLength(100)]
        public string Nickname { get; set; } = string.Empty;

        [MaxLength(4)]
        public string Last4Digits { get; set; } = "XXXX";

        public int BillingCycleDay { get; set; } = 15; // Day of the month

        public int PaymentDueDay { get; set; } = 5; // Day of the month

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CreditLimit { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public ICollection<RewardBalance> RewardBalances { get; set; } = new List<RewardBalance>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
