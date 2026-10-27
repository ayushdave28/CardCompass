using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;

namespace CardCompass.Web.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ICardService _cardService;
        private readonly IRewardCalculatorService _calculatorService;

        public RecommendationService(ICardService cardService, IRewardCalculatorService calculatorService)
        {
            _cardService = cardService;
            _calculatorService = calculatorService;
        }

        public async Task<List<CardRecommendationResult>> GetRecommendationsAsync(RecommendationWizardViewModel model)
        {
            var allCards = await _cardService.GetAllActiveCardsAsync();
            var results = new List<CardRecommendationResult>();

            // Exclude cards already owned
            var candidateCards = allCards
                .Where(c => !model.ExistingCardIds.Contains(c.Id))
                .ToList();

            if (!candidateCards.Any())
            {
                candidateCards = allCards;
            }

            // Pre-calculate rewards for all candidates to find max return
            var calculatedCandidates = candidateCards.Select(card => new
            {
                Card = card,
                Calculation = _calculatorService.CalculateRewards(card, model.Spending)
            }).ToList();

            var maxNetBenefit = calculatedCandidates.Max(c => Math.Max(0, c.Calculation.NetRenewalYearBenefit));
            if (maxNetBenefit == 0) maxNetBenefit = 1;

            foreach (var item in calculatedCandidates)
            {
                var card = item.Card;
                var calc = item.Calculation;

                var matchReasons = new List<string>();
                var limitations = new List<string>();

                // 1. Eligibility evaluation
                var minIncomeRequired = card.MinimumIncomeMonthly > 0
                    ? card.MinimumIncomeMonthly
                    : (card.Eligibility != null ? card.Eligibility.MinAnnualIncome / 12m : 25000);

                string eligibilityStatus;
                if (model.MonthlyIncome >= minIncomeRequired)
                {
                    eligibilityStatus = "Likely Eligible";
                    matchReasons.Add($"Your monthly income (₹{model.MonthlyIncome:N0}) meets the issuer criteria (₹{minIncomeRequired:N0}).");
                }
                else if (model.MonthlyIncome >= minIncomeRequired * 0.8m)
                {
                    eligibilityStatus = "Uncertain / Additional criteria may apply";
                    limitations.Add($"Issuer benchmark is ₹{minIncomeRequired:N0}/mo; approval depends on existing banking relationship.");
                }
                else
                {
                    eligibilityStatus = "Possible Income Mismatch";
                    limitations.Add($"Published minimum monthly income is ₹{minIncomeRequired:N0}.");
                }

                // 2. Score Calculation (0 - 100)
                // Financial Benefit Weight (up to 45 pts)
                var benefitRatio = calc.NetRenewalYearBenefit > 0
                    ? (double)(calc.NetRenewalYearBenefit / maxNetBenefit)
                    : 0.0;
                double financialScore = Math.Min(45.0, benefitRatio * 45.0);

                // Preference alignment (up to 55 pts)
                double preferenceScore = 0;

                // Fee alignment
                if (model.FeePreference == "none")
                {
                    if (card.AnnualFee == 0)
                    {
                        preferenceScore += 20;
                        matchReasons.Add("Lifetime Free card: Zero annual fee, matching your preference.");
                    }
                    else if (calc.IsFeeWaived)
                    {
                        preferenceScore += 15;
                        matchReasons.Add($"Annual fee of ₹{card.AnnualFee:N0} is 100% waived based on your spending volume.");
                    }
                    else
                    {
                        limitations.Add($"Has an annual fee of ₹{card.AnnualFee:N0} with spend waiver threshold of ₹{card.FeeWaiverSpendThreshold:N0}.");
                    }
                }
                else if (model.FeePreference == "low_or_none")
                {
                    if (card.AnnualFee <= 1000 || calc.IsFeeWaived)
                    {
                        preferenceScore += 18;
                        matchReasons.Add(card.AnnualFee == 0 ? "Lifetime Free card." : $"Low annual fee of ₹{card.AnnualFee:N0} with fee waiver support.");
                    }
                    else
                    {
                        preferenceScore += 8;
                    }
                }
                else
                {
                    preferenceScore += 15; // User is open to annual fees
                }

                // Lounge alignment
                if (model.RequiresLoungeAccess)
                {
                    if (card.LoungeAccessDomestic > 0 || card.LoungeAccessInternational > 0)
                    {
                        preferenceScore += 15;
                        matchReasons.Add($"Includes {card.LoungeAccessDomestic} domestic and {card.LoungeAccessInternational} international lounge visits.");
                    }
                    else
                    {
                        limitations.Add("No complimentary airport lounge access included.");
                    }
                }
                else
                {
                    preferenceScore += 10;
                }

                // RuPay UPI alignment
                if (model.PrefersRuPayUPI)
                {
                    if (card.RuPaySupported)
                    {
                        preferenceScore += 12;
                        matchReasons.Add("RuPay card enabled: can be linked directly to UPI apps (GPay, PhonePe, Paytm).");
                    }
                    else
                    {
                        limitations.Add("Issued on Visa/Mastercard network; direct UPI QR linking is not supported.");
                    }
                }
                else
                {
                    preferenceScore += 10;
                }

                // International Travel
                if (model.InternationalTraveler)
                {
                    if (card.ForexMarkupPercent <= 2.0m)
                    {
                        preferenceScore += 8;
                        matchReasons.Add($"Low foreign exchange markup of {card.ForexMarkupPercent}%.");
                    }
                    else
                    {
                        limitations.Add($"Standard foreign currency markup fee of {card.ForexMarkupPercent}%.");
                    }
                }

                // Spending Category specialization
                if (model.Spending.OnlineShopping >= 10000 && card.Category == CardCategory.Cashback)
                {
                    preferenceScore += 5;
                    matchReasons.Add("High online cashback rate suits your digital shopping volume.");
                }
                if (model.Spending.Travel >= 8000 && card.Category == CardCategory.Travel)
                {
                    preferenceScore += 5;
                    matchReasons.Add("Accelerated travel rewards and airline partner redemption options.");
                }
                if (model.Spending.Fuel >= 4000 && card.Category == CardCategory.Fuel)
                {
                    preferenceScore += 5;
                    matchReasons.Add("Optimized fuel surcharge waivers and pump rewards.");
                }

                var totalScore = (int)Math.Clamp(Math.Round(financialScore + preferenceScore), 15, 99);

                // Add caps notices to limitations
                if (calc.AppliedCaps.Any())
                {
                    limitations.AddRange(calc.AppliedCaps);
                }

                string advantage = $"Yields an estimated net value of ₹{calc.NetRenewalYearBenefit:N0}/year on your spending profile.";

                results.Add(new CardRecommendationResult
                {
                    Card = card,
                    MatchScore = totalScore,
                    EligibilityStatus = eligibilityStatus,
                    Calculation = calc,
                    MatchReasons = matchReasons.Distinct().Take(4).ToList(),
                    Limitations = limitations.Distinct().Take(3).ToList(),
                    ComparisonAdvantage = advantage
                });
            }

            // Return top 5 ranked by NetRenewalYearBenefit descending and MatchScore descending
            return results
                .OrderByDescending(r => r.Calculation.NetRenewalYearBenefit)
                .ThenByDescending(r => r.MatchScore)
                .Take(5)
                .ToList();
        }
    }
}
