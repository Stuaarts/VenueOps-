using System.ComponentModel.DataAnnotations;
using VenueOps.Api.Domain;

namespace VenueOps.Api.Dtos;

public sealed record UserDto(Guid Id, string FullName, string Email, UserRole Role, bool IsActive);

public sealed record CreateUserRequest(
    [Required, MaxLength(120)] string FullName,
    [Required, EmailAddress, MaxLength(180)] string Email,
    [Required, MinLength(8)] string Password,
    [Required] UserRole Role,
    bool IsActive = true);

public sealed record UpdateUserRequest(
    [Required, MaxLength(120)] string FullName,
    [Required, EmailAddress, MaxLength(180)] string Email,
    [Required] UserRole Role,
    bool IsActive);
