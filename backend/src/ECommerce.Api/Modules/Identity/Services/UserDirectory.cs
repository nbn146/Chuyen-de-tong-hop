using ECommerce.Api.Data;
using ECommerce.Api.Modules.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Modules.Identity;

public sealed class UserDirectory(AppDbContext context) : IUserDirectory
{
    public async Task<UserPage> GetPageAsync(
        int pageNumber, int pageSize, string? keyword, CancellationToken cancellationToken = default)
    {
        var query = context.Users.Include(u => u.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(kw) || u.Email.ToLower().Contains(kw));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserSummary(
                u.UserID, u.Email, u.FullName, u.Phone, u.Role.RoleName, u.IsActive, u.CreatedAt, u.Orders.Count))
            .ToListAsync(cancellationToken);

        return new UserPage(users, totalCount);
    }
}
