// Controllers/AdminController.cs
using Alhadis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


public class AdminController : Controller
{
    private readonly HadithDbContext _context;

    public AdminController(HadithDbContext context)
    {
        _context = context;
    }

    // Hadis Ekleme Sayfası
    public IActionResult AddHadith()
    {
        ViewBag.Years = _context.Years.ToList();
        ViewBag.Months = _context.Months.ToList();
        ViewBag.Weeks = _context.Weeks.ToList();
        ViewBag.Languages = _context.Languages.ToList();
        return View();
    }

    [HttpPost]
    [HttpPost]
    public IActionResult AddHadith(int yearNumber, string monthName, int weekNumber, string turkishContent, string arabicContent, string ottomanContent)
    {
        // Yıl kontrolü ve ekleme
        var year = _context.Years.FirstOrDefault(y => y.YearNumber == yearNumber);
        if (year == null)
        {
            year = new Year { YearNumber = yearNumber };
            _context.Years.Add(year);
            _context.SaveChanges();
        }

        // Ay kontrolü ve ekleme
        var month = _context.Months.FirstOrDefault(m => m.MonthName == monthName && m.YearId == year.Id);
        if (month == null)
        {
            month = new Month { MonthName = monthName, YearId = year.Id };
            _context.Months.Add(month);
            _context.SaveChanges();
        }

        // Hafta kontrolü ve ekleme
        var week = _context.Weeks.FirstOrDefault(w => w.WeekNumber == weekNumber && w.MonthId == month.Id);
        if (week == null)
        {
            week = new Week { WeekNumber = weekNumber, MonthId = month.Id };
            _context.Weeks.Add(week);
            _context.SaveChanges();
        }

        // Kontrol: İlgili hafta için zaten hadis eklenmiş mi?
        if (_context.Hadiths.Any(h => h.WeekId == week.Id))
        {
            ModelState.AddModelError("", "Bir hadis bir kere eklenebilir, ikinci olamaz.");
            // Formda kullanılmak üzere ViewBag'leri yeniden dolduruyoruz.
            ViewBag.Years = _context.Years.ToList();
            ViewBag.Months = _context.Months.ToList();
            ViewBag.Weeks = _context.Weeks.ToList();
            ViewBag.Languages = _context.Languages.ToList();
            return View();
        }

        // Hadis ekleme (3 dilde)
        _context.Hadiths.AddRange(
            new Hadith { Content = turkishContent, WeekId = week.Id, LanguageId = 1 },
            new Hadith { Content = arabicContent, WeekId = week.Id, LanguageId = 2 },
            new Hadith { Content = ottomanContent, WeekId = week.Id, LanguageId = 3 }
        );
        _context.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    public IActionResult ListHadiths()
    {
        // Hadisleri ilgili ilişkileriyle birlikte çekiyoruz.
        var hadiths = _context.Hadiths
            .Include(h => h.Week)
                .ThenInclude(w => w.Month)
                    .ThenInclude(m => m.Year)
            .Include(h => h.Language)
            .OrderBy(h => h.Week.Month.Year.YearNumber)
            .ThenBy(h => h.Week.Month.MonthName)
            .ThenBy(h => h.Week.WeekNumber)
            .ToList();

        return View(hadiths);
    }

    // GET: EditHadith
    public IActionResult EditHadith(int id)
    {
        var hadith = _context.Hadiths
            .Include(h => h.Week)
                .ThenInclude(w => w.Month)
                    .ThenInclude(m => m.Year)
            .Include(h => h.Language)
            .FirstOrDefault(h => h.Id == id);

        if (hadith == null)
        {
            return NotFound();
        }
        return View(hadith);
    }

    // POST: EditHadith
    [HttpPost]
    
    public IActionResult EditHadith(Hadith model)
    {
        // Veritabanından mevcut hadisi getiriyoruz
        var existingHadith = _context.Hadiths.Find(model.Id);
        if (existingHadith == null)
        {
            return NotFound();
        }

        // Sadece güncelleme yapılacak alanı değiştiriyoruz
        existingHadith.Content = model.Content;

        _context.SaveChanges();
        return RedirectToAction("ListHadiths");
    }


    // GET: DeleteHadith
    public IActionResult DeleteHadith(int id)
    {
        var hadith = _context.Hadiths
            .Include(h => h.Week)
                .ThenInclude(w => w.Month)
                    .ThenInclude(m => m.Year)
            .Include(h => h.Language)
            .FirstOrDefault(h => h.Id == id);

        if (hadith == null)
        {
            return NotFound();
        }
        return View(hadith);
    }

    // POST: DeleteHadith
    [HttpPost, ActionName("DeleteHadith")]
    public IActionResult DeleteConfirmed(int id)
    {
        var hadith = _context.Hadiths.Find(id);
        if (hadith == null)
        {
            return NotFound();
        }
        _context.Hadiths.Remove(hadith);
        _context.SaveChanges();
        return RedirectToAction("ListHadiths");
    }


    // Diğer CRUD işlemleri (güncelleme, silme) buraya eklenebilir
}