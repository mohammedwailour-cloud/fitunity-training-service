using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Training.Application.Calendar.DTOs;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

public class CalendarTests : IClassFixture<TrainingApiFactory>
{
    private readonly TrainingApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public CalendarTests(TrainingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCalendar_ReturnsUserReservationsAndEvents()
    {
        await _factory.ResetDatabaseAsync();

        Guid currentUserId = Guid.NewGuid();
        ActivitySportive activity = new("Pilates", "Core training");
        Space space = IntegrationTestHelper.CreateActiveSpace("CALENDAR-SPACE", "Pilates Room");
        Event ev = new(
            "Open Day",
            "Club event",
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(5).AddHours(2),
            50,
            space.Id);
        Session session1 = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(6),
            DateTime.UtcNow.AddDays(6).AddHours(1),
            10,
            22m,
            false,
            space.Id,
            activity.Id);
        Session session2 = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(7),
            DateTime.UtcNow.AddDays(7).AddHours(1),
            10,
            22m,
            false,
            space.Id,
            activity.Id);

        Reservation currentUserReservation = new(currentUserId, session1.Id);
        Reservation otherUserReservation = new(Guid.NewGuid(), session2.Id);

        await _factory.SeedAsync(activity, space, ev, session1, session2, currentUserReservation, otherUserReservation);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(currentUserId));

        HttpResponseMessage response = await client.GetAsync("/api/calendar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<CalendarItemDto>? payload = await response.Content.ReadFromJsonAsync<List<CalendarItemDto>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Count);
        Assert.Equal(ev.Id, payload[0].Id);
        Assert.Equal("event", payload[0].Type);
        Assert.Equal("Open Day", payload[0].Title);
        Assert.Equal("Pilates Room", payload[0].SpaceName);
        Assert.Null(payload[0].ActivityName);
        Assert.Equal(session1.Id, payload[1].Id);
        Assert.Equal("session", payload[1].Type);
        Assert.Equal("Pilates", payload[1].Title);
        Assert.Equal("Pilates", payload[1].ActivityName);
        Assert.Equal("Pilates Room", payload[1].SpaceName);
    }

    [Fact]
    public async Task GetCalendar_WithoutAuthentication_ReturnsUnauthorized()
    {
        await _factory.ResetDatabaseAsync();

        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/calendar");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCalendar_WithUnauthorizedRole_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Coach"));

        HttpResponseMessage response = await client.GetAsync("/api/calendar");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCalendar_WithoutReservations_ReturnsEmptyList()
    {
        await _factory.ResetDatabaseAsync();

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid()));

        HttpResponseMessage response = await client.GetAsync("/api/calendar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<CalendarItemDto>? payload = await response.Content.ReadFromJsonAsync<List<CalendarItemDto>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Empty(payload!);
    }
}
