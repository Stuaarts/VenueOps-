using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VenueOps.Api.Domain;
using VenueOps.Api.Dtos;

namespace VenueOps.Api.Services;

public interface IJwtTokenService
{
    LoginResponse CreateToken(AppUser user);
}

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public LoginResponse CreateToken(AppUser user)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "VenueOps";
        var audience = configuration["Jwt:Audience"] ?? "VenueOpsClient";
        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            new AuthUserDto(user.Id, user.FullName, user.Email, user.Role));
    }
}
