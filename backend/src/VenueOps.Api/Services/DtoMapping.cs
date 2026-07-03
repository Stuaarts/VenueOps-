using VenueOps.Api.Domain;
using VenueOps.Api.Dtos;

namespace VenueOps.Api.Services;

public static class DtoMapping
{
    public static UserDto ToDto(this AppUser user) =>
        new(user.Id, user.FullName, user.Email, user.Role, user.IsActive);

    public static ClientDto ToDto(this Client client) =>
        new(client.Id, client.Name, client.ContactName, client.Email, client.Phone, client.Notes, client.Bookings.Count);

    public static VenueRoomDto ToDto(this VenueRoom venue, int upcomingBookings = 0) =>
        new(venue.Id, venue.Name, venue.Location, venue.Capacity, venue.IsActive, venue.Notes, upcomingBookings);

    public static BookingSummaryDto ToSummaryDto(this EventBooking booking) =>
        new(
            booking.Id,
            booking.EventName,
            booking.Client?.Name ?? string.Empty,
            booking.VenueRoom?.Name ?? string.Empty,
            booking.EventDate,
            booking.StartTime,
            booking.EndTime,
            booking.GuestCount,
            booking.EventType,
            booking.Status,
            booking.StaffAssignments.Count);

    public static BookingDetailDto ToDetailDto(this EventBooking booking) =>
        new(
            booking.Id,
            booking.EventName,
            booking.ClientId,
            booking.Client?.Name ?? string.Empty,
            booking.VenueRoomId,
            booking.VenueRoom?.Name ?? string.Empty,
            booking.EventDate,
            booking.StartTime,
            booking.EndTime,
            booking.GuestCount,
            booking.EventType,
            booking.Status,
            booking.InternalNotes,
            booking.CreatedAt,
            booking.UpdatedAt,
            booking.StaffAssignments
                .OrderBy(x => x.ShiftStart)
                .Select(x => x.ToDto())
                .ToList(),
            booking.ShiftNotes
                .OrderByDescending(x => x.IsPinned)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => x.ToDto())
                .ToList());

    public static StaffAssignmentDto ToDto(this StaffAssignment assignment) =>
        new(
            assignment.Id,
            assignment.EventBookingId,
            assignment.EventBooking?.EventName ?? string.Empty,
            assignment.StaffUserId,
            assignment.StaffUser?.FullName ?? string.Empty,
            assignment.Role,
            assignment.ShiftStart,
            assignment.ShiftEnd,
            assignment.Status,
            assignment.Notes);

    public static ShiftNoteDto ToDto(this ShiftNote note) =>
        new(
            note.Id,
            note.EventBookingId,
            note.EventBooking?.EventName ?? string.Empty,
            note.StaffUserId,
            note.StaffUser?.FullName ?? string.Empty,
            note.NoteType,
            note.Body,
            note.IsPinned,
            note.CreatedAt);
}
