using CentralizedSalesSystem.API.Models.Auth.enums;

namespace CentralizedSalesSystem.API.Models.Users;

public class UserResponseDto
{
    public long Id { get; set; }
    public long BusinessId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Status Activity { get; set; }
}
