using System.ComponentModel.DataAnnotations;
using VenueOps.Api.Domain;

namespace VenueOps.Api.Dtos;

public sealed record BookingSummaryDto(
    Guid Id,
    string EventName,
    string ClientName,
    string VenueRoomName,
    DateOnly EventDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int GuestCount,
    string EventType,
    BookingStatus Status,
    int AssignedStaffCount);

public sealed record BookingDetailDto(
    Guid Id,
    string EventName,
    Guid ClientId,
    string ClientName,
    Guid VenueRoomId,
    string VenueRoomName,
    DateOnly EventDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int GuestCount,
    string EventType,
    BookingStatus Status,
    string? InternalNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<StaffAssignmentDto> StaffAssignments,
    IReadOnlyList<ShiftNoteDto> ShiftNotes);

public sealed record UpsertBookingRequest(
    [Required, MaxLength(180)] string EventName,
    [Required] Guid ClientId,
    [Required] Guid VenueRoomId,
    [Required] DateOnly EventDate,
    [Required] TimeOnly StartTime,
    [Required] TimeOnly EndTime,
    [Range(1, 10000)] int GuestCount,
    [Required, MaxLength(80)] string EventType,
    [Required] BookingStatus Status,
    [MaxLength(2000)] string? InternalNotes);

public sealed record UpdateBookingStatusRequest([Required] BookingStatus Status);
