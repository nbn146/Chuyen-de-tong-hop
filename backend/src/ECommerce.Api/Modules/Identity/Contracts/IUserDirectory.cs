namespace ECommerce.Api.Modules.Identity.Contracts;

public sealed record UserSummary(
    int UserId,
    string Email,
    string FullName,
    string? Phone,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    int OrderCount);

public sealed record UserPage(IReadOnlyList<UserSummary> Items, int TotalCount);

/// <summary>Public contract of the Identity module for browsing users (used by administration).</summary>
public interface IUserDirectory
{
    Task<UserPage> GetPageAsync(int pageNumber, int pageSize, string? keyword, CancellationToken cancellationToken = default);
}
