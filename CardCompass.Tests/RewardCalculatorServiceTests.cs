using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Xunit;

namespace CardCompass.Tests
{
    public class RewardCalculatorServiceTests
    {
        private readonly RewardCalculatorService _service;

        public RewardCalculatorServiceTests()
        {
            _service = new RewardCalculatorService();
        }

        [Fact]
        public void CalculateRewards_DirectCashbackRate_ComputesCorrectEarnings()
        {
            // Arrange: 5% cashback card with no cap
            var card = new CreditCard
            {
                Id = 1,
                Name = "Test Cashback Card",
                JoiningFee = 0,
                AnnualFee = 0,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule
                    {
                        Category = SpendingCategory.OnlineShopping,
                        RewardType = RewardType.Cashback,
                        EarningRatePercent = 5.0m,
                        MonthlyCapRupees = null
                    }
                }
            };

            var spend = new SpendingInputModel
            {
                OnlineShopping = 20000,
                Grocery = 0,
                Dining = 0,
                Travel = 0,
                Fuel = 0,
                Utilities = 0,
                GeneralSpends = 0
            };

            // Act
            var result = _service.CalculateRewards(card, spend);

            // Assert: 5% of 20,000 = 1,000 / month; 12,000 / year
            Assert.Equal(1000m, result.MonthlyRewardsRupees);
            Assert.Equal(12000m, result.AnnualRewardsRupees);
            Assert.Equal(12000m, result.NetRenewalYearBenefit);
        }

        [Fact]
        public void CalculateRewards_MonthlyCapEnforced_CapsEarnings()
        {
            // Arrange: 5% cashback capped at ₹1,000/month
            var card = new CreditCard
            {
                Id = 2,
                Name = "Test Capped Card",
                JoiningFee = 500,
                AnnualFee = 500,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule
                    {
                        Category = SpendingCategory.OnlineShopping,
                        RewardType = RewardType.Cashback,
                        EarningRatePercent = 5.0m,
                        MonthlyCapRupees = 1000m // Cap of ₹1,000/month
                    }
                }
            };

            // Spend ₹40,000 (5% would normally be ₹2,000)
            var spend = new SpendingInputModel
            {
                OnlineShopping = 40000,
                Grocery = 0,
                Dining = 0,
                Travel = 0,
                Fuel = 0,
                Utilities = 0,
                GeneralSpends = 0
            };

            // Act
            var result = _service.CalculateRewards(card, spend);

