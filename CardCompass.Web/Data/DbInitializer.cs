using CardCompass.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CardCompass.Web.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger logger)
        {
            try
            {
                // Ensure database created
                await context.Database.EnsureCreatedAsync();

                // Seed Roles
                string[] roleNames = { "Admin", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Seed Admin User
                var adminEmail = configuration["AdminSettings:Email"] ?? "admin@cardcompass.in";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = configuration["AdminSettings:FullName"] ?? "CardCompass Administrator",
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var adminPassword = configuration["AdminSettings:Password"] ?? "CardCompass@Admin2026!";
                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Admin account seeded: {Email}", adminEmail);
                    }
                    else
                    {
                        logger.LogError("Failed to seed admin account: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                // Check if cards already seeded
                if (await context.CreditCards.AnyAsync())
                {
                    return; // DB has already been seeded
                }

                // 1. Seed Banks
                var banks = GetBanks();
                await context.Banks.AddRangeAsync(banks);
                await context.SaveChangesAsync();

                var bankMap = await context.Banks.ToDictionaryAsync(b => b.Code, b => b.Id);

                // 2. Seed 30 Credit Cards with verified parameters
                var cards = GetCreditCards(bankMap);
                await context.CreditCards.AddRangeAsync(cards);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully seeded {Count} credit cards and banks into the database.", cards.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing and seeding the database.");
                throw;
            }
        }

        private static List<Bank> GetBanks()
        {
            return new List<Bank>
            {
                new Bank { Name = "HDFC Bank", Code = "HDFC", WebsiteUrl = "https://www.hdfcbank.com", CustomerCareNumber = "1800 202 6161", PrimaryColor = "#004c8f" },
                new Bank { Name = "SBI Card", Code = "SBI", WebsiteUrl = "https://www.sbicard.com", CustomerCareNumber = "1860 180 1290", PrimaryColor = "#2253a3" },
                new Bank { Name = "ICICI Bank", Code = "ICICI", WebsiteUrl = "https://www.icicibank.com", CustomerCareNumber = "1800 1080", PrimaryColor = "#b02a30" },
                new Bank { Name = "Axis Bank", Code = "AXIS", WebsiteUrl = "https://www.axisbank.com", CustomerCareNumber = "1860 419 5555", PrimaryColor = "#97144d" },
                new Bank { Name = "IDFC FIRST Bank", Code = "IDFC", WebsiteUrl = "https://www.idfcfirstbank.com", CustomerCareNumber = "1800 10 888", PrimaryColor = "#9b111e" },
                new Bank { Name = "American Express", Code = "AMEX", WebsiteUrl = "https://www.americanexpress.com/in", CustomerCareNumber = "1800 419 2122", PrimaryColor = "#006fcf" },
                new Bank { Name = "HSBC India", Code = "HSBC", WebsiteUrl = "https://www.hsbc.co.in", CustomerCareNumber = "1800 266 3456", PrimaryColor = "#db0011" },
                new Bank { Name = "AU Small Finance Bank", Code = "AUBANK", WebsiteUrl = "https://www.aubank.in", CustomerCareNumber = "1800 1200 1200", PrimaryColor = "#6b2c91" },
                new Bank { Name = "IndusInd Bank", Code = "INDUSIND", WebsiteUrl = "https://www.indusind.com", CustomerCareNumber = "1860 267 7777", PrimaryColor = "#800000" },
                new Bank { Name = "Standard Chartered", Code = "SC", WebsiteUrl = "https://www.sc.com/in", CustomerCareNumber = "1800 345 1000", PrimaryColor = "#008752" }
            };
        }

        private static List<CreditCard> GetCreditCards(Dictionary<string, int> bankMap)
        {
            var verifiedDate = new DateTime(2026, 10, 15);
            var list = new List<CreditCard>();

            // 1. HDFC Regalia Gold
            list.Add(new CreditCard
            {
                BankId = bankMap["HDFC"],
                Name = "HDFC Regalia Gold",
                Slug = "hdfc-regalia-gold",
                Network = CardNetwork.Visa,
                Category = CardCategory.Travel,
                JoiningFee = 2500,
                AnnualFee = 2500,
                FeeWaiverSpendThreshold = 400000,
                FeeWaiverDescription = "Waived on annual spends of ₹4,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 12,
                LoungeAccessInternational = 6,
                LoungeDetails = "12 complimentary domestic lounge visits/year (subject to ₹50,000 spend in preceding quarter) + 6 international visits via Priority Pass",
                ForexMarkupPercent = 2.0m,
                WelcomeBonus = "Gift voucher worth ₹2,500 from Marks & Spencer / Myntra / Reliance Digital on paying joining fee",
                WelcomeBonusMonetaryValue = 2500,
                MinimumIncomeMonthly = 100000,
                PublishedEligibility = "Salaried: ₹1,00,000 net monthly income. Self-employed: ITR > ₹12 Lakhs p.a.",
                Pros = "Low 2% forex markup|5X rewards on top fashion & retail brands|Complimentary Priority Pass membership",
                Cons = "Domestic lounge access requires ₹50k spend in previous quarter|Points devaluation on direct cash conversion (₹0.20/pt)",
                OfficialProductUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/regalia-gold-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #b8860b 0%, #1e293b 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.33m, PointsPer100Rupees = 2.67m, PointRupeeValue = 0.50m, ExcludedTransactions = "Fuel, Rent, Govt, Wallet", Notes = "4 reward points per ₹150 spend" },
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.RewardPoints, EarningRatePercent = 6.67m, PointsPer100Rupees = 13.33m, PointRupeeValue = 0.50m, ExcludedTransactions = "Wallet load", Notes = "5X points (20 pts per ₹150) on M&S, Myntra, Nykaa, Reliance Digital" },
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.RewardPoints, EarningRatePercent = 5.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Railway counters", Notes = "Up to 5X on SmartBuy travel bookings" },
                    new RewardRule { Category = SpendingCategory.Fuel, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.0m, PointsPer100Rupees = 0m, PointRupeeValue = 1.0m, ExcludedTransactions = "Fuel purchases under ₹400", Notes = "1% fuel surcharge waiver up to ₹400/month" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Airport Lounge Access", Description = "12 domestic and 6 international lounge visits per year", EstimatedAnnualValue = 18000 },
                    new CardBenefit { BenefitType = BenefitType.MilestoneBonus, Title = "Quarterly & Annual Milestones", Description = "Flight vouchers worth ₹1,500 on ₹1.5L quarterly spend; ₹5,000 on ₹5L annual spend", EstimatedAnnualValue = 11000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 1200000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 750, Notes = "Subject to bank credit policy" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/regalia-gold-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 2. HDFC Infinia Metal Edition
            list.Add(new CreditCard
            {
                BankId = bankMap["HDFC"],
                Name = "HDFC Infinia Metal Edition",
                Slug = "hdfc-infinia-metal",
                Network = CardNetwork.Visa,
                Category = CardCategory.Lifestyle,
                JoiningFee = 12500,
                AnnualFee = 12500,
                FeeWaiverSpendThreshold = 1000000,
                FeeWaiverDescription = "Waived on spends of ₹10,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 999,
                LoungeAccessInternational = 999,
                LoungeDetails = "Unlimited complimentary domestic & international lounge access for primary and add-on cardholders with Priority Pass",
                ForexMarkupPercent = 2.0m,
                WelcomeBonus = "12,500 Reward Points on payment of joining fee",
                WelcomeBonusMonetaryValue = 12500,
                MinimumIncomeMonthly = 275000,
                PublishedEligibility = "By invitation only; typical requirement is net monthly salary > ₹2.75L or ITR > ₹36L",
                Pros = "Industry-best 3.33% base reward rate (1 pt = ₹1 on flights/hotels)|Unlimited domestic and international lounge visits|Up to 10X points (33% return) on SmartBuy portal",
                Cons = "Strictly invite-only with steep income threshold|High annual fee of ₹12,500|Daily reward cap of 10,000 points on SmartBuy",
                OfficialProductUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/infinia-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #0f172a 0%, #334155 50%, #020617 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 3.33m, PointsPer100Rupees = 3.33m, PointRupeeValue = 1.0m, ExcludedTransactions = "Fuel, Rent, Wallet, Govt", Notes = "5 reward points per ₹150 spend" },
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.RewardPoints, EarningRatePercent = 16.65m, PointsPer100Rupees = 16.65m, PointRupeeValue = 1.0m, ExcludedTransactions = "Offline agent bookings", Notes = "5X on flights, 10X on hotels via SmartBuy" },
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.RewardPoints, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, ExcludedTransactions = "Direct merchant checkout", Notes = "3X-5X on SmartBuy partner vouchers" },
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.RewardPoints, EarningRatePercent = 6.67m, PointsPer100Rupees = 6.67m, PointRupeeValue = 1.0m, ExcludedTransactions = "Fuel station food", Notes = "2X on Swiggy Dineout" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Unlimited Lounge Access", Description = "Worldwide unlimited domestic and international lounge access with complimentary guest visits", EstimatedAnnualValue = 40000 },
                    new CardBenefit { BenefitType = BenefitType.GolfAccess, Title = "Unlimited Complimentary Golf", Description = "Unlimited golf games and coaching across leading domestic and international golf courses", EstimatedAnnualValue = 35000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 70, MinAnnualIncome = 3600000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 780, Notes = "Invite-only or existing HDFC Imperia relationship" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/infinia-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 3. HDFC Millennia
            list.Add(new CreditCard
            {
                BankId = bankMap["HDFC"],
                Name = "HDFC Millennia",
                Slug = "hdfc-millennia",
                Network = CardNetwork.Visa,
                Category = CardCategory.Cashback,
                JoiningFee = 1000,
                AnnualFee = 1000,
                FeeWaiverSpendThreshold = 100000,
                FeeWaiverDescription = "Waived on annual spends of ₹1,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 4,
                LoungeAccessInternational = 0,
                LoungeDetails = "4 complimentary domestic lounge visits per calendar year (1/quarter, requires ₹50k spend in previous quarter)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "1,000 CashPoints on payment of joining fee",
                WelcomeBonusMonetaryValue = 1000,
                MinimumIncomeMonthly = 35000,
                PublishedEligibility = "Salaried: Gross monthly income > ₹35,000. Self-employed: ITR > ₹6 Lakhs p.a.",
                Pros = "5% cashback on 10 popular partner merchants (Amazon, Flipkart, Swiggy, Zomato, Myntra, etc.)|Cashback directly credited at 1:1 against credit card bill|Low spend waiver threshold of ₹1L",
                Cons = "Partner 5% cashback capped at ₹1,000 per month|Domestic lounge access requires ₹50k spend in preceding quarter|High 3.5% foreign currency markup",
                OfficialProductUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/millennia-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #0284c7 0%, #0369a1 50%, #0f172a 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 1000, ExcludedTransactions = "Fuel, Wallet load, Rent, Govt", Notes = "5% cashback on Amazon, Flipkart, Myntra, Tata CLiQ, Cult.fit, Sony LIV, BookMyShow, Uber" },
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 1000, ExcludedTransactions = "Alcohol shops", Notes = "5% cashback on Swiggy and Zomato (shared ₹1,000 monthly cap)" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.Cashback, EarningRatePercent = 1.0m, PointsPer100Rupees = 1.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 1000, ExcludedTransactions = "Fuel, Wallet, Rent, EMI", Notes = "1% cashback on other retail spends (cap ₹1,000/month)" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.DiningDiscount, Title = "Good Food Trail Dining", Description = "Up to 20% savings on restaurant bill payments via Swiggy Dineout", EstimatedAnnualValue = 3000 },
                    new CardBenefit { BenefitType = BenefitType.MilestoneBonus, Title = "Quarterly Milestone Voucher", Description = "₹1,000 gift voucher on spends of ₹1,00,000 and above in each calendar quarter", EstimatedAnnualValue = 4000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 40, MinAnnualIncome = 420000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 740, Notes = "Entry-level millennial demographic" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/millennia-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 4. HDFC Tata Neu Infinity
            list.Add(new CreditCard
            {
                BankId = bankMap["HDFC"],
                Name = "HDFC Tata Neu Infinity",
                Slug = "hdfc-tata-neu-infinity",
                Network = CardNetwork.RuPay,
                Category = CardCategory.UPI,
                JoiningFee = 1499,
                AnnualFee = 1499,
                FeeWaiverSpendThreshold = 300000,
                FeeWaiverDescription = "Waived on spends of ₹3,00,000 or more in the preceding year",
                RuPaySupported = true,
                LoungeAccessDomestic = 8,
                LoungeAccessInternational = 4,
                LoungeDetails = "8 complimentary domestic lounge visits per year (2/quarter) + 4 international lounge visits via Priority Pass",
                ForexMarkupPercent = 2.0m,
                WelcomeBonus = "1,499 NeuCoins on the Tata Neu App upon 1st transaction within 30 days",
                WelcomeBonusMonetaryValue = 1499,
                MinimumIncomeMonthly = 100000,
                PublishedEligibility = "Salaried: Monthly income > ₹1,00,000. Self-employed: ITR > ₹12 Lakhs p.a.",
                Pros = "RuPay variant links to UPI apps (Google Pay, PhonePe, Paytm)|Up to 10% NeuCoins on Tata Neu brands (BigBasket, 1mg, Croma, Air India Express)|1.5% NeuCoins on UPI spends",
                Cons = "NeuCoins can only be redeemed within Tata ecosystem brands|UPI rewards capped at 500 NeuCoins per calendar month",
                OfficialProductUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/tata-neu-infinity-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #4c1d95 0%, #1e1b4b 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.NeuCoins, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, ExcludedTransactions = "Fuel, Rent, Wallet", Notes = "5% NeuCoins on Tata Neu app + 5% additional with NeuPass (BigBasket, Croma, Tata CLiQ, 1mg, IHCL)" },
                    new RewardRule { Category = SpendingCategory.UPI, RewardType = RewardType.NeuCoins, EarningRatePercent = 1.5m, PointsPer100Rupees = 1.5m, PointRupeeValue = 1.0m, MonthlyCapRupees = 500, ExcludedTransactions = "P2P transfers, wallet top-up", Notes = "1.5% NeuCoins on UPI merchant transactions (capped at 500 NeuCoins/month)" },
                    new RewardRule { Category = SpendingCategory.Grocery, RewardType = RewardType.NeuCoins, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, ExcludedTransactions = "Local mandis", Notes = "10% NeuCoins on BigBasket grocery orders" },
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.NeuCoins, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, ExcludedTransactions = "Unregistered travel agents", Notes = "10% NeuCoins on Air India Express & IHCL bookings via Tata Neu" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Airport Lounge Access", Description = "8 domestic and 4 international lounge visits per calendar year", EstimatedAnnualValue = 12000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 1200000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 750, Notes = "Available on RuPay network for UPI payments" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/tata-neu-infinity-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 5. HDFC Swiggy Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["HDFC"],
                Name = "HDFC Swiggy Credit Card",
                Slug = "hdfc-swiggy",
                Network = CardNetwork.Mastercard,
                Category = CardCategory.Dining,
                JoiningFee = 500,
                AnnualFee = 500,
                FeeWaiverSpendThreshold = 200000,
                FeeWaiverDescription = "Waived on spends of ₹2,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 0,
                LoungeAccessInternational = 0,
                LoungeDetails = "No airport lounge access",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Complimentary 3-month Swiggy One membership on card activation",
                WelcomeBonusMonetaryValue = 899,
                MinimumIncomeMonthly = 25000,
                PublishedEligibility = "Salaried: Monthly income > ₹25,000. Self-employed: ITR > ₹6 Lakhs p.a.",
                Pros = "10% cashback on Swiggy (Food, Instamart, Dineout, Genie)|5% cashback on leading online platforms (Amazon, Flipkart, Myntra, Uber, Nykaa)|Cashback credited directly as Swiggy Money / statement credit",
                Cons = "Swiggy 10% cashback capped at ₹1,500 per month|No airport lounge access|Excludes rent, fuel, and jewelry spends",
                OfficialProductUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/swiggy-hdfc-bank-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #fc8019 0%, #d9531e 50%, #1e293b 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.Cashback, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 1500, ExcludedTransactions = "Fuel, wallet load", Notes = "10% cashback on Swiggy food delivery, Instamart, Dineout & Genie (cap ₹1,500/month)" },
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 1500, ExcludedTransactions = "Gift cards", Notes = "5% cashback on Amazon, Flipkart, Myntra, Nykaa, Uber, Ola, Urban Company (cap ₹1,500/mo)" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.Cashback, EarningRatePercent = 1.0m, PointsPer100Rupees = 1.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 500, ExcludedTransactions = "Fuel, rent, wallet, EMI", Notes = "1% on all other spends (cap ₹500/month)" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.WelcomeVoucher, Title = "Swiggy One Membership", Description = "3 months of free food delivery, Instamart zero delivery fee, and exclusive Dineout discounts", EstimatedAnnualValue = 1200 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 60, MinAnnualIncome = 300000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 730, Notes = "Application also available via Swiggy app" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.hdfcbank.com/personal/pay/cards/credit-cards/swiggy-hdfc-bank-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 6. SBI Cashback Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["SBI"],
                Name = "SBI Cashback Credit Card",
                Slug = "sbi-cashback",
                Network = CardNetwork.Visa,
                Category = CardCategory.Cashback,
                JoiningFee = 999,
                AnnualFee = 999,
                FeeWaiverSpendThreshold = 200000,
                FeeWaiverDescription = "Waived on annual spends of ₹2,00,000 or more in the preceding year",
                RuPaySupported = false,
                LoungeAccessDomestic = 0,
                LoungeAccessInternational = 0,
                LoungeDetails = "No airport lounge access (discontinued in revision)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Nil (no formal welcome bonus)",
                WelcomeBonusMonetaryValue = 0,
                MinimumIncomeMonthly = 30000,
                PublishedEligibility = "Salaried / Self-Employed with minimum stable income",
                Pros = "Flat 5% cashback on almost all online spends across all merchants|Generous ₹5,000 monthly cashback cap (spends up to ₹1,00,000)|Cashback auto-credited to statement balance within 2 days of bill generation",
                Cons = "Offline spends earn only 1% cashback|No airport lounge access or movie perks|Excluded categories: rent, fuel, utilities, insurance, educational fees, jewelry",
                OfficialProductUrl = "https://www.sbicard.com/en/personal/credit-cards/rewards/cashback-sbi-card.page",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #0284c7 0%, #1e3a8a 50%, #091024 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 5000, ExcludedTransactions = "Fuel, rent, utility, insurance, jewelry, school fee, wallet load, railway", Notes = "5% cashback on all online shopping transactions without merchant lock-in (cap ₹5,000/billing cycle)" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.Cashback, EarningRatePercent = 1.0m, PointsPer100Rupees = 1.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Fuel, rent, wallet load", Notes = "1% uncapped cashback on eligible offline retail spends" },
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 5000, ExcludedTransactions = "Offline restaurant POS", Notes = "5% cashback on online food delivery (Swiggy, Zomato, etc.)" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.FuelSurchargeWaiver, Title = "1% Fuel Surcharge Waiver", Description = "1% waiver on fuel transactions between ₹500 and ₹3,000 across all petrol pumps, max ₹100/month", EstimatedAnnualValue = 1200 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 360000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 750, Notes = "Strict underwriting on online shopping card category" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.sbicard.com/en/personal/credit-cards/rewards/cashback-sbi-card.page", VerifiedDate = verifiedDate }
                }
            });

            // 7. SimplyCLICK SBI Card
            list.Add(new CreditCard
            {
                BankId = bankMap["SBI"],
                Name = "SimplyCLICK SBI Card",
                Slug = "simplyclick-sbi",
                Network = CardNetwork.Visa,
                Category = CardCategory.Shopping,
                JoiningFee = 499,
                AnnualFee = 499,
                FeeWaiverSpendThreshold = 100000,
                FeeWaiverDescription = "Waived on spends of ₹1,00,000 or more in the preceding year",
                RuPaySupported = false,
                LoungeAccessDomestic = 0,
                LoungeAccessInternational = 0,
                LoungeDetails = "No airport lounge access",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Amazon.in gift card worth ₹500 on payment of joining fee",
                WelcomeBonusMonetaryValue = 500,
                MinimumIncomeMonthly = 20000,
                PublishedEligibility = "Salaried: Monthly income > ₹20,000. Self-employed: ITR > ₹3.6 Lakhs p.a.",
                Pros = "10X reward points on partner merchants (Apollo 24/7, BookMyShow, Cleartrip, Dominos, Myntra, Netmeds, Yatra)|5X reward points on all other online spends|Low annual fee with easy ₹1L waiver",
                Cons = "Low point value (4 points = ₹1, i.e., ₹0.25 per point)|Domestic lounge access not available|Fuel and utilities do not earn accelerated reward points",
                OfficialProductUrl = "https://www.sbicard.com/en/personal/credit-cards/shopping/simplyclick-sbi-card.page",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #0284c7 0%, #1d4ed8 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent", Notes = "10X points (2.5% return) on partner merchants: Apollo 24/7, BookMyShow, Cleartrip, Dominos, Myntra, Netmeds, Yatra" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.25m, PointsPer100Rupees = 5.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "5X points (1.25% return) on all other online retail transactions" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MilestoneBonus, Title = "Cleartrip Milestone Vouchers", Description = "₹2,000 Cleartrip e-voucher each on achieving annual online spends of ₹1 Lakh and ₹2 Lakhs", EstimatedAnnualValue = 4000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 240000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 720, Notes = "Popular entry-level card in India" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.sbicard.com/en/personal/credit-cards/shopping/simplyclick-sbi-card.page", VerifiedDate = verifiedDate }
                }
            });

            // 8. SBI Card ELITE
            list.Add(new CreditCard
            {
                BankId = bankMap["SBI"],
                Name = "SBI Card ELITE",
                Slug = "sbi-card-elite",
                Network = CardNetwork.Visa,
                Category = CardCategory.Lifestyle,
                JoiningFee = 4999,
                AnnualFee = 4999,
                FeeWaiverSpendThreshold = 1000000,
                FeeWaiverDescription = "Waived on spends of ₹10,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 8,
                LoungeAccessInternational = 6,
                LoungeDetails = "8 complimentary domestic airport lounge visits per year (max 2/quarter) + 6 international visits via Priority Pass",
                ForexMarkupPercent = 1.99m,
                WelcomeBonus = "Welcome e-gift voucher worth ₹5,000 from brands like Yatra, Hush Puppies, Pantaloons, Shoppers Stop",
                WelcomeBonusMonetaryValue = 5000,
                MinimumIncomeMonthly = 60000,
                PublishedEligibility = "Salaried / Self-employed with monthly income > ₹60,000",
                Pros = "Free movie tickets worth ₹6,000/year on BookMyShow (up to ₹250/ticket, 2 tickets/month)|Low forex markup of 1.99%|5X rewards on Dining, Departmental stores, and Grocery",
                Cons = "High annual fee of ₹4,999|Base reward rate is standard 0.5% (2 pts/₹100, 4 pts = ₹1)|High ₹10L spend threshold for annual fee waiver",
                OfficialProductUrl = "https://www.sbicard.com/en/personal/credit-cards/lifestyle/sbi-card-elite.page",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #1e1b4b 0%, #312e81 50%, #0f172a 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel station dining", Notes = "5X reward points (10 pts per ₹100) on Dining, Departmental stores, and Grocery" },
                    new RewardRule { Category = SpendingCategory.Grocery, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Wholesale cash & carry", Notes = "5X reward points on grocery purchases" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.5m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "2 reward points per ₹100 spend" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MovieTicket, Title = "Free Movie Tickets", Description = "Free movie tickets worth ₹6,000/year (up to ₹250 per ticket, 2 tickets per month on BookMyShow)", EstimatedAnnualValue = 6000 },
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Domestic & International Lounges", Description = "8 domestic and 6 international lounge visits per year", EstimatedAnnualValue = 18000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 720000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 750, Notes = "Premium tier cardholder profile" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.sbicard.com/en/personal/credit-cards/lifestyle/sbi-card-elite.page", VerifiedDate = verifiedDate }
                }
            });

            // 9. BPCL SBI Card Octane
            list.Add(new CreditCard
            {
                BankId = bankMap["SBI"],
                Name = "BPCL SBI Card Octane",
                Slug = "bpcl-sbi-card-octane",
                Network = CardNetwork.Visa,
                Category = CardCategory.Fuel,
                JoiningFee = 1499,
                AnnualFee = 1499,
                FeeWaiverSpendThreshold = 200000,
                FeeWaiverDescription = "Waived on annual spends of ₹2,00,000 or more in the preceding year",
                RuPaySupported = false,
                LoungeAccessDomestic = 4,
                LoungeAccessInternational = 0,
                LoungeDetails = "4 complimentary domestic airport lounge visits per calendar year (1 per quarter)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "6,000 Reward Points (worth ₹1,500) upon payment of joining fee",
                WelcomeBonusMonetaryValue = 1500,
                MinimumIncomeMonthly = 30000,
                PublishedEligibility = "Salaried / Self-employed with monthly income > ₹30,000",
                Pros = "7.25% value back on BPCL fuel & lubricants (25X points = 6.25% + 1% surcharge waiver)|Includes 4 domestic airport lounge visits per year|10X reward points on Dining, Grocery, Departmental stores, and Movies",
                Cons = "Fuel benefit strictly restricted to Bharat Petroleum (BPCL) fuel outlets|Maximum fuel reward points capped at 2,500 points per billing cycle (~₹10,000 monthly fuel spend)|3.5% foreign currency markup",
                OfficialProductUrl = "https://www.sbicard.com/en/personal/credit-cards/fuel/bpcl-sbi-card-octane.page",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #15803d 0%, #047857 50%, #0f172a 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.Fuel, RewardType = RewardType.RewardPoints, EarningRatePercent = 7.25m, PointsPer100Rupees = 25.0m, PointRupeeValue = 0.25m, MonthlyCapRupees = 625, ExcludedTransactions = "Non-BPCL petrol pumps", Notes = "25X points (6.25% value back) + 1% fuel surcharge waiver on BPCL fuel (cap 2,500 pts/mo)" },
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel station dining", Notes = "10X points (2.5% return) on Dining, Movies & Groceries" },
                    new RewardRule { Category = SpendingCategory.Grocery, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Wholesale cash & carry", Notes = "10X points on grocery spends" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.25m, PointsPer100Rupees = 1.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "1 reward point per ₹100 on other retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Domestic Airport Lounge Access", Description = "4 complimentary visits per year (1 per quarter)", EstimatedAnnualValue = 4000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 360000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 730, Notes = "High value for commuters using BPCL" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.sbicard.com/en/personal/credit-cards/fuel/bpcl-sbi-card-octane.page", VerifiedDate = verifiedDate }
                }
            });

            // 10. Air India SBI Platinum
            list.Add(new CreditCard
            {
                BankId = bankMap["SBI"],
                Name = "Air India SBI Platinum",
                Slug = "air-india-sbi-platinum",
                Network = CardNetwork.Visa,
                Category = CardCategory.Travel,
                JoiningFee = 1499,
                AnnualFee = 1499,
                FeeWaiverSpendThreshold = null,
                FeeWaiverDescription = "No annual fee waiver; compensated with 2,000 anniversary reward points",
                RuPaySupported = false,
                LoungeAccessDomestic = 8,
                LoungeAccessInternational = 0,
                LoungeDetails = "8 complimentary domestic airport lounge visits per year (2 per quarter)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "5,000 Air India reward points on payment of joining fee",
                WelcomeBonusMonetaryValue = 2500,
                MinimumIncomeMonthly = 30000,
                PublishedEligibility = "Salaried / Self-employed with monthly income > ₹30,000",
                Pros = "Up to 15 reward points per ₹100 spent on Air India tickets booked on website/app|1 Air India reward point = 1 Air India Flying Returns Maharaja Club point|8 domestic airport lounge visits per year",
                Cons = "Points redemption value tied directly to Air India award flights|Low base reward rate of 2 points per ₹100 on general spends|No renewal fee waiver threshold",
                OfficialProductUrl = "https://www.sbicard.com/en/personal/credit-cards/travel/air-india-sbi-platinum-card.page",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #b91c1c 0%, #7f1d1d 50%, #0f172a 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.AirMiles, EarningRatePercent = 7.5m, PointsPer100Rupees = 15.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Other airlines", Notes = "15 Air India reward points per ₹100 on Air India bookings via website or mobile app" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.AirMiles, EarningRatePercent = 1.0m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "2 reward points per ₹100 on other retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Domestic Lounge Access", Description = "8 domestic lounge visits per year (max 2/quarter)", EstimatedAnnualValue = 8000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 360000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 730, Notes = "Frequent flyer profile" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.sbicard.com/en/personal/credit-cards/travel/air-india-sbi-platinum-card.page", VerifiedDate = verifiedDate }
                }
            });

            // 11. Amazon Pay ICICI Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["ICICI"],
                Name = "Amazon Pay ICICI Credit Card",
                Slug = "amazon-pay-icici",
                Network = CardNetwork.Visa,
                Category = CardCategory.Cashback,
                JoiningFee = 0,
                AnnualFee = 0,
                FeeWaiverSpendThreshold = null,
                FeeWaiverDescription = "Lifetime Free — No joining fee, no annual fee ever",
                RuPaySupported = false,
                LoungeAccessDomestic = 0,
                LoungeAccessInternational = 0,
                LoungeDetails = "No airport lounge access",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Up to ₹2,500 in Amazon coupons and cashback upon activation",
                WelcomeBonusMonetaryValue = 1000,
                MinimumIncomeMonthly = 25000,
                PublishedEligibility = "Existing ICICI relationship or pre-approved on Amazon India app",
                Pros = "Lifetime Free card with zero joining and zero renewal fees|5% unlimited cashback on Amazon for Prime members (3% for non-Prime)|2% unlimited cashback on 100+ partner merchants & flight/utility bills on Amazon|Cashback automatically credited to Amazon Pay wallet with zero friction",
                Cons = "Cashback earned strictly as Amazon Pay balance, cannot be withdrawn as cash to bank|No airport lounge access|Excludes rent and gold purchases",
                OfficialProductUrl = "https://www.icicibank.com/personal-banking/cards/credit-cards/amazon-pay-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #131921 0%, #232f3e 50%, #ff9900 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Gold/Silver purchases, rent", Notes = "5% unlimited cashback on Amazon for Prime members (3% for non-Prime)" },
                    new RewardRule { Category = SpendingCategory.Utilities, RewardType = RewardType.Cashback, EarningRatePercent = 2.0m, PointsPer100Rupees = 2.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Non-Amazon recharge portals", Notes = "2% unlimited cashback on utility bill payments and recharges via Amazon Pay" },
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.Cashback, EarningRatePercent = 2.0m, PointsPer100Rupees = 2.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Offline swipe", Notes = "2% cashback on Swiggy, Zomato, BookMyShow when paid using card" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.Cashback, EarningRatePercent = 1.0m, PointsPer100Rupees = 1.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Fuel, rent, wallet reload", Notes = "1% unlimited cashback on all other retail transactions" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.FuelSurchargeWaiver, Title = "1% Fuel Surcharge Waiver", Description = "1% fuel surcharge waiver across all petrol pumps in India", EstimatedAnnualValue = 1200 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 60, MinAnnualIncome = 300000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 730, Notes = "Must have an Amazon.in account" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.icicibank.com/personal-banking/cards/credit-cards/amazon-pay-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 12. ICICI Sapphiro Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["ICICI"],
                Name = "ICICI Sapphiro Credit Card",
                Slug = "icici-sapphiro",
                Network = CardNetwork.Visa,
                Category = CardCategory.Lifestyle,
                JoiningFee = 6500,
                AnnualFee = 3500,
                FeeWaiverSpendThreshold = 600000,
                FeeWaiverDescription = "Waived on spends of ₹6,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 16,
                LoungeAccessInternational = 2,
                LoungeDetails = "4 complimentary domestic airport lounge visits per quarter (subject to ₹50,000 spend in preceding calendar quarter) + 2 international visits via Dreamfolks",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Welcome vouchers worth ₹9,000 from Tata CLiQ, EaseMyTrip, and Croma",
                WelcomeBonusMonetaryValue = 9000,
                MinimumIncomeMonthly = 150000,
                PublishedEligibility = "Salaried: Monthly income > ₹1,50,000. Often offered LTF to top university alumni.",
                Pros = "Buy 1 Get 1 free movie ticket on BookMyShow up to ₹500 twice a month (₹12,000 annual value)|Complimentary golf games/lessons per quarter|4 domestic airport lounges/quarter + 2 international visits",
                Cons = "Domestic lounge access requires ₹50,000 spend in preceding calendar quarter|Base reward rate is modest (2 points per ₹100, 1 pt = ₹0.25)|High joining fee unless approved under Alumni program",
                OfficialProductUrl = "https://www.icicibank.com/personal-banking/cards/credit-cards/sapphiro-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #1e3a8a 0%, #0369a1 50%, #0c4a6e 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.International, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.0m, PointsPer100Rupees = 4.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "ATM withdrawals", Notes = "4 ICICI reward points per ₹100 on international spends" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.5m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "2 reward points per ₹100 on domestic retail spends" },
                    new RewardRule { Category = SpendingCategory.Utilities, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.25m, PointsPer100Rupees = 1.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Government payments", Notes = "1 reward point per ₹100 on utilities and insurance" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MovieTicket, Title = "BOGO Movie Tickets", Description = "Buy 1 Get 1 free movie ticket up to ₹500 twice every month on BookMyShow", EstimatedAnnualValue = 12000 },
                    new CardBenefit { BenefitType = BenefitType.GolfAccess, Title = "Complimentary Golf Rounds", Description = "4 complimentary golf games/lessons per year on leading golf courses", EstimatedAnnualValue = 16000 }
                },
                Eligibility = new EligibilityRule { MinAge = 23, MaxAge = 65, MinAnnualIncome = 1800000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 760, Notes = "Premier banking customers" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.icicibank.com/personal-banking/cards/credit-cards/sapphiro-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 13. ICICI Coral RuPay Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["ICICI"],
                Name = "ICICI Coral RuPay Credit Card",
                Slug = "icici-coral-rupay",
                Network = CardNetwork.RuPay,
                Category = CardCategory.UPI,
                JoiningFee = 500,
                AnnualFee = 500,
                FeeWaiverSpendThreshold = 150000,
                FeeWaiverDescription = "Waived on annual spends of ₹1,50,000 or more in the preceding anniversary year",
                RuPaySupported = true,
                LoungeAccessDomestic = 4,
                LoungeAccessInternational = 0,
                LoungeDetails = "1 complimentary domestic airport lounge visit per quarter (subject to ₹35,000 spend in preceding quarter) + 1 railway lounge visit/quarter",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Nil",
                WelcomeBonusMonetaryValue = 0,
                MinimumIncomeMonthly = 25000,
                PublishedEligibility = "Salaried / Self-employed with monthly income > ₹25,000",
                Pros = "RuPay credit card enabled for instant UPI QR code payments|25% discount up to ₹100 on BookMyShow and Inox twice a month|1 railway lounge visit per quarter with no spend criteria",
                Cons = "Airport lounge requires ₹35,000 spend in preceding quarter|Low base reward rate of 2 points per ₹100 (₹0.50 value per ₹100)|Excludes rent, fuel surcharge, and wallet load",
                OfficialProductUrl = "https://www.icicibank.com/personal-banking/cards/credit-cards/coral-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #b91c1c 0%, #c2410c 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.UPI, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.50m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "P2P transfers", Notes = "2 reward points per ₹100 spent on UPI merchant transactions" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.50m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "2 reward points per ₹100 on other retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MovieTicket, Title = "Movie Discounts", Description = "25% discount up to ₹100 on purchase of minimum 2 tickets on BookMyShow & Inox twice a month", EstimatedAnnualValue = 2400 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 60, MinAnnualIncome = 300000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 720, Notes = "Easy approval for existing ICICI bank savings account holders" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.icicibank.com/personal-banking/cards/credit-cards/coral-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 14. ICICI Emeralde Private Metal
            list.Add(new CreditCard
            {
                BankId = bankMap["ICICI"],
                Name = "ICICI Emeralde Private Metal",
                Slug = "icici-emeralde-private-metal",
                Network = CardNetwork.Visa,
                Category = CardCategory.Lifestyle,
                JoiningFee = 12500,
                AnnualFee = 12500,
                FeeWaiverSpendThreshold = 1000000,
                FeeWaiverDescription = "Waived on spends of ₹10,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 999,
                LoungeAccessInternational = 999,
                LoungeDetails = "Unlimited complimentary domestic & international airport lounge visits for primary and add-on cardholders",
                ForexMarkupPercent = 1.5m,
                WelcomeBonus = "Taj Epicure membership + 12,500 bonus reward points",
                WelcomeBonusMonetaryValue = 12500,
                MinimumIncomeMonthly = 300000,
                PublishedEligibility = "Ultra HNIs and Private Banking clients; ITR > ₹36 Lakhs p.a.",
                Pros = "Unlimited domestic and international lounge visits worldwide|Low 1.5% forex markup rate|6 points per ₹200 on all spends with 1:1 redemption value (3% return) on air tickets and hotels",
                Cons = "Very high joining and annual fee of ₹12,500|Strictly by invitation or high private banking net worth|Excludes rent, wallet, and government spends",
                OfficialProductUrl = "https://www.icicibank.com/personal-banking/cards/credit-cards/emeralde-private-metal-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #064e3b 0%, #022c22 50%, #0f172a 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 3.0m, PointsPer100Rupees = 3.0m, PointRupeeValue = 1.0m, ExcludedTransactions = "Fuel, rent, wallet, govt", Notes = "6 reward points per ₹200 spend (1 pt = ₹1 on flights/hotels)" },
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.RewardPoints, EarningRatePercent = 3.0m, PointsPer100Rupees = 3.0m, PointRupeeValue = 1.0m, ExcludedTransactions = "None", Notes = "3% uncapped return on all travel bookings" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Unlimited Lounge Access", Description = "Unlimited domestic and international lounge visits with guest access privileges", EstimatedAnnualValue = 35000 },
                    new CardBenefit { BenefitType = BenefitType.GolfAccess, Title = "Complimentary Golf", Description = "Unlimited complimentary golf rounds and lessons every month", EstimatedAnnualValue = 30000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 70, MinAnnualIncome = 3600000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 780, Notes = "ICICI Wealth and Private Banking invitation" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.icicibank.com/personal-banking/cards/credit-cards/emeralde-private-metal-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 15. Axis Bank Airtel Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["AXIS"],
                Name = "Axis Bank Airtel Credit Card",
                Slug = "axis-airtel",
                Network = CardNetwork.Visa,
                Category = CardCategory.Cashback,
                JoiningFee = 500,
                AnnualFee = 500,
                FeeWaiverSpendThreshold = 200000,
                FeeWaiverDescription = "Waived on annual spends of ₹2,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 4,
                LoungeAccessInternational = 0,
                LoungeDetails = "4 complimentary domestic airport lounge visits per year (1/quarter, subject to ₹50,000 spend in preceding 3 months)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Amazon.in eVoucher worth ₹500 on 1st transaction within 30 days",
                WelcomeBonusMonetaryValue = 500,
                MinimumIncomeMonthly = 25000,
                PublishedEligibility = "Salaried / Self-employed with monthly income > ₹25,000",
                Pros = "25% cashback on Airtel Mobile, Broadband, DTH, and Wi-Fi bills via Airtel Thanks App (cap ₹250/mo)|10% cashback on utility bills (Electricity, Gas, Water) via Airtel Thanks App (cap ₹250/mo)|10% cashback on Swiggy, Zomato, and BigBasket (cap ₹500/mo)",
                Cons = "Cashback caps are monthly and strict (max ₹1,000 total monthly across categories)|Domestic lounge requires ₹50k spend in preceding 3 months|Airtel app mandatory to earn 25% and 10% utility cashback",
                OfficialProductUrl = "https://www.axisbank.com/retail/cards/credit-card/airtel-axis-bank-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #ef4444 0%, #991b1b 50%, #1e1b4b 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.Utilities, RewardType = RewardType.Cashback, EarningRatePercent = 25.0m, PointsPer100Rupees = 25.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 250, ExcludedTransactions = "Bill payment outside Airtel Thanks App", Notes = "25% cashback on Airtel Mobile, Broadband, DTH recharges (cap ₹250/month) + 10% on utilities (cap ₹250/month)" },
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.Cashback, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 500, ExcludedTransactions = "POS swipe", Notes = "10% cashback on Swiggy, Zomato and BigBasket (shared cap ₹500/month)" },
                    new RewardRule { Category = SpendingCategory.Grocery, RewardType = RewardType.Cashback, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 500, ExcludedTransactions = "Local stores", Notes = "10% cashback on BigBasket grocery orders" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.Cashback, EarningRatePercent = 1.0m, PointsPer100Rupees = 1.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Fuel, rent, wallet reload", Notes = "1% uncapped cashback on other eligible retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Domestic Airport Lounge Access", Description = "4 visits per year (1/quarter upon spending ₹50,000 in preceding 3 months)", EstimatedAnnualValue = 4000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 300000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 730, Notes = "Best utility bill card in India" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.axisbank.com/retail/cards/credit-card/airtel-axis-bank-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 16. Axis Bank Flipkart Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["AXIS"],
                Name = "Axis Bank Flipkart Credit Card",
                Slug = "axis-flipkart",
                Network = CardNetwork.Visa,
                Category = CardCategory.Cashback,
                JoiningFee = 500,
                AnnualFee = 500,
                FeeWaiverSpendThreshold = 350000,
                FeeWaiverDescription = "Waived on annual spends of ₹3,50,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 0,
                LoungeAccessInternational = 0,
                LoungeDetails = "No airport lounge access (discontinued in recent revision)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "₹500 Flipkart voucher on 1st transaction within 30 days",
                WelcomeBonusMonetaryValue = 500,
                MinimumIncomeMonthly = 25000,
                PublishedEligibility = "Salaried: Monthly income > ₹25,000. Self-employed: ITR > ₹6 Lakhs p.a.",
                Pros = "Flat 5% unlimited cashback on Flipkart|4% cashback on preferred partners (Cleartrip, Cult.fit, PVR, Swiggy, Uber)|1% unlimited cashback on other eligible retail spends with direct statement credit",
                Cons = "Domestic airport lounge access removed in revision|Fee waiver threshold increased from ₹2L to ₹3.5L|Fuel, rent, and government spends earn 0% cashback",
                OfficialProductUrl = "https://www.axisbank.com/retail/cards/credit-card/flipkart-axis-bank-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #2563eb 0%, #1d4ed8 50%, #0f172a 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Fuel, rent, wallet reload", Notes = "5% unlimited cashback on Flipkart" },
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.Cashback, EarningRatePercent = 4.0m, PointsPer100Rupees = 4.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "POS", Notes = "4% cashback on Swiggy food orders" },
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.Cashback, EarningRatePercent = 4.0m, PointsPer100Rupees = 4.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Non-partner bookings", Notes = "4% cashback on Cleartrip and Uber" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.Cashback, EarningRatePercent = 1.0m, PointsPer100Rupees = 1.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Fuel, rent, govt, jewelry", Notes = "1% unlimited cashback on other eligible spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.FuelSurchargeWaiver, Title = "1% Fuel Surcharge Waiver", Description = "1% waiver on transactions between ₹400 and ₹4,000, capped at ₹400/month", EstimatedAnnualValue = 1200 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 300000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 730, Notes = "High approval rate on Flipkart app" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.axisbank.com/retail/cards/credit-card/flipkart-axis-bank-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 17. Axis Bank Atlas Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["AXIS"],
                Name = "Axis Bank Atlas Credit Card",
                Slug = "axis-atlas",
                Network = CardNetwork.Visa,
                Category = CardCategory.Travel,
                JoiningFee = 5000,
                AnnualFee = 5000,
                FeeWaiverSpendThreshold = null,
                FeeWaiverDescription = "No annual fee waiver; compensated with 2,500 to 5,000 renewal EDGE Miles based on tier",
                RuPaySupported = false,
                LoungeAccessDomestic = 12,
                LoungeAccessInternational = 6,
                LoungeDetails = "Tier-based: Silver (8 domestic + 4 intl), Gold (12 domestic + 6 intl), Platinum (18 domestic + 12 intl)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "5,000 EDGE Miles on completing 1st transaction within 30 days",
                WelcomeBonusMonetaryValue = 10000,
                MinimumIncomeMonthly = 100000,
                PublishedEligibility = "Salaried: Monthly income > ₹1,00,000. Self-employed: ITR > ₹12 Lakhs p.a.",
                Pros = "5 EDGE Miles per ₹100 spent directly on airlines and hotels (equivalent to 10% return in partner miles)|1 EDGE Mile = 2 Partner Points/Miles (Accor, Singapore Airlines, Qatar Airways, ITC, Marriott)|Welcome gift of 5,000 EDGE Miles (worth ₹10,000 in hotel stays)",
                Cons = "Accelerated airline/hotel spends capped at ₹2,00,000 per month|General spends earn 2 EDGE Miles per ₹100|No renewal fee waiver mechanism",
                OfficialProductUrl = "https://www.axisbank.com/retail/cards/credit-card/axis-bank-atlas-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #1e293b 0%, #334155 50%, #475569 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.EdgeMiles, EarningRatePercent = 10.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 2.0m, MonthlyCapRupees = 4000, ExcludedTransactions = "Travel aggregators like Makemytrip (must be direct airline/hotel)", Notes = "5 EDGE Miles per ₹100 on direct airline and hotel bookings (1 EDGE Mile = 2 Partner Points/Miles, e.g. Accor ALL)" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.EdgeMiles, EarningRatePercent = 4.0m, PointsPer100Rupees = 2.0m, PointRupeeValue = 2.0m, ExcludedTransactions = "Fuel, rent, wallet, insurance, govt", Notes = "2 EDGE Miles per ₹100 on other retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Airport Lounge Access", Description = "Up to 18 domestic and 12 international lounge visits based on tier", EstimatedAnnualValue = 20000 },
                    new CardBenefit { BenefitType = BenefitType.MilestoneBonus, Title = "Tier Milestone EDGE Miles", Description = "Up to 10,000 bonus EDGE Miles on achieving annual spend milestones", EstimatedAnnualValue = 20000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 70, MinAnnualIncome = 1200000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 750, Notes = "Premier choice for frequent flyers and hotel stayers" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.axisbank.com/retail/cards/credit-card/axis-bank-atlas-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 18. Axis Bank Magnus
            list.Add(new CreditCard
            {
                BankId = bankMap["AXIS"],
                Name = "Axis Bank Magnus",
                Slug = "axis-magnus",
                Network = CardNetwork.Mastercard,
                Category = CardCategory.Lifestyle,
                JoiningFee = 12500,
                AnnualFee = 12500,
                FeeWaiverSpendThreshold = 2500000,
                FeeWaiverDescription = "Waived on spends of ₹25,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 999,
                LoungeAccessInternational = 999,
                LoungeDetails = "Unlimited complimentary domestic airport lounge visits + 8 guest visits with Priority Pass. Unlimited international visits.",
                ForexMarkupPercent = 2.0m,
                WelcomeBonus = "Choice of luxury hotel voucher or flight ticket worth ₹10,000",
                WelcomeBonusMonetaryValue = 10000,
                MinimumIncomeMonthly = 250000,
                PublishedEligibility = "Salaried: Monthly income > ₹2.5 Lakhs. Self-employed: ITR > ₹30 Lakhs p.a.",
                Pros = "Unlimited domestic and international airport lounge access with guest visits|Low 2% forex markup fee|8 complimentary airport concierge meet & greet services per year",
                Cons = "Devalued transfer ratio (5:2 for partner points)|High spend threshold of ₹25 Lakhs for annual fee waiver|Monthly 25,000 bonus points milestone discontinued in 2023",
                OfficialProductUrl = "https://www.axisbank.com/retail/cards/credit-card/magnus-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #701a75 0%, #4a044e 50%, #0f172a 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.4m, PointsPer100Rupees = 6.0m, PointRupeeValue = 0.40m, ExcludedTransactions = "Fuel, rent, wallet, govt", Notes = "12 EDGE Reward Points per ₹200 on regular spends" },
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.RewardPoints, EarningRatePercent = 7.0m, PointsPer100Rupees = 17.5m, PointRupeeValue = 0.40m, ExcludedTransactions = "Aggregator portals", Notes = "35 EDGE Reward Points per ₹200 on Travel EDGE portal" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Unlimited Lounge + Guest Privileges", Description = "Unlimited domestic and international airport lounge access with 8 guest visits per year", EstimatedAnnualValue = 35000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 70, MinAnnualIncome = 3000000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 770, Notes = "Super-premium credit card" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.axisbank.com/retail/cards/credit-card/magnus-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 19. Axis Bank Neo Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["AXIS"],
                Name = "Axis Bank Neo Credit Card",
                Slug = "axis-neo",
                Network = CardNetwork.Visa,
                Category = CardCategory.Shopping,
                JoiningFee = 250,
                AnnualFee = 250,
                FeeWaiverSpendThreshold = 100000,
                FeeWaiverDescription = "Waived on annual spends of ₹1,00,000 or more in the preceding anniversary year",
                RuPaySupported = true,
                LoungeAccessDomestic = 0,
                LoungeAccessInternational = 0,
                LoungeDetails = "No airport lounge access",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Amazon Pay gift voucher worth ₹250 on 1st transaction",
                WelcomeBonusMonetaryValue = 250,
                MinimumIncomeMonthly = 20000,
                PublishedEligibility = "Salaried: Monthly income > ₹20,000. Self-employed: ITR > ₹3.6 Lakhs p.a.",
                Pros = "10% off on Myntra (up to ₹100 per transaction)|10% off on BookMyShow (up to ₹100 per month)|5% off on utility bill payments via Amazon Pay (up to ₹150 per month)|Very low ₹250 annual fee",
                Cons = "No airport lounge access|Modest discount caps per transaction|Low base reward rate of 1 EDGE point per ₹200",
                OfficialProductUrl = "https://www.axisbank.com/retail/cards/credit-card/neo-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #0284c7 0%, #0891b2 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.Cashback, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 100, ExcludedTransactions = "Gift cards", Notes = "10% instant discount on Myntra up to ₹100 per transaction" },
                    new RewardRule { Category = SpendingCategory.Utilities, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 150, ExcludedTransactions = "Direct biller", Notes = "5% off on utility bill payment via Amazon Pay up to ₹150/month" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.20m, PointsPer100Rupees = 0.50m, PointRupeeValue = 0.40m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "1 EDGE point per ₹200 on other retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MovieTicket, Title = "BookMyShow Discount", Description = "10% off on movie tickets up to ₹100 per month on BookMyShow", EstimatedAnnualValue = 1200 }
                },
                Eligibility = new EligibilityRule { MinAge = 18, MaxAge = 65, MinAnnualIncome = 240000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 710, Notes = "Student and beginner friendly" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.axisbank.com/retail/cards/credit-card/neo-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 20. IDFC FIRST Wealth Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["IDFC"],
                Name = "IDFC FIRST Wealth Credit Card",
                Slug = "idfc-first-wealth",
                Network = CardNetwork.Visa,
                Category = CardCategory.Lifestyle,
                JoiningFee = 0,
                AnnualFee = 0,
                FeeWaiverSpendThreshold = null,
                FeeWaiverDescription = "Lifetime Free — No joining fee, no annual fee ever",
                RuPaySupported = false,
                LoungeAccessDomestic = 16,
                LoungeAccessInternational = 16,
                LoungeDetails = "4 complimentary domestic airport lounge & spa visits per quarter (subject to ₹20,000 spend in preceding month) + 4 international lounge visits via Dreamfolks",
                ForexMarkupPercent = 1.5m,
                WelcomeBonus = "Gift voucher worth ₹500 on spending ₹15,000 within 90 days of card issuance",
                WelcomeBonusMonetaryValue = 500,
                MinimumIncomeMonthly = 150000,
                PublishedEligibility = "Salaried: Monthly income > ₹1,50,000 or high relationship balance in IDFC Wealth account",
                Pros = "Lifetime Free card with no annual fee ever|Low 1.5% forex markup fee|10X reward points (2.5% value) on incremental spends above ₹30,000/month with points that never expire|Buy 1 Get 1 free movie ticket up to ₹500 twice a month on Paytm",
                Cons = "Domestic lounge access requires ₹20,000 spend in the preceding billing cycle|Base offline reward rate is 0.75% (3X points)|Interest rates vary dynamically from 9% to 36% based on credit score",
                OfficialProductUrl = "https://www.idfcfirstbank.com/credit-card/wealth",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #881337 0%, #4c0519 50%, #0f172a 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet, insurance, govt", Notes = "10X points (2.5% return) on incremental monthly spends above ₹30,000 and birthday spends" },
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.5m, PointsPer100Rupees = 6.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent", Notes = "6X points (1.5% return) on online spends up to ₹30,000/month" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Domestic & Intl Lounge + Spa", Description = "4 domestic airport lounge & spa visits per quarter + 4 international visits per quarter", EstimatedAnnualValue = 24000 },
                    new CardBenefit { BenefitType = BenefitType.MovieTicket, Title = "BOGO Movie Tickets", Description = "Buy 1 Get 1 free movie ticket up to ₹500 twice a month on Paytm/District", EstimatedAnnualValue = 12000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 1800000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 750, Notes = "Lifetime Free premium tier card" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.idfcfirstbank.com/credit-card/wealth", VerifiedDate = verifiedDate }
                }
            });

            // 21. IDFC FIRST Select Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["IDFC"],
                Name = "IDFC FIRST Select Credit Card",
                Slug = "idfc-first-select",
                Network = CardNetwork.Visa,
                Category = CardCategory.Rewards,
                JoiningFee = 0,
                AnnualFee = 0,
                FeeWaiverSpendThreshold = null,
                FeeWaiverDescription = "Lifetime Free — No joining fee, no annual fee ever",
                RuPaySupported = false,
                LoungeAccessDomestic = 16,
                LoungeAccessInternational = 0,
                LoungeDetails = "4 complimentary domestic airport lounge visits per quarter (subject to ₹20,000 spend in preceding month) + 4 railway lounge visits/quarter",
                ForexMarkupPercent = 1.99m,
                WelcomeBonus = "Gift voucher worth ₹500 on spending ₹15,000 within 90 days",
                WelcomeBonusMonetaryValue = 500,
                MinimumIncomeMonthly = 100000,
                PublishedEligibility = "Salaried: Monthly income > ₹1,00,000. Self-employed: ITR > ₹12 Lakhs p.a.",
                Pros = "Lifetime Free with no annual fees ever|Low 1.99% forex markup fee|Buy 1 Get 1 free movie ticket up to ₹250 twice a month on Paytm|Points never expire and can be redeemed directly against online shopping checkout",
                Cons = "Domestic lounge access requires ₹20,000 spend in the preceding billing cycle|No international airport lounge access|Accelerated 10X points kicks in only after ₹25,000 monthly spend",
                OfficialProductUrl = "https://www.idfcfirstbank.com/credit-card/select",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #991b1b 0%, #7f1d1d 50%, #0f172a 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet, govt", Notes = "10X points (2.5% return) on incremental monthly spends above ₹25,000" },
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.5m, PointsPer100Rupees = 6.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent", Notes = "6X points on online spends up to ₹25,000/month" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MovieTicket, Title = "BOGO Movie Tickets", Description = "Buy 1 Get 1 free movie ticket up to ₹250 twice every month on Paytm", EstimatedAnnualValue = 6000 },
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Airport & Railway Lounges", Description = "4 domestic airport lounge visits + 4 railway lounge visits per quarter", EstimatedAnnualValue = 12000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 1200000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 740, Notes = "Lifetime Free affluent lifestyle card" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.idfcfirstbank.com/credit-card/select", VerifiedDate = verifiedDate }
                }
            });

            // 22. IDFC FIRST Classic Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["IDFC"],
                Name = "IDFC FIRST Classic Credit Card",
                Slug = "idfc-first-classic",
                Network = CardNetwork.Visa,
                Category = CardCategory.Rewards,
                JoiningFee = 0,
                AnnualFee = 0,
                FeeWaiverSpendThreshold = null,
                FeeWaiverDescription = "Lifetime Free — No annual fee ever",
                RuPaySupported = false,
                LoungeAccessDomestic = 0,
                LoungeAccessInternational = 0,
                LoungeDetails = "4 complimentary railway lounge visits per quarter (no airport lounge)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Gift voucher worth ₹500 on spending ₹15,000 within 90 days",
                WelcomeBonusMonetaryValue = 500,
                MinimumIncomeMonthly = 25000,
                PublishedEligibility = "Salaried: Monthly income > ₹25,000",
                Pros = "Lifetime Free with zero joining and annual fees|Points never expire|Low dynamic interest rate from 9% p.a.|25% off on movie tickets up to ₹100 once a month",
                Cons = "No airport lounge access (only railway lounges)|Base rewards rate is 0.75% for offline transactions|Accelerated 10X rewards requires spending above ₹20,000 in a month",
                OfficialProductUrl = "https://www.idfcfirstbank.com/credit-card/classic",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "10X points on spends above ₹20,000 in a month" },
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.5m, PointsPer100Rupees = 6.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Wallet load", Notes = "6X points on online spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MovieTicket, Title = "Movie Discount", Description = "25% discount on movie tickets up to ₹100 once a month on Paytm", EstimatedAnnualValue = 1200 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 300000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 710, Notes = "Beginner friendly entry card" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.idfcfirstbank.com/credit-card/classic", VerifiedDate = verifiedDate }
                }
            });

            // 23. IDFC FIRST SWYP Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["IDFC"],
                Name = "IDFC FIRST SWYP Credit Card",
                Slug = "idfc-first-swyp",
                Network = CardNetwork.Mastercard,
                Category = CardCategory.Shopping,
                JoiningFee = 499,
                AnnualFee = 499,
                FeeWaiverSpendThreshold = 100000,
                FeeWaiverDescription = "Waived on annual spends of ₹1,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 0,
                LoungeAccessInternational = 0,
                LoungeDetails = "No airport lounge access",
                ForexMarkupPercent = 1.99m,
                WelcomeBonus = "1,000 reward points on card activation",
                WelcomeBonusMonetaryValue = 250,
                MinimumIncomeMonthly = 25000,
                PublishedEligibility = "Ages 18-35 with minimum monthly income of ₹25,000",
                Pros = "Monthly milestone bonus points (up to 1,400 points monthly on spends between ₹5k and ₹15k)|Low 1.99% forex markup fee|10% discounts on youth partner brands (Zomato, Domino's, Tata CLiQ)|No-cost EMI conversion fee waivers",
                Cons = "Annual fee of ₹499|No airport lounge access|Base rewards rate is low without hitting monthly spend milestones",
                OfficialProductUrl = "https://www.idfcfirstbank.com/credit-card/swyp",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #0d9488 0%, #115e59 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent", Notes = "Milestone reward points equivalent to 2.5% return on achieving ₹15k monthly spends" },
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.Cashback, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 150, ExcludedTransactions = "Offline dining", Notes = "10% discount on Zomato and Domino's up to ₹150 per month" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MilestoneBonus, Title = "Monthly Milestone Points", Description = "Earn 400 pts on ₹5k spend, 1,000 pts on ₹10k spend, and 1,400 pts on ₹15k spend every month", EstimatedAnnualValue = 4200 }
                },
                Eligibility = new EligibilityRule { MinAge = 18, MaxAge = 35, MinAnnualIncome = 300000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 710, Notes = "Aimed at Gen-Z and first-job professionals" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.idfcfirstbank.com/credit-card/swyp", VerifiedDate = verifiedDate }
                }
            });

            // 24. American Express Platinum Travel Card
            list.Add(new CreditCard
            {
                BankId = bankMap["AMEX"],
                Name = "American Express Platinum Travel Card",
                Slug = "amex-platinum-travel",
                Network = CardNetwork.AmericanExpress,
                Category = CardCategory.Travel,
                JoiningFee = 3500,
                AnnualFee = 5000,
                FeeWaiverSpendThreshold = null,
                FeeWaiverDescription = "No annual fee waiver threshold",
                RuPaySupported = false,
                LoungeAccessDomestic = 8,
                LoungeAccessInternational = 0,
                LoungeDetails = "8 complimentary domestic airport lounge visits per year (2 per quarter)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "10,000 Membership Rewards (MR) points upon spending ₹15,000 within 90 days",
                WelcomeBonusMonetaryValue = 5000,
                MinimumIncomeMonthly = 50000,
                PublishedEligibility = "Salaried / Self-employed with annual income > ₹6,00,000",
                Pros = "Superb milestone rewards: 15,000 MR points on ₹1.9L annual spend + 25,000 MR points and ₹10,000 Taj Hotels voucher on ₹4L annual spend|40,000 MR points can be transferred to Marriott Bonvoy (1:1) for ~₹20,000+ in hotel stays|8 domestic airport lounge visits per year",
                Cons = "Rewards rate drops sharply beyond ₹4,00,000 annual spend|High renewal fee of ₹5,000 with no official waiver mechanism|Amex acceptance in small local offline shops in India is lower than Visa/Mastercard",
                OfficialProductUrl = "https://www.americanexpress.com/in/credit-cards/platinum-travel-credit-card/",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #64748b 0%, #475569 50%, #1e293b 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.0m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Fuel, utilities, insurance, cash", Notes = "1 Membership Rewards point per ₹50 spent" },
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.RewardPoints, EarningRatePercent = 8.5m, PointsPer100Rupees = 17.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Fuel, wallet", Notes = "Effective 8.5% return on reaching exact ₹4,00,000 milestone (40,000 MR points + ₹10,000 Taj voucher)" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MilestoneBonus, Title = "Taj Hotels Luxury Stay Voucher", Description = "₹10,000 Taj Hotels voucher upon reaching ₹4,00,000 annual milestone spend", EstimatedAnnualValue = 10000 },
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Domestic Airport Lounge Access", Description = "8 domestic lounge visits per year (2 per quarter)", EstimatedAnnualValue = 8000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 600000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 750, Notes = "Must have a serviceable Amex pin code" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.americanexpress.com/in/credit-cards/platinum-travel-credit-card/", VerifiedDate = verifiedDate }
                }
            });

            // 25. American Express Membership Rewards Credit Card (MRCC)
            list.Add(new CreditCard
            {
                BankId = bankMap["AMEX"],
                Name = "American Express Membership Rewards Credit Card",
                Slug = "amex-mrcc",
                Network = CardNetwork.AmericanExpress,
                Category = CardCategory.Rewards,
                JoiningFee = 1000,
                AnnualFee = 4500,
                FeeWaiverSpendThreshold = 150000,
                FeeWaiverDescription = "Waived 100% on annual spends of ₹1,50,000 or more (50% fee waiver on ₹90,000)",
                RuPaySupported = false,
                LoungeAccessDomestic = 0,
                LoungeAccessInternational = 0,
                LoungeDetails = "No airport lounge access",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "4,000 bonus Membership Rewards points upon spending ₹15,000 within 90 days",
                WelcomeBonusMonetaryValue = 2000,
                MinimumIncomeMonthly = 50000,
                PublishedEligibility = "Salaried / Self-employed with annual income > ₹6,00,000",
                Pros = "1,000 bonus MR points on doing 4 transactions of ₹1,500+ in a calendar month|Extra 1,000 bonus MR points on spending ₹20,000 in a month|Up to 24,000 bonus MR points per year just from monthly milestones (worth ₹12,000 in vouchers or 24,000 Marriott Bonvoy points)",
                Cons = "No airport lounge access|Offline acceptance limitations compared to Visa/RuPay|High renewal fee of ₹4,500 if the ₹1.5L spend waiver is missed",
                OfficialProductUrl = "https://www.americanexpress.com/in/credit-cards/membership-rewards-credit-card/",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #d97706 0%, #b45309 50%, #1e293b 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.0m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Fuel, utilities, insurance, cash", Notes = "1 MR point per ₹50 spent" },
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.RewardPoints, EarningRatePercent = 5.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Fuel", Notes = "Up to 5% effective return when hitting monthly 4x₹1,500 and ₹20k spend milestones (24k bonus MR/year)" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MilestoneBonus, Title = "Monthly Milestone Bonus Points", Description = "1,000 MR points for 4 transactions of ₹1,500+ and 1,000 MR points for ₹20,000 monthly spend", EstimatedAnnualValue = 12000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 600000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 750, Notes = "Amex serviceable cities" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.americanexpress.com/in/credit-cards/membership-rewards-credit-card/", VerifiedDate = verifiedDate }
                }
            });

            // 26. HSBC Cashback Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["HSBC"],
                Name = "HSBC Cashback Credit Card",
                Slug = "hsbc-cashback",
                Network = CardNetwork.Visa,
                Category = CardCategory.Cashback,
                JoiningFee = 999,
                AnnualFee = 999,
                FeeWaiverSpendThreshold = 200000,
                FeeWaiverDescription = "Waived on annual spends of ₹2,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 4,
                LoungeAccessInternational = 0,
                LoungeDetails = "4 complimentary domestic airport lounge visits per year (1 per quarter)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Amazon voucher worth ₹1,000 on spending ₹10,000 within 30 days",
                WelcomeBonusMonetaryValue = 1000,
                MinimumIncomeMonthly = 35000,
                PublishedEligibility = "Salaried: Monthly income > ₹35,000 in metro cities",
                Pros = "10% cashback on dining, food delivery (Zomato, Swiggy), and grocery spends (cap ₹1,000/mo)|1.5% unlimited cashback on all other eligible retail transactions|Includes 4 domestic airport lounge visits per year",
                Cons = "10% accelerated cashback requires minimum transaction amount of ₹500|Combined cap of ₹1,000 per month across dining and groceries|HSBC credit cards are primarily issued in tier-1 metro cities",
                OfficialProductUrl = "https://www.hsbc.co.in/credit-cards/products/cashback/",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #b91c1c 0%, #991b1b 50%, #0f172a 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.Cashback, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 1000, MinimumSpendAmount = 500, ExcludedTransactions = "Fuel station food", Notes = "10% cashback on dining, Swiggy, Zomato and grocery (min txn ₹500, combined cap ₹1,000/mo)" },
                    new RewardRule { Category = SpendingCategory.Grocery, RewardType = RewardType.Cashback, EarningRatePercent = 10.0m, PointsPer100Rupees = 10.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 1000, MinimumSpendAmount = 500, ExcludedTransactions = "Local cash mandis", Notes = "10% cashback on grocery purchases (shared cap ₹1,000/month)" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.Cashback, EarningRatePercent = 1.5m, PointsPer100Rupees = 1.5m, PointRupeeValue = 1.0m, MonthlyCapRupees = null, ExcludedTransactions = "Fuel, rent, wallet reload", Notes = "1.5% uncapped cashback on all other retail transactions" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Domestic Airport Lounge Access", Description = "4 complimentary lounge visits per year (1 per calendar quarter)", EstimatedAnnualValue = 4000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 420000, EmploymentType = EmploymentType.Salaried, SuggestedCreditScore = 750, Notes = "Metro cities serviceable only" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.hsbc.co.in/credit-cards/products/cashback/", VerifiedDate = verifiedDate }
                }
            });

            // 27. AU Small Finance Bank Zenith+ Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["AUBANK"],
                Name = "AU Small Finance Bank Zenith+",
                Slug = "au-zenith-plus",
                Network = CardNetwork.Visa,
                Category = CardCategory.Lifestyle,
                JoiningFee = 4999,
                AnnualFee = 4999,
                FeeWaiverSpendThreshold = 800000,
                FeeWaiverDescription = "Waived on annual spends of ₹8,00,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 16,
                LoungeAccessInternational = 16,
                LoungeDetails = "16 domestic airport lounge visits per year (4/quarter) + 16 international lounge visits via Priority Pass",
                ForexMarkupPercent = 0.99m,
                WelcomeBonus = "Welcome voucher worth ₹5,000 on luxury brands (Taj, Postcard Hotels) upon payment of joining fee",
                WelcomeBonusMonetaryValue = 5000,
                MinimumIncomeMonthly = 250000,
                PublishedEligibility = "Salaried: Monthly income > ₹2,50,000. Self-employed: ITR > ₹30 Lakhs p.a.",
                Pros = "Ultra-low 0.99% forex markup fee (one of the lowest in India)|16 domestic and 16 international airport lounge visits per year|Buy 1 Get 1 free movie ticket on BookMyShow up to ₹500 twice a month|Dining and International travel earn 4 reward points per ₹100",
                Cons = "Annual fee of ₹4,999|AU Small Finance Bank branch presence is concentrated in select northern/western states|1 reward point = ₹0.25 on standard merchandise",
                OfficialProductUrl = "https://www.aubank.in/personal-banking/credit-cards/zenith-plus-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #4a044e 0%, #701a75 50%, #0f172a 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.International, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.0m, PointsPer100Rupees = 4.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Cash advance", Notes = "4 reward points per ₹100 on dining, international travel, and luxury brands" },
                    new RewardRule { Category = SpendingCategory.Dining, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.0m, PointsPer100Rupees = 4.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Fuel station food", Notes = "4 reward points per ₹100 on dining spends" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.0m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.50m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "2 reward points per ₹100 on other retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Airport Lounge Access", Description = "16 domestic and 16 international lounge visits per calendar year", EstimatedAnnualValue = 32000 },
                    new CardBenefit { BenefitType = BenefitType.MovieTicket, Title = "BOGO Movie Tickets", Description = "Buy 1 Get 1 free movie ticket up to ₹500 twice a month on BookMyShow", EstimatedAnnualValue = 12000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 60, MinAnnualIncome = 3000000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 760, Notes = "Super-premium lifestyle product" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.aubank.in/personal-banking/credit-cards/zenith-plus-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 28. AU Small Finance Bank LIT Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["AUBANK"],
                Name = "AU Small Finance Bank LIT Credit Card",
                Slug = "au-lit",
                Network = CardNetwork.Visa,
                Category = CardCategory.Cashback,
                JoiningFee = 0,
                AnnualFee = 0,
                FeeWaiverSpendThreshold = null,
                FeeWaiverDescription = "Lifetime Free base card — Pay only for the specific features you activate",
                RuPaySupported = false,
                LoungeAccessDomestic = 4,
                LoungeAccessInternational = 0,
                LoungeDetails = "Customizable: Activate 4 domestic lounge visits for 90 days at ₹299 micro-subscription",
                ForexMarkupPercent = 1.99m,
                WelcomeBonus = "Nil",
                WelcomeBonusMonetaryValue = 0,
                MinimumIncomeMonthly = 25000,
                PublishedEligibility = "Salaried / Self-employed with monthly income > ₹25,000",
                Pros = "India's first customizable credit card: switch on/off features via mobile app|Lifetime Free base card with zero fixed annual fees|Activate 5% cashback on groceries or travel when you know you will spend",
                Cons = "Feature subscriptions expire after 30 to 90 days and require manual renewal|Base card without active feature packs earns only 1 point per ₹100|Requires frequent monitoring via AU 0101 app",
                OfficialProductUrl = "https://www.aubank.in/personal-banking/credit-cards/lit-credit-card",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.Grocery, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 1000, ExcludedTransactions = "Fuel, rent", Notes = "5% cashback feature pack on grocery spends (cap ₹1,000 per 30 days)" },
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.Cashback, EarningRatePercent = 5.0m, PointsPer100Rupees = 5.0m, PointRupeeValue = 1.0m, MonthlyCapRupees = 1000, ExcludedTransactions = "Fuel, rent", Notes = "5% cashback feature pack on online shopping (cap ₹1,000 per 30 days)" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.25m, PointsPer100Rupees = 1.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "1 reward point per ₹100 on other retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.Other, Title = "Customizable Micro-Features", Description = "Switch on cashback, lounge access, or milestone bonuses dynamically as needed", EstimatedAnnualValue = 3000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 60, MinAnnualIncome = 300000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 720, Notes = "Ideal for agile spenders who like flexibility" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.aubank.in/personal-banking/credit-cards/lit-credit-card", VerifiedDate = verifiedDate }
                }
            });

            // 29. IndusInd Bank Legend Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["INDUSIND"],
                Name = "IndusInd Bank Legend Credit Card",
                Slug = "indusind-legend",
                Network = CardNetwork.Visa,
                Category = CardCategory.Lifestyle,
                JoiningFee = 0,
                AnnualFee = 0,
                FeeWaiverSpendThreshold = null,
                FeeWaiverDescription = "Lifetime Free (often provided with zero annual fee on special invite)",
                RuPaySupported = false,
                LoungeAccessDomestic = 4,
                LoungeAccessInternational = 0,
                LoungeDetails = "1 complimentary domestic airport lounge visit per quarter (4 per year)",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Gift vouchers from Oberoi Hotels, Luxe gift cards or Montblanc upon joining",
                WelcomeBonusMonetaryValue = 1000,
                MinimumIncomeMonthly = 80000,
                PublishedEligibility = "Salaried: Monthly income > ₹80,000. Self-employed: ITR > ₹10 Lakhs p.a.",
                Pros = "Lifetime Free card with no annual maintenance charges|1 complimentary domestic airport lounge visit per quarter|Buy 1 Get 1 free movie ticket on BookMyShow up to ₹200 three times a month (₹7,200 annual value)|Weekend spends earn 2 reward points per ₹100",
                Cons = "Weekday retail spends earn only 1 reward point per ₹100 (₹0.75 value per ₹100)|High foreign currency markup of 3.5%|No complimentary international lounge visits",
                OfficialProductUrl = "https://www.indusind.com/in/en/personal/cards/credit-cards/legend-credit-card.html",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = false,
                CardGradient = "linear-gradient(135deg, #78350f 0%, #451a03 50%, #0f172a 100%)",
                ChipColor = "gold",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 1.5m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.75m, ExcludedTransactions = "Fuel, rent, wallet, govt", Notes = "2 reward points per ₹100 on weekend retail spends (1 pt = ₹0.75 cash credit)" },
                    new RewardRule { Category = SpendingCategory.OnlineShopping, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.75m, PointsPer100Rupees = 1.0m, PointRupeeValue = 0.75m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "1 reward point per ₹100 on weekday retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.MovieTicket, Title = "BOGO Movie Tickets", Description = "Buy 1 Get 1 free movie ticket up to ₹200 on BookMyShow 3 times every month", EstimatedAnnualValue = 7200 },
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Domestic Airport Lounge Access", Description = "1 complimentary domestic airport lounge visit per quarter", EstimatedAnnualValue = 4000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 960000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 740, Notes = "High approval rate under special corporate promotions" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.indusind.com/in/en/personal/cards/credit-cards/legend-credit-card.html", VerifiedDate = verifiedDate }
                }
            });

            // 30. Standard Chartered EaseMyTrip Credit Card
            list.Add(new CreditCard
            {
                BankId = bankMap["SC"],
                Name = "Standard Chartered EaseMyTrip Credit Card",
                Slug = "sc-easemytrip",
                Network = CardNetwork.Visa,
                Category = CardCategory.Travel,
                JoiningFee = 350,
                AnnualFee = 350,
                FeeWaiverSpendThreshold = 50000,
                FeeWaiverDescription = "Waived on annual spends of ₹50,000 or more in the preceding anniversary year",
                RuPaySupported = false,
                LoungeAccessDomestic = 4,
                LoungeAccessInternational = 2,
                LoungeDetails = "1 complimentary domestic airport lounge visit per calendar quarter (4/year) + 2 international lounge visits/year",
                ForexMarkupPercent = 3.5m,
                WelcomeBonus = "Flat discounts on EaseMyTrip travel bookings immediately upon card activation",
                WelcomeBonusMonetaryValue = 350,
                MinimumIncomeMonthly = 35000,
                PublishedEligibility = "Salaried / Self-employed with monthly income > ₹35,000",
                Pros = "Flat 20% instant discount on domestic & international hotels on EaseMyTrip (up to ₹5,000)|Flat 10% instant discount on domestic & international flights on EaseMyTrip (up to ₹1,000)|Includes 4 domestic and 2 international airport lounge visits per year|Very low ₹350 annual fee waived on just ₹50,000 annual spend",
                Cons = "Discounts apply only when booked via EaseMyTrip website or app with promo code|Low base reward rate of 2 points per ₹100 on other retail spends (1 pt = ₹0.25)|Standard 3.5% foreign currency markup",
                OfficialProductUrl = "https://www.sc.com/in/credit-cards/easemytrip-credit-card/",
                SourceVerifiedDate = verifiedDate,
                IsFeatured = true,
                CardGradient = "linear-gradient(135deg, #047857 0%, #065f46 50%, #0f172a 100%)",
                ChipColor = "silver",
                RewardRules = new List<RewardRule>
                {
                    new RewardRule { Category = SpendingCategory.Travel, RewardType = RewardType.RewardPoints, EarningRatePercent = 2.5m, PointsPer100Rupees = 10.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent", Notes = "10 reward points per ₹100 on standalone flight and hotel tickets booked outside EaseMyTrip portal" },
                    new RewardRule { Category = SpendingCategory.GeneralSpends, RewardType = RewardType.RewardPoints, EarningRatePercent = 0.50m, PointsPer100Rupees = 2.0m, PointRupeeValue = 0.25m, ExcludedTransactions = "Fuel, rent, wallet", Notes = "2 reward points per ₹100 on other retail spends" }
                },
                Benefits = new List<CardBenefit>
                {
                    new CardBenefit { BenefitType = BenefitType.LoungeAccess, Title = "Airport Lounge Access", Description = "4 domestic lounge visits per year (1/quarter) + 2 international lounge visits per year", EstimatedAnnualValue = 8000 },
                    new CardBenefit { BenefitType = BenefitType.Other, Title = "EaseMyTrip Instant Discounts", Description = "Flat 20% off on hotel bookings (up to ₹5,000) and 10% off on flights (up to ₹1,000)", EstimatedAnnualValue = 12000 }
                },
                Eligibility = new EligibilityRule { MinAge = 21, MaxAge = 65, MinAnnualIncome = 420000, EmploymentType = EmploymentType.Any, SuggestedCreditScore = 730, Notes = "Outstanding low-fee card for frequent travel bookers" },
                Sources = new List<CardSource>
                {
                    new CardSource { SourceType = "Official Issuer Page", SourceUrl = "https://www.sc.com/in/credit-cards/easemytrip-credit-card/", VerifiedDate = verifiedDate }
                }
            });

            return list;
        }
    }
}
