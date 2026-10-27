# CardCompass 🧭

> **Find the card that works for you.**
> A modern, production-grade Indian credit card recommendation and comparison platform built with ASP.NET Core 8 MVC, Entity Framework Core, SQL Server / SQLite, and ASP.NET Core Identity.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/Unit%20Tests-12%20Passing-success.svg)](https://github.com/)
[![Database](https://img.shields.io/badge/Database-SQL%20Server%20%7C%20SQLite-informational.svg)](https://learn.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 1. Product Overview

Indian consumers face hundreds of credit cards with complex fee structures, category-specific reward rates, monthly cashback caps, exclusion lists, lounge access spend criteria, and point-to-rupee devaluation rules.

**CardCompass** solves this problem by providing transparent, source-backed financial tools to answer four core questions:
1. **Which credit cards suit my income and profile?** (*Personalized Match Wizard*)
2. **Which card delivers the highest net financial return on my spending?** (*Rewards Calculator*)
3. **How do 2 or 3 cards compare side-by-side across all terms?** (*3-Card Comparison Matrix*)
4. **Which card in my wallet should I swipe for a specific transaction?** (*Purchase Optimizer*)

---

## 2. Architecture & Technology Stack

The application is structured as a maintainable, modular monolith following clean architecture principles:

* **Backend Framework**: ASP.NET Core 8.0 MVC (C#)
* **View Layer**: Razor Views (`.cshtml`) with strongly-typed ViewModels, Partial Views, and ViewComponents
* **Styling & UI**: Custom **CardCompass Design System** built on Vanilla CSS variables, Bootstrap 5 grid/utilities, Google Fonts (*Outfit* + *Inter*), and Bootstrap Icons
* **Data Persistence**: Entity Framework Core 8.0 with dual-provider architecture:
  * **Microsoft SQL Server** (Production target via `Microsoft.EntityFrameworkCore.SqlServer`)
  * **SQLite** (Zero-dependency local development via `Microsoft.EntityFrameworkCore.Sqlite`)
* **Security & Auth**: ASP.NET Core Identity with password hashing, anti-forgery tokens (CSRF protection), cookie sliding expiration, and role-based authorization (`Admin` and `User` roles)
* **Visualizations**: Chart.js for portfolio spend distribution and rewards yield
* **Test Suite**: xUnit with Moq for business logic, reward cap calculations, and ranking algorithms

---

## 3. Key Features

### 🌟 Customer-Facing Features
* **Homepage**: Hero search, benefit category shortcuts, featured cards carousel, transparent methodology breakdown, FAQ accordion, and RBI compliance notes.
* **Card Catalogue (`/Cards`)**: Complete directory of 30 researched Indian credit cards with real-time multi-facet filtering (Bank, Category, Card Network, Lifetime Free / Fee Tiers, RuPay UPI Support, Lounge Access) and multi-attribute sorting.
* **Card Detail Page (`/Cards/Details/{slug}`)**: Comprehensive breakdown of verified fees, waiver conditions, category-specific reward rules, points-to-rupee conversion values, lounge terms, forex markup, eligibility criteria, pros/cons, official issuer links, and source verification dates.
* **Side-by-Side Comparison (`/Compare`)**: Compare up to 3 cards simultaneously with difference highlighting, category winners, and interactive net annual benefit simulation.
* **Personalized Match Wizard (`/Recommendation`)**: Deterministic recommendation engine based on income, employment, spend distribution across 7 categories, and preferred perks. Provides transparent 0–100 match scores and plain-English rationales.
* **Rewards Calculator (`/Calculator`)**: Calculates gross rewards, caps applied, fee waivers, first-year net benefit, and renewal-year net benefit for any selected card.
* **Purchase Optimizer (`/PurchaseOptimizer`)**: Swiping advisor that recommends the best card to use for a specific transaction amount and merchant category.
* **Customer Dashboard (`/Dashboard`)**:
  * **My Wallet (`/Dashboard/Wallet`)**: Add owned cards, track payment due dates, and billing cycle days.
  * **Expense Ledger (`/Dashboard/Expenses`)**: Log purchases and view automatic cashback and reward calculations.
  * **Reward Points Tracker**: Monitor reward points balance and expiry dates.

### 🛡️ Admin Management Console (`/Admin`)
* **Protected by Server-Side Role Authorization** (`[Authorize(Roles = "Admin")]`).
* **Catalogue Management**: Create, edit, toggle active status, or delete credit cards.
* **Bank Management**: Manage bank records, customer care hotlines, and brand colors.
* **Feedback Inbox**: Review and action inquiries submitted through the contact page.
* **Directory Telemetry**: View live database counts of cards, banks, users, and inquiries.

---

## 4. 30 Researched Indian Credit Cards Dataset

The application seeds 30 credit cards across 10 major Indian issuers with source-verified terms as of October 2026:

| # | Card Name | Issuing Bank | Category | Network | Annual Fee | Key Feature |
|---|---|---|---|---|---|---|
| 1 | **HDFC Regalia Gold** | HDFC Bank | Travel | Visa | ₹2,500 | 12 Domestic + 6 Intl Lounges, 5X on retail brands |
| 2 | **HDFC Infinia Metal Edition** | HDFC Bank | Lifestyle | Visa | ₹12,500 | 3.33% base rewards, 10X on SmartBuy, unlimited lounge |
| 3 | **HDFC Millennia** | HDFC Bank | Cashback | Mastercard | ₹1,000 | 5% cashback on Amazon, Flipkart, Swiggy, Zomato |
| 4 | **Tata Neu Infinity HDFC** | HDFC Bank | UPI / Shopping | RuPay | ₹1,499 | 10% NeuCoins on Tata Neu, 1.5% cashback on RuPay UPI |
| 5 | **Swiggy HDFC Bank** | HDFC Bank | Dining / Food | Mastercard | ₹500 | 10% cashback on Swiggy, 5% on top online merchants |
| 6 | **SBI Cashback Credit Card** | SBI Card | Cashback | Visa | ₹999 | Flat 5% online cashback capped at ₹5,000/month |
| 7 | **SBI Card ELITE** | SBI Card | Premium | Visa | ₹4,999 | 6 domestic + 6 intl lounges, free movie tickets |
| 8 | **SBI SimplyCLICK** | SBI Card | Entry-Level | Visa | ₹499 | 10X reward points on Amazon, Cleartrip, BookMyShow |
| 9 | **BPCL SBI Card Octane** | SBI Card | Fuel | RuPay | ₹1,499 | 7.25% value back on BPCL fuel & lubricants |
| 10 | **Air India SBI Signature** | SBI Card | Travel | Visa | ₹4,999 | Up to 30 reward points per ₹100 on Air India bookings |
| 11 | **Amazon Pay ICICI Bank** | ICICI Bank | Cashback | Visa | Lifetime Free | Unlimited 5% cashback for Prime, 1% on other spends |
| 12 | **ICICI Sapphiro** | ICICI Bank | Premium | Mastercard | ₹3,500 | Buy 1 Get 1 free movies, 4 domestic + 2 intl lounges |
| 13 | **ICICI Coral RuPay** | ICICI Bank | UPI / Dining | RuPay | ₹500 | RuPay UPI enabled, 25% off BookMyShow, 1 lounge/qtr |
| 14 | **ICICI Emeralde Private Metal** | ICICI Bank | Super-Premium | Visa | ₹12,500 | Low 1.99% forex markup, unlimited domestic & intl lounges |
| 15 | **ICICI Platinum Chip** | ICICI Bank | Entry-Level | Visa | Lifetime Free | Zero annual fee, 2 payback points per ₹100 |
| 16 | **Axis Bank Atlas** | Axis Bank | Travel | Visa | ₹5,000 | EDGE Miles with 1:2 airline & hotel conversion |
| 17 | **Axis Bank Magnus** | Axis Bank | Super-Premium | Mastercard | ₹12,500 | Unlimited lounges + 8 guest visits, 2% forex markup |
| 18 | **Flipkart Axis Bank** | Axis Bank | Cashback | Visa | ₹500 | Flat 5% unlimited cashback on Flipkart, 4% on partners |
| 19 | **Airtel Axis Bank** | Axis Bank | Utilities | Mastercard | ₹500 | 25% cashback on Airtel bills, 10% on Swiggy/Zomato |
| 20 | **IndianOil Axis Bank** | Axis Bank | Fuel | RuPay | ₹500 | 4% value back on fuel via RuPay UPI |
| 21 | **IDFC FIRST Wealth** | IDFC FIRST | Premium | Visa | Lifetime Free | Zero fee, low 1.5% forex markup, domestic + intl lounge |
| 22 | **IDFC FIRST Select** | IDFC FIRST | Rewards | Visa | Lifetime Free | Zero fee, 10X rewards above ₹20k spend, 4 lounges/qtr |
| 23 | **IDFC FIRST Classic** | IDFC FIRST | Entry-Level | Visa | Lifetime Free | Zero fee, rewards that never expire, interest rate from 9% |
| 24 | **American Express Platinum Travel** | Amex | Travel | Amex | ₹5,000 | ₹32,000+ flight/hotel vouchers on ₹4 Lakhs spend |
| 25 | **American Express SmartEarn** | Amex | Cashback | Amex | ₹495 | 10X points on Flipkart, Uber, Amazon |
| 26 | **HSBC Cashback Credit Card** | HSBC | Dining / Groceries | Visa | ₹999 | 10% cashback on dining, groceries, and food delivery |
| 27 | **AU LIT Credit Card** | AU Small Finance | Customizable | Visa | Lifetime Free | Customizable feature-based billing, 5% cashback booster |
| 28 | **AU Zenith+ Metal** | AU Small Finance | Super-Premium | Visa | ₹4,999 | Low 0.99% forex markup, 16 domestic + 16 intl lounges |
| 29 | **IndusInd Legend** | IndusInd Bank | Rewards | Visa | Lifetime Free | BOGO BookMyShow tickets 3x/mo, 1 lounge/quarter |
| 30 | **Standard Chartered EaseMyTrip** | Standard Chartered | Travel | Visa | ₹350 | Flat 20% off hotels & 10% off flights on EaseMyTrip |

---

## 5. Getting Started & Running Locally

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.

### Option A: Running with SQLite (Default - Ready Out of the Box)
The application defaults to SQLite (`cardcompass.db`) with zero setup required:

```bash
# 1. Clone repository
git clone https://github.com/your-username/CardCompass.git
cd CardCompass

# 2. Run the application
cd CardCompass.Web
dotnet run --launch-profile "http"
```

The application will start on: **`http://localhost:5079`**  
On first run, the database is automatically created and seeded with:
* All 10 issuing banks
* All 30 credit cards with reward rules and benefits
* Identity Roles (`Admin`, `User`)
* Seed Administrator account

### Default Administrator Credentials
* **Email**: `admin@cardcompass.in`
* **Password**: `CardCompass@Admin2026!`

*(Note: Change these credentials in `appsettings.json` or environment variables for production environments).*

---

### Option B: Running with Microsoft SQL Server

1. Open `CardCompass.Web/appsettings.json` and update the settings:
   ```json
   {
     "DatabaseProvider": "SqlServer",
     "ConnectionStrings": {
       "SqlServerConnection": "Server=localhost,1433;Database=CardCompassDb;User Id=sa;Password=YourSecurePassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
     }
   }
   ```
2. Or set via environment variables:
   ```bash
   export DatabaseProvider="SqlServer"
   export ConnectionStrings__SqlServerConnection="Server=localhost,1433;Database=CardCompassDb;User Id=sa;Password=YourSecurePassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```
3. Run the application:
   ```bash
   dotnet run --project CardCompass.Web
   ```

---

## 6. Running Automated Tests

The solution includes automated unit tests verifying calculations, reward caps, fee waivers, exclusions, and preference ranking:

```bash
# Run all tests
dotnet test

# Run tests with detailed test results
dotnet test --logger "console;verbosity=detailed"
```

### Verified Test Cases:
* Direct cashback calculation
* Monthly cap enforcement on high spenders
* Excluded categories (e.g. rent, wallet, fuel)
* Minimum transaction thresholds
* Reward points conversion to rupee value
* Fee waiver conditions & Net Benefit calculation
* User existing card exclusion in recommendations
* Income threshold filtering (*Likely Eligible* vs *Possible Income Mismatch*)
* Lifetime Free booster scoring
* Purchase optimizer card selection

---

## 7. Project Directory Structure

```
CardCompass/
├── CardCompass.sln                     # .NET Solution file
├── CardCompass.Web/                    # Main ASP.NET Core MVC Web Project
│   ├── Controllers/                    # MVC Controllers
│   │   ├── AccountController.cs        # Register, Login, Logout, Profile
│   │   ├── AdminController.cs          # Catalog, Bank, and Feedback CRUD
│   │   ├── CalculatorController.cs     # Rewards Calculator
│   │   ├── CardsController.cs          # Directory and Card Details
│   │   ├── CompareController.cs        # 3-Card Side-by-Side Comparison
│   │   ├── DashboardController.cs      # Customer Wallet, Ledger, Metrics
│   │   ├── HomeController.cs           # Landing, About, Contact, Privacy, Terms
│   │   ├── PurchaseOptimizerController.cs # Best Card for Purchase Advisor
│   │   └── RecommendationController.cs # Match Wizard
│   ├── Data/                           # Database & Entity Framework Core
│   │   ├── ApplicationDbContext.cs     # DbSets, Relationships, Decimal Precision
│   │   └── DbInitializer.cs            # 30-Card Verified Dataset & Admin Seeder
│   ├── Models/
│   │   ├── Entities/                   # Domain Entities & Enums
│   │   │   ├── ApplicationUser.cs      # Identity User Extension
│   │   │   ├── Bank.cs                 # Bank Entity
│   │   │   ├── CreditCard.cs           # Credit Card Entity
│   │   │   ├── RewardRule.cs           # Category Earning & Cap Rules
│   │   │   ├── CardBenefit.cs          # Perks & Monetizable Benefits
│   │   │   ├── EligibilityRule.cs      # Age & Income Criteria
│   │   │   ├── CardSource.cs           # Official Links & Verification Dates
│   │   │   ├── Expense.cs              # User Ledger Swipes
│   │   │   ├── UserCard.cs             # User Wallet Portfolio
│   │   │   └── Feedback.cs             # Inquiries & User Submissions
│   │   └── ViewModels/                 # Strongly-Typed ViewModels
│   ├── Services/                       # Business Logic & Calculation Engines
│   │   ├── CardService.cs              # Filtering, Search, Pagination
│   │   ├── RewardCalculatorService.cs  # Deterministic Mathematical Valuation
│   │   ├── RecommendationService.cs    # Transparent Matching Algorithm
│   │   ├── PurchaseOptimizerService.cs # Transaction Optimization Engine
│   │   ├── UserPortfolioService.cs     # Wallet & Ledger Management
│   │   └── FeedbackService.cs          # Inquiries Inbox
│   ├── Views/                          # Razor Views & Layouts
│   │   ├── Shared/_Layout.cshtml       # Fintech Design System Shell
│   │   ├── Home/                       # Landing Page & Legal Disclaimers
│   │   ├── Cards/                      # Directory & Card Detail Views
│   │   ├── Compare/                    # Comparison Matrix View
│   │   ├── Recommendation/             # Multi-step Questionnaire & Results
│   │   ├── Calculator/                 # Interactive Rewards Calculator
│   │   ├── PurchaseOptimizer/          # Swiping Advisor View
│   │   ├── Dashboard/                  # Customer Dashboard, Wallet & Ledger
│   │   ├── Admin/                      # Management Console & Inquiries
│   │   └── Account/                    # Authentication Views
│   ├── wwwroot/                        # Static Assets (CSS, JS, Favicon)
│   │   ├── css/site.css                # CardCompass Design System (Navy/Teal)
│   │   └── js/site.js                  # Client scripts (Comparison Tray, Toast)
│   └── Program.cs                      # Dependency Injection & Pipeline Config
└── CardCompass.Tests/                  # Automated xUnit Test Suite
    ├── RewardCalculatorServiceTests.cs
    ├── RecommendationServiceTests.cs
    └── PurchaseOptimizerServiceTests.cs
```

---

## 8. Production Deployment

### Azure App Service / Linux Container
To deploy as a Docker container or Azure App Service:

```bash
# Publish release build
dotnet publish CardCompass.Web/CardCompass.Web.csproj -c Release -o ./publish
```

### Environment Variables Template
Set the following environment variables in production:
```bash
ASPNETCORE_ENVIRONMENT=Production
DatabaseProvider=SqlServer
ConnectionStrings__SqlServerConnection="Server=tcp:your-server.database.windows.net,1433;Database=CardCompassDb;User Id=your-admin;Password=YourStrongPassword!;Encrypt=True;"
AdminSettings__Email="admin@cardcompass.in"
AdminSettings__Password="YourStrongProductionPassword!"
AdminSettings__FullName="CardCompass Production Administrator"
```

---

## 9. Regulatory & Financial Disclaimer

CardCompass is an independent financial technology comparison and recommendation platform. We are not a scheduled commercial bank, non-banking financial company (NBFC), or credit bureau. CardCompass does not issue credit cards or make underwriting decisions.

All credit approvals, credit limits, interest rates (APR), reward terms, and fee waivers are determined exclusively by the respective issuing banks in accordance with Reserve Bank of India (RBI) guidelines and their internal credit policies. Financial calculations and reward estimates are provided for informational and comparative purposes only.

---

## 10. License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
