using CardCompass.Web.Models.Entities;

namespace CardCompass.Web.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<CreditCard> FeaturedCards { get; set; } = new();
        public int TotalCardsCount { get; set; }
        public int TotalBanksCount { get; set; }
        public int LifetimeFreeCount { get; set; }
        public int RuPayCount { get; set; }
        public List<CreditCard> TopCashbackCards { get; set; } = new();
        public List<CreditCard> TopTravelCards { get; set; } = new();
        public List<CreditCard> TopRuPayCards { get; set; } = new();
        public List<Bank> SupportedBanks { get; set; } = new();
    }
}
