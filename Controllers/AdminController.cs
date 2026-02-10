using Alhadis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Alhadis.Controllers;

public class AdminController : Controller
{
    private readonly HadithDbContext _context;

    public AdminController(HadithDbContext context)
    {
        _context = context;
    }

    public IActionResult AddHadith()
    {
        ViewData["ActivePage"] = "Archive";
        return View(new AdminAddHadithInputModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddHadith(AdminAddHadithInputModel model)
    {
        ViewData["ActivePage"] = "Archive";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cleanedMonthName = model.MonthName.Trim();
        var trimmedTurkish = model.TurkishContent.Trim();
        var trimmedArabic = model.ArabicContent.Trim();
        var trimmedOromic = model.OromicContent.Trim();
        var trimmedAmharic = model.AmharicContent.Trim();

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var year = await _context.Years.FirstOrDefaultAsync(y => y.YearNumber == model.YearNumber);
        if (year is null)
        {
            year = new Year { YearNumber = model.YearNumber };
            _context.Years.Add(year);
            await _context.SaveChangesAsync();
        }

        var month = await _context.Months.FirstOrDefaultAsync(m => m.YearId == year.Id && m.MonthName == cleanedMonthName);
        if (month is null)
        {
            month = new Month { MonthName = cleanedMonthName, YearId = year.Id };
            _context.Months.Add(month);
            await _context.SaveChangesAsync();
        }

        var week = await _context.Weeks.FirstOrDefaultAsync(w => w.MonthId == month.Id && w.WeekNumber == model.WeekNumber);
        if (week is null)
        {
            week = new Week { MonthId = month.Id, WeekNumber = model.WeekNumber };
            _context.Weeks.Add(week);
            await _context.SaveChangesAsync();
        }

        var alreadyExists = await _context.Hadiths.AnyAsync(h => h.WeekId == week.Id);
        if (alreadyExists)
        {
            ModelState.AddModelError(string.Empty, "Bu yıl/ay/hafta için hadis zaten mevcut. Lütfen farklı bir kayıt seçin.");
            return View(model);
        }

        var hadiths = new List<Hadith>
        {
            new() { Content = trimmedTurkish, WeekId = week.Id, LanguageId = 1 },
            new() { Content = trimmedArabic, WeekId = week.Id, LanguageId = 2 },
            new() { Content = trimmedOromic, WeekId = week.Id, LanguageId = 3 },
            new() { Content = trimmedAmharic, WeekId = week.Id, LanguageId = 4 }
        };

        _context.Hadiths.AddRange(hadiths);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["SuccessMessage"] = "Hadis başarıyla eklendi.";
        return RedirectToAction(nameof(ListHadiths));
    }

    public async Task<IActionResult> ListHadiths()
    {
        ViewData["ActivePage"] = "Archive";

        var hadiths = await _context.Hadiths
            .Include(h => h.Week)
                .ThenInclude(w => w.Month)
                    .ThenInclude(m => m.Year)
            .Include(h => h.Language)
            .AsNoTracking()
            .OrderByDescending(h => h.Week.Month.Year.YearNumber)
            .ThenByDescending(h => h.Week.Month.Id)
            .ThenBy(h => h.Week.WeekNumber)
            .ThenBy(h => h.LanguageId)
            .ToListAsync();

        return View(hadiths);
    }

    public async Task<IActionResult> EditHadith(int id)
    {
        ViewData["ActivePage"] = "Archive";

        var hadith = await _context.Hadiths
            .Include(h => h.Week)
                .ThenInclude(w => w.Month)
                    .ThenInclude(m => m.Year)
            .Include(h => h.Language)
            .FirstOrDefaultAsync(h => h.Id == id);

        return hadith is null ? NotFound() : View(hadith);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditHadith(Hadith model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingHadith = await _context.Hadiths.FindAsync(model.Id);
        if (existingHadith is null)
        {
            return NotFound();
        }

        existingHadith.Content = model.Content.Trim();
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Hadis güncellendi.";
        return RedirectToAction(nameof(ListHadiths));
    }

    public async Task<IActionResult> DeleteHadith(int id)
    {
        ViewData["ActivePage"] = "Archive";

        var hadith = await _context.Hadiths
            .Include(h => h.Week)
                .ThenInclude(w => w.Month)
                    .ThenInclude(m => m.Year)
            .Include(h => h.Language)
            .FirstOrDefaultAsync(h => h.Id == id);

        return hadith is null ? NotFound() : View(hadith);
    }

    [HttpPost, ActionName("DeleteHadith")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var hadith = await _context.Hadiths.FindAsync(id);
        if (hadith is null)
        {
            return NotFound();
        }

        _context.Hadiths.Remove(hadith);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Hadis silindi.";
        return RedirectToAction(nameof(ListHadiths));
    }
}
