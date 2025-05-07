using Alhadis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class HomeController : Controller
{
    private readonly HadithDbContext _context;

    public HomeController(HadithDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(int? year, int? month, int? week, int languageId = 1)
    {
        // 1) Yýllarý al
        var availableYears = _context.Years
            .Where(y => _context.Months.Any(m => m.YearId == y.Id))
            .OrderByDescending(y => y.YearNumber)
            .ToList();
        if (!availableYears.Any())
            return View("Error");

        // 2) Seçili yýlý belirle (parametre yoksa sistemin yýlý)
        int defaultYear = DateTime.Now.Year;
        int selectedYear = year.HasValue ? year.Value : defaultYear;
        if (!availableYears.Any(y => y.YearNumber == selectedYear))
            selectedYear = availableYears.First().YearNumber;

        // 3) O yýla ait aylarý, ay numarasýna göre sýrala
        var tr = new CultureInfo("tr-TR");
        var dtfi = tr.DateTimeFormat;
        var monthsInYear = _context.Months
            .Where(m => m.Year.YearNumber == selectedYear)
            .AsEnumerable()
            .OrderBy(m =>
            {
                // Ay adýný kültüre göre küçük harfe çevirip MonthNames dizisinde bul
                var nameLower = m.MonthName.ToLower(tr);
                var index = Array.FindIndex(dtfi.MonthNames, mn => mn.ToLower(tr) == nameLower);
                return index >= 0 ? index + 1 : int.MaxValue;
            })
            .ToList();
        if (!monthsInYear.Any())
        {
            PopulateViewBags(availableYears, monthsInYear, new List<Week>(), languageId, selectedYear, 0, 0);
            return View(null);
        }

        // 4) Seçili ayý belirle (parametre yoksa sistemin ayý / eksikse son ay)
        int selectedMonthId;
        if (month.HasValue && monthsInYear.Any(m => m.Id == month.Value))
        {
            selectedMonthId = month.Value;
        }
        else
        {
            int sysMonth = DateTime.Now.Month;
            var monthEntity = monthsInYear
                .FirstOrDefault(m =>
                {
                    var nameLower = m.MonthName.ToLower(tr);
                    return Array.FindIndex(dtfi.MonthNames, mn => mn.ToLower(tr) == nameLower) + 1 == sysMonth;
                });
            selectedMonthId = monthEntity != null
                ? monthEntity.Id
                : monthsInYear.Last().Id;
        }

        // 5) Haftalarý al
        var weeksInMonth = _context.Weeks
            .Where(w => w.MonthId == selectedMonthId)
            .OrderBy(w => w.WeekNumber)
            .ToList();
        if (!weeksInMonth.Any())
        {
            PopulateViewBags(availableYears, monthsInYear, weeksInMonth, languageId, selectedYear, selectedMonthId, 0);
            return View(null);
        }

        // 6) Seçili haftayý belirle (parametre yoksa sistemin haftasý / eksikse son hafta)
        int selectedWeekNumber;
        if (week.HasValue && weeksInMonth.Any(w => w.WeekNumber == week.Value))
        {
            selectedWeekNumber = week.Value;
        }
        else
        {
            int sysWeek = (DateTime.Now.Day - 1) / 7 + 1;
            selectedWeekNumber = weeksInMonth.Any(w => w.WeekNumber == sysWeek)
                ? sysWeek
                : weeksInMonth.Last().WeekNumber;
        }

        // 7) Ýstenen hadisi getir
        var hadith = _context.Hadiths
            .Include(h => h.Week).ThenInclude(w => w.Month).ThenInclude(m => m.Year)
            .Include(h => h.Language)
            .FirstOrDefault(h =>
                h.LanguageId == languageId &&
                h.Week.WeekNumber == selectedWeekNumber &&
                h.Week.MonthId == selectedMonthId &&
                h.Week.Month.Year.YearNumber == selectedYear
            );

        // 8) Ýlk giriþte verisi yoksa en son kaydý getir
        if (!year.HasValue && !month.HasValue && !week.HasValue && hadith == null)
        {
            hadith = _context.Hadiths
                .Include(h => h.Week).ThenInclude(w => w.Month).ThenInclude(m => m.Year)
                .Include(h => h.Language)
                .OrderByDescending(h => h.Week.Month.Year.YearNumber)
                .ThenByDescending(h =>
                    Array.FindIndex(
                        dtfi.MonthNames,
                        mn => mn.ToLower(tr) == h.Week.Month.MonthName.ToLower(tr)
                    )
                )
                .ThenByDescending(h => h.Week.WeekNumber)
                .FirstOrDefault(h => h.LanguageId == languageId);

            if (hadith != null)
            {
                selectedYear = hadith.Week.Month.Year.YearNumber;
                selectedMonthId = hadith.Week.MonthId;
                selectedWeekNumber = hadith.Week.WeekNumber;
                weeksInMonth = _context.Weeks
                    .Where(w => w.MonthId == selectedMonthId)
                    .OrderBy(w => w.WeekNumber)
                    .ToList();
            }
        }

        // 9) ViewBag'leri doldur ve View'a dön
        PopulateViewBags(availableYears, monthsInYear, weeksInMonth,
            languageId, selectedYear, selectedMonthId, selectedWeekNumber);
        return View(hadith);
    }

    // Yardýmcý metot: ViewBag doldur
    private void PopulateViewBags(
        List<Year> years,
        List<Month> months,
        List<Week> weeks,
        int languageId,
        int selYear,
        int selMonthId,
        int selWeekNumber)
    {
        ViewBag.Years = years;
        ViewBag.Months = months;
        ViewBag.Weeks = weeks;
        ViewBag.Languages = _context.Languages.ToList();
        ViewBag.SelectedYear = selYear;
        ViewBag.SelectedMonth = selMonthId;
        ViewBag.SelectedWeek = selWeekNumber;
        ViewBag.SelectedLanguageId = languageId;
    }


    public IActionResult Archive()
    {
        // Veritabanýndaki yýllarý azalan sýrada çekiyoruz.
        var years = _context.Years
            .OrderByDescending(y => y.YearNumber)
            .Select(y => y.YearNumber)
            .ToList();

        return View(years);
    }


    public IActionResult Settings()
    {
        ViewData["ActivePage"] = "Settings"; // Ayarlar sayfasý aktif
        return View();
    }

    public async Task<IActionResult> MonthListesi()
    {
        // Aylarý veritabanýndan ID'ye göre (sýralý) çekiyoruz.
        var months = await _context.Months
                                   .OrderBy(m => m.Id)
                                   .ToListAsync();
        return View(months);
    }

    public IActionResult Month(int id, int languageId = 1)
    {
        // ID ile ayý buluyoruz.
        var month = _context.Months
                    .Include(m => m.Year)
                    .FirstOrDefault(m => m.Id == id);

        if (month == null)
        {
            return NotFound();
        }

        // Örneðin, ayýn ilk haftasýný varsayýlan olarak seçelim veya baþka bir kritere göre haftayý belirleyin
        var selectedWeek = _context.Weeks
            .Include(w => w.Hadiths)
            .ThenInclude(h => h.Language)
            .Include(w => w.Month)
            .FirstOrDefault(w => w.MonthId == id);

        // Seçilen dilde hadis alalým
        var hadith = selectedWeek?.Hadiths.FirstOrDefault(h => h.LanguageId == languageId);

        // ViewBag üzerinden navigasyon için gerekli veriler
        ViewBag.Years = _context.Years.ToList();
        ViewBag.Months = _context.Months.Where(m => m.Year.YearNumber == month.Year.YearNumber).ToList();
        ViewBag.Weeks = selectedWeek != null
            ? _context.Weeks.Where(w => w.MonthId == id).ToList()
            : new List<Week>();
        ViewBag.Languages = _context.Languages.ToList();
        ViewBag.SelectedYear = month.Year.YearNumber;
        ViewBag.SelectedMonth = month.Id;
        ViewBag.SelectedWeek = selectedWeek?.WeekNumber;
        ViewBag.SelectedLanguageId = languageId;

        // Eðer hadis bulunamazsa
        if (hadith == null)
        {
            ViewBag.Message = "Seçilen ay için hadis bulunamadý.";
        }

        return View("Index", hadith); // Index view'ini kullanýyoruz
    }




}