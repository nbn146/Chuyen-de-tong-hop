using System.ComponentModel.DataAnnotations;

namespace ECommerce.Api.Modules.ShoppingCart;

public sealed class UpdateCartItemDto
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}
