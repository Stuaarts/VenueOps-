using System.Security.Claims;
using VenueOps.Api.Domain;

namespace VenueOps.Api.Services;

public interface ICurrentUser
{
    Guid UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(params UserRole[] roles);
}

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public UserRole? Role
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(value, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public bool IsInRole(params UserRole[] roles)
    {
        var role = Role;
        return role.HasValue && roles.Contains(role.Value);
    }
}
