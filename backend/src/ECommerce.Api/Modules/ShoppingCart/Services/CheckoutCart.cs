using ECommerce.Api.Data;
using ECommerce.Api.Modules.ShoppingCart.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Modules.ShoppingCart;

public sealed class CheckoutCart(AppDbContext context) : ICheckoutCart
{
    public async Task<IReadOnlyList<CheckoutLine>?> GetLinesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cart = await context.Carts
            .AsNoTracking()
            .Where(x => x.UserID == userId)
            .Select(x => new
            {
                Lines = x.Items
                    .OrderBy(i => i.CartItemID)
                    .Select(i => new CheckoutLine(
                        i.ProductID,
                        i.Product.ProductName,
                        i.Product.SKU,
                        i.Product.Price,
                        i.Quantity,
                        i.Product.StockQuantity,
                        i.Product.IsActive))
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return cart?.Lines;
    }

    public async Task ClearAsync(int userId, CancellationToken cancellationToken = default)
    {
        var items = await context.CartItems
            .Where(x => x.Cart.UserID == userId)
            .ToListAsync(cancellationToken);
        context.CartItems.RemoveRange(items);
    }
}
