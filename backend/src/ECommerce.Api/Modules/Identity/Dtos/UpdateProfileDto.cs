using System.ComponentModel.DataAnnotations;

namespace ECommerce.Api.Modules.Identity;

public sealed class UpdateProfileDto
{
    [Required, StringLength(100)]
    public string FullName { get; init; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; init; }

    [StringLength(500)]
    public string? AvatarURL { get; init; }
}
