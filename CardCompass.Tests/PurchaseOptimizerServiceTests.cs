using CardCompass.Web.Data;
using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CardCompass.Tests
{
    public class PurchaseOptimizerServiceTests
    {
        [Fact]
        public async Task OptimizePurchaseAsync_SelectsHighestReturnCardForCategory()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PurchaseOptimizerTestDb1")
                .Options;

            using var context = new ApplicationDbContext(options);

            var cardA = new CreditCard
            {
                Id = 1,
                Name = "General Card",
                IsActive = true,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, EarningRatePercent = 1.0m }
                }
            };

            var cardB = new CreditCard
            {
                Id = 2,
                Name = "Online Shopping Pro",
                IsActive = true,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.OnlineShopping, EarningRatePercent = 5.0m }
                }
            };

            var mockCardService = new Mock<ICardService>();
            mockCardService.Setup(s => s.GetAllActiveCardsAsync())
                .ReturnsAsync(new List<CreditCard> { cardA, cardB });

            var service = new PurchaseOptimizerService(context, mockCardService.Object);

            var input = new PurchaseOptimizerViewModel
            {
                Amount = 10000,
                Category = SpendingCategory.OnlineShopping,
                SelectionMode = "all"
            };

            // Act
            var result = await service.OptimizePurchaseAsync(input);

            // Assert
            Assert.NotNull(result.BestCardOption);
            // Card B earns 5% on 10,000 = ₹500 vs Card A 1% = ₹100
            Assert.Equal(cardB.Id, result.BestCardOption.Card.Id);
            Assert.Equal(500m, result.BestCardOption.EstimatedReturnRupees);
            Assert.Equal(5.0m, result.BestCardOption.EffectiveRatePercent);
            Assert.Single(result.AlternativeOptions);
            Assert.Equal(cardA.Id, result.AlternativeOptions[0].Card.Id);
            Assert.Equal(100m, result.AlternativeOptions[0].EstimatedReturnRupees);
        }

        [Fact]
        public async Task OptimizePurchaseAsync_MinimumSpendShortfall_ReturnsZeroAndExplains()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PurchaseOptimizerTestDb2")
                .Options;

            using var context = new ApplicationDbContext(options);

            var card = new CreditCard
            {
                Id = 1,
                Name = "Min Spend Card",
                IsActive = true,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule
                    {
                        Category = SpendingCategory.Dining,
                        EarningRatePercent = 10.0m,
                        MinimumSpendAmount = 500m
                    }
                }
            };

            var mockCardService = new Mock<ICardService>();
            mockCardService.Setup(s => s.GetAllActiveCardsAsync())
                .ReturnsAsync(new List<CreditCard> { card });

            var service = new PurchaseOptimizerService(context, mockCardService.Object);

            var input = new PurchaseOptimizerViewModel
            {
                Amount = 250, // Below ₹500
                Category = SpendingCategory.Dining,
                SelectionMode = "all"
            };

            // Act
            var result = await service.OptimizePurchaseAsync(input);

            // Assert
            Assert.NotNull(result.BestCardOption);
            Assert.Equal(0m, result.BestCardOption.EstimatedReturnRupees);
            Assert.Contains("below the qualifying minimum", result.BestCardOption.Explanation);
        }
    }
}
