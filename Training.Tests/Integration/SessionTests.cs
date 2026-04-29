using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Training.Application.Sessions.DTOs;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

public class SessionTests : IClassFixture<TrainingApiFactory>
{
    private readonly TrainingApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SessionTests(TrainingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateOpenSession_WithValidPayload_ReturnsCreatedAndPayload()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("OPEN-SPACE");
        ActivitySportive activity = new("Musculation", "Libre");
        await _factory.SeedAsync(space, activity);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Coach"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.Open,
            dateDebut = DateTime.UtcNow.AddDays(3),
            dateFin = DateTime.UtcNow.AddDays(3).AddHours(2),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = true,
            spaceId = space.Id,
            activityId = activity.Id,
            coachId = (Guid?)null,
            eventId = (Guid?)null
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        SessionResponse? payload = await response.Content.ReadFromJsonAsync<SessionResponse>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.True(payload!.IsOpenSession);
        Assert.Equal(SessionType.Open, payload.Type);
        Assert.Equal(space.Name, payload.SpaceName);
        Assert.Equal("Musculation", payload.ActivityName);
    }

    [Fact]
    public async Task CreateSession_WithoutAuthentication_ReturnsUnauthorized()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("SESSION-401");
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.CoachingGroupe,
            dateDebut = DateTime.UtcNow.AddDays(3),
            dateFin = DateTime.UtcNow.AddDays(3).AddHours(2),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = false,
            spaceId = space.Id,
            activityId = (Guid?)null,
            coachId = (Guid?)null,
            eventId = (Guid?)null
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOpenSession_WithCoach_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("OPEN-COACH");
        ActivitySportive activity = new("Musculation", "Libre");
        Coach coach = new("Coach A", "coach@test.local", activity.Id);
        await _factory.SeedAsync(space, activity, coach);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Coach"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.Open,
            dateDebut = DateTime.UtcNow.AddDays(3),
            dateFin = DateTime.UtcNow.AddDays(3).AddHours(2),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = true,
            spaceId = space.Id,
            activityId = activity.Id,
            coachId = coach.Id,
            eventId = (Guid?)null
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_open_session", _jsonOptions);
    }

    [Fact]
    public async Task CreateOpenSession_WithoutActivity_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("OPEN-NO-ACTIVITY");
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Coach"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.Open,
            dateDebut = DateTime.UtcNow.AddDays(3),
            dateFin = DateTime.UtcNow.AddDays(3).AddHours(2),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = true,
            spaceId = space.Id,
            activityId = (Guid?)null,
            coachId = (Guid?)null,
            eventId = (Guid?)null
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_open_session", _jsonOptions);
    }

    [Fact]
    public async Task CreateSession_WithOverlapOnSameSpace_ReturnsConflict()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("SESSION-OVERLAP");
        Session existingSession = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(4),
            DateTime.UtcNow.AddDays(4).AddHours(2),
            10,
            20m,
            false,
            space.Id);

        await _factory.SeedAsync(space, existingSession);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Coach"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.CoachingGroupe,
            dateDebut = DateTime.UtcNow.AddDays(4).AddMinutes(30),
            dateFin = DateTime.UtcNow.AddDays(4).AddHours(3),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = false,
            spaceId = space.Id,
            activityId = (Guid?)null,
            coachId = (Guid?)null,
            eventId = (Guid?)null
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.Conflict, "space_unavailable", _jsonOptions);
    }

    [Fact]
    public async Task CreateSession_WithInactiveSpace_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseAsync();
        Space inactiveSpace = new(
            "Inactive Space",
            "SESSION-INACTIVE",
            "Inactive",
            SpaceType.MultiPurposeRoom,
            20,
            false,
            false);

        await _factory.SeedAsync(inactiveSpace);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Coach"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.CoachingGroupe,
            dateDebut = DateTime.UtcNow.AddDays(3),
            dateFin = DateTime.UtcNow.AddDays(3).AddHours(2),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = false,
            spaceId = inactiveSpace.Id,
            activityId = (Guid?)null,
            coachId = (Guid?)null,
            eventId = (Guid?)null
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.BadRequest, "space_inactive", _jsonOptions);
    }

    [Fact]
    public async Task DeleteSession_WithUserRole_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("SESSION-DELETE");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(3).AddHours(2),
            10,
            20m,
            false,
            space.Id);

        await _factory.SeedAsync(space, session);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "User"));

        HttpResponseMessage response = await client.DeleteAsync($"/api/sessions/{session.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSession_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseAsync();
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.DeleteAsync($"/api/sessions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        OperationNotSupportedResponse? payload = await response.Content.ReadFromJsonAsync<OperationNotSupportedResponse>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Equal("operation_not_supported", payload!.Error);
    }

    [Fact]
    public async Task UpdateSession_WithDifferentCoachUser_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();
        ActivitySportive activity = new("Yoga", "Flow");
        Coach coach = new("Coach A", "coach@test.local", activity.Id);
        Space space = IntegrationTestHelper.CreateActiveSpace("SESSION-OWNER");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(3).AddHours(1),
            10,
            20m,
            false,
            space.Id,
            activity.Id,
            coach.Id);

        await _factory.SeedAsync(activity, coach, space, session);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Coach"));

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/sessions/{session.Id}", new
        {
            dateDebut = session.DateDebut,
            dateFin = session.DateFin,
            capacite = session.Capacite,
            prix = session.Prix,
            abonnementRequis = session.AbonnementRequis,
            isOpenSession = false,
            spaceId = session.SpaceId,
            activityId = session.ActivityId,
            coachId = session.CoachId
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.Forbidden, "forbidden", _jsonOptions);
    }

    private sealed class OperationNotSupportedResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
