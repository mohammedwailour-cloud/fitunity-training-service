using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

public class SessionPipelineTests : IClassFixture<TrainingApiFactory>
{
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
        Space space = CreateActiveSpace("SPACE-DATES");
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.CoachingGroupe,
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(4),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = false,
            spaceId = space.Id,
            activityId = (Guid?)null,
            coachId = (Guid?)null,
            eventId = (Guid?)null
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_session_dates");
    }

    [Fact]
    public async Task CreateOpenSession_WithoutCoach_ReturnsOk()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateActiveSpace("SPACE-OPEN-OK");
        ActivitySportive activity = new("Musculation", "Libre");
        await _factory.SeedAsync(space, activity);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.Open,
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
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
        Assert.Equal("Musculation", payload.ActivityName);
        Assert.Equal(space.Name, payload.SpaceName);
    }

    [Fact]
    public async Task CreateOpenSession_WithCoach_ReturnsFail()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateActiveSpace("SPACE-OPEN-COACH");
        ActivitySportive activity = new("Musculation", "Libre");
        Coach coach = new("Coach A", "coach@test.local", activity.Id);
        await _factory.SeedAsync(space, activity, coach);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.Open,
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = true,
            spaceId = space.Id,
            activityId = activity.Id,
            coachId = coach.Id,
            eventId = (Guid?)null
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_open_session");
    }

    [Fact]
    public async Task CreateOpenSession_WithoutActivity_ReturnsFail()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateActiveSpace("SPACE-OPEN-NO-ACTIVITY");
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.Open,
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = true,
            spaceId = space.Id,
            activityId = (Guid?)null,
            coachId = (Guid?)null,
            eventId = (Guid?)null
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_open_session");
    }

    [Fact]
    public async Task UpdateSession_WithCapacityConflict_Returns409()
    {
        await _factory.ResetDatabaseAsync();

        Space space = CreateActiveSpace("SPACE-CAPACITY");
        ActivitySportive activity = new("Yoga", "Flow");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(3).AddHours(1),
            3,
            25m,
            false,
            space.Id,
            activity.Id);
        Reservation reservation1 = new(Guid.NewGuid(), session.Id);
        Reservation reservation2 = new(Guid.NewGuid(), session.Id);

        await _factory.SeedAsync(space, activity, session, reservation1, reservation2);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/sessions/{session.Id}", new
        {
            dateDebut = session.DateDebut,
            dateFin = session.DateFin,
            capacite = 1,
            prix = session.Prix,
            abonnementRequis = session.AbonnementRequis,
            isOpenSession = false,
            spaceId = space.Id,
            activityId = activity.Id,
            coachId = session.CoachId
        });

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "session_capacity_conflict");
    }

    [Fact]
    public async Task ReserveSession_WithDuplicateReservation_Returns409()
    {
        await _factory.ResetDatabaseAsync();

        Guid userId = Guid.NewGuid();
        Space space = CreateActiveSpace("SPACE-DUPLICATE");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            false,
            space.Id);
        Reservation reservation = new(userId, session.Id);

        await _factory.SeedAsync(space, session, reservation);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(userId));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "duplicate_reservation");
    }

    [Fact]
    public async Task ReserveOpenSession_ReturnsOk()
    {
        await _factory.ResetDatabaseAsync();

        Guid userId = Guid.NewGuid();
        Space space = CreateActiveSpace("SPACE-OPEN-RESERVE");
        ActivitySportive activity = new("Musculation", "Libre");
        Session session = new(
            SessionType.Open,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            false,
            space.Id,
            activity.Id,
            null,
            null,
            true);

        await _factory.SeedAsync(space, activity, session);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(userId));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReserveSession_WhenFull_Returns409()
    {
        await _factory.ResetDatabaseAsync();

        Space space = CreateActiveSpace("SPACE-FULL");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            1,
            30m,
            false,
            space.Id);
        Reservation reservation = new(Guid.NewGuid(), session.Id);

        await _factory.SeedAsync(space, session, reservation);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid()));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "session_full");
    }

    [Fact]
    public async Task ReservePastSession_Returns400()
    {
        await _factory.ResetDatabaseAsync();

        Space space = CreateActiveSpace("SPACE-PAST");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddHours(-3),
            DateTime.UtcNow.AddHours(-2),
            5,
            30m,
            false,
            space.Id);

        await _factory.SeedAsync(space, session);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid()));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_session_state");
    }

    [Fact]
    public async Task CreateSession_WithCoachFromDifferentActivity_Returns400()
    {
        await _factory.ResetDatabaseAsync();

        ActivitySportive activity1 = new("Yoga", "Flow");
        ActivitySportive activity2 = new("Boxe", "Combat");
        Coach coach = new("Coach A", "coach@test.local", activity1.Id);
        Space space = CreateActiveSpace("SPACE-COACH");

        await _factory.SeedAsync(activity1, activity2, coach, space);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.CoachingGroupe,
            dateDebut = DateTime.UtcNow.AddDays(4),
            dateFin = DateTime.UtcNow.AddDays(4).AddHours(1),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            isOpenSession = false,
            spaceId = space.Id,
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
        Space space = CreateActiveSpace("SPACE-UNKNOWN");
        ActivitySportive activity = new("Yoga", "Flow");
        await _factory.SeedAsync(space, activity);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/sessions/{Guid.NewGuid()}", new
        {
            dateDebut = DateTime.UtcNow.AddDays(3),
            dateFin = DateTime.UtcNow.AddDays(3).AddHours(1),
            capacite = 5,
            prix = 15m,
            abonnementRequis = false,
            isOpenSession = false,
            spaceId = space.Id,
            activityId = activity.Id,
            coachId = (Guid?)null
        });

        await AssertErrorAsync(response, HttpStatusCode.NotFound, "session_not_found");
    }

    [Fact]
    public async Task GetCalendar_ReturnsCurrentUserSchedule()
    {
        await _factory.ResetDatabaseAsync();

        Guid currentUserId = Guid.NewGuid();
        ActivitySportive activity = new("Pilates", "Core training");
        Space space = CreateActiveSpace("SPACE-CALENDAR", "Pilates Room");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(6),
            DateTime.UtcNow.AddDays(6).AddHours(1),
            10,
            22m,
            false,
            space.Id,
            activity.Id);
        Reservation currentUserReservation = new(currentUserId, session.Id);
        Reservation otherUserReservation = new(Guid.NewGuid(), session.Id);

        await _factory.SeedAsync(activity, space, session, currentUserReservation, otherUserReservation);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(currentUserId));

        HttpResponseMessage response = await client.GetAsync("/api/calendar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<CalendarItemResponse>? payload = await response.Content.ReadFromJsonAsync<List<CalendarItemResponse>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Single(payload!);
        Assert.Equal(session.Id, payload[0].SessionId);
        Assert.Equal("Pilates", payload[0].ActivityName);
        Assert.Equal("Pilates Room", payload[0].SpaceName);
        Assert.Equal(session.DateDebut, payload[0].DateDebut);
        Assert.Equal(session.DateFin, payload[0].DateFin);
        Assert.Equal(SessionType.CoachingGroupe, payload[0].Type);
    }

    private static Space CreateActiveSpace(string code, string? name = null)
    {
        return new Space(
            name ?? code,
            code,
            "Test space",
            SpaceType.MultiPurposeRoom,
            50,
            false,
            true);
    }

    private async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode, string expectedError)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);

        ErrorResponse? payload = await response.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Equal((int)expectedStatusCode, payload!.StatusCode);
        Assert.Equal(expectedError, payload.Error);
        Assert.False(string.IsNullOrWhiteSpace(payload.Message));
    }

    private sealed class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    private sealed class SessionResponse
    {
        public Guid Id { get; set; }
        public bool IsOpenSession { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string SpaceName { get; set; } = string.Empty;
    }

    private sealed class CalendarItemResponse
    {
        public Guid SessionId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string SpaceName { get; set; } = string.Empty;
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public SessionType Type { get; set; }
    }
}
