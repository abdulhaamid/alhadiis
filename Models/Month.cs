using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models
{
    public class Month
    {
        public int Id { get; set; }
        public string MonthName { get; set; }
        public int YearId { get; set; }
        public Year Year { get; set; }
        public List<Week> Weeks { get; set; } = new List<Week>();
    }
}
