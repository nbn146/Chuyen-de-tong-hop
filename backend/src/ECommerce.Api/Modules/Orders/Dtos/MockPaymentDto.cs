namespace ECommerce.Api.Modules.Orders;

public sealed record MockPaymentDto(
    long OrderID,
    decimal Amount,
    string Currency,
    string TransferContent,
    string BankName,
    string AccountNumber,
    string AccountName,
    string PaymentStatus);
