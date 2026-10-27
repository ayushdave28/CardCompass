using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCompass.Web.Models.Entities
{
    public class RewardBalance
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int UserCardId { get; set; }
        public UserCard? UserCard { get; set; }

        [Required, MaxLength(100)]
        public string ProgramName { get; set; } = "Reward Points";

        [Column(TypeName = "decimal(18,2)")]
        public decimal PointsBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedRupeeValue { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
