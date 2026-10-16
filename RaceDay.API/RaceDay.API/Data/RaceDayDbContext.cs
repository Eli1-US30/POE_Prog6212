using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Enrolment> Enrolments { get; set; } = null!;
        public DbSet<Result> Results { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Roles
            modelBuilder.Entity<Role>().Property(r => r.RoleName).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<Role>().HasIndex(r => r.RoleName).IsUnique();
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Organiser" },
                new Role { RoleId = 2, RoleName = "Participant" });

            // Users
            modelBuilder.Entity<User>().Property(u => u.FullName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
            modelBuilder.Entity<User>().Property(u => u.ProfilePictureUrl).HasMaxLength(300);
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // Events
            modelBuilder.Entity<Event>().Property(e => e.Name).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<Event>().Property(e => e.Description).HasMaxLength(1000);
            modelBuilder.Entity<Event>().Property(e => e.EventDate).HasColumnType("date");
            modelBuilder.Entity<Event>().Property(e => e.Location).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<Event>().Property(e => e.DistanceKm).HasPrecision(6, 2);
            modelBuilder.Entity<Event>().Property(e => e.EventType).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<Event>().Property(e => e.BannerImageUrl).HasMaxLength(300);
            modelBuilder.Entity<Event>().ToTable(t => t.HasCheckConstraint("CK_Events_EventType", "[EventType] IN ('run', 'walk', 'cycle')"));

            // Categories
            modelBuilder.Entity<Category>().Property(c => c.CategoryName).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Category>().Property(c => c.DistanceKm).HasPrecision(6, 2);

            // Enrolments
            modelBuilder.Entity<Enrolment>().Property(e => e.EnrolmentDate).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Enrolment>().Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Confirmed");
            modelBuilder.Entity<Enrolment>().HasIndex(e => new { e.ParticipantId, e.EventId }).IsUnique();
            modelBuilder.Entity<Enrolment>().ToTable(t => t.HasCheckConstraint("CK_Enrolments_Status", "[Status] IN ('Confirmed', 'Pending', 'Cancelled')"));

            // Results
            modelBuilder.Entity<Result>().HasIndex(r => r.EnrolmentId).IsUnique();
            modelBuilder.Entity<Result>().Property(r => r.RecordedAt).HasDefaultValueSql("GETDATE()");

            // No cascading deletes (matches Part 1 and avoids SQL Server "multiple cascade paths" errors)
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }

}