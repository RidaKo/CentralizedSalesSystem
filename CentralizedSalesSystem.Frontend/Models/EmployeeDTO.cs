namespace CentralizedSalesSystem.Frontend.Models
{
    public class EmployeeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "Worker"; // "Manager", "Worker", "Cashier"
        public string Status { get; set; } = "active"; // "active", "inactive"
    }
}