using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models;

public class Tenant
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Slug { get; set; } = string.Empty;
}
