using ECommerce.Api.Modules.Identity.Contracts;
using ECommerce.Api.Modules.Orders.Contracts;
using ECommerce.Api.Shared.Exceptions;

namespace ECommerce.Api.Modules.Admin;

/// <summary>
/// Back-office facade. It owns no tables: users come from Identity and orders from Orders,
/// both through their public contracts.
/// </summary>
public class AdminService(IUserDirectory users, IOrderAdministration orders) : IAdminService
{
    public async Task<PagedUserResult> GetUsersAsync(
        int pageNumber, int pageSize, string? keyword, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
            throw new DomainValidationException();

        var page = await users.GetPageAsync(pageNumber, pageSize, keyword, cancellationToken);

        return new PagedUserResult
        {
            Items = page.Items.Select(u => new UserListDto
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                OrderCount = u.OrderCount
            }).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public Task<PagedAdminOrderResult> GetOrdersAsync(
        int pageNumber, int pageSize, string? status, CancellationToken cancellationToken = default) =>
        orders.GetOrdersAsync(pageNumber, pageSize, status, cancellationToken);

    public Task<AdminOrderDto> GetOrderAsync(long orderId, CancellationToken cancellationToken = default) =>
        orders.GetOrderAsync(orderId, cancellationToken);

    public Task<AdminOrderDto> UpdateOrderStatusAsync(
        long orderId, int adminUserId, string newStatus, string? note, CancellationToken cancellationToken = default) =>
        orders.UpdateOrderStatusAsync(orderId, adminUserId, newStatus, note, cancellationToken);
}
