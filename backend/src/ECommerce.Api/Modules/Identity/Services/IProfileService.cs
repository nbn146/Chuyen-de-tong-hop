
namespace ECommerce.Api.Modules.Identity;

public interface IProfileService
{
    Task<ProfileDto> GetAsync(int userId, CancellationToken cancellationToken = default);
    Task<ProfileDto> UpdateAsync(int userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);
}
