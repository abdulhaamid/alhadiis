namespace Alhadis.Models;

public class StockTransaction : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public StockTransactionType Type { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Note { get; set; } = string.Empty;
}

public enum StockTransactionType
{
    In = 1,
    Out = 2,
    Adjustment = 3
}
