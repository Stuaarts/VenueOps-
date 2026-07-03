using System.ComponentModel.DataAnnotations;
using VenueOps.Api.Domain;

namespace VenueOps.Api.Dtos;

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);

public sealed record AuthUserDto(Guid Id, string FullName, string Email, UserRole Role);

public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAt, AuthUserDto User);
