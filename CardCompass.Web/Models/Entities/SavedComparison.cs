using System.ComponentModel.DataAnnotations;

namespace CardCompass.Web.Models.Entities
{
    public class SavedComparison
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = "My Card Comparison";

        [Required]
        public string CardIdsJson { get; set; } = "[]"; // e.g. "[1, 2, 3]"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
