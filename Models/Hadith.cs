using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models
{
    public class Hadith
    {
        public int Id { get; set; }

        [Required]
        [MinLength(10)]
        public string Content { get; set; } = string.Empty;

        public int LanguageId { get; set; }
        public Language Language { get; set; } = null!;

        public int WeekId { get; set; }
        public Week Week { get; set; } = null!;
    }
}
