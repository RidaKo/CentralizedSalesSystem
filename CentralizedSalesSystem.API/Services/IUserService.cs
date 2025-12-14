using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Users;

namespace CentralizedSalesSystem.API.Services;

public interface IUserService
{
    Task<object> GetAllAsync(int page, int limit, string? sortBy, string? sortDirection,
        string? filterByName, string? filterByPhone, string? filterByEmail, Status? filterByActivity, long? filterByBusinessId);

    Task<UserResponseDto?> GetByIdAsync(long id);
    Task<UserResponseDto> CreateAsync(UserCreateDto dto);
    Task<UserResponseDto?> PatchAsync(long id, UserPatchDto dto);
    Task<bool> DeleteAsync(long id);
}
