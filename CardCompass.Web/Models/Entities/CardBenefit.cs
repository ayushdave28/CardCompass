using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCompass.Web.Models.Entities
{
    public class CardBenefit
    {
        public int Id { get; set; }

        public int CreditCardId { get; set; }
        public CreditCard? CreditCard { get; set; }

        public BenefitType BenefitType { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedAnnualValue { get; set; }

        [MaxLength(500)]
        public string Terms { get; set; } = string.Empty;
    }
}
