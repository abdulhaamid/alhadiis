using Alhadis.Models;

namespace Alhadis.Services;

public interface IStockService
{
    Task<(bool IsSuccess, string? ErrorMessage)> AddProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> CreateOrderAsync(CreateOrderViewModel model, CancellationToken cancellationToken = default);
}
