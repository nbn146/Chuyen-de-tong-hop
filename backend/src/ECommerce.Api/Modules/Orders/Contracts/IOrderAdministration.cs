namespace ECommerce.Api.Modules.Orders.Contracts;

/// <summary>
/// Public contract of the Orders module for back-office order management. The Admin module
/// delegates to this instead of querying order tables itself.
/// </summary>
public interface IOrderAdministration
{
    Task<PagedAdminOrderResult> GetOrdersAsync(
        int pageNumber, int pageSize, string? status, CancellationToken cancellationToken = default);

    Task<AdminOrderDto> GetOrderAsync(long orderId, CancellationToken cancellationToken = default);

    Task<AdminOrderDto> UpdateOrderStatusAsync(
        long orderId, int adminUserId, string newStatus, string? note, CancellationToken cancellationToken = default);
}
