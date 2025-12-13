using CentralizedSalesSystem.API.Models.Reservations;

namespace CentralizedSalesSystem.API.Models.Reservations
{
    public class ReservationCreateDto
    {
        public long BusinessId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerNote { get; set; }
        public DateTimeOffset AppointmentTime { get; set; }
        public long CreatedBy { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Scheduled;
        public long? AssignedEmployee { get; set; }
        public int GuestNumber { get; set; }
        public long? TableId { get; set; }
    }
}
