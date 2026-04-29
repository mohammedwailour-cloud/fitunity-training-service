using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Training.Application.Activities.DTOs;
using Training.Application.Common.DTOs;
using Training.Domain.Entities;
using Xunit;

namespace Training.Tests.Integration;

public class ActivityTests : IClassFixture<TrainingApiFactory>
{
    private readonly TrainingApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ActivityTests(TrainingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetActivities_WithUserRole_ReturnsOk()
    {
        await _factory.ResetDatabaseAsync();
        ActivitySportive activity = new("Yoga", "Flow");
        await _factory.SeedAsync(activity);
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "User"));

        HttpResponseMessage response = await client.GetAsync("/api/activities?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PagedResult<ActivityResponse>? payload = await response.Content.ReadFromJsonAsync<PagedResult<ActivityResponse>>(_jsonOptions);

        Assert.NotNull(payload);
        Assert.Single(payload!.Data);
    }

    [Fact]
    public async Task CreateActivity_WithUserRole_ReturnsForbidden()
    {
        await _factory.ResetDatabaseAsync();
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestHelper.CreateJwt(Guid.NewGuid(), "User"));

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/activities", new
        {
            name = "Yoga",
            description = "Flow"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
