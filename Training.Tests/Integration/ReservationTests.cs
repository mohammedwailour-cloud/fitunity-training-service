using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Training.Application.Reservations.DTOs;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

public class ReservationTests : IClassFixture<TrainingApiFactory>
{
    private readonly TrainingApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ReservationTests(TrainingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReserveOpenSession_ReturnsOkAndPayload()
    {
        await _factory.ResetDatabaseAsync();

        Guid userId = Guid.NewGuid();
        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-OPEN");
        ActivitySportive activity = new("Musculation", "Libre");
        Session openSession = IntegrationTestHelper.CreateOpenSession(
            space,
            activity,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1));

        await _factory.SeedAsync(space, activity, openSession);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(userId));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = openSession.Id
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ReservationResponse? payload = await response.Content.ReadFromJsonAsync<ReservationResponse>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(openSession.Id, payload!.SessionId);
        Assert.Equal(userId, payload.UserId);
        Assert.Equal(ReservationStatus.EnAttente, payload.Status);
    }

    [Fact]
    public async Task ReserveSession_WithDuplicateReservation_ReturnsConflict()
    {
        await _factory.ResetDatabaseAsync();

        Guid userId = Guid.NewGuid();
        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-DUPLICATE");
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

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.Conflict, "duplicate_reservation", _jsonOptions);
    }

    [Fact]
    public async Task ReserveSession_WhenFull_ReturnsConflict()
    {
        await _factory.ResetDatabaseAsync();

        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-FULL");
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

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.Conflict, "session_full", _jsonOptions);
    }

    [Fact]
    public async Task ReservePastSession_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseAsync();

        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-PAST");
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

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_session_state", _jsonOptions);
    }
}
