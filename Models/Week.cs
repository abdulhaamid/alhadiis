using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models
{
    public class Week
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; } // 1-4
        public int MonthId { get; set; }
        public Month Month { get; set; }
        public List<Hadith> Hadiths { get; set; } = new List<Hadith>();
    }
}
