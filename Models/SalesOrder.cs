using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models;

public class SalesOrder : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}
