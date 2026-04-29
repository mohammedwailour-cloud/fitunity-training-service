using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Training.Application.Common.DTOs;
using Training.Application.Events.DTOs;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

public class EventTests : IClassFixture<TrainingApiFactory>
{
    private readonly TrainingApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public EventTests(TrainingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateEvent_WithValidPayload_ReturnsCreatedAndPayload()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("EVENT-SUCCESS");
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
            capacite = 40,
            spaceId = space.Id
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        Guid? eventId = await createResponse.Content.ReadFromJsonAsync<Guid?>(_jsonOptions);
        Assert.NotNull(eventId);

        HttpResponseMessage getResponse = await client.GetAsync($"/api/events/{eventId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        EventDto? payload = await getResponse.Content.ReadFromJsonAsync<EventDto>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Equal("Conference", payload!.Titre);
        Assert.Equal(space.Id, payload.SpaceId);
        Assert.Equal(space.Name, payload.SpaceName);
    }

    [Fact]
    public async Task GetEvents_WithUserRole_ReturnsOk()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("EVENT-GET");
        Event ev = new(
            "Conference",
            "Planning annuel",
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(5).AddHours(2),
            40,
            space.Id);

        await _factory.SeedAsync(space, ev);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "User"));

        HttpResponseMessage response = await client.GetAsync("/api/events?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PagedResult<EventDto>? payload = await response.Content.ReadFromJsonAsync<PagedResult<EventDto>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Single(payload!.Data);
    }

    [Fact]
    public async Task CreateEvent_WithUserRole_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("EVENT-403");
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "User"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
            capacite = 40,
            spaceId = space.Id
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseAsync();
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.DeleteAsync($"/api/events/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        OperationNotSupportedResponse? payload = await response.Content.ReadFromJsonAsync<OperationNotSupportedResponse>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Equal("operation_not_supported", payload!.Error);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidDates_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("EVENT-DATES");
        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(4),
            capacite = 40,
            spaceId = space.Id
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_event_dates", _jsonOptions);
    }

    [Fact]
    public async Task CreateEvent_WithOverlapOnSameSpace_ReturnsConflict()
    {
        await _factory.ResetDatabaseAsync();
        Space space = IntegrationTestHelper.CreateActiveSpace("EVENT-OVERLAP");
        Event existingEvent = new(
            "Existing Event",
            "Overlap",
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(5).AddHours(2),
            40,
            space.Id);

        await _factory.SeedAsync(space, existingEvent);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5).AddMinutes(30),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(3),
            capacite = 40,
            spaceId = space.Id
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.Conflict, "space_unavailable", _jsonOptions);
    }

    [Fact]
    public async Task CreateEvent_WithInactiveSpace_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseAsync();
        Space inactiveSpace = new(
            "Inactive Space",
            "EVENT-INACTIVE",
            "Inactive",
            SpaceType.MultiPurposeRoom,
            20,
            false,
            false);

        await _factory.SeedAsync(inactiveSpace);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/events", new
        {
            titre = "Conference",
            description = "Planning annuel",
            dateDebut = DateTime.UtcNow.AddDays(5),
            dateFin = DateTime.UtcNow.AddDays(5).AddHours(2),
            capacite = 10,
            spaceId = inactiveSpace.Id
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.BadRequest, "space_inactive", _jsonOptions);
    }

    private sealed class OperationNotSupportedResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
