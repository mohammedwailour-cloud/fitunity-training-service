using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Training.Application.Common.DTOs;
using Training.Application.Spaces.DTOs;
using Training.Domain.Entities;
using Training.Domain.Enums;
using Xunit;

namespace Training.Tests.Integration;

public class SpaceTests : IClassFixture<TrainingApiFactory>
{
    private readonly TrainingApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SpaceTests(TrainingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateSpace_WithValidPayload_ReturnsCreatedAndPayload()
    {
        await _factory.ResetDatabaseAsync();
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/spaces", new
        {
            name = "Salle Danse 1",
            code = "DANCE-1",
            description = "Studio principal",
            type = SpaceType.MultiPurposeRoom,
            capacity = 25,
            supportsSeatManagement = false,
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        SpaceResponse? payload = await response.Content.ReadFromJsonAsync<SpaceResponse>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Equal("Salle Danse 1", payload!.Name);
        Assert.Equal("DANCE-1", payload.Code);
        Assert.Equal(25, payload.Capacity);
        Assert.True(payload.IsActive);
    }

    [Fact]
    public async Task CreateSpace_WithoutAuthentication_ReturnsUnauthorized()
    {
        await _factory.ResetDatabaseAsync();
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/spaces", new
        {
            name = "Salle Danse 1",
            code = "DANCE-401",
            description = "Studio principal",
            type = SpaceType.MultiPurposeRoom,
            capacity = 25,
            supportsSeatManagement = false,
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSpace_WithUserRole_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "User"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/spaces", new
        {
            name = "Salle Danse 1",
            code = "DANCE-403",
            description = "Studio principal",
            type = SpaceType.MultiPurposeRoom,
            capacity = 25,
            supportsSeatManagement = false,
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetSpaces_WithCoachRole_ReturnsOk()
    {
        await _factory.ResetDatabaseAsync();
        Space activeSpace = IntegrationTestHelper.CreateActiveSpace("COACH-SPACE");
        await _factory.SeedAsync(activeSpace);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Coach"));

        HttpResponseMessage response = await client.GetAsync("/api/spaces?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PagedResult<SpaceResponse>? payload = await response.Content.ReadFromJsonAsync<PagedResult<SpaceResponse>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Single(payload!.Data);
    }

    [Fact]
    public async Task GetSpaces_WithUserRole_ReturnsOk()
    {
        await _factory.ResetDatabaseAsync();
        Space activeSpace = IntegrationTestHelper.CreateActiveSpace("USER-SPACE");
        await _factory.SeedAsync(activeSpace);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "User"));

        HttpResponseMessage response = await client.GetAsync("/api/spaces?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PagedResult<SpaceResponse>? payload = await response.Content.ReadFromJsonAsync<PagedResult<SpaceResponse>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Single(payload!.Data);
    }

    [Fact]
    public async Task CreateSpace_WithInvalidName_ReturnsBadRequest()
    {
        await _factory.ResetDatabaseAsync();
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/spaces", new
        {
            name = "",
            code = "INVALID-1",
            description = "Studio principal",
            type = SpaceType.MultiPurposeRoom,
            capacity = 25,
            supportsSeatManagement = false,
            isActive = true
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.BadRequest, "invalid_space_name", _jsonOptions);
    }

    [Fact]
    public async Task CreateSpace_WithDuplicateCode_ReturnsConflict()
    {
        await _factory.ResetDatabaseAsync();
        Space existingSpace = IntegrationTestHelper.CreateActiveSpace("DUPLICATE-1");
        await _factory.SeedAsync(existingSpace);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/spaces", new
        {
            name = "Salle Danse 2",
            code = "duplicate-1",
            description = "Studio principal",
            type = SpaceType.MultiPurposeRoom,
            capacity = 25,
            supportsSeatManagement = false,
            isActive = true
        });

        await IntegrationTestHelper.AssertErrorAsync(response, HttpStatusCode.Conflict, "space_code_already_exists", _jsonOptions);
    }

    [Fact]
    public async Task GetSpaces_FiltersInactiveSpaces()
    {
        await _factory.ResetDatabaseAsync();
        Space activeSpace = IntegrationTestHelper.CreateActiveSpace("ACTIVE-1");
        Space inactiveSpace = new(
            "Inactive Space",
            "INACTIVE-1",
            "Should be filtered",
            SpaceType.MultiPurposeRoom,
            20,
            false,
            false);

        await _factory.SeedAsync(activeSpace, inactiveSpace);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "Admin"));

        HttpResponseMessage response = await client.GetAsync("/api/spaces?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PagedResult<SpaceResponse>? payload = await response.Content.ReadFromJsonAsync<PagedResult<SpaceResponse>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Single(payload!.Data);
        Assert.Contains(payload.Data, item => item.Code == "ACTIVE-1");
        Assert.DoesNotContain(payload.Data, item => item.Code == "INACTIVE-1");
    }
}
