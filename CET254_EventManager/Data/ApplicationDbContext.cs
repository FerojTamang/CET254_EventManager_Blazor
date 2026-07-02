using CET254_EventManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CET254_EventManager.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }

        public DbSet<Participant> Participants { get; set; }

        public DbSet<Venue> Venues { get; set; }

        public DbSet<CET254_EventManager.Models.Activity> Activities { get; set; }

        public DbSet<Registration> Registrations { get; set; }

        public DbSet<EventVenue> EventVenues { get; set; }

        public DbSet<EventActivity> EventActivities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigureEvent(builder);
            ConfigureParticipant(builder);
            ConfigureVenue(builder);
            ConfigureActivity(builder);
            ConfigureRegistration(builder);
            ConfigureEventVenue(builder);
            ConfigureEventActivity(builder);
        }

        private static void ConfigureEvent(ModelBuilder builder)
        {
            builder.Entity<Event>(entity =>
            {
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.EventDate)
                    .IsRequired();

                entity.Property(e => e.StartTime)
                    .IsRequired();

                entity.Property(e => e.EndTime)
                    .IsRequired();

                entity.HasMany(e => e.Registrations)
                    .WithOne(r => r.Event)
                    .HasForeignKey(r => r.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.EventVenues)
                    .WithOne(ev => ev.Event)
                    .HasForeignKey(ev => ev.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.EventActivities)
                    .WithOne(ea => ea.Event)
                    .HasForeignKey(ea => ea.EventId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureParticipant(ModelBuilder builder)
        {
            builder.Entity<Participant>(entity =>
            {
                entity.Property(p => p.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(p => p.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(p => p.PhoneNumber)
                    .HasMaxLength(30);

                entity.Property(p => p.ApplicationUserId)
                    .HasMaxLength(450);

                entity.HasIndex(p => p.Email)
                    .IsUnique();

                entity.HasMany(p => p.Registrations)
                    .WithOne(r => r.Participant)
                    .HasForeignKey(r => r.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureVenue(ModelBuilder builder)
        {
            builder.Entity<Venue>(entity =>
            {
                entity.ToTable(tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint("CK_Venues_Capacity_Positive", "[Capacity] > 0");
                });

                entity.Property(v => v.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(v => v.Address)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(v => v.Capacity)
                    .IsRequired();

                entity.HasMany(v => v.EventVenues)
                    .WithOne(ev => ev.Venue)
                    .HasForeignKey(ev => ev.VenueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureActivity(ModelBuilder builder)
        {
            builder.Entity<CET254_EventManager.Models.Activity>(entity =>
            {
                entity.Property(a => a.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(a => a.Description)
                    .HasMaxLength(500);

                entity.Property(a => a.ActivityType)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.HasMany(a => a.EventActivities)
                    .WithOne(ea => ea.Activity)
                    .HasForeignKey(ea => ea.ActivityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureRegistration(ModelBuilder builder)
        {
            builder.Entity<Registration>(entity =>
            {
                // Registration is the junction table for the Event-Participant many-to-many relationship.
                entity.Property(r => r.EventId)
                    .IsRequired();

                entity.Property(r => r.ParticipantId)
                    .IsRequired();

                entity.Property(r => r.RegistrationDate)
                    .IsRequired();

                entity.Property(r => r.Status)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.HasIndex(r => new { r.EventId, r.ParticipantId })
                    .IsUnique();

                entity.HasOne(r => r.Event)
                    .WithMany(e => e.Registrations)
                    .HasForeignKey(r => r.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Participant)
                    .WithMany(p => p.Registrations)
                    .HasForeignKey(r => r.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureEventVenue(ModelBuilder builder)
        {
            builder.Entity<EventVenue>(entity =>
            {
                // EventVenue is the junction table for the Event-Venue many-to-many relationship.
                entity.HasKey(ev => new { ev.EventId, ev.VenueId });

                entity.HasOne(ev => ev.Event)
                    .WithMany(e => e.EventVenues)
                    .HasForeignKey(ev => ev.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ev => ev.Venue)
                    .WithMany(v => v.EventVenues)
                    .HasForeignKey(ev => ev.VenueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureEventActivity(ModelBuilder builder)
        {
            builder.Entity<EventActivity>(entity =>
            {
                // EventActivity is the junction table for the Event-Activity many-to-many relationship.
                entity.HasKey(ea => new { ea.EventId, ea.ActivityId });

                entity.HasOne(ea => ea.Event)
                    .WithMany(e => e.EventActivities)
                    .HasForeignKey(ea => ea.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ea => ea.Activity)
                    .WithMany(a => a.EventActivities)
                    .HasForeignKey(ea => ea.ActivityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
