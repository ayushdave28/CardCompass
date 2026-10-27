using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;

namespace CardCompass.Web.Services
{
    public interface ICardService
    {
        Task<CardCatalogViewModel> GetCatalogAsync(CardFilterViewModel filter, string? userId = null);
        Task<CreditCard?> GetCardBySlugAsync(string slug);
        Task<CreditCard?> GetCardByIdAsync(int id);
        Task<List<CreditCard>> GetFeaturedCardsAsync(int count = 6);
        Task<List<CreditCard>> GetCardsByIdsAsync(IEnumerable<int> ids);
        Task<List<Bank>> GetAllBanksAsync();
        Task<List<CreditCard>> GetAllActiveCardsAsync();

        // Admin CRUD
        Task<int> CreateCardAsync(CreditCard card);
        Task UpdateCardAsync(CreditCard card);
        Task ToggleCardActiveAsync(int id);
        Task DeleteCardAsync(int id);
        Task<int> GetTotalCardsCountAsync();
        Task<int> GetActiveCardsCountAsync();
    }
}
