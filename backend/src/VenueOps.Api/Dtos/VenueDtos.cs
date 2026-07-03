using System.ComponentModel.DataAnnotations;

namespace VenueOps.Api.Dtos;

public sealed record VenueRoomDto(
    Guid Id,
    string Name,
    string Location,
    int Capacity,
    bool IsActive,
    string? Notes,
    int UpcomingBookings);

public sealed record UpsertVenueRoomRequest(
    [Required, MaxLength(120)] string Name,
    [Required, MaxLength(160)] string Location,
    [Range(1, 10000)] int Capacity,
    bool IsActive,
    [MaxLength(1000)] string? Notes);
