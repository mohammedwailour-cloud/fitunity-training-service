using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Training.Application.Common.Security;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

internal static class IntegrationTestHelper
{
    private const string JwtIssuer = "Training.Api";
    private const string JwtAudience = "Training.Client";
    private const string JwtKey = "TrainingApiDevKey-PleaseReplaceWithAStrongSecret-2026";

    internal static Space CreateActiveSpace(string code, string? name = null, int? capacity = 50)
    {
        return new Space(
            name ?? code,
            code,
            "Test space",
            SpaceType.MultiPurposeRoom,
            capacity,
            false,
            true);
    }

    internal static Session CreateOpenSession(Space space, ActivitySportive activity, DateTime dateDebut, DateTime dateFin, int? capacite = 10, decimal? prix = 20m)
    {
        return new Session(
            SessionType.Open,
            dateDebut,
            dateFin,
            capacite,
            prix,
            false,
            space.Id,
            activity.Id,
            null,
            null,
            true);
    }

    internal static string CreateJwt(Guid userId, string role = "User")
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

    internal static async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode, string expectedError, JsonSerializerOptions? options = null)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);

        ErrorResponse? payload = await response.Content.ReadFromJsonAsync<ErrorResponse>(options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(payload);
        Assert.Equal((int)expectedStatusCode, payload!.StatusCode);
        Assert.Equal(expectedError, payload.Error);
        Assert.False(string.IsNullOrWhiteSpace(payload.Message));
    }

    internal sealed class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
