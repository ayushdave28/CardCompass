using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;

namespace CardCompass.Web.Services
{
    public interface IRewardCalculatorService
    {
        RewardCalculationResult CalculateRewards(CreditCard card, SpendingInputModel spend);
    }
}
