using System.Linq;
using MudBlazor;
using CentralizedSalesSystem.Frontend.Models;
using CentralizedSalesSystem.Frontend.Services;
using CentralizedSalesSystem.Frontend.Json;
using Heron.MudCalendar;
using Microsoft.AspNetCore.Components;

namespace CentralizedSalesSystem.Frontend.Pages.Employee.BeautySalon;

public partial class BeautyPortal : ComponentBase
{
    private long BusinessId;
    private bool IsLoading = true;
    private PortalView CurrentView = PortalView.Reservation;
    private bool IsReservationOpen;
    private bool IsEditing;
    private long? EditingReservationId;
    private string? EditingOriginalSlot;

    private List<ReservationDto> Reservations = new();
    private string SearchActive = string.Empty;
    private string SearchHistory = string.Empty;
    private string SearchStaff = string.Empty;
    private string SearchClients = string.Empty;
    private List<ReservationDto> ActiveReservations => FilterReservations(Reservations.Where(r => r.Status == ReservationStatus.Scheduled), SearchActive)
        .OrderBy(r => r.AppointmentTime)
        .ToList();
    private List<ReservationDto> HistoricalReservations => FilterReservations(Reservations.Where(r => r.Status != ReservationStatus.Scheduled), SearchHistory)
        .OrderByDescending(r => r.AppointmentTime)
        .ToList();

    private List<StaffDto> Staff = new();
    private List<ClientDto> Clients = new();
    private List<MenuItemDto> Services = new();
    private long? SelectedStaffFilter;
    private DateTime CalendarCurrentDay = DateTime.Today;
    private CalendarView CurrentCalendarView = CalendarView.Month;
    private static readonly TimeOnly CalendarDayStart = new(9, 0);
    private DateTime? CalendarVisibleStart;
    private DateTime? CalendarVisibleEnd;
    private Dictionary<DateTime, List<string>> Availability = new();
    private List<CalendarItem> CalendarItems = new();
    private const string PastMarkerText = "__past__";
    private string? SelectedSlot;

    private ReservationCreateRequest NewReservation = new();
    private long? SelectedServiceId;
    private long? SelectedStaffValue;
    private DateTime? SelectedDate = DateTime.Today;
    private TimeSpan? SelectedTime;

    private Dictionary<long, string> StaffLookup => Staff.ToDictionary(s => s.Id, s => $"{s.FirstName} {s.LastName}");

    private bool CanCreate =>
        !string.IsNullOrWhiteSpace(NewReservation.CustomerName) &&
        !string.IsNullOrWhiteSpace(NewReservation.CustomerPhone) &&
        SelectedDate.HasValue &&
        !IsPastDate(SelectedDate.Value) &&
        !string.IsNullOrWhiteSpace(SelectedSlot) &&
        SelectedServiceId.HasValue &&
        SelectedStaffValue.HasValue;

    protected override async Task OnInitializedAsync()
    {
        await InitializeContextAsync();
        await LoadDataAsync();
        IsLoading = false;
    }

    private async Task InitializeContextAsync()
    {
        await BusinessContext.EnsureLoadedAsync();
        BusinessId = BusinessContext.BusinessId ?? 1;
    }

    private async Task LoadDataAsync()
    {
        var (start, end) = GetMonthRange(CalendarCurrentDay);
        CalendarVisibleStart = start;
        CalendarVisibleEnd = end;
        await Task.WhenAll(LoadReservations(start, end), LoadServices(), LoadStaff(), LoadClients());
        EnsureAvailability(start, end);
        RefreshCalendarItemsForRange();
    }

