using Alhadis.Models;
using Alhadis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Alhadis.Controllers;

public class StockController : Controller
{
    private readonly HadithDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IStockService _stockService;

    public StockController(HadithDbContext context, ITenantContext tenantContext, IStockService stockService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _stockService = stockService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Stock";

        var products = await _context.Products.AsNoTracking().ToListAsync();
        var customers = await _context.Customers.AsNoTracking().ToListAsync();
        var orders = await _context.SalesOrders.AsNoTracking().ToListAsync();

        ViewBag.TenantId = _tenantContext.TenantId;
        ViewBag.ProductCount = products.Count;
        ViewBag.CustomerCount = customers.Count;
        ViewBag.OrderCount = orders.Count;
        ViewBag.StockValue = products.Sum(x => x.StockQuantity * x.UnitPrice);
        ViewBag.LowStockProducts = products.Where(x => x.StockQuantity < 10).OrderBy(x => x.StockQuantity).Take(5).ToList();

        return View();
    }

    public async Task<IActionResult> Products()
    {
        ViewData["ActivePage"] = "Stock";
        var products = await _context.Products.OrderBy(x => x.Name).AsNoTracking().ToListAsync();
        return View(products);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(Product product, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["StockError"] = "Geçersiz ürün bilgisi.";
            return RedirectToAction(nameof(Products));
        }

        var result = await _stockService.AddProductAsync(product, cancellationToken);
        if (!result.IsSuccess)
        {
            TempData["StockError"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> Customers()
    {
        ViewData["ActivePage"] = "Stock";
        var customers = await _context.Customers.OrderBy(x => x.Name).AsNoTracking().ToListAsync();
        return View(customers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCustomer(Customer customer, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["StockError"] = "Geçersiz müşteri bilgisi.";
            return RedirectToAction(nameof(Customers));
        }

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Customers));
    }

    public async Task<IActionResult> Orders()
    {
        ViewData["ActivePage"] = "Stock";

        ViewBag.Customers = await _context.Customers.OrderBy(x => x.Name).AsNoTracking().ToListAsync();
        ViewBag.Products = await _context.Products.OrderBy(x => x.Name).AsNoTracking().ToListAsync();

        var orders = await _context.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(CreateOrderViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["StockError"] = "Geçersiz sipariş bilgisi.";
            return RedirectToAction(nameof(Orders));
        }

        var result = await _stockService.CreateOrderAsync(model, cancellationToken);
        if (!result.IsSuccess)
        {
            TempData["StockError"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Orders));
    }
}
