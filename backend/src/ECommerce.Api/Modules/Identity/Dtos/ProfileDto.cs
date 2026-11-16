namespace ECommerce.Api.Modules.Identity;

public sealed record ProfileDto(
    int UserID,
    string Email,
    string FullName,
    string? Phone,
    string? AvatarURL);
