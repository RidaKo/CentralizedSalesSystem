namespace CentralizedSalesSystem.API.Models.Auth
{
    public class UserRole
    {
        public long Id { get; set; }
        public long UserId{get; set;} 

        public long RoleId{get; set;}
        public DateTimeOffset AssignedAt {get; set;}

        public required User User { get; set; }
        public required Role Role { get; set; }
    }
}
