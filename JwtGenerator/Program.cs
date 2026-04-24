using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Training.Application.Common.Security;

const string JwtIssuer = "Training.Api";
const string JwtAudience = "Training.Client";
const string JwtKey = "TrainingApiDevKey-PleaseReplaceWithAStrongSecret-2026";

static string CreateJwt(Guid userId, string role = "User")
{
    Claim[] claims =
    [
        new Claim(JwtClaimNames.UserId, userId.ToString()),
        new Claim(JwtClaimNames.Role, role),
        new Claim(JwtClaimNames.Subscription, JwtClaimValues.ActiveSubscription)
    ];

    SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(JwtKey));
    SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

    JwtSecurityToken token = new(
        issuer: JwtIssuer,
        audience: JwtAudience,
        claims: claims,
        notBefore: DateTime.UtcNow.AddMinutes(-1),
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

Console.WriteLine("USER A:");
Console.WriteLine(CreateJwt(Guid.Parse("11111111-1111-1111-1111-111111111111")));

Console.WriteLine("\nUSER B:");
Console.WriteLine(CreateJwt(Guid.Parse("33333333-3333-3333-3333-333333333333")));
