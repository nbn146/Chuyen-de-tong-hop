using ECommerce.Api.Shared;


namespace ECommerce.Api.Modules.Orders;

public interface IOrderService
{
    Task<OrderDetailDto> CheckoutAsync(int userId, CheckoutDto dto, CancellationToken ct = default);
    Task<PagedResult<OrderSummaryDto>> ListAsync(int userId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<OrderDetailDto> GetAsync(int userId, long orderId, CancellationToken ct = default);
    Task<MockPaymentDto> GetMockPaymentAsync(int userId, long orderId, CancellationToken ct = default);
    Task<MockPaymentDto> ConfirmMockPaymentAsync(int userId, long orderId, CancellationToken ct = default);
    Task<OrderDetailDto> CancelOrderAsync(int userId, long orderId, CancellationToken ct = default);
}
