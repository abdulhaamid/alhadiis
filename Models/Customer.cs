using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models;

public class Customer : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(250)]
    public string? Email { get; set; }
}
