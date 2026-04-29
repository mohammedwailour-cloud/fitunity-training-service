using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Training.Application.Reservations.DTOs;
using Training.Application.Common.DTOs;
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
    public async Task ReserveSession_WithAdminRole_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();

        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-ADMIN-FORBIDDEN");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            false,
            space.Id);

        await _factory.SeedAsync(space, session);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ReserveSession_WithoutAuthentication_ReturnsUnauthorized()
    {
        await _factory.ResetDatabaseAsync();

        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-401");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            false,
            space.Id);

        await _factory.SeedAsync(space, session);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

    [Fact]
    public async Task ReserveSession_WithoutSubscription_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();

        Guid userId = Guid.NewGuid();
        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-SUB");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            true,
            space.Id);

        await _factory.SeedAsync(space, session);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            IntegrationTestHelper.CreateJwt(userId, "User", "INACTIVE"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/reservations", new
        {
            sessionId = session.Id
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.Forbidden, "subscription_required", _jsonOptions);
    }

    [Fact]
    public async Task CancelReservation_FromAnotherUser_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();

        Guid ownerUserId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-OWNER");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            false,
            space.Id);
        Reservation reservation = new(ownerUserId, session.Id);

        await _factory.SeedAsync(space, session, reservation);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(otherUserId, "User"));

        HttpResponseMessage response = await client.PatchAsync($"/api/reservations/{reservation.Id}/cancel", null);

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.Forbidden, "invalid_reservation_user", _jsonOptions);
    }

    [Fact]
    public async Task CancelReservation_FromOwner_ReturnsNoContent()
    {
        await _factory.ResetDatabaseAsync();

        Guid ownerUserId = Guid.NewGuid();
        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-CANCEL-OWN");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            false,
            space.Id);
        Reservation reservation = new(ownerUserId, session.Id);

        await _factory.SeedAsync(space, session, reservation);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(ownerUserId, "User"));

        HttpResponseMessage response = await client.PatchAsync($"/api/reservations/{reservation.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CancelReservation_WithAdminRole_ReturnsNoContent()
    {
        await _factory.ResetDatabaseAsync();

        Guid ownerUserId = Guid.NewGuid();
        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-CANCEL-ADMIN");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            false,
            space.Id);
        Reservation reservation = new(ownerUserId, session.Id);

        await _factory.SeedAsync(space, session, reservation);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PatchAsync($"/api/reservations/{reservation.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetReservations_WithUserRole_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "User"));

        HttpResponseMessage response = await client.GetAsync("/api/reservations?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMyReservations_WithUserRole_ReturnsOk()
    {
        await _factory.ResetDatabaseAsync();

        Guid userId = Guid.NewGuid();
        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-ME");
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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(userId, "User"));

        HttpResponseMessage response = await client.GetAsync("/api/reservations/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<ReservationResponse>? payload = await response.Content.ReadFromJsonAsync<List<ReservationResponse>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Single(payload!);
    }

    [Fact]
    public async Task GetReservations_WithAdminRole_ReturnsPagedReservations()
    {
        await _factory.ResetDatabaseAsync();

        Space space = IntegrationTestHelper.CreateActiveSpace("RESERVE-ADMIN");
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            5,
            30m,
            false,
            space.Id);
        Reservation reservation = new(Guid.NewGuid(), session.Id);

        await _factory.SeedAsync(space, session, reservation);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.GetAsync("/api/reservations?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PagedResult<ReservationResponse>? payload = await response.Content.ReadFromJsonAsync<PagedResult<ReservationResponse>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Single(payload!.Data);
    }
}
