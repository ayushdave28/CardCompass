using CardCompass.Web.Models.ViewModels;

namespace CardCompass.Web.Services
{
    public interface IRecommendationService
    {
        Task<List<CardRecommendationResult>> GetRecommendationsAsync(RecommendationWizardViewModel model);
    }
}
