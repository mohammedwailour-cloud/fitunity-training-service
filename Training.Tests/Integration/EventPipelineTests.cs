using System.Net;
using System.Net.Http.Json;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

public class EventPipelineTests : IClassFixture<TrainingApiFactory>
{
    private readonly TrainingApiFactory _factory;

    public EventPipelineTests(TrainingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateEvent_WithValidSpaceAndDates_Returns201()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateSpace("CONF-VALID", 100, true);
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference Fitness",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
            capacite = 50,
            spaceId = space.Id
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidDates_Returns400()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateSpace("CONF-DATES", 100, true);
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference Fitness",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(4),
            capacite = 50,
            spaceId = space.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_event_dates");
    }

    [Fact]
    public async Task CreateEvent_WithMissingSpace_Returns404()
    {
        await _factory.ResetDatabaseAsync();
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference Fitness",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
            capacite = 50,
            spaceId = Guid.NewGuid()
        });

        await AssertErrorAsync(response, HttpStatusCode.NotFound, "space_not_found");
    }

    [Fact]
    public async Task CreateEvent_WithInactiveSpace_Returns400()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateSpace("CONF-INACTIVE", 100, false);
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference Fitness",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
            capacite = 50,
            spaceId = space.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "space_inactive");
    }

    [Fact]
    public async Task CreateEvent_WithCapacityGreaterThanSpaceCapacity_Returns400()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateSpace("CONF-CAP", 20, true);
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference Fitness",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
            capacite = 50,
            spaceId = space.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_event_capacity");
    }

    [Fact]
    public async Task CreateEvent_OverlappingAnotherEventOnSameSpace_Returns409()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateSpace("CONF-OVERLAP-EVENT", 100, true);
        Event existingEvent = new(
            "Existing Event",
            "Overlap",
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(5).AddHours(2),
            40,
            space.Id);

        await _factory.SeedAsync(space, existingEvent);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference Fitness",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5).AddMinutes(30),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(3),
            capacite = 50,
            spaceId = space.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "space_unavailable");
    }

    [Fact]
    public async Task CreateEvent_OverlappingSessionOnSameSpace_Returns409()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateSpace("CONF-OVERLAP-SESSION", 100, true);
        Session session = new(
            SessionType.CoachingGroupe,
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(5).AddHours(2),
            20,
            30m,
            false,
            space.Id);

        await _factory.SeedAsync(space, session);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference Fitness",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5).AddMinutes(30),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(3),
            capacite = 50,
            spaceId = space.Id
        });

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "space_unavailable");
    }

    [Fact]
    public async Task CreateSession_OverlappingEventOnSameSpace_Returns409()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateSpace("CONF-SESSION-EVENT", 100, true);
        Event existingEvent = new(
            "Existing Event",
            "Overlap",
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(5).AddHours(2),
            40,
            space.Id);

        await _factory.SeedAsync(space, existingEvent);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/sessions", new
        {
            type = (int)SessionType.CoachingGroupe,
            dateDebut = DateTime.UtcNow.AddDays(5).AddMinutes(30),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(3),
            capacite = 10,
            prix = 20m,
            abonnementRequis = false,
            spaceId = space.Id,
            activityId = (Guid?)null,
            coachId = (Guid?)null,
            eventId = (Guid?)null
        });

        await AssertErrorAsync(response, HttpStatusCode.Conflict, "space_unavailable");
    }

    [Fact]
    public async Task UpdateEvent_WithSameSlot_DoesNotSelfConflict()
    {
        await _factory.ResetDatabaseAsync();
        Space space = CreateSpace("CONF-UPDATE", 100, true);
        Event existingEvent = new(
            "Existing Event",
            "Overlap",
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(5).AddHours(2),
            40,
            space.Id);

        await _factory.SeedAsync(space, existingEvent);
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/events/{existingEvent.Id}", new
        {
            titre = existingEvent.Titre,
            description = existingEvent.Description,
            dateDebut = existingEvent.DateDebut,
            dateFin = existingEvent.DateFin,
            capacite = existingEvent.Capacite,
            spaceId = existingEvent.SpaceId
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static Space CreateSpace(string code, int capacity, bool isActive)
    {
        return new Space(
            code,
            code,
            "Test space",
            SpaceType.ConferenceRoom,
            capacity,
            false,
            isActive);
    }

    private static async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode, string expectedError)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);

        ErrorResponse? payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(payload);
        Assert.Equal((int)expectedStatusCode, payload!.StatusCode);
        Assert.Equal(expectedError, payload.Error);
    }

    private sealed class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
