namespace Alhadis.Models;

public interface ITenantContext
{
    int TenantId { get; set; }
}

public class TenantContext : ITenantContext
{
    public int TenantId { get; set; } = 1;
}
