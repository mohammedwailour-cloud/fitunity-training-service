using Microsoft.EntityFrameworkCore;
using Training.Domain.Entities;
using Training.Domain.Enums;

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
        public DbSet<Space> Spaces { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Space>(entity =>
            {
                entity.HasKey(space => space.Id);

                entity.Property(space => space.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(space => space.Code)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(space => space.Code)
                    .IsUnique();

                entity.Property(space => space.Description)
                    .HasMaxLength(500);

                entity.Property(space => space.Type)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(space => space.Capacity)
                    .IsRequired(false);

                entity.Property(space => space.SupportsSeatManagement)
                    .IsRequired();

                entity.Property(space => space.IsActive)
                    .IsRequired();
            });

            modelBuilder.Entity<ActivitySportive>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Nom)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(a => a.Description)
                      .HasMaxLength(500);

                entity.HasMany(a => a.Coaches)
                      .WithOne(c => c.Activity)
                      .HasForeignKey(c => c.ActivityId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.Sessions)
                      .WithOne(s => s.Activity)
                      .HasForeignKey(s => s.ActivityId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Coach>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Nom)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(c => c.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.HasOne(c => c.Activity)
                      .WithMany(a => a.Coaches)
                      .HasForeignKey(c => c.ActivityId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Titre)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(e => e.Description)
                      .HasMaxLength(1000);

                entity.Property(e => e.DateDebut)
                      .IsRequired();

                entity.Property(e => e.DateFin)
                      .IsRequired();

                entity.Property(e => e.Capacite)
                      .IsRequired();

                entity.Property(e => e.SpaceId)
                      .IsRequired();

                entity.HasOne(e => e.Space)
                      .WithMany()
                      .HasForeignKey(e => e.SpaceId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Sessions)
                      .WithOne(s => s.Event)
                      .HasForeignKey(s => s.EventId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Type)
                      .IsRequired()
                      .HasConversion<string>();

                entity.Property(s => s.DateDebut)
                      .IsRequired();

                entity.Property(s => s.DateFin)
                      .IsRequired();

                entity.Property(s => s.Capacite);

                entity.Property(s => s.Prix)
                      .HasColumnType("decimal(18,2)");

                entity.Property(s => s.AbonnementRequis)
                      .IsRequired();

                entity.Property(s => s.SpaceId)
                      .IsRequired();

                entity.HasOne(s => s.Space)
                      .WithMany()
                      .HasForeignKey(s => s.SpaceId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Activity)
                      .WithMany(a => a.Sessions)
                      .HasForeignKey(s => s.ActivityId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(s => s.Coach)
                      .WithMany()
                      .HasForeignKey(s => s.CoachId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(s => s.Event)
                      .WithMany(e => e.Sessions)
                      .HasForeignKey(s => s.EventId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(s => s.Reservations)
                      .WithOne(r => r.Session)
                      .HasForeignKey(r => r.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.UserId)
                      .IsRequired();

                entity.Property(r => r.SessionId)
                      .IsRequired();

                entity.Property(r => r.DateReservation)
                      .IsRequired();

                entity.Property(r => r.Status)
                      .HasConversion<string>()
                      .IsRequired();

                entity.HasOne(r => r.Session)
                      .WithMany(s => s.Reservations)
                      .HasForeignKey(r => r.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => new { r.SessionId, r.UserId })
                      .IsUnique();
            });
        }
    }
}
