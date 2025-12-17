using System;
using System.Collections.Generic;

namespace CentralizedSalesSystem.Frontend.Models
{
    public enum ReservationStatus
    {
        Scheduled,
        Completed,
        Cancelled
    }

    public class ReservationDto
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
        public long? AssignedEmployee { get; set; }
        public int GuestNumber { get; set; }
        public long? TableId { get; set; }
    }

    public class ReservationCreateRequest
    {
        public long BusinessId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerNote { get; set; }
        public DateTimeOffset AppointmentTime { get; set; }
        public long CreatedBy { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Scheduled;
        public long? AssignedEmployee { get; set; }
        public int GuestNumber { get; set; } = 1;
        public long? TableId { get; set; }
    }

    public class ReservationPatchRequest
    {
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerNote { get; set; }
        public DateTimeOffset? AppointmentTime { get; set; }
        public ReservationStatus? Status { get; set; }
        public long? AssignedEmployee { get; set; }
    }

    public class StaffDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
    }

    public class ClientDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
