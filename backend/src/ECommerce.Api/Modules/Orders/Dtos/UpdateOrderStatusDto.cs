using System.ComponentModel.DataAnnotations;

namespace ECommerce.Api.Modules.Orders;

public class UpdateOrderStatusDto
{
    [Required]
    public string NewStatus { get; set; } = null!;

    public string? Note { get; set; }
}
