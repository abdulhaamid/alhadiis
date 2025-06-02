// Controllers/HomeController.cs
using Alhadis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Alhadis.Controllers
{
    public class HomeController : Controller
    {
        private readonly HadithDbContext _context;
        private readonly CultureInfo _turkishCulture;

        public HomeController(HadithDbContext context)
        {
            _context = context;
            _turkishCulture = new CultureInfo("tr-TR");
        }

        public async Task<IActionResult> Index(int? year, int? month, int? week, int languageId = 2) // Varsayýlan Arapça (Id=2)
        {
            try
            {
                ViewData["ActivePage"] = "Home";

                // 1) Get available years
                var availableYears = await _context.Years
                    .Where(y => _context.Months.Any(m => m.YearId == y.Id))
                    .OrderByDescending(y => y.YearNumber)
                    .ToListAsync();

                if (!availableYears.Any())
                {
                    PopulateViewBags(new List<Year>(), new List<Month>(), new List<Week>(), languageId, 0, 0, 0);
                    return View(null);
                }

                // 2) Determine selected year - ÝLK AÇILIÞTA MEVCUT YIL
                int currentYear = DateTime.Now.Year;
                int selectedYear = year ?? currentYear;
                if (!availableYears.Any(y => y.YearNumber == selectedYear))
                    selectedYear = availableYears.First().YearNumber;

                // 3) Get months for selected year, ordered by month number
                var monthsInYear = await _context.Months
                    .Where(m => m.Year.YearNumber == selectedYear)
                    .Include(m => m.Year)
                    .ToListAsync();

                var orderedMonths = monthsInYear
                    .OrderBy(m => GetMonthNumber(m.MonthName))
                    .ToList();

                // 4) Determine selected month - ÝLK AÇILIÞTA MEVCUT AY
                int selectedMonthId;
                if (month.HasValue && orderedMonths.Any(m => m.Id == month.Value))
                {
                    selectedMonthId = month.Value;
                }
                else
                {
                    // Ýlk açýlýþta mevcut ayý bul
                    int currentMonth = DateTime.Now.Month;
                    var currentMonthEntity = orderedMonths
                        .FirstOrDefault(m => GetMonthNumber(m.MonthName) == currentMonth);
                    selectedMonthId = currentMonthEntity?.Id ?? orderedMonths.FirstOrDefault()?.Id ?? 0;
                }

                // 5) Get weeks for selected month
                var weeksInMonth = await _context.Weeks
                    .Where(w => w.MonthId == selectedMonthId)
                    .OrderBy(w => w.WeekNumber)
                    .ToListAsync();

                // 6) Determine selected week - ÝLK AÇILIÞTA MEVCUT HAFTA
                int selectedWeekNumber;
                if (week.HasValue && weeksInMonth.Any(w => w.WeekNumber == week.Value))
                {
                    selectedWeekNumber = week.Value;
                }
                else
                {
                    // Ýlk açýlýþta mevcut haftayý hesapla
                    int currentWeek = (DateTime.Now.Day - 1) / 7 + 1;
                    currentWeek = Math.Min(currentWeek, 4); // Maksimum 4. hafta
                    selectedWeekNumber = weeksInMonth.Any(w => w.WeekNumber == currentWeek)
                        ? currentWeek
                        : weeksInMonth.FirstOrDefault()?.WeekNumber ?? 1;
                }

                // 7) Get the hadith
                var hadith = await _context.Hadiths
                    .Include(h => h.Week)
                        .ThenInclude(w => w.Month)
                            .ThenInclude(m => m.Year)
                    .Include(h => h.Language)
                    .FirstOrDefaultAsync(h =>
                        h.LanguageId == languageId &&
                        h.Week.WeekNumber == selectedWeekNumber &&
                        h.Week.MonthId == selectedMonthId &&
                        h.Week.Month.Year.YearNumber == selectedYear);

                // 8) DROPDOWNLARI HER ZAMAN DOLDUR - hadis olsun ya da olmasýn
                PopulateViewBags(availableYears, orderedMonths, weeksInMonth, languageId, selectedYear, selectedMonthId, selectedWeekNumber);

                // Hadis yoksa null döndür ama ViewBag'ler dolu olacak
                return View(hadith);
            }
            catch (Exception ex)
            {
                // Hata durumunda bile mevcut tarih bilgileriyle dropdown'larý doldur
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;
                var currentWeek = Math.Min((DateTime.Now.Day - 1) / 7 + 1, 4);

                var years = await _context.Years.OrderByDescending(y => y.YearNumber).ToListAsync();
                var months = await _context.Months
                    .Where(m => m.Year.YearNumber == currentYear)
                    .Include(m => m.Year)
                    .OrderBy(m => GetMonthNumber(m.MonthName))
                    .ToListAsync();
                var weeks = await _context.Weeks
                    .Where(w => w.Month.Year.YearNumber == currentYear && GetMonthNumber(w.Month.MonthName) == currentMonth)
                    .OrderBy(w => w.WeekNumber)
                    .ToListAsync();

                var selectedMonthId = months.FirstOrDefault(m => GetMonthNumber(m.MonthName) == currentMonth)?.Id ?? 0;

                PopulateViewBags(years, months, weeks, languageId, currentYear, selectedMonthId, currentWeek);
                ViewData["Error"] = "Bir hata oluþtu. Lütfen daha sonra tekrar deneyin.";
                return View(null);
            }
        }

        public async Task<IActionResult> Archive()
        {
            ViewData["ActivePage"] = "Archive";

            var years = await _context.Years
                .OrderByDescending(y => y.YearNumber)
                .Select(y => y.YearNumber)
                .ToListAsync();

            return View(years);
        }

        public IActionResult Settings()
        {
            ViewData["ActivePage"] = "Settings";
            return View();
        }

        public async Task<IActionResult> MonthListesi()
        {
            ViewData["ActivePage"] = "Archive";

            var months = await _context.Months
                .Include(m => m.Year)
                .OrderBy(m => m.Year.YearNumber)
                .ThenBy(m => GetMonthNumber(m.MonthName))
                .ToListAsync();

            return View(months);
        }

        public async Task<IActionResult> Month(int id, int languageId = 2) // Varsayýlan Arapça
        {
            ViewData["ActivePage"] = "Archive";

            var month = await _context.Months
                .Include(m => m.Year)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (month == null)
                return NotFound();

            // Ýlk haftayý seç
            var selectedWeek = await _context.Weeks
                .Include(w => w.Hadiths)
                    .ThenInclude(h => h.Language)
                .Include(w => w.Month)
                .Where(w => w.MonthId == id)
                .OrderBy(w => w.WeekNumber)
                .FirstOrDefaultAsync(); // Ýlk hafta

            var hadith = selectedWeek?.Hadiths.FirstOrDefault(h => h.LanguageId == languageId);

            // Populate ViewBags for navigation
            var years = await _context.Years.OrderByDescending(y => y.YearNumber).ToListAsync();
            var monthsInYear = await _context.Months
                .Where(m => m.Year.YearNumber == month.Year.YearNumber)
                .Include(m => m.Year)
                .ToListAsync();
            var orderedMonths = monthsInYear.OrderBy(m => GetMonthNumber(m.MonthName)).ToList();
            var weeksInMonth = await _context.Weeks
                .Where(w => w.MonthId == id)
                .OrderBy(w => w.WeekNumber)
                .ToListAsync();

            PopulateViewBags(years, orderedMonths, weeksInMonth, languageId,
                month.Year.YearNumber, month.Id, selectedWeek?.WeekNumber ?? 1);

            return View("Index", hadith);
        }

        // Helper Methods
        private void PopulateViewBags(List<Year> years, List<Month> months, List<Week> weeks,
            int languageId, int selectedYear, int selectedMonthId, int selectedWeekNumber)
        {
            ViewBag.Years = years;
            ViewBag.Months = months;
            ViewBag.Weeks = weeks;
            ViewBag.Languages = _context.Languages.ToList();
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonth = selectedMonthId;
            ViewBag.SelectedWeek = selectedWeekNumber;
            ViewBag.SelectedLanguageId = languageId;
        }

        private async Task<Hadith> GetLatestHadithAsync(int languageId)
        {
            return await _context.Hadiths
                .Include(h => h.Week)
                    .ThenInclude(w => w.Month)
                        .ThenInclude(m => m.Year)
                .Include(h => h.Language)
                .Where(h => h.LanguageId == languageId)
                .OrderByDescending(h => h.Week.Month.Year.YearNumber)
                .ThenByDescending(h => GetMonthNumber(h.Week.Month.MonthName))
                .ThenByDescending(h => h.Week.WeekNumber)
                .FirstOrDefaultAsync();
        }

        private int GetMonthNumber(string monthName)
        {
            var monthNames = _turkishCulture.DateTimeFormat.MonthNames;
            var index = Array.FindIndex(monthNames, mn =>
                string.Equals(mn, monthName, StringComparison.OrdinalIgnoreCase));
            return index >= 0 ? index + 1 : int.MaxValue;
        }
    }
}