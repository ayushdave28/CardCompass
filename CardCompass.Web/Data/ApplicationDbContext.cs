using CardCompass.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CardCompass.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bank> Banks => Set<Bank>();
        public DbSet<CreditCard> CreditCards => Set<CreditCard>();
        public DbSet<RewardRule> RewardRules => Set<RewardRule>();
        public DbSet<CardBenefit> CardBenefits => Set<CardBenefit>();
        public DbSet<EligibilityRule> EligibilityRules => Set<EligibilityRule>();
        public DbSet<CardSource> CardSources => Set<CardSource>();
        public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();
        public DbSet<UserCard> UserCards => Set<UserCard>();
        public DbSet<RewardBalance> RewardBalances => Set<RewardBalance>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<SavedComparison> SavedComparisons => Set<SavedComparison>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Bank indexes & relationships
            builder.Entity<Bank>()
                .HasIndex(b => b.Code)
                .IsUnique();

            builder.Entity<Bank>()
                .HasMany(b => b.Cards)
                .WithOne(c => c.Bank)
                .HasForeignKey(c => c.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            // CreditCard indexes & relationships
            builder.Entity<CreditCard>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            builder.Entity<CreditCard>()
                .HasMany(c => c.RewardRules)
                .WithOne(r => r.CreditCard)
                .HasForeignKey(r => r.CreditCardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CreditCard>()
                .HasMany(c => c.Benefits)
                .WithOne(b => b.CreditCard)
                .HasForeignKey(b => b.CreditCardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CreditCard>()
                .HasOne(c => c.Eligibility)
                .WithOne(e => e.CreditCard)
                .HasForeignKey<EligibilityRule>(e => e.CreditCardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CreditCard>()
                .HasMany(c => c.Sources)
                .WithOne(s => s.CreditCard)
                .HasForeignKey(s => s.CreditCardId)
                .OnDelete(DeleteBehavior.Cascade);

            // User Favorite unique composite index
            builder.Entity<UserFavorite>()
                .HasIndex(f => new { f.UserId, f.CreditCardId })
                .IsUnique();

            builder.Entity<UserFavorite>()
                .HasOne(f => f.CreditCard)
                .WithMany(c => c.Favorites)
                .HasForeignKey(f => f.CreditCardId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserCard relationships
            builder.Entity<UserCard>()
                .HasMany(uc => uc.RewardBalances)
                .WithOne(rb => rb.UserCard)
                .HasForeignKey(rb => rb.UserCardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserCard>()
                .HasMany(uc => uc.Expenses)
                .WithOne(e => e.UserCard)
                .HasForeignKey(e => e.UserCardId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure decimal precision universally
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // RewardRule specific scale
            builder.Entity<RewardRule>()
                .Property(r => r.PointRupeeValue)
                .HasPrecision(18, 4);

            builder.Entity<CreditCard>()
                .Property(c => c.ForexMarkupPercent)
                .HasPrecision(5, 2);
        }
    }
}