            // Assert: Should be capped at ₹1,000 monthly
            Assert.Equal(1000m, result.MonthlyRewardsRupees);
            Assert.Equal(12000m, result.AnnualRewardsRupees);
            Assert.True(result.CategoryDetails.First(c => c.Category == SpendingCategory.OnlineShopping).CapReached);
            Assert.Contains(result.AppliedCaps, c => c.Contains("capped"));
        }

        [Fact]
        public void CalculateRewards_PointsValuation_ComputesRupeeValueCorrectly()
        {
            // Arrange: 4 points per ₹100 where 1 point = ₹0.25 (i.e. 1% return)
            var card = new CreditCard
            {
                Id = 3,
                Name = "Test Points Card",
                JoiningFee = 0,
                AnnualFee = 0,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule
                    {
                        Category = SpendingCategory.GeneralSpends,
                        RewardType = RewardType.RewardPoints,
                        PointsPer100Rupees = 4.0m,
                        PointRupeeValue = 0.25m
                    }
                }
            };

            var spend = new SpendingInputModel
            {
                GeneralSpends = 10000,
                OnlineShopping = 0,
                Grocery = 0,
                Dining = 0,
                Travel = 0,
                Fuel = 0,
                Utilities = 0
            };

            // Act
            var result = _service.CalculateRewards(card, spend);

            // Assert: 10,000 / 100 = 100 * 4 = 400 points * 0.25 = ₹100 monthly; ₹1,200 annually
            Assert.Equal(100m, result.MonthlyRewardsRupees);
            Assert.Equal(1200m, result.AnnualRewardsRupees);
        }

        [Fact]
        public void CalculateRewards_FeeWaiverCondition_WaivesFeeWhenSpendThresholdMet()
        {
            // Arrange: Annual fee ₹1,000 waived on ₹1,00,000 annual spend
            var card = new CreditCard
            {
                Id = 4,
                Name = "Test Waiver Card",
                JoiningFee = 1000,
                AnnualFee = 1000,
                FeeWaiverSpendThreshold = 100000m,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule
                    {
                        Category = SpendingCategory.GeneralSpends,
                        RewardType = RewardType.Cashback,
                        EarningRatePercent = 1.0m
                    }
                }
            };

            // Spend ₹10,000/month = ₹1,20,000/year (exceeds ₹1,00,000)
            var spend = new SpendingInputModel
            {
                GeneralSpends = 10000,
                OnlineShopping = 0,
                Grocery = 0,
                Dining = 0,
                Travel = 0,
                Fuel = 0,
                Utilities = 0
            };

            // Act
            var result = _service.CalculateRewards(card, spend);

            // Assert
            Assert.True(result.IsFeeWaived);
            Assert.Equal(0m, result.EffectiveAnnualFee);
            // Net renewal = Annual Rewards (1,200) - 0 = 1,200
            Assert.Equal(1200m, result.NetRenewalYearBenefit);
        }

        [Fact]
        public void CalculateRewards_FeeWaiverCondition_AppliesFeeWhenSpendThresholdNotMet()
        {
            // Arrange: Annual fee ₹1,000 waived on ₹1,00,000 annual spend
            var card = new CreditCard
            {
                Id = 5,
                Name = "Test Unwaived Card",
                JoiningFee = 1000,
                AnnualFee = 1000,
                FeeWaiverSpendThreshold = 100000m,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule
                    {
                        Category = SpendingCategory.GeneralSpends,
                        RewardType = RewardType.Cashback,
                        EarningRatePercent = 1.0m
                    }
                }
            };

            // Spend ₹5,000/month = ₹60,000/year (below ₹1,00,000)
            var spend = new SpendingInputModel
            {
                GeneralSpends = 5000,
                OnlineShopping = 0,
                Grocery = 0,
                Dining = 0,
                Travel = 0,
                Fuel = 0,
                Utilities = 0
            };

            // Act
            var result = _service.CalculateRewards(card, spend);

            // Assert
            Assert.False(result.IsFeeWaived);
            Assert.Equal(1000m, result.EffectiveAnnualFee);
            // Net renewal = Annual Rewards (600) - 1,000 = -400
            Assert.Equal(-400m, result.NetRenewalYearBenefit);
        }

        [Fact]
        public void CalculateRewards_FirstYearVsRenewalYear_FactorsJoiningFeeAndWelcomeVoucher()
        {
            // Arrange: Joining Fee ₹2,500, Welcome Voucher ₹2,500, Annual Fee ₹2,500 (unwaived)
            var card = new CreditCard
            {
                Id = 6,
                Name = "Test Welcome Benefit Card",
                JoiningFee = 2500,
                AnnualFee = 2500,
                WelcomeBonusMonetaryValue = 2500,
                FeeWaiverSpendThreshold = 500000m,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule
                    {
                        Category = SpendingCategory.GeneralSpends,
                        RewardType = RewardType.Cashback,
                        EarningRatePercent = 2.0m
                    }
                }
            };

            // Spend ₹10,000/mo = ₹1,20,000/yr (Rewards = ₹2,400)
            var spend = new SpendingInputModel
            {
                GeneralSpends = 10000,
                OnlineShopping = 0,
                Grocery = 0,
                Dining = 0,
                Travel = 0,
                Fuel = 0,
                Utilities = 0
            };

            // Act
            var result = _service.CalculateRewards(card, spend);

            // Assert
            // First year = 2,400 (rewards) + 2,500 (welcome) - 2,500 (joining fee) = 2,400
            Assert.Equal(2400m, result.NetFirstYearBenefit);

            // Renewal year = 2,400 (rewards) - 2,500 (annual fee) = -100
            Assert.Equal(-100m, result.NetRenewalYearBenefit);
        }

        [Fact]
        public void CalculateRewards_MinimumSpendRequirement_EnforcesMinimumTransaction()
        {
            // Arrange: Minimum spend of ₹500 required to earn rewards
            var card = new CreditCard
            {
                Id = 7,
                Name = "Test Min Spend Card",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule
                    {
                        Category = SpendingCategory.Dining,
                        RewardType = RewardType.Cashback,
                        EarningRatePercent = 10.0m,
                        MinimumSpendAmount = 500m
                    }
                }
            };

            // Spend ₹300 (below ₹500 min spend)
            var spend = new SpendingInputModel
            {
                Dining = 300,
                OnlineShopping = 0,
                Grocery = 0,
                Travel = 0,
                Fuel = 0,
                Utilities = 0,
                GeneralSpends = 0
            };

            // Act
            var result = _service.CalculateRewards(card, spend);

            // Assert
            Assert.Equal(0m, result.MonthlyRewardsRupees);
        }
    }
}
