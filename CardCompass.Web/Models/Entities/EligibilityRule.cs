using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCompass.Web.Models.Entities
{
    public class EligibilityRule
    {
        public int Id { get; set; }

        public int CreditCardId { get; set; }
        public CreditCard? CreditCard { get; set; }

        public int MinAge { get; set; } = 21;

        public int MaxAge { get; set; } = 65;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinAnnualIncome { get; set; }

        public EmploymentType EmploymentType { get; set; } = EmploymentType.Any;

        public int SuggestedCreditScore { get; set; } = 750;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
