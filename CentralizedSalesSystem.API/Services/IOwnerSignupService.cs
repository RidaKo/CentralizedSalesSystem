using CentralizedSalesSystem.API.Models.Auth.DTOs;

namespace CentralizedSalesSystem.API.Services;

public interface IOwnerSignupService
{
    Task<OwnerSignupResponse> RegisterOwnerAsync(OwnerSignupRequest request, CancellationToken cancellationToken = default);
}
