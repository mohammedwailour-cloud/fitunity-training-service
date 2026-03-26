using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Training.Domain.Entities;

namespace Training.Infrastructure.Persistence
{
    public class TrainingDbContext : DbContext
    {
        public TrainingDbContext(DbContextOptions<TrainingDbContext> options)
            : base(options)
        {
        }

        public DbSet<ActivitySportive> Activities { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Coach> Coaches { get; set; }
    }
}
