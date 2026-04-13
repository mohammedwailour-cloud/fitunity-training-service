using System.Security.Claims;
using Microsoft.Extensions.Options;
using Training.Application.Common.Interfaces;
using Training.Application.Exceptions;

namespace Training.Api.Security;

public class JwtUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtOptions _jwtOptions;

    public JwtUserContext(IHttpContextAccessor httpContextAccessor, IOptions<JwtOptions> jwtOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtOptions = jwtOptions.Value;
    }

    public Guid UserId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(claimValue, out var userId))
            {
                return userId;
            }

            if (_jwtOptions.EnableDevelopmentFallback && Guid.TryParse(_jwtOptions.DefaultUserId, out var defaultUserId))
            {
                return defaultUserId;
            }

            throw new UserContextUnavailableException();
        }
    }

    public string? Role
    {
        get
        {
            var role = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

            if (!string.IsNullOrWhiteSpace(role))
            {
                return role;
            }

            if (_jwtOptions.EnableDevelopmentFallback)
            {
                return _jwtOptions.DefaultRole;
            }

            throw new UserContextUnavailableException();
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
