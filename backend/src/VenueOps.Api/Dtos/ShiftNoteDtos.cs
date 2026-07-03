using System.ComponentModel.DataAnnotations;
using VenueOps.Api.Domain;

namespace VenueOps.Api.Dtos;

public sealed record ShiftNoteDto(
    Guid Id,
    Guid EventBookingId,
    string EventName,
    Guid StaffUserId,
    string StaffName,
    ShiftNoteType NoteType,
    string Body,
    bool IsPinned,
    DateTimeOffset CreatedAt);

public sealed record CreateShiftNoteRequest(
    [Required] Guid EventBookingId,
    Guid? StaffUserId,
    [Required] ShiftNoteType NoteType,
    [Required, MinLength(3), MaxLength(1600)] string Body,
    bool IsPinned = false);
