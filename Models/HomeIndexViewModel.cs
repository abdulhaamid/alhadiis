using System.Collections.Generic;

namespace Alhadis.Models
{
    public class HomeIndexViewModel
    {
        public Hadith? Hadith { get; set; }
        public IReadOnlyList<Year> Years { get; set; } = [];
        public IReadOnlyList<Month> Months { get; set; } = [];
        public IReadOnlyList<Week> Weeks { get; set; } = [];
        public IReadOnlyList<Language> Languages { get; set; } = [];
        public int SelectedYear { get; set; }
        public int SelectedMonthId { get; set; }
        public int SelectedWeekNumber { get; set; }
        public int SelectedLanguageId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
