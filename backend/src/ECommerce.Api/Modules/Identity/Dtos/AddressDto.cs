namespace ECommerce.Api.Modules.Identity;

public sealed record AddressDto(
    int AddressID,
    string ReceiverName,
    string ReceiverPhone,
    string? Province,
    string? District,
    string? Ward,
    string FullAddress,
    bool IsDefault);
