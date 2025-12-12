using CentralizedSalesSystem.API.Models.Auth;
using CentralizedSalesSystem.API.Models.Auth.enums;
using BusinessEntity = CentralizedSalesSystem.API.Models.Business.Business;

namespace CentralizedSalesSystem.API.Models
{
    public class User
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }

        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public Status Status{  get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public BusinessEntity Business { get; set; } = null!;

    }
}
