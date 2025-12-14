using CentralizedSalesSystem.API.Models.Auth.enums;

namespace CentralizedSalesSystem.API.Models.Users;

public class UserCreateDto
{
    public long BusinessId { get; set; }
    public string? Name { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Status Activity { get; set; } = Status.Active;
}
