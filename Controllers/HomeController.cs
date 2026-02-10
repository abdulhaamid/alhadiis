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

        public async Task<IActionResult> Index(int? year, int? month, int? week, int languageId = 2)
        {
            ViewData["ActivePage"] = "Home";
            var viewModel = await BuildHomeViewModelAsync(year, month, week, languageId);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetHadithData(int? year, int? month, int? week, int languageId = 2)
        {
            var viewModel = await BuildHomeViewModelAsync(year, month, week, languageId);

            return Json(new
            {
                selectedYear = viewModel.SelectedYear,
                selectedMonthId = viewModel.SelectedMonthId,
                selectedWeekNumber = viewModel.SelectedWeekNumber,
                selectedLanguageId = viewModel.SelectedLanguageId,
                errorMessage = viewModel.ErrorMessage,
                hadith = viewModel.Hadith is null ? null : new
                {
                    content = viewModel.Hadith.Content,
                    reference = $"{viewModel.Hadith.Week.Month.Year.YearNumber} - {viewModel.Hadith.Week.Month.MonthName} - {viewModel.Hadith.Week.WeekNumber}. HAFTA"
                },
                weeks = viewModel.Weeks.Select(w => new { weekNumber = w.WeekNumber }),
                months = viewModel.Months.Select(m => new { id = m.Id, name = m.MonthName }),
                years = viewModel.Years.Select(y => new { yearNumber = y.YearNumber }),
                languages = viewModel.Languages.Select(l => new { id = l.Id, name = l.Name })
            });
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

        public async Task<IActionResult> Month(int id, int languageId = 2)
        {
            ViewData["ActivePage"] = "Archive";

            var month = await _context.Months
                .Include(m => m.Year)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (month is null)
            {
                return NotFound();
            }

            var firstWeek = await _context.Weeks
                .Where(w => w.MonthId == id)
                .OrderBy(w => w.WeekNumber)
                .Select(w => w.WeekNumber)
                .FirstOrDefaultAsync();

            var vm = await BuildHomeViewModelAsync(month.Year.YearNumber, id, firstWeek, languageId);
            return View("Index", vm);
        }

        private async Task<HomeIndexViewModel> BuildHomeViewModelAsync(int? year, int? monthId, int? weekNumber, int languageId)
        {
            var viewModel = new HomeIndexViewModel();

            try
            {
                var years = await _context.Years
                    .Where(y => _context.Months.Any(m => m.YearId == y.Id))
                    .OrderByDescending(y => y.YearNumber)
                    .ToListAsync();

                if (years.Count == 0)
                {
                    viewModel.ErrorMessage = "Sistemde henüz hadis verisi bulunmuyor.";
                    viewModel.Languages = await _context.Languages.OrderBy(l => l.Id).ToListAsync();
                    viewModel.SelectedLanguageId = languageId;
                    return viewModel;
                }

                var selectedYear = year ?? DateTime.UtcNow.Year;
                if (!years.Any(y => y.YearNumber == selectedYear))
                {
                    selectedYear = years[0].YearNumber;
                }

                var months = await _context.Months
                    .Where(m => m.Year.YearNumber == selectedYear)
                    .Include(m => m.Year)
                    .ToListAsync();

                var orderedMonths = months
                    .OrderBy(m => GetMonthNumber(m.MonthName))
                    .ToList();

                var selectedMonthId = monthId.HasValue && orderedMonths.Any(m => m.Id == monthId.Value)
                    ? monthId.Value
                    : SelectCurrentMonthId(orderedMonths);

                var weeks = await _context.Weeks
                    .Where(w => w.MonthId == selectedMonthId)
                    .OrderBy(w => w.WeekNumber)
                    .ToListAsync();

                var selectedWeekNumber = SelectWeekNumber(weeks, weekNumber);

                var hadith = await _context.Hadiths
                    .Include(h => h.Week)
                        .ThenInclude(w => w.Month)
                            .ThenInclude(m => m.Year)
                    .Include(h => h.Language)
                    .FirstOrDefaultAsync(h =>
                        h.LanguageId == languageId &&
                        h.Week.MonthId == selectedMonthId &&
                        h.Week.WeekNumber == selectedWeekNumber &&
                        h.Week.Month.Year.YearNumber == selectedYear);

                viewModel.Years = years;
                viewModel.Months = orderedMonths;
                viewModel.Weeks = weeks;
                viewModel.Languages = await _context.Languages.OrderBy(l => l.Id).ToListAsync();
                viewModel.Hadith = hadith;
                viewModel.SelectedYear = selectedYear;
                viewModel.SelectedMonthId = selectedMonthId;
                viewModel.SelectedWeekNumber = selectedWeekNumber;
                viewModel.SelectedLanguageId = languageId;
                return viewModel;
            }
            catch
            {
                viewModel.ErrorMessage = "İçerik yüklenirken bir hata oluştu. Lütfen tekrar deneyin.";
                viewModel.Languages = await _context.Languages.OrderBy(l => l.Id).ToListAsync();
                viewModel.SelectedLanguageId = languageId;
                return viewModel;
            }
        }

        private int SelectCurrentMonthId(List<Month> orderedMonths)
        {
            if (orderedMonths.Count == 0)
            {
                return 0;
            }

            var currentMonth = DateTime.UtcNow.Month;
            return orderedMonths.FirstOrDefault(m => GetMonthNumber(m.MonthName) == currentMonth)?.Id
                   ?? orderedMonths[0].Id;
        }

        private static int SelectWeekNumber(List<Week> weeks, int? requestedWeek)
        {
            if (weeks.Count == 0)
            {
                return 1;
            }

            if (requestedWeek.HasValue && weeks.Any(w => w.WeekNumber == requestedWeek.Value))
            {
                return requestedWeek.Value;
            }

            var currentWeek = Math.Min((DateTime.UtcNow.Day - 1) / 7 + 1, 4);
            return weeks.Any(w => w.WeekNumber == currentWeek)
                ? currentWeek
                : weeks[0].WeekNumber;
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
