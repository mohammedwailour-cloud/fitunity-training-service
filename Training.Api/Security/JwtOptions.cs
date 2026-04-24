namespace Training.Api.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DefaultUserId { get; set; } = "11111111-1111-1111-1111-111111111111";
    public string DefaultRole { get; set; } = "User";
    public string DefaultSubscriptionStatus { get; set; } = "ACTIVE";
    public bool EnableDevelopmentFallback { get; set; } = true;
}
