using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models
{
    public class Hadith
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int LanguageId { get; set; }
        public Language Language { get; set; }
        public int WeekId { get; set; }
        public Week Week { get; set; }
    }
}
