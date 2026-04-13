using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Training.Infrastructure.Persistence;

namespace Training.Tests.Integration;

public class TrainingApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"training-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(TrainingDbContext));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(DbContextOptions<TrainingDbContext>));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<TrainingDbContext>));

            services.AddDbContext<TrainingDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TrainingDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task SeedAsync(params object[] entities)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TrainingDbContext>();
        await dbContext.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }
}
