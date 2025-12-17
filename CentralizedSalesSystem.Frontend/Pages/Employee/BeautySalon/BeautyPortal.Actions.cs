using System.Net.Http.Headers;
using System.Net.Http.Json;
using CentralizedSalesSystem.Frontend.Models;
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
        var appointment = SelectedDate.Value.Date + (SelectedTime ?? TimeSpan.Zero);
        var payload = new ReservationCreateRequest
        {
            BusinessId = BusinessId,
            CustomerName = NewReservation.CustomerName,
            CustomerPhone = NewReservation.CustomerPhone,
            CustomerNote = NewReservation.CustomerNote,
            AppointmentTime = new DateTimeOffset(appointment, TimeZoneInfo.Local.GetUtcOffset(appointment)),
            CreatedBy = NewReservation.CreatedBy == 0 ? 1 : NewReservation.CreatedBy,
            Status = ReservationStatus.Scheduled,
            AssignedEmployee = ResolveStaffValue(),
            GuestNumber = NewReservation.GuestNumber,
            TableId = null
        };

        if (UseMockData)
        {
            var mock = new ReservationDto
            {
                Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                BusinessId = payload.BusinessId,
                CustomerName = payload.CustomerName,
                CustomerPhone = payload.CustomerPhone,
                CustomerNote = payload.CustomerNote,
                AppointmentTime = payload.AppointmentTime,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = payload.CreatedBy,
                Status = ReservationStatus.Scheduled,
                AssignedEmployee = payload.AssignedEmployee,
                GuestNumber = payload.GuestNumber,
                TableId = payload.TableId
            };
            Reservations.Insert(0, mock);
            RemoveSlotFromAvailability(SelectedDate.Value.Date, SelectedSlot);
            Snackbar.Add("Reservation created (mock).", Severity.Info);
            ResetForm();
            return;
        }

        var token = await TokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            Snackbar.Add("Please sign in to create reservations.", Severity.Warning);
            return;
        }

        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Snackbar.Add("Session expired. Please sign in again.", Severity.Error);
                }
                else
                {
                    UseMockData = true;
                    var mockFallback = new ReservationDto
                    {
                        Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        BusinessId = payload.BusinessId,
                        CustomerName = payload.CustomerName,
                        CustomerPhone = payload.CustomerPhone,
                        CustomerNote = payload.CustomerNote,
                        AppointmentTime = payload.AppointmentTime,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = payload.CreatedBy,
                        Status = ReservationStatus.Scheduled,
                        AssignedEmployee = payload.AssignedEmployee,
                        GuestNumber = payload.GuestNumber,
                        TableId = payload.TableId
                    };
                    Reservations.Insert(0, mockFallback);
                    RemoveSlotFromAvailability(SelectedDate.Value.Date, SelectedSlot);
                    Snackbar.Add($"Failed to create reservation (API {result.StatusCode}). Stored locally.", Severity.Warning);
                    ResetForm();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create reservation failed: {ex.Message}");
            UseMockData = true;
            var mockFallback = new ReservationDto
            {
                Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                BusinessId = payload.BusinessId,
                CustomerName = payload.CustomerName,
                CustomerPhone = payload.CustomerPhone,
                CustomerNote = payload.CustomerNote,
                AppointmentTime = payload.AppointmentTime,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = payload.CreatedBy,
                Status = ReservationStatus.Scheduled,
                AssignedEmployee = payload.AssignedEmployee,
                GuestNumber = payload.GuestNumber,
                TableId = payload.TableId
            };
            Reservations.Insert(0, mockFallback);
            RemoveSlotFromAvailability(SelectedDate.Value.Date, SelectedSlot);
            Snackbar.Add("Failed to create reservation. Stored locally.", Severity.Warning);
            ResetForm();
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

        if (UseMockData)
        {
            var updated = Reservations.FirstOrDefault(r => r.Id == EditingReservationId);
            if (updated != null)
            {
                updated.CustomerName = patch.CustomerName ?? updated.CustomerName;
                updated.CustomerPhone = patch.CustomerPhone ?? updated.CustomerPhone;
                updated.CustomerNote = patch.CustomerNote ?? updated.CustomerNote;
                updated.AppointmentTime = patch.AppointmentTime ?? updated.AppointmentTime;
                updated.AssignedEmployee = patch.AssignedEmployee;
                updated.Status = ReservationStatus.Scheduled;
                RemoveSlotFromAvailability(SelectedDate.Value.Date, SelectedSlot);
            }
            Snackbar.Add("Reservation updated (mock).", Severity.Info);
            ResetForm();
            IsEditing = false;
            EditingReservationId = null;
            IsReservationOpen = false;
            return;
        }

        var token = await TokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            Snackbar.Add("Please sign in to update reservations.", Severity.Warning);
            return;
        }
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var result = await Http.PatchAsJsonAsync($"reservations/{EditingReservationId}", patch);
            if (result.IsSuccessStatusCode)
            {
                var updated = Reservations.FirstOrDefault(r => r.Id == EditingReservationId);
                if (updated != null)
                {
                    updated.CustomerName = patch.CustomerName ?? updated.CustomerName;
                    updated.CustomerPhone = patch.CustomerPhone ?? updated.CustomerPhone;
                    updated.CustomerNote = patch.CustomerNote ?? updated.CustomerNote;
                    updated.AppointmentTime = patch.AppointmentTime ?? updated.AppointmentTime;
                    updated.AssignedEmployee = patch.AssignedEmployee;
                    updated.Status = ReservationStatus.Scheduled;
                    RemoveSlotFromAvailability(SelectedDate.Value.Date, SelectedSlot);
                    await LoadReservations();
                }
                Snackbar.Add("Reservation updated", Severity.Success);
                ResetForm();
                IsEditing = false;
                EditingReservationId = null;
                IsReservationOpen = false;
            }
            else
            {
                UseMockData = true;
                var updatedLocal = Reservations.FirstOrDefault(r => r.Id == EditingReservationId);
                if (updatedLocal != null)
                {
                    updatedLocal.CustomerName = patch.CustomerName ?? updatedLocal.CustomerName;
                    updatedLocal.CustomerPhone = patch.CustomerPhone ?? updatedLocal.CustomerPhone;
                    updatedLocal.CustomerNote = patch.CustomerNote ?? updatedLocal.CustomerNote;
                    updatedLocal.AppointmentTime = patch.AppointmentTime ?? updatedLocal.AppointmentTime;
                    updatedLocal.AssignedEmployee = patch.AssignedEmployee;
                    updatedLocal.Status = ReservationStatus.Scheduled;
                    RemoveSlotFromAvailability(SelectedDate.Value.Date, SelectedSlot);
                }
                Snackbar.Add($"Failed to update reservation (API {result.StatusCode}). Updated locally.", Severity.Warning);
                ResetForm();
                IsEditing = false;
                EditingReservationId = null;
                IsReservationOpen = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update reservation failed: {ex.Message}");
            UseMockData = true;
            var updatedLocal = Reservations.FirstOrDefault(r => r.Id == EditingReservationId);
            if (updatedLocal != null)
            {
                updatedLocal.CustomerName = patch.CustomerName ?? updatedLocal.CustomerName;
                updatedLocal.CustomerPhone = patch.CustomerPhone ?? updatedLocal.CustomerPhone;
                updatedLocal.CustomerNote = patch.CustomerNote ?? updatedLocal.CustomerNote;
                updatedLocal.AppointmentTime = patch.AppointmentTime ?? updatedLocal.AppointmentTime;
                updatedLocal.AssignedEmployee = patch.AssignedEmployee;
                updatedLocal.Status = ReservationStatus.Scheduled;
                RemoveSlotFromAvailability(SelectedDate.Value.Date, SelectedSlot);
            }
            Snackbar.Add("Failed to update reservation. Updated locally.", Severity.Warning);
            ResetForm();
            IsEditing = false;
            EditingReservationId = null;
            IsReservationOpen = false;
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
        var slot = FormatSlot(local);
        SelectedSlot = slot;
        SelectedTime = local.TimeOfDay;
        EditingOriginalSlot = slot;
    }

    private async Task CancelReservation(ReservationDto reservation)
    {
        if (UseMockData)
        {
            reservation.Status = ReservationStatus.Cancelled;
            var slotMock = FormatSlot(reservation.AppointmentTime);
            AddSlotIfMissing(reservation.AppointmentTime.Date, slotMock);
            Snackbar.Add("Reservation cancelled (mock).", Severity.Info);
            return;
        }

        var token = await TokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            Snackbar.Add("Please sign in to cancel reservations.", Severity.Warning);
            return;
        }
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
                UseMockData = true;
                reservation.Status = ReservationStatus.Cancelled;
                var slotLocal = FormatSlot(reservation.AppointmentTime);
                AddSlotIfMissing(reservation.AppointmentTime.Date, slotLocal);
                Snackbar.Add($"Failed to cancel (API {result.StatusCode}). Cancelled locally.", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cancel reservation failed: {ex.Message}");
            UseMockData = true;
            reservation.Status = ReservationStatus.Cancelled;
            var slotLocal = FormatSlot(reservation.AppointmentTime);
            AddSlotIfMissing(reservation.AppointmentTime.Date, slotLocal);
            Snackbar.Add("Failed to cancel reservation. Cancelled locally.", Severity.Warning);
        }
    }

    private string FormatSlot(DateTimeOffset appointment)
    {
        var local = appointment.ToLocalTime();
        var start = local.TimeOfDay;
        var end = start.Add(TimeSpan.FromMinutes(60));
        return $"{start:hh\\:mm}-{end:hh\\:mm}";
    }
}
