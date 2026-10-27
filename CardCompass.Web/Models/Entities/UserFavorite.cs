namespace CardCompass.Web.Models.Entities
{
    public class UserFavorite
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int CreditCardId { get; set; }
        public CreditCard? CreditCard { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
