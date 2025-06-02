using Microsoft.EntityFrameworkCore;
// Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General namespace'i genellikle DbContext içinde doğrudan kullanılmaz.
// Eğer özel bir sebepten dolayı ekli değilse kaldırılabilir.
// using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace Alhadis.Models
{
    public class HadithDbContext : DbContext
    {
        public HadithDbContext(DbContextOptions<HadithDbContext> options)
            : base(options) { }

        public DbSet<Year> Years { get; set; }
        public DbSet<Month> Months { get; set; }
        public DbSet<Week> Weeks { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Hadith> Hadiths { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Year -> Month relationship
            modelBuilder.Entity<Month>()
                .HasOne(m => m.Year)
                .WithMany(y => y.Months)
                .HasForeignKey(m => m.YearId)
                .OnDelete(DeleteBehavior.Cascade);

            // Month -> Week relationship
            modelBuilder.Entity<Week>()
                .HasOne(w => w.Month)
                .WithMany(m => m.Weeks)
                .HasForeignKey(w => w.MonthId)
                .OnDelete(DeleteBehavior.Cascade);

            // Week -> Hadith relationship
            modelBuilder.Entity<Hadith>()
                .HasOne(h => h.Week)
                .WithMany(w => w.Hadiths)
                .HasForeignKey(h => h.WeekId)
                .OnDelete(DeleteBehavior.Cascade);

            // Language -> Hadith relationship
            modelBuilder.Entity<Hadith>()
                .HasOne(h => h.Language)
                .WithMany(l => l.Hadiths)
                .HasForeignKey(h => h.LanguageId)
                .OnDelete(DeleteBehavior.Restrict); // Önemli: Osmanlıca dilinde hadisler varsa bu kısıtlama sorun çıkarabilir.

            // Unique constraint: One hadith per week per language
            modelBuilder.Entity<Hadith>()
                .HasIndex(h => new { h.WeekId, h.LanguageId })
                .IsUnique();

            // Default languages seed data
            modelBuilder.Entity<Language>().HasData(
                new Language { Id = 1, Name = "Türkçe" },
                new Language { Id = 2, Name = "Arapça" },
                // new Language { Id = 3, Name = "Osmanlıca" }, // OSMANLICA KALDIRILDI
                new Language { Id = 3, Name = "Oromiçe" },
                new Language { Id = 4, Name = "Amhariçe" }
            );

            // --- SABİT YIL-AY-HAFTA SEED'İ ---
            // 1) Yıl verisi
            modelBuilder.Entity<Year>().HasData(
                new Year { Id = 1, YearNumber = 2025 }
            );

            // 2) Ay verileri (YearId = 1)
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

            // 3) Hafta verileri (Her ay için 4 hafta: 1-4)
            var weekId = 1;
            for (int m = 1; m <= 12; m++) // Ay Id'leri 1'den 12'ye kadar olduğu için döngü bu şekilde kalmalı
            {
                for (int w = 1; w <= 4; w++)
                {
                    modelBuilder.Entity<Week>().HasData(
                        new Week { Id = weekId, MonthId = m, WeekNumber = w }
                    );
                    weekId++;
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}