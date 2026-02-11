using Alhadis.Models;
using Microsoft.EntityFrameworkCore;

namespace Alhadis.Services;

public class StockService : IStockService
{
    private readonly HadithDbContext _context;

    public StockService(HadithDbContext context)
    {
        _context = context;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> AddProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Products.AnyAsync(x => x.Code == product.Code, cancellationToken);
        if (exists)
        {
            return (false, "Aynı koda sahip ürün zaten mevcut.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        if (product.StockQuantity > 0)
        {
            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                Quantity = product.StockQuantity,
                Type = StockTransactionType.In,
                Note = "İlk ürün kaydı"
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> CreateOrderAsync(CreateOrderViewModel model, CancellationToken cancellationToken = default)
    {
        var customerExists = await _context.Customers.AnyAsync(x => x.Id == model.CustomerId, cancellationToken);
        if (!customerExists)
        {
            return (false, "Müşteri bulunamadı.");
        }

        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == model.ProductId, cancellationToken);
        if (product == null)
        {
            return (false, "Ürün bulunamadı.");
        }

        if (product.StockQuantity < model.Quantity)
        {
            return (false, "Yetersiz stok.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var order = new SalesOrder
        {
            CustomerId = model.CustomerId,
            OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMddHHmmssfff}"
        };

        var line = new SalesOrderLine
        {
            ProductId = product.Id,
            Quantity = model.Quantity,
            UnitPrice = product.UnitPrice,
            LineTotal = model.Quantity * product.UnitPrice
        };

        order.TotalAmount = line.LineTotal;
        order.Lines.Add(line);

        product.StockQuantity -= model.Quantity;

        _context.SalesOrders.Add(order);
        _context.StockTransactions.Add(new StockTransaction
        {
            ProductId = product.Id,
            Type = StockTransactionType.Out,
            Quantity = model.Quantity,
            Note = $"Sipariş: {order.OrderNumber}"
        });

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (true, null);
    }
}
