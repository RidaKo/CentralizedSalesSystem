using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;
namespace CentralizedSalesSystem.API.Models.Reservations

{
    public class ReservationResponseDto
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerNote { get; set; }
        public DateTimeOffset AppointmentTime { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Scheduled;
        public List<OrderItemResponseDto>? Items { get; set; }
        public long? AssignedEmployee { get; set; }
        public int GuestNumber { get; set; }
        public long? TableId { get; set; }
    }
}
