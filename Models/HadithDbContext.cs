using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

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
            modelBuilder.Entity<Month>()
                .HasOne(m => m.Year)
                .WithMany(y => y.Months)
                .HasForeignKey(m => m.YearId);

            modelBuilder.Entity<Week>()
                .HasOne(w => w.Month)
                .WithMany(m => m.Weeks)
                .HasForeignKey(w => w.MonthId);

            modelBuilder.Entity<Hadith>()
                .HasOne(h => h.Week)
                .WithMany(w => w.Hadiths)
                .HasForeignKey(h => h.WeekId);

            modelBuilder.Entity<Hadith>()
                .HasOne(h => h.Language)
                .WithMany(l => l.Hadiths)
                .HasForeignKey(h => h.LanguageId);

            // Varsayılan diller
            modelBuilder.Entity<Language>().HasData(
                new Language { Id = 1, Name = "Türkçe" },
                new Language { Id = 2, Name = "Arapça" },
                new Language { Id = 3, Name = "Osmanlıca" }
            );
        }
    }
}