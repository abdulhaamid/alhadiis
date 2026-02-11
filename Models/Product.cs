using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models;

public class Product : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    [Required, MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal UnitPrice { get; set; }

    public int StockQuantity { get; set; }
}
