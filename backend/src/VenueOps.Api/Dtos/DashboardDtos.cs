using VenueOps.Api.Domain;

namespace VenueOps.Api.Dtos;

public sealed record DashboardMetricsDto(
    int UpcomingEvents,
    int TotalBookingsThisMonth,
    int TotalGuestCountThisMonth,
    int StaffAssignedThisWeek,
    int CancelledEvents,
    int EventsNeedingStaff);

public sealed record StatusCountDto(BookingStatus Status, int Count);

public sealed record DashboardDto(
    DashboardMetricsDto Metrics,
    IReadOnlyList<StatusCountDto> EventsByStatus,
    IReadOnlyList<BookingSummaryDto> UpcomingEvents,
    IReadOnlyList<StaffAssignmentDto> StaffAssignedThisWeek,
    IReadOnlyList<ShiftNoteDto> RecentShiftNotes);
