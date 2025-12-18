using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using CentralizedSalesSystem.Frontend.Models;
using CentralizedSalesSystem.Frontend.Json;
using MudBlazor;

namespace CentralizedSalesSystem.Frontend.Pages.Employee.BeautySalon;

public partial class BeautyPortal
{
    private async Task CreateReservation()
    {
        if (!SelectedDate.HasValue || string.IsNullOrWhiteSpace(SelectedSlot)) return;
        if (IsPastDate(SelectedDate.Value))
        {
            Snackbar.Add("Past dates cannot be booked.", Severity.Warning);
            return;
        }
        var createdBy = await GetCurrentUserIdAsync();
        if (!createdBy.HasValue)
        {
            Snackbar.Add("Unable to determine the current user. Please sign in again.", Severity.Warning);
            return;
        }
        
        var appointment = SelectedDate.Value.Date + (SelectedTime ?? TimeSpan.Zero);
        var startTime = new DateTimeOffset(appointment, TimeZoneInfo.Local.GetUtcOffset(appointment));
        var duration = SelectedServiceId.HasValue 
            ? Services.FirstOrDefault(s => s.Id == SelectedServiceId.Value)?.Duration ?? 60 
            : 60;
        var endTime = startTime.AddMinutes(duration);
        
        var payload = new ReservationCreateRequest
        {
            BusinessId = BusinessId,
            CustomerName = NewReservation.CustomerName,
            CustomerPhone = NewReservation.CustomerPhone,
            CustomerNote = NewReservation.CustomerNote,
            AppointmentTime = startTime,
            StartTime = startTime,
            EndTime = endTime,
            CreatedBy = createdBy.Value,
            Status = ReservationStatus.Scheduled,
            AssignedEmployee = ResolveStaffValue(),
            GuestNumber = NewReservation.GuestNumber,
            TableId = null
        };

        try
        {
            var result = await Http.PostAsJsonAsync("reservations", payload);
            if (result.IsSuccessStatusCode)
            {
                var created = await result.Content.ReadFromJsonAsync<ReservationDto>();
                if (created != null)
                {
                    Reservations.Insert(0, created);
                    Snackbar.Add("Reservation created", Severity.Success);
                    RemoveSlotFromAvailability(SelectedDate.Value.Date, SelectedSlot);
                    await LoadReservations();
                }
                ResetForm();
            }
            else
            {
                var errorContent = await result.Content.ReadAsStringAsync();
                Snackbar.Add($"Failed to create reservation: {errorContent}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error creating reservation: {ex.Message}", Severity.Error);
        }
    }

    private async Task SaveEditedReservation()
    {
        if (!IsEditing || EditingReservationId == null || !SelectedDate.HasValue || string.IsNullOrWhiteSpace(SelectedSlot)) return;
        if (IsPastDate(SelectedDate.Value))
        {
            Snackbar.Add("Past dates cannot be booked.", Severity.Warning);
            return;
        }
        
        var appointment = SelectedDate.Value.Date + (SelectedTime ?? TimeSpan.Zero);
        var patch = new ReservationPatchRequest
        {
            CustomerName = NewReservation.CustomerName,
            CustomerPhone = NewReservation.CustomerPhone,
            CustomerNote = NewReservation.CustomerNote,
            AppointmentTime = new DateTimeOffset(appointment, TimeZoneInfo.Local.GetUtcOffset(appointment)),
            AssignedEmployee = ResolveStaffValue(),
            Status = ReservationStatus.Scheduled
        };

        try
        {
            var result = await Http.PatchAsJsonAsync($"reservations/{EditingReservationId}", patch);
            if (result.IsSuccessStatusCode)
            {
                await LoadReservations();
                Snackbar.Add("Reservation updated", Severity.Success);
                RemoveSlotFromAvailability(SelectedDate.Value.Date, SelectedSlot);
                ResetForm();
                IsEditing = false;
                EditingReservationId = null;
                IsReservationOpen = false;
            }
            else
            {
                var errorContent = await result.Content.ReadAsStringAsync();
                Snackbar.Add($"Failed to update reservation: {errorContent}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error updating reservation: {ex.Message}", Severity.Error);
        }
    }

    private void ResetForm()
    {
        NewReservation = new ReservationCreateRequest { BusinessId = BusinessId, GuestNumber = 1 };
        SelectedServiceId = null;
        SelectedStaffValue = null;
        SelectedDate = DateTime.Today;
        SelectedTime = null;
        SelectedSlot = null;
        IsReservationOpen = false;
        EditingOriginalSlot = null;
        RefreshCalendarItemsForRange();
    }

    private Task CompleteReservation(ReservationDto reservation)
    {
        reservation.Status = ReservationStatus.Completed;
        return Task.CompletedTask;
    }

    private void BeginEditReservation(ReservationDto reservation)
    {
        CurrentView = PortalView.Reservation;
        IsReservationOpen = true;
        IsEditing = true;
        EditingReservationId = reservation.Id;
        NewReservation.CustomerName = reservation.CustomerName ?? string.Empty;
        NewReservation.CustomerPhone = reservation.CustomerPhone ?? string.Empty;
        NewReservation.CustomerNote = reservation.CustomerNote ?? string.Empty;
        SelectedStaffValue = reservation.AssignedEmployee ?? -1;
        SelectedServiceId = null; // service not stored on dto, user must pick
        var local = reservation.AppointmentTime.ToLocalTime();
        SelectedDate = local.Date;
        var slot = FormatSlot(reservation.AppointmentTime, reservation.StartTime, reservation.EndTime);
        SelectedSlot = slot;
        SelectedTime = local.TimeOfDay;
        EditingOriginalSlot = slot;
    }

    private async Task CancelReservation(ReservationDto reservation)
    {
        var patch = new ReservationPatchRequest { Status = ReservationStatus.Cancelled };
        try
        {
            var result = await Http.PatchAsJsonAsync($"reservations/{reservation.Id}", patch);
            if (result.IsSuccessStatusCode)
            {
                reservation.Status = ReservationStatus.Cancelled;
                Snackbar.Add("Reservation cancelled.", Severity.Success);
                var slot = FormatSlot(reservation.AppointmentTime);
                AddSlotIfMissing(reservation.AppointmentTime.Date, slot);
                await LoadReservations();
            }
            else
            {
                var errorContent = await result.Content.ReadAsStringAsync();
                Snackbar.Add($"Failed to cancel reservation: {errorContent}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error cancelling reservation: {ex.Message}", Severity.Error);
        }
    }

    private string FormatSlot(DateTimeOffset appointment, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null)
    {
        var start = startTime ?? appointment;
        var end = endTime ?? start.AddMinutes(60);
        var localStart = start.ToLocalTime().TimeOfDay;
        var localEnd = end.ToLocalTime().TimeOfDay;
        return $"{localStart:hh\\:mm}-{localEnd:hh\\:mm}";
    }

    private async Task<long?> GetCurrentUserIdAsync()
    {
        var token = await TokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            foreach (var claim in jwt.Claims)
            {
                if (claim.Type == JwtRegisteredClaimNames.Sub ||
                    claim.Type == ClaimTypes.NameIdentifier ||
                    string.Equals(claim.Type, "sub", StringComparison.OrdinalIgnoreCase))
                {
                    if (long.TryParse(claim.Value, out var userId))
                    {
                        return userId;
                    }
                }
            }
        }
        catch
        {
            // ignore invalid token formats
        }

        return null;
    }
}
