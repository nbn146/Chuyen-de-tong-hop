namespace ECommerce.Api.Modules.Identity.Contracts;

public sealed record AddressSnapshot(
    string ReceiverName,
    string ReceiverPhone,
    string? Province,
    string? District,
    string? Ward,
    string FullAddress);

/// <summary>Public contract of the Identity module for reading saved addresses.</summary>
public interface IAddressLookup
{
    /// <summary>Returns the address only when it belongs to the given user.</summary>
    Task<AddressSnapshot?> FindOwnedAsync(int userId, int addressId, CancellationToken cancellationToken = default);
}
