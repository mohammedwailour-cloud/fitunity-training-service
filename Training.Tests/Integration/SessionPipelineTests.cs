using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

public class SessionPipelineTests : IClassFixture<TrainingApiFactory>
{
    private const string JwtIssuer = "Training.Api";
    private const string JwtAudience = "Training.Client";
    private const string JwtKey = "TrainingApiDevKey-PleaseReplaceWithAStrongSecret-2026";

    private readonly TrainingApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SessionPipelineTests(TrainingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateSession_WithInvalidDates_Returns400()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.CoachingGroupe,
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(4),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            activityId = (Guid?)null,
            coachId = (Guid?)null,
            eventId = (Guid?)null
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_session_dates");
    }

    [Fact]
    public async Task UpdateSession_WithCapacityConflict_Returns409()
    {
        await _factory.ResetDatabaseAsync();

        var session = new Session(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(3).AddHours(1),
            3,
            25m,
            false);
        var reservation1 = new Reservation(Guid.NewGuid(), session.Id);
        var reservation2 = new Reservation(Guid.NewGuid(), session.Id);

        await _factory.SeedAsync(session, reservation1, reservation2);
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/sessions/{session.Id}", new
        {
            dateDebut = session.DateDebut,
            dateFin = session.DateFin,
            capacite = 1,
            prix = session.Prix,
            abonnementRequis = session.AbonnementRequis,
            coachId = session.CoachId
        });

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "session_capacity_conflict");
    }

    [Fact]
    public async Task ReserveSession_WithDuplicateReservation_Returns409()
    {
        await _factory.ResetDatabaseAsync();

        var userId = Guid.NewGuid();
        var session = new Session(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            false);
        var reservation = new Reservation(userId, session.Id);

        await _factory.SeedAsync(session, reservation);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(userId));

        var response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "duplicate_reservation");
    }

    [Fact]
    public async Task ReserveSession_WhenFull_Returns409()
    {
        await _factory.ResetDatabaseAsync();

        var session = new Session(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            1,
            30m,
            false);
        var reservation = new Reservation(Guid.NewGuid(), session.Id);

        await _factory.SeedAsync(session, reservation);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(Guid.NewGuid()));

        var response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "session_full");
    }

    [Fact]
    public async Task ReservePastSession_Returns400()
    {
        await _factory.ResetDatabaseAsync();

        var session = new Session(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddHours(-3),
            DateTime.UtcNow.AddHours(-2),
            5,
            30m,
            false);

        await _factory.SeedAsync(session);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt(Guid.NewGuid()));

        var response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_session_state");
    }

    [Fact]
    public async Task CreateSession_WithCoachFromDifferentActivity_Returns400()
    {
        await _factory.ResetDatabaseAsync();

        var activity1 = new ActivitySportive("Yoga", "Flow");
        var activity2 = new ActivitySportive("Boxe", "Combat");
        var coach = new Coach("Coach A", "coach@test.local", activity1.Id);

        await _factory.SeedAsync(activity1, activity2, coach);
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.CoachingGroupe,
            dateDebut = DateTime.UtcNow.AddDays(4),
            dateFin = DateTime.UtcNow.AddDays(4).AddHours(1),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            activityId = activity2.Id,
            coachId = coach.Id,
            eventId = (Guid?)null
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "coach_activity_mismatch");
    }

    [Fact]
    public async Task UpdateUnknownSession_Returns404()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/sessions/{Guid.NewGuid()}", new
        {
            dateDebut = DateTime.UtcNow.AddDays(3),
            dateFin = DateTime.UtcNow.AddDays(3).AddHours(1),
            capacite = 5,
            prix = 15m,
            abonnementRequis = false,
            coachId = (Guid?)null
        });

        await AssertErrorAsync(response, HttpStatusCode.NotFound, "session_not_found");
    }

    private async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode, string expectedError)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Equal((int)expectedStatusCode, payload!.StatusCode);
        Assert.Equal(expectedError, payload.Error);
        Assert.False(string.IsNullOrWhiteSpace(payload.Message));
    }

    private static string CreateJwt(Guid userId, string role = "User")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
