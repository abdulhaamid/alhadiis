using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models
{
    public class Language
    {
        public int Id { get; set; }
        public string Name { get; set; } // Türkçe, Arapça, Osmanlıca
        public List<Hadith> Hadiths { get; set; } = new List<Hadith>();
    }
}
