using System.ComponentModel.DataAnnotations;

namespace VenueOps.Api.Dtos;

public sealed record ClientDto(
    Guid Id,
    string Name,
    string ContactName,
    string Email,
    string? Phone,
    string? Notes,
    int BookingCount);

public sealed record UpsertClientRequest(
    [Required, MaxLength(160)] string Name,
    [Required, MaxLength(120)] string ContactName,
    [Required, EmailAddress, MaxLength(180)] string Email,
    [MaxLength(40)] string? Phone,
    [MaxLength(1000)] string? Notes);
