using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Training.Infrastructure.Persistence
{
    public class TrainingDbContextFactory : IDesignTimeDbContextFactory<TrainingDbContext>
    {
        public TrainingDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TrainingDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=localhost\\SQLEXPRESS;Database=FitUnityTraining;Trusted_Connection=True;TrustServerCertificate=True"
            );

            return new TrainingDbContext(optionsBuilder.Options);
        }
    }
}