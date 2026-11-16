using ECommerce.Api.Data;
using ECommerce.Api.Modules.Inventory.Contracts;
using ECommerce.Api.Modules.Orders.Contracts;
using ECommerce.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Modules.Orders;

public sealed class OrderAdministration(AppDbContext context, IStockLedger stockLedger) : IOrderAdministration
{
    private static readonly string[] ValidOrderStatuses = ["PENDING", "CONFIRMED", "SHIPPING", "COMPLETED", "CANCELLED"];

    public async Task<PagedAdminOrderResult> GetOrdersAsync(
        int pageNumber, int pageSize, string? status, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
            throw new DomainValidationException();
        if (!string.IsNullOrWhiteSpace(status) && !ValidOrderStatuses.Contains(status))
            throw new DomainValidationException();

        var query = context.Orders.Include(o => o.User).Include(o => o.OrderDetails).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(o => o.OrderStatus == status);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedAdminOrderResult
        {
            Items = orders.Select(MapOrder).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminOrderDto> GetOrderAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders.Include(o => o.User).Include(o => o.OrderDetails)
            .SingleOrDefaultAsync(o => o.OrderID == orderId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return MapOrder(order);
    }

    public async Task<AdminOrderDto> UpdateOrderStatusAsync(
        long orderId, int adminUserId, string newStatus, string? note, CancellationToken cancellationToken = default)
    {
        if (!ValidOrderStatuses.Contains(newStatus)) throw new DomainValidationException();

        var order = await context.Orders.Include(o => o.User).Include(o => o.OrderDetails)
            .SingleOrDefaultAsync(o => o.OrderID == orderId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        if (order.OrderStatus == newStatus ||
            order.OrderStatus is "COMPLETED" or "CANCELLED")
            throw new DomainConflictException();

        var oldStatus = order.OrderStatus;
        order.OrderStatus = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        if (newStatus == "CONFIRMED") order.ConfirmedAt = DateTime.UtcNow;
        if (newStatus == "COMPLETED")
        {
            order.CompletedAt = DateTime.UtcNow;
            if (order.PaymentMethod == OrderConstants.Cod)
                order.PaymentStatus = OrderConstants.Paid;
        }
        if (newStatus == "CANCELLED")
        {
            order.CancelledAt = DateTime.UtcNow;
            await stockLedger.RestoreAsync(
                order.OrderDetails.Select(d => new StockRestoreLine(d.ProductID, d.Quantity)).ToList(),
                "CANCEL_ORDER",
                $"Admin cancel order #{order.OrderID}",
                adminUserId,
                cancellationToken);
        }

        order.StatusHistories.Add(new OrderStatusHistory
        {
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Note = note,
            ChangedBy = adminUserId,
            ChangedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);
        return MapOrder(order);
    }

    private static AdminOrderDto MapOrder(Order order) => new()
    {
        OrderId = order.OrderID,
        UserId = order.UserID,
        CustomerName = order.User?.FullName ?? string.Empty,
        CustomerEmail = order.User?.Email ?? string.Empty,
        ReceiverName = order.ReceiverName,
        ReceiverPhone = order.ReceiverPhone,
        ShippingAddress = order.ShippingAddress,
        TotalAmount = order.TotalAmount,
        PaymentMethod = order.PaymentMethod,
        PaymentStatus = order.PaymentStatus,
        OrderStatus = order.OrderStatus,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
        Items = order.OrderDetails.Select(item => new AdminOrderDetailDto
        {
            OrderDetailId = item.OrderDetailID,
            ProductId = item.ProductID,
            ProductName = item.ProductName,
            Sku = item.SKU,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice
        }).ToList()
    };
}
