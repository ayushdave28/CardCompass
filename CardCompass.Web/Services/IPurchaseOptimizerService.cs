using CardCompass.Web.Models.ViewModels;

namespace CardCompass.Web.Services
{
    public interface IPurchaseOptimizerService
    {
        Task<PurchaseOptimizerViewModel> OptimizePurchaseAsync(PurchaseOptimizerViewModel input, string? userId = null);
    }
}
