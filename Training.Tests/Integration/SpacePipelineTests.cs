using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

public class SpacePipelineTests : IClassFixture<TrainingApiFactory>
{
    private readonly TrainingApiFactory _factory;

    public SpacePipelineTests(TrainingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateSpace_WithValidPayload_Returns201()
    {
        await _factory.ResetDatabaseAsync();
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/spaces", new
        {
            name = "Salle Danse 1",
            code = "DANCE-1",
            description = "Studio principal",
            type = SpaceType.DanceRoom,
            capacity = 25,
            supportsSeatManagement = false,
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        SpaceResponse? payload = await response.Content.ReadFromJsonAsync<SpaceResponse>();

        Assert.NotNull(payload);
        Assert.Equal("Salle Danse 1", payload!.Name);
        Assert.Equal("DANCE-1", payload.Code);
        Assert.Equal(SpaceType.DanceRoom, payload.Type);
        Assert.Equal(25, payload.Capacity);
        Assert.False(payload.SupportsSeatManagement);
        Assert.True(payload.IsActive);
    }

    [Fact]
    public async Task CreateSpace_WithDuplicateCode_Returns409()
    {
        await _factory.ResetDatabaseAsync();
        Space existingSpace = new(
            "Salle Conference A",
            "CONF-A",
            "Salle existante",
            SpaceType.ConferenceRoom,
            80,
            false,
            true);

        await _factory.SeedAsync(existingSpace);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/spaces", new
        {
            name = "Salle Conference B",
            code = "CONF-A",
            description = "Doublon de code",
            type = SpaceType.ConferenceRoom,
            capacity = 60,
            supportsSeatManagement = false,
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetSpaceById_WithExistingSpace_Returns200()
    {
        await _factory.ResetDatabaseAsync();
        Space space = new(
            "Terrain Basket 1",
            "BASKET-1",
            "Terrain interieur",
            SpaceType.BasketballCourt,
            20,
            false,
            true);

        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.GetAsync($"/api/spaces/{space.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSpace_WithExistingSpace_Returns200()
    {
        await _factory.ResetDatabaseAsync();
        Space space = new(
            "Gym Room A",
            "GYM-A",
            "Musculation",
            SpaceType.GymRoom,
            30,
            false,
            true);

        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/spaces/{space.Id}", new
        {
            name = "Gym Room B",
            code = "GYM-B",
            description = "Musculation premium",
            type = SpaceType.GymRoom,
            capacity = 35,
            supportsSeatManagement = false,
            isActive = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        SpaceResponse? payload = await response.Content.ReadFromJsonAsync<SpaceResponse>();

        Assert.NotNull(payload);
        Assert.Equal("Gym Room B", payload!.Name);
        Assert.Equal("GYM-B", payload.Code);
        Assert.Equal(35, payload.Capacity);
    }

    [Fact]
    public async Task DeleteSpace_WithExistingSpace_Returns204()
    {
        await _factory.ResetDatabaseAsync();
        Space space = new(
            "Cinema Room 1",
            "CINEMA-1",
            "Projection",
            SpaceType.CinemaRoom,
            100,
            true,
            true);

        await _factory.SeedAsync(space);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.DeleteAsync($"/api/spaces/{space.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private sealed class SpaceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public SpaceType Type { get; set; }
        public int? Capacity { get; set; }
        public bool SupportsSeatManagement { get; set; }
        public bool IsActive { get; set; }
    }
}
