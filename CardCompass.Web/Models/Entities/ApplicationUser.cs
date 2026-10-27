using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCompass.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlyIncome { get; set; }

        [MaxLength(100)]
        public string? Occupation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation collections
        public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
        public ICollection<UserCard> UserCards { get; set; } = new List<UserCard>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<SavedComparison> SavedComparisons { get; set; } = new List<SavedComparison>();
    }
}
