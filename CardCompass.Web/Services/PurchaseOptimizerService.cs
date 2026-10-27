using CardCompass.Web.Data;
using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CardCompass.Web.Services
{
    public class PurchaseOptimizerService : IPurchaseOptimizerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICardService _cardService;

        public PurchaseOptimizerService(ApplicationDbContext context, ICardService cardService)
        {
            _context = context;
            _cardService = cardService;
        }

        public async Task<PurchaseOptimizerViewModel> OptimizePurchaseAsync(PurchaseOptimizerViewModel input, string? userId = null)
        {
            List<CreditCard> candidateCards;

            if (input.SelectionMode == "wallet" && !string.IsNullOrEmpty(userId))
            {
                var userCardIds = await _context.UserCards
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => uc.CreditCardId)
                    .ToListAsync();

                if (userCardIds.Any())
                {
                    candidateCards = await _cardService.GetCardsByIdsAsync(userCardIds);
                }
                else
                {
                    candidateCards = await _cardService.GetAllActiveCardsAsync();
                }
            }
            else
            {
                candidateCards = await _cardService.GetAllActiveCardsAsync();
            }

            var options = new List<PurchaseOptionResult>();

            foreach (var card in candidateCards)
            {
                var rule = card.RewardRules.FirstOrDefault(r => r.Category == input.Category)
                           ?? card.RewardRules.FirstOrDefault(r => r.Category == SpendingCategory.GeneralSpends)
                           ?? card.RewardRules.FirstOrDefault();

                decimal returnRupees = 0;
                decimal effectiveRate = 0;
                string explanation = "";
                string capNotice = "";

                if (rule != null)
                {
                    if (rule.MinimumSpendAmount.HasValue && input.Amount < rule.MinimumSpendAmount.Value)
                    {
                        explanation = $"Transaction amount ₹{input.Amount:N0} is below the qualifying minimum of ₹{rule.MinimumSpendAmount:N0}.";
                        returnRupees = 0;
                        effectiveRate = 0;
                    }
                    else
                    {
                        if (rule.PointsPer100Rupees > 0)
                        {
                            var points = (input.Amount / 100m) * rule.PointsPer100Rupees;
                            returnRupees = points * rule.PointRupeeValue;
                            effectiveRate = rule.PointsPer100Rupees * rule.PointRupeeValue;
                            explanation = $"Earns {rule.PointsPer100Rupees:0.##} points per ₹100 (1 pt = ₹{rule.PointRupeeValue:0.##}). Value: ₹{returnRupees:N2}.";
                        }
                        else
                        {
                            effectiveRate = rule.EarningRatePercent;
                            returnRupees = input.Amount * (effectiveRate / 100m);
                            explanation = $"Earns {effectiveRate:0.##}% direct cashback. Value: ₹{returnRupees:N2}.";
                        }

                        if (rule.MonthlyCapRupees.HasValue)
                        {
                            capNotice = $"Monthly category cashback cap: ₹{rule.MonthlyCapRupees:N0}.";
                        }

                        if (!string.IsNullOrEmpty(rule.ExcludedTransactions))
                        {
                            capNotice += $" Exclusions apply: {rule.ExcludedTransactions}.";
                        }
                    }
                }
                else
                {
                    explanation = "Standard retail earning rules apply.";
                }

                options.Add(new PurchaseOptionResult
                {
                    Card = card,
                    EstimatedReturnRupees = returnRupees,
                    EffectiveRatePercent = effectiveRate,
                    RewardTypeDescription = rule?.RewardType.ToString() ?? "Rewards",
                    Explanation = explanation,
                    WarningOrCapNotice = capNotice
                });
            }

            var sortedOptions = options
                .OrderByDescending(o => o.EstimatedReturnRupees)
                .ThenByDescending(o => o.EffectiveRatePercent)
                .ToList();

            input.BestCardOption = sortedOptions.FirstOrDefault();
            input.AlternativeOptions = sortedOptions.Skip(1).Take(4).ToList();
            input.HasCalculated = true;

            return input;
        }
    }
}
