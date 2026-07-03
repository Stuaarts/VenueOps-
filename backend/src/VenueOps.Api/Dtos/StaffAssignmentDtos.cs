using System.ComponentModel.DataAnnotations;
using VenueOps.Api.Domain;

namespace VenueOps.Api.Dtos;

public sealed record StaffAssignmentDto(
    Guid Id,
    Guid EventBookingId,
    string EventName,
    Guid StaffUserId,
    string StaffName,
    StaffRole Role,
    DateTimeOffset ShiftStart,
    DateTimeOffset ShiftEnd,
    AssignmentStatus Status,
    string? Notes);

public sealed record UpsertStaffAssignmentRequest(
    [Required] Guid EventBookingId,
    [Required] Guid StaffUserId,
    [Required] StaffRole Role,
    [Required] DateTimeOffset ShiftStart,
    [Required] DateTimeOffset ShiftEnd,
    [Required] AssignmentStatus Status,
    [MaxLength(1000)] string? Notes);

public sealed record UpdateAssignmentStatusRequest([Required] AssignmentStatus Status);
