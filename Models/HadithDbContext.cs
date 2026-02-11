using Microsoft.EntityFrameworkCore;

namespace Alhadis.Models
{
    public class HadithDbContext : DbContext
    {
        private readonly ITenantContext _tenantContext;

        public HadithDbContext(DbContextOptions<HadithDbContext> options, ITenantContext tenantContext)
            : base(options)
        {
            _tenantContext = tenantContext;
        }

        public DbSet<Year> Years { get; set; }
        public DbSet<Month> Months { get; set; }
        public DbSet<Week> Weeks { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Hadith> Hadiths { get; set; }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        public int CurrentTenantId => _tenantContext.TenantId;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Month>()
                .HasOne(m => m.Year)
                .WithMany(y => y.Months)
                .HasForeignKey(m => m.YearId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Week>()
                .HasOne(w => w.Month)
                .WithMany(m => m.Weeks)
                .HasForeignKey(w => w.MonthId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hadith>()
                .HasOne(h => h.Week)
                .WithMany(w => w.Hadiths)
                .HasForeignKey(h => h.WeekId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hadith>()
                .HasOne(h => h.Language)
                .WithMany(l => l.Hadiths)
                .HasForeignKey(h => h.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Hadith>()
                .HasIndex(h => new { h.WeekId, h.LanguageId })
                .IsUnique();

            modelBuilder.Entity<SalesOrderLine>()
                .HasOne(x => x.SalesOrder)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesOrderLine>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.TenantId, p.Code })
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => new { c.TenantId, c.Name });

            modelBuilder.Entity<SalesOrder>()
                .HasIndex(o => new { o.TenantId, o.OrderNumber })
                .IsUnique();

            modelBuilder.Entity<StockTransaction>()
                .HasIndex(t => new { t.TenantId, t.ProductId, t.CreatedAt });

            modelBuilder.Entity<SalesOrderLine>()
                .HasIndex(t => new { t.TenantId, t.SalesOrderId, t.ProductId });

            modelBuilder.Entity<Product>().HasQueryFilter(x => x.TenantId == CurrentTenantId);
            modelBuilder.Entity<Customer>().HasQueryFilter(x => x.TenantId == CurrentTenantId);
            modelBuilder.Entity<SalesOrder>().HasQueryFilter(x => x.TenantId == CurrentTenantId);
            modelBuilder.Entity<SalesOrderLine>().HasQueryFilter(x => x.TenantId == CurrentTenantId);
            modelBuilder.Entity<StockTransaction>().HasQueryFilter(x => x.TenantId == CurrentTenantId);

            modelBuilder.Entity<Language>().HasData(
                new Language { Id = 1, Name = "Türkçe" },
                new Language { Id = 2, Name = "Arapça" },
                new Language { Id = 3, Name = "Oromiçe" },
                new Language { Id = 4, Name = "Amhariçe" }
            );

            modelBuilder.Entity<Year>().HasData(new Year { Id = 1, YearNumber = 2025 });

            modelBuilder.Entity<Month>().HasData(
                new Month { Id = 1, MonthName = "Ocak", YearId = 1 },
                new Month { Id = 2, MonthName = "Şubat", YearId = 1 },
                new Month { Id = 3, MonthName = "Mart", YearId = 1 },
                new Month { Id = 4, MonthName = "Nisan", YearId = 1 },
                new Month { Id = 5, MonthName = "Mayıs", YearId = 1 },
                new Month { Id = 6, MonthName = "Haziran", YearId = 1 },
                new Month { Id = 7, MonthName = "Temmuz", YearId = 1 },
                new Month { Id = 8, MonthName = "Ağustos", YearId = 1 },
                new Month { Id = 9, MonthName = "Eylül", YearId = 1 },
                new Month { Id = 10, MonthName = "Ekim", YearId = 1 },
                new Month { Id = 11, MonthName = "Kasım", YearId = 1 },
                new Month { Id = 12, MonthName = "Aralık", YearId = 1 }
            );

            var weekId = 1;
            for (var m = 1; m <= 12; m++)
            {
                for (var w = 1; w <= 4; w++)
                {
                    modelBuilder.Entity<Week>().HasData(new Week { Id = weekId, MonthId = m, WeekNumber = w });
                    weekId++;
                }
            }

            modelBuilder.Entity<Tenant>().HasData(new Tenant { Id = 1, Name = "Demo Tenant", Slug = "demo" });

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            ApplyTenantIds();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTenantIds();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyTenantIds()
        {
            var entries = ChangeTracker.Entries<ITenantEntity>()
                .Where(x => x.State == EntityState.Added);

            foreach (var entry in entries)
            {
                entry.Entity.TenantId = CurrentTenantId;
            }
        }
    }
}
