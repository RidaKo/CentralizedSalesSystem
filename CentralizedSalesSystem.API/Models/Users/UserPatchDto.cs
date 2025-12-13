using CentralizedSalesSystem.API.Models.Auth.enums;

namespace CentralizedSalesSystem.API.Models.Users;

public class UserPatchDto
{
    public long? BusinessId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Phone { get; set; }
    public Status? Activity { get; set; }
}
