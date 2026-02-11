using System.ComponentModel.DataAnnotations;

namespace Alhadis.Models;

public class CreateOrderViewModel
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(1, 999999)]
    public int Quantity { get; set; }
}
