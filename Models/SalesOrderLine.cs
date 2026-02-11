using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models;

public class SalesOrderLine : ITenantEntity
{
    public int Id { get; set; }

    public int SalesOrderId { get; set; }

    public SalesOrder? SalesOrder { get; set; }

    public int TenantId { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Range(1, 999999)]
    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}