    private async Task LoadReservations(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var url = $"reservations?limit=500&filterByBusinessId={BusinessId}";
            
            if (startDate.HasValue && endDate.HasValue)
            {
                // Filter by appointment time range
                var start = new DateTimeOffset(startDate.Value, TimeZoneInfo.Local.GetUtcOffset(startDate.Value));
                var end = new DateTimeOffset(endDate.Value.AddDays(1).AddSeconds(-1), TimeZoneInfo.Local.GetUtcOffset(endDate.Value));
                var startParam = Uri.EscapeDataString(start.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                var endParam = Uri.EscapeDataString(end.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                url += $"&filterByAppointmentTimeStart={startParam}&filterByAppointmentTimeEnd={endParam}";
            }
            
            var response = await Http.GetFromJsonAsync<PaginatedResponse<ReservationDto>>(url);
            Reservations = response?.Data ?? new List<ReservationDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load reservations: {ex.Message}");
            Reservations = new List<ReservationDto>();
        }
    }

    private async Task LoadServices()
    {
        try
        {
            var resp = await Http.GetFromJsonAsync<PaginatedResponse<MenuItemDto>>($"items?limit=200&filterByBusinessId={BusinessId}");
            if (resp?.Data != null)
            {
                Services = resp.Data.Where(i => i.Type == ItemType.Service).ToList();
                Console.WriteLine($"Loaded {Services.Count} services out of {resp.Data.Count} total items");
            }
            else
            {
                Services = new List<MenuItemDto>();
                Console.WriteLine("No items data received from API");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load services: {ex.Message}");
            Services = new List<MenuItemDto>();
        }
    }

    private async Task LoadStaff()
    {
        try
        {
            var resp = await Http.GetFromJsonAsync<PaginatedResponse<UserResponseDto>>($"users?limit=200&filterByBusinessId={BusinessId}");
            if (resp?.Data != null)
            {
                Staff = resp.Data.Select(u => new StaffDto
                {
                    Id = u.Id,
                    FirstName = u.Name ?? string.Empty,
                    LastName = string.Empty,
                    Email = u.Email ?? string.Empty,
                    Phone = u.Phone ?? string.Empty,
                    Schedule = string.Empty
                }).ToList();
                Console.WriteLine($"Loaded {Staff.Count} staff members");
            }
            else
            {
                Staff = new List<StaffDto>();
                Console.WriteLine("No users data received from API");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load staff: {ex.Message}");
            Staff = new List<StaffDto>();
        }
    }

    private async Task LoadClients()
    {
        // TODO: Implement clients API endpoint
        // For now, using empty list until clients endpoint is available
        Clients = new List<ClientDto>();
    }

    private static (DateTime start, DateTime end) GetMonthRange(DateTime date)
    {
        var start = new DateTime(date.Year, date.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return (start, end);
    }

    private static bool IsPastDate(DateTime date) => date.Date < DateTime.Today;

    private async void OnCalendarDateRangeChanged(DateRange range)
    {
        if (range.Start.HasValue && range.End.HasValue)
        {
            CalendarVisibleStart = range.Start.Value.Date;
            CalendarVisibleEnd = range.End.Value.Date;
            await LoadReservations(CalendarVisibleStart.Value, CalendarVisibleEnd.Value);
            EnsureAvailability(CalendarVisibleStart.Value, CalendarVisibleEnd.Value);
            RefreshCalendarItemsForRange();
        }
    }

    private async void OnCalendarCurrentDayChanged(DateTime date)
    {
        CalendarCurrentDay = date;
        if (CalendarVisibleStart == null || CalendarVisibleEnd == null)
        {
            var (start, end) = GetMonthRange(date);
            CalendarVisibleStart = start;
            CalendarVisibleEnd = end;
            await LoadReservations(start, end);
            EnsureAvailability(start, end);
            RefreshCalendarItemsForRange();
        }
    }

    private void OnCalendarViewChanged(CalendarView view)
    {
        CurrentCalendarView = view;
        if (view == CalendarView.Month)
        {
            var (start, end) = GetMonthRange(CalendarCurrentDay);
            CalendarVisibleStart = start;
            CalendarVisibleEnd = end;
        }
        RefreshCalendarItemsForRange();
    }

    private void OnDaySelected(DateTime date)
    {
        if (IsPastDate(date))
        {
            Snackbar.Add("Past dates cannot be booked.", Severity.Info);
            return;
        }
        SelectedDate = date;
        SelectFirstAvailableSlot(date);
        IsReservationOpen = true;
        IsEditing = false;
        EditingReservationId = null;
        EditingOriginalSlot = null;
    }

    private void OnCalendarCellClicked(DateTime date)
    {
        if (CurrentCalendarView == CalendarView.Month)
        {
            if (date.Month != CalendarCurrentDay.Month || date.Year != CalendarCurrentDay.Year)
            {
                return;
            }
            OnDaySelected(date.Date);
            return;
        }

        OpenReservationForDateTime(date);
    }

    private void OnCalendarItemClicked(CalendarItem item)
    {
        if (item.AllDay) return;
        if (string.IsNullOrWhiteSpace(item.Text)) return;
        if (string.Equals(item.Text, PastMarkerText, StringComparison.Ordinal)) return;
        if (IsPastDate(item.Start))
        {
            Snackbar.Add("Past dates cannot be booked.", Severity.Info);
            return;
        }
        if (item.Text.StartsWith("+", StringComparison.Ordinal) || item.Text.StartsWith("No ", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var start = item.Start;
        var end = item.End ?? item.Start.AddHours(1);
        var slot = BuildSlotLabel(start, end);
        if (!GetFilteredSlots(start.Date).Contains(slot))
        {
            Snackbar.Add("Selected time is not available.", Severity.Info);
            return;
        }

        OpenReservationAtSlot(start.Date, slot);
    }

    private IEnumerable<string> GetAvailableSlots()
    {
        if (!SelectedDate.HasValue) return Enumerable.Empty<string>();
        return GetFilteredSlots(SelectedDate.Value.Date);
    }

    private void SelectSlot(string slot)
    {
        SelectedSlot = slot;
        SelectedTime = ParseSlotStart(slot) ?? TimeSpan.FromHours(10);
    }

    private void OpenReservationForDateTime(DateTime dateTime)
    {
        if (IsPastDate(dateTime))
        {
            Snackbar.Add("Past dates cannot be booked.", Severity.Info);
            return;
        }
        var slotStart = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
        var slotEnd = slotStart.AddHours(1);
        var slot = BuildSlotLabel(slotStart, slotEnd);
        if (!GetFilteredSlots(slotStart.Date).Contains(slot))
        {
            Snackbar.Add("Selected time is not available.", Severity.Info);
            return;
        }

        OpenReservationAtSlot(slotStart.Date, slot);
    }

    private void OpenReservationAtSlot(DateTime date, string slot)
    {
        if (IsPastDate(date))
        {
            Snackbar.Add("Past dates cannot be booked.", Severity.Info);
            return;
        }
        SelectedDate = date;
        SelectSlot(slot);
        IsReservationOpen = true;
        IsEditing = false;
        EditingReservationId = null;
        EditingOriginalSlot = null;
    }

    private void SelectFirstAvailableSlot(DateTime date)
    {
        var slots = GetFilteredSlots(date.Date).ToList();
        if (slots.Any())
        {
            var first = slots.First();
            SelectedSlot = first;
            SelectedTime = ParseSlotStart(first);
        }
        else
        {
            SelectedSlot = null;
            SelectedTime = null;
        }
    }

    private static TimeSpan? ParseSlotStart(string slot)
    {
        if (string.IsNullOrWhiteSpace(slot)) return null;
        var parts = slot.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return TimeSpan.TryParse(parts.FirstOrDefault(), out var start) ? start : null;
    }

    private void RemoveSlotFromAvailability(DateTime date, string? slot)
    {
        if (string.IsNullOrWhiteSpace(slot)) return;
        if (Availability.TryGetValue(date.Date, out var slots))
        {
            slots.Remove(slot);
            RefreshCalendarItemsForRange();
        }
    }

    private void AddSlotIfMissing(DateTime date, string slot)
    {
        if (!Availability.TryGetValue(date.Date, out var slots))
        {
            Availability[date.Date] = new List<string> { slot };
            RefreshCalendarItemsForRange();
            return;
        }
        if (!slots.Contains(slot))
        {
            slots.Add(slot);
            RefreshCalendarItemsForRange();
        }
    }

    private IEnumerable<string> GetFilteredSlots(DateTime date)
    {
        if (IsPastDate(date)) return Enumerable.Empty<string>();
        if (!Availability.TryGetValue(date.Date, out var slots)) return Enumerable.Empty<string>();
        if (SelectedStaffFilter.HasValue && SelectedStaffFilter.Value != -1)
        {
            var staff = Staff.FirstOrDefault(s => s.Id == SelectedStaffFilter.Value);
            if (staff != null && !IsStaffWorkingOn(staff, date))
            {
                return Enumerable.Empty<string>();
            }
        }

        var filtered = slots.AsEnumerable();
        if (IsEditing && EditingReservationId.HasValue && EditingOriginalSlot != null && SelectedDate.HasValue && SelectedDate.Value.Date == date.Date)
        {
            filtered = filtered.Where(s => !string.Equals(s, EditingOriginalSlot, StringComparison.OrdinalIgnoreCase));
        }

        return filtered.Distinct();
    }

    private bool IsStaffWorkingOn(StaffDto staff, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(staff.Schedule)) return true;
        var sched = staff.Schedule.Replace(" ", "");
        var parts = sched.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return true;
        var start = ParseDay(parts[0]);
        var end = ParseDay(parts[1]);
        if (start == null || end == null) return true;
        return InDayRange(start.Value, end.Value, date.DayOfWeek);
    }

    private static DayOfWeek? ParseDay(string s)
    {
        s = s.ToLowerInvariant();
        return s switch
        {
            "mon" => DayOfWeek.Monday,
            "tue" or "tues" => DayOfWeek.Tuesday,
            "wed" => DayOfWeek.Wednesday,
            "thu" or "thur" or "thurs" => DayOfWeek.Thursday,
            "fri" => DayOfWeek.Friday,
            "sat" => DayOfWeek.Saturday,
            "sun" => DayOfWeek.Sunday,
            _ => null
        };
    }

    private static bool InDayRange(DayOfWeek start, DayOfWeek end, DayOfWeek target)
    {
        if (start <= end)
        {
            return target >= start && target <= end;
        }
        return target >= start || target <= end;
    }

    private void ShowCalendar()
    {
        IsReservationOpen = false;
        SelectedSlot = null;
        IsEditing = false;
        EditingReservationId = null;
        EditingOriginalSlot = null;
        RefreshCalendarItemsForRange();
    }

    private async void SetView(PortalView view)
    {
        CurrentView = view;
        if (view == PortalView.Reservation)
        {
            // Open the reservation form for new reservation
            IsReservationOpen = true;
            IsEditing = false;
            EditingReservationId = null;
            ResetForm();
            // Ensure services are loaded
            if (Services == null || Services.Count == 0)
            {
                await LoadServices();
            }
            StateHasChanged();
        }
    }

    private Color GetNavColor(PortalView view) => CurrentView == view ? Color.Success : Color.Default;

    private long? ResolveStaffValue()
    {
        if (!SelectedStaffValue.HasValue) return null;
        if (SelectedStaffValue == -1) return null; // any staff
        return SelectedStaffValue;
    }

    private void UpdateStaffSelectionFromForm(long? value)
    {
        SelectedStaffValue = value;
        SelectedStaffFilter = value;
        RefreshCalendarItemsForRange();
        if (SelectedDate.HasValue)
        {
            SelectFirstAvailableSlot(SelectedDate.Value);
        }
    }

    private void UpdateStaffSelectionFromCalendar(long? value)
    {
        SelectedStaffFilter = value;
        SelectedStaffValue = value;
        RefreshCalendarItemsForRange();
        if (SelectedDate.HasValue)
        {
            SelectFirstAvailableSlot(SelectedDate.Value);
        }
    }

    private IEnumerable<ReservationDto> FilterReservations(IEnumerable<ReservationDto> source, string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return source;
        term = term.ToLowerInvariant();
        return source.Where(r =>
            r.CustomerName?.ToLowerInvariant().Contains(term) == true ||
            r.Id.ToString().Contains(term));
    }

    private IEnumerable<StaffDto> FilterStaff()
    {
        if (string.IsNullOrWhiteSpace(SearchStaff)) return Staff;
        var term = SearchStaff.ToLowerInvariant();
        return Staff.Where(s =>
            $"{s.FirstName} {s.LastName}".ToLowerInvariant().Contains(term) ||
            s.Id.ToString().Contains(term));
    }

    private IEnumerable<ClientDto> FilterClients()
    {
        if (string.IsNullOrWhiteSpace(SearchClients)) return Clients;
        var term = SearchClients.ToLowerInvariant();
        return Clients.Where(c =>
            $"{c.FirstName} {c.LastName}".ToLowerInvariant().Contains(term) ||
            c.Id.ToString().Contains(term));
    }

    private void EnsureAvailability(DateTime start, DateTime end)
    {
        // Generate hourly slots from 9 AM to 6 PM for business days
        var slots = new[] { "09:00-10:00", "10:00-11:00", "11:00-12:00", "14:00-15:00", "15:00-16:00", "17:00-18:00" };
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            if (day.DayOfWeek == DayOfWeek.Sunday) continue;
            if (!Availability.ContainsKey(day.Date))
            {
                // Remove slots that have existing reservations
                var availableSlots = slots.ToList();
                var dayReservations = Reservations.Where(r => 
                    r.AppointmentTime.Date == day.Date && 
                    r.Status == ReservationStatus.Scheduled).ToList();
                
                foreach (var reservation in dayReservations)
                {
                    var startTime = reservation.StartTime ?? reservation.AppointmentTime;
                    var endTime = reservation.EndTime ?? startTime.AddMinutes(60);
                    var slot = BuildSlotLabel(startTime.ToLocalTime().DateTime, endTime.ToLocalTime().DateTime);
                    availableSlots.Remove(slot);
                }
                
                Availability[day.Date] = availableSlots;
            }
        }
    }

    private void RefreshCalendarItemsForRange()
    {
        var (start, end) = GetMonthRange(CalendarCurrentDay);
        if (CalendarVisibleStart.HasValue && CalendarVisibleEnd.HasValue)
        {
            start = CalendarVisibleStart.Value.Date;
            end = CalendarVisibleEnd.Value.Date;
        }

        var items = new List<CalendarItem>();
        var isMonthView = CurrentCalendarView == CalendarView.Month;
        
        // Add booked reservations to calendar
        var relevantReservations = Reservations
            .Where(r => r.Status == ReservationStatus.Scheduled && 
                       r.AppointmentTime.Date >= start.Date && 
                       r.AppointmentTime.Date <= end.Date)
            .ToList();

        foreach (var reservation in relevantReservations)
        {
            var resStart = reservation.StartTime ?? reservation.AppointmentTime;
            var resEnd = reservation.EndTime ?? resStart.AddMinutes(60);
            var localStart = resStart.ToLocalTime();
            var localEnd = resEnd.ToLocalTime();
            
            if (isMonthView && (localStart.Month != CalendarCurrentDay.Month || localStart.Year != CalendarCurrentDay.Year))
            {
                continue;
            }

            var customerName = reservation.CustomerName ?? "Unnamed";
            var staffName = reservation.AssignedEmployee.HasValue && StaffLookup.ContainsKey(reservation.AssignedEmployee.Value)
                ? StaffLookup[reservation.AssignedEmployee.Value]
                : "Any Staff";
            
            items.Add(new CalendarItem
            {
                Start = localStart.DateTime,
                End = localEnd.DateTime,
                Text = isMonthView ? $"{customerName}" : $"{customerName} - {staffName}",
                AllDay = false
            });
        }

        for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
        {
            if (isMonthView && (day.Month != CalendarCurrentDay.Month || day.Year != CalendarCurrentDay.Year))
            {
                continue;
            }

            if (IsPastDate(day))
            {
                if (isMonthView)
                {
                    items.Add(new CalendarItem
                    {
                        Start = day,
                        End = day,
                        Text = PastMarkerText,
                        AllDay = true
                    });
                }
                continue;
            }

            var dayReservations = relevantReservations.Count(r => r.AppointmentTime.Date == day.Date);
            if (isMonthView && dayReservations == 0)
            {
                items.Add(new CalendarItem
                {
                    Start = day,
                    End = day,
                    Text = "No bookings",
                    AllDay = true
                });
            }
        }

        CalendarItems = items;
    }

    private CalendarItem? CreateCalendarItem(DateTime date, string slot)
    {
        if (string.IsNullOrWhiteSpace(slot)) return null;
        var parts = slot.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return null;
        if (!TimeSpan.TryParse(parts[0], out var start)) return null;
        TimeSpan end = start.Add(TimeSpan.FromMinutes(60));
        if (parts.Length > 1 && TimeSpan.TryParse(parts[1], out var parsedEnd))
        {
            end = parsedEnd;
        }
        return new CalendarItem
        {
            Start = date.Date + start,
            End = date.Date + end,
            Text = slot
        };
    }

    private static string BuildSlotLabel(DateTime start, DateTime end) =>
        $"{start:HH:mm}-{end:HH:mm}";

    private enum PortalView
    {
        Reservation,
        Active,
        History,
        Staff,
        Clients
    }
}
