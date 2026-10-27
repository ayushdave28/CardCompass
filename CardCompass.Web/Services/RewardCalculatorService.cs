using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;

namespace CardCompass.Web.Services
{
    public class RewardCalculatorService : IRewardCalculatorService
    {
        public RewardCalculationResult CalculateRewards(CreditCard card, SpendingInputModel spend)
        {
            var result = new RewardCalculationResult
            {
                CreditCardId = card.Id,
                CardName = card.Name,
                JoiningFee = card.JoiningFee,
                WelcomeBonusValue = card.WelcomeBonusMonetaryValue
            };

            var spendCategories = new Dictionary<SpendingCategory, decimal>
            {
                { SpendingCategory.OnlineShopping, spend.OnlineShopping },
                { SpendingCategory.Grocery, spend.Grocery },
                { SpendingCategory.Dining, spend.Dining },
                { SpendingCategory.Travel, spend.Travel },
                { SpendingCategory.Fuel, spend.Fuel },
                { SpendingCategory.Utilities, spend.Utilities },
                { SpendingCategory.Entertainment, spend.Entertainment },
                { SpendingCategory.UPI, spend.UPI },
                { SpendingCategory.GeneralSpends, spend.GeneralSpends },
                { SpendingCategory.International, spend.International }
            };

            result.MonthlySpendTotal = spendCategories.Values.Sum();
            decimal totalMonthlyRewards = 0;

            // Find general fallback rule
            var generalRule = card.RewardRules.FirstOrDefault(r => r.Category == SpendingCategory.GeneralSpends);

            foreach (var kvp in spendCategories)
            {
                var category = kvp.Key;
                var monthlySpend = kvp.Value;

                if (monthlySpend <= 0) continue;

                // Find category-specific rule or fallback to general rule
                var rule = card.RewardRules.FirstOrDefault(r => r.Category == category) ?? generalRule;

                var detail = new CategoryRewardDetail
                {
                    Category = category,
                    MonthlySpend = monthlySpend
                };

                if (rule != null)
                {
                    // Check minimum spend requirement if applicable
                    if (rule.MinimumSpendAmount.HasValue && monthlySpend < rule.MinimumSpendAmount.Value)
                    {
                        detail.MonthlyEarnedRupees = 0;
                        detail.EffectiveRatePercent = 0;
                        detail.RuleApplied = $"Minimum spend of ₹{rule.MinimumSpendAmount:N0} not met";
                        result.AssumptionsAndDisclosures.Add($"{category}: Minimum qualifying transaction requirement was not met.");
                    }
                    else
                    {
                        decimal rawRupeeEarnings = 0;

                        if (rule.PointsPer100Rupees > 0)
                        {
                            var points = (monthlySpend / 100m) * rule.PointsPer100Rupees;
                            rawRupeeEarnings = points * rule.PointRupeeValue;
                            detail.EffectiveRatePercent = (rule.PointsPer100Rupees * rule.PointRupeeValue);
                        }
                        else
                        {
                            rawRupeeEarnings = monthlySpend * (rule.EarningRatePercent / 100m);
                            detail.EffectiveRatePercent = rule.EarningRatePercent;
                        }

                        // Apply Monthly Cap if specified
                        if (rule.MonthlyCapRupees.HasValue && rawRupeeEarnings > rule.MonthlyCapRupees.Value)
                        {
                            detail.MonthlyEarnedRupees = rule.MonthlyCapRupees.Value;
                            detail.CapReached = true;
                            detail.CapAmountRupees = rule.MonthlyCapRupees.Value;
                            detail.RuleApplied = $"{detail.EffectiveRatePercent:0.##}% capped at ₹{rule.MonthlyCapRupees:N0}/mo";
                            result.AppliedCaps.Add($"{category} cashback capped at ₹{rule.MonthlyCapRupees.Value:N0}/month.");
                        }
                        else
                        {
                            detail.MonthlyEarnedRupees = rawRupeeEarnings;
                            detail.CapReached = false;
                            detail.RuleApplied = $"{detail.EffectiveRatePercent:0.##}% return rate";
                        }

                        if (!string.IsNullOrEmpty(rule.ExcludedTransactions))
                        {
                            result.ExclusionsNotes.Add($"{category}: Excludes {rule.ExcludedTransactions}.");
                        }
                    }
                }
                else
                {
                    detail.MonthlyEarnedRupees = 0;
                    detail.EffectiveRatePercent = 0;
                    detail.RuleApplied = "No eligible reward rule found";
                }

                totalMonthlyRewards += detail.MonthlyEarnedRupees;
                result.CategoryDetails.Add(detail);
            }

            result.MonthlyRewardsRupees = totalMonthlyRewards;

            // Annual Fee Waiver check
            if (card.AnnualFee == 0)
            {
                result.EffectiveAnnualFee = 0;
                result.IsFeeWaived = true;
            }
            else if (card.FeeWaiverSpendThreshold.HasValue && result.AnnualSpendTotal >= card.FeeWaiverSpendThreshold.Value)
            {
                result.EffectiveAnnualFee = 0;
                result.IsFeeWaived = true;
                result.AssumptionsAndDisclosures.Add($"Annual renewal fee of ₹{card.AnnualFee:N0} is waived because annual spending (₹{result.AnnualSpendTotal:N0}) exceeds the ₹{card.FeeWaiverSpendThreshold:N0} waiver threshold.");
            }
            else
            {
                result.EffectiveAnnualFee = card.AnnualFee;
                result.IsFeeWaived = false;
                if (card.FeeWaiverSpendThreshold.HasValue)
                {
                    var shortfall = card.FeeWaiverSpendThreshold.Value - result.AnnualSpendTotal;
                    result.AssumptionsAndDisclosures.Add($"Annual fee of ₹{card.AnnualFee:N0} applies. Spend ₹{shortfall:N0} more annually to trigger fee waiver.");
                }
            }

            // Net First Year Benefit = Annual Rewards + Welcome Bonus Value - Joining Fee
            result.NetFirstYearBenefit = result.AnnualRewardsRupees + result.WelcomeBonusValue - card.JoiningFee;

            // Net Renewal Year Benefit = Annual Rewards - Effective Annual Fee
            result.NetRenewalYearBenefit = result.AnnualRewardsRupees - result.EffectiveAnnualFee;

            result.AssumptionsAndDisclosures.Add("Reward values assume points are redeemed at documented optimal rate for flights, statements, or vouchers. Excludes taxes, EMI interest, and late payment fees.");

            return result;
        }
    }
}
