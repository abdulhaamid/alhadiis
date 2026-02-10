using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models
{
    public class AdminAddHadithInputModel
    {
        [Range(2000, 2100, ErrorMessage = "Yıl 2000-2100 arasında olmalıdır.")]
        public int YearNumber { get; set; }

        [Required(ErrorMessage = "Ay adı zorunludur.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Ay adı 2-30 karakter olmalıdır.")]
        public string MonthName { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Hafta değeri 1-4 arasında olmalıdır.")]
        public int WeekNumber { get; set; }

        [Required(ErrorMessage = "Türkçe içerik zorunludur.")]
        [MinLength(10, ErrorMessage = "Türkçe içerik en az 10 karakter olmalıdır.")]
        public string TurkishContent { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arapça içerik zorunludur.")]
        [MinLength(10, ErrorMessage = "Arapça içerik en az 10 karakter olmalıdır.")]
        public string ArabicContent { get; set; } = string.Empty;

        [Required(ErrorMessage = "Oromiçe içerik zorunludur.")]
        [MinLength(10, ErrorMessage = "Oromiçe içerik en az 10 karakter olmalıdır.")]
        public string OromicContent { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amhariçe içerik zorunludur.")]
        [MinLength(10, ErrorMessage = "Amhariçe içerik en az 10 karakter olmalıdır.")]
        public string AmharicContent { get; set; } = string.Empty;
    }
}
