using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models
{
    public class Year
    {
        public int Id { get; set; }
        public int YearNumber { get; set; }
        public List<Month> Months { get; set; } = new List<Month>();
    }
}
