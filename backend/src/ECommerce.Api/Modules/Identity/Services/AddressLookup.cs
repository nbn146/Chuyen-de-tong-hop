using ECommerce.Api.Data;
using ECommerce.Api.Modules.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Modules.Identity;

public sealed class AddressLookup(AppDbContext context) : IAddressLookup
{
    public Task<AddressSnapshot?> FindOwnedAsync(int userId, int addressId, CancellationToken cancellationToken = default) =>
        context.Addresses
            .AsNoTracking()
            .Where(x => x.AddressID == addressId && x.UserID == userId)
            .Select(x => new AddressSnapshot(x.ReceiverName, x.ReceiverPhone, x.Province, x.District, x.Ward, x.FullAddress))
            .SingleOrDefaultAsync(cancellationToken);
}
