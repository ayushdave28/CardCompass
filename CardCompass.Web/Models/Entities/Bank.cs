using System.ComponentModel.DataAnnotations;

namespace CardCompass.Web.Models.Entities
{
    public class Bank
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string LogoUrl { get; set; } = string.Empty;

        [MaxLength(255)]
        public string WebsiteUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        public string CustomerCareNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PrimaryColor { get; set; } = "#0F2744";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CreditCard> Cards { get; set; } = new List<CreditCard>();
    }
}
