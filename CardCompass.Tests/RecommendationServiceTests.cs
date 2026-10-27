using CardCompass.Web.Models.Entities;
using CardCompass.Web.Models.ViewModels;
using CardCompass.Web.Services;
using Moq;
using Xunit;

namespace CardCompass.Tests
{
    public class RecommendationServiceTests
    {
        private readonly Mock<ICardService> _cardServiceMock;
        private readonly RewardCalculatorService _calculatorService;
        private readonly RecommendationService _service;

        public RecommendationServiceTests()
        {
            _cardServiceMock = new Mock<ICardService>();
            _calculatorService = new RewardCalculatorService();
            _service = new RecommendationService(_cardServiceMock.Object, _calculatorService);
        }

        [Fact]
        public async Task GetRecommendationsAsync_ExcludesExistingCards()
        {
            // Arrange
            var card1 = new CreditCard { Id = 1, Name = "Card One", AnnualFee = 0, IsActive = true };
            var card2 = new CreditCard { Id = 2, Name = "Card Two", AnnualFee = 0, IsActive = true };

            _cardServiceMock.Setup(s => s.GetAllActiveCardsAsync())
                .ReturnsAsync(new List<CreditCard> { card1, card2 });

            var model = new RecommendationWizardViewModel
            {
                MonthlyIncome = 60000,
                ExistingCardIds = new List<int> { 1 } // User already owns Card 1
            };

            // Act
            var results = await _service.GetRecommendationsAsync(model);

            // Assert: Card 1 must be excluded, only Card 2 returned
            Assert.DoesNotContain(results, r => r.Card.Id == 1);
            Assert.Contains(results, r => r.Card.Id == 2);
        }

        [Fact]
        public async Task GetRecommendationsAsync_IncomeThreshold_TagsEligibleCorrectly()
        {
            // Arrange: Card requiring ₹1,00,000 monthly income
            var highIncomeCard = new CreditCard
            {
                Id = 1,
                Name = "Super Premium Card",
                MinimumIncomeMonthly = 100000,
                IsActive = true
            };

            _cardServiceMock.Setup(s => s.GetAllActiveCardsAsync())
                .ReturnsAsync(new List<CreditCard> { highIncomeCard });

            var modelSufficientIncome = new RecommendationWizardViewModel
            {
                MonthlyIncome = 120000
            };

            var modelInsufficientIncome = new RecommendationWizardViewModel
            {
                MonthlyIncome = 30000
            };

            // Act
            var eligibleResult = (await _service.GetRecommendationsAsync(modelSufficientIncome)).First();
            var ineligibleResult = (await _service.GetRecommendationsAsync(modelInsufficientIncome)).First();

            // Assert
            Assert.Equal("Likely Eligible", eligibleResult.EligibilityStatus);
            Assert.Equal("Possible Income Mismatch", ineligibleResult.EligibilityStatus);
        }

        [Fact]
        public async Task GetRecommendationsAsync_LifetimeFreePreference_BoostsZeroFeeCards()
        {
            // Arrange: Free card vs Card with ₹1,000 fee
            var freeCard = new CreditCard
            {
                Id = 1,
                Name = "Free Card",
                AnnualFee = 0,
                IsActive = true,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, EarningRatePercent = 1.0m }
                }
            };

            var paidCard = new CreditCard
            {
                Id = 2,
                Name = "Paid Card",
                AnnualFee = 1000,
                FeeWaiverSpendThreshold = 500000m,
                IsActive = true,
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, EarningRatePercent = 1.0m }
                }
            };

            _cardServiceMock.Setup(s => s.GetAllActiveCardsAsync())
                .ReturnsAsync(new List<CreditCard> { freeCard, paidCard });

            var wizardModel = new RecommendationWizardViewModel
            {
                MonthlyIncome = 50000,
                FeePreference = "none", // Strictly wants lifetime free
                Spending = new SpendingInputModel { GeneralSpends = 5000 }
            };

            // Act
            var results = await _service.GetRecommendationsAsync(wizardModel);

            // Assert: Free card should be top ranked
            Assert.Equal(freeCard.Id, results.First().Card.Id);
            Assert.Contains(results.First().MatchReasons, r => r.Contains("Lifetime Free"));
        }
    }
}
