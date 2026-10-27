using System.ComponentModel.DataAnnotations;

namespace CardCompass.Web.Models.Entities
{
    public class CardSource
    {
        public int Id { get; set; }

        public int CreditCardId { get; set; }
        public CreditCard? CreditCard { get; set; }

        [Required, MaxLength(100)]
        public string SourceType { get; set; } = "Official Issuer Product Page"; // MITC, Schedule of Charges, etc.

        [Required, MaxLength(500)]
        public string SourceUrl { get; set; } = string.Empty;

        public DateTime VerifiedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string VerifiedBy { get; set; } = "CardCompass Research Team";

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
