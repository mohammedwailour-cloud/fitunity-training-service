using Microsoft.Extensions.Options;
using Training.Application.Common.Interfaces;
using Training.Application.Common.Security;
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
            string? claimValue = GetClaimValue(JwtClaimNames.UserId);

            if (Guid.TryParse(claimValue, out Guid userId))
            {
                return userId;
            }

            if (_jwtOptions.EnableDevelopmentFallback && Guid.TryParse(_jwtOptions.DefaultUserId, out Guid defaultUserId))
            {
                return defaultUserId;
            }

            throw new UserContextUnavailableException();
        }
    }

    public string Role
    {
        get
        {
            string? role = GetClaimValue(JwtClaimNames.Role);

            if (!string.IsNullOrWhiteSpace(role))
            {
                string normalized = role.ToUpperInvariant();

                return normalized switch
                {
                    "ADMIN" => "Admin",
                    "COACH" => "Coach",
                    "CLIENT" => "User",
                    "USER" => "User",
                    _ => throw new UserContextUnavailableException()
                };
            }

            if (_jwtOptions.EnableDevelopmentFallback && !string.IsNullOrWhiteSpace(_jwtOptions.DefaultRole))
            {
                return _jwtOptions.DefaultRole;
            }

            throw new UserContextUnavailableException();
        }
    }

    public bool HasActiveSubscription
    {
        get
        {
            string? subscription = GetClaimValue(JwtClaimNames.Subscription);

            if (!string.IsNullOrWhiteSpace(subscription))
            {
                return string.Equals(subscription, JwtClaimValues.ActiveSubscription, StringComparison.OrdinalIgnoreCase);
            }

            if (_jwtOptions.EnableDevelopmentFallback)
            {
                return string.Equals(_jwtOptions.DefaultSubscriptionStatus, JwtClaimValues.ActiveSubscription, StringComparison.OrdinalIgnoreCase);
            }

            throw new UserContextUnavailableException();
        }
    }

    private string? GetClaimValue(string claimName)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        string? claimValue = httpContext?.User?.FindFirst(claimName)?.Value;

        return string.IsNullOrWhiteSpace(claimValue) ? null : claimValue;
    }
}
