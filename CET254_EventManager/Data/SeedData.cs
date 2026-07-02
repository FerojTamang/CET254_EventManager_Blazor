using CET254_EventManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DomainActivity = CET254_EventManager.Models.Activity;

namespace CET254_EventManager.Data
{
    public static class SeedData
    {
        public const string AdminRole = "Admin";
        public const string UserRole = "User";
        public const string AdminEmail = "admin@eventmanager.local";
        public const string UserEmail = "user@eventmanager.local";

        private const string AdminPassword = "Admin@12345";
        private const string UserPassword = "User@12345";

        // Seed data supports repeatable testing, screenshots, and demonstration.
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedVenuesAsync(context);
            await SeedActivitiesAsync(context);
            await SeedEventsAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in new[] { AdminRole, UserRole })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    EnsureIdentityResultSucceeded(result, $"Unable to create role '{roleName}'.");
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
        {
            await EnsureUserAsync(userManager, AdminEmail, AdminPassword, AdminRole);
            await EnsureUserAsync(userManager, UserEmail, UserPassword, UserRole);
        }

        private static async Task EnsureUserAsync(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            string roleName)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, password);
                EnsureIdentityResultSucceeded(createResult, $"Unable to create user '{email}'.");
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                var updateResult = await userManager.UpdateAsync(user);
                EnsureIdentityResultSucceeded(updateResult, $"Unable to confirm email for user '{email}'.");
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var roleResult = await userManager.AddToRoleAsync(user, roleName);
                EnsureIdentityResultSucceeded(roleResult, $"Unable to assign role '{roleName}' to user '{email}'.");
            }
        }

        private static async Task SeedVenuesAsync(ApplicationDbContext context)
        {
            var venues = new[]
            {
                new Venue("Riverside Community Hall", "12 River Road, City Centre", 120),
                new Venue("Greenfield Library Auditorium", "45 Greenfield Avenue", 80),
                new Venue("Northside Sports Centre", "8 Northside Lane", 200)
            };

            foreach (var venue in venues)
            {
                if (!await context.Venues.AnyAsync(existing => existing.Name == venue.Name))
                {
                    await context.Venues.AddAsync(venue);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedActivitiesAsync(ApplicationDbContext context)
        {
            var activities = new[]
            {
                new DomainActivity("Digital Skills Workshop", ActivityType.Workshop, "Hands-on digital confidence session for residents."),
                new DomainActivity("Community Safety Talk", ActivityType.Talk, "A local safety and awareness presentation."),
                new DomainActivity("Family Games Session", ActivityType.Game, "Inclusive games and team activities for all ages."),
                new DomainActivity("Volunteer Training", ActivityType.Training, "Training for new and returning community volunteers."),
                new DomainActivity("Neighbourhood Networking", ActivityType.Networking, "Meet local groups, organisers, and residents.")
            };

            foreach (var activity in activities)
            {
                if (!await context.Activities.AnyAsync(existing => existing.Name == activity.Name))
                {
                    await context.Activities.AddAsync(activity);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedEventsAsync(ApplicationDbContext context)
        {
            var venues = await context.Venues.ToDictionaryAsync(venue => venue.Name);
            var activities = await context.Activities.ToDictionaryAsync(activity => activity.Name);

            var eventSeeds = new[]
            {
                new EventSeed(
                    "Community Welcome Day",
                    "A friendly event introducing residents to local services, activities, and volunteer groups.",
                    DateTime.Today.AddDays(7),
                    new TimeSpan(10, 0, 0),
                    new TimeSpan(14, 0, 0),
                    "Riverside Community Hall",
                    new[] { "Neighbourhood Networking", "Community Safety Talk" }),
                new EventSeed(
                    "Digital Confidence Morning",
                    "A practical morning session helping attendees improve everyday digital skills.",
                    DateTime.Today.AddDays(14),
                    new TimeSpan(9, 30, 0),
                    new TimeSpan(12, 30, 0),
                    "Greenfield Library Auditorium",
                    new[] { "Digital Skills Workshop", "Volunteer Training" }),
                new EventSeed(
                    "Family Activity Afternoon",
                    "An accessible afternoon of games and social activities for families.",
                    DateTime.Today.AddDays(21),
                    new TimeSpan(13, 0, 0),
                    new TimeSpan(16, 30, 0),
                    "Northside Sports Centre",
                    new[] { "Family Games Session", "Neighbourhood Networking" }),
                new EventSeed(
                    "Volunteer Preparation Evening",
                    "An evening event for people interested in supporting future community programmes.",
                    DateTime.Today.AddDays(30),
                    new TimeSpan(18, 0, 0),
                    new TimeSpan(20, 0, 0),
                    "Riverside Community Hall",
                    new[] { "Volunteer Training", "Community Safety Talk" })
            };

            foreach (var eventSeed in eventSeeds)
            {
                var eventItem = await context.Events
                    .FirstOrDefaultAsync(existing => existing.Title == eventSeed.Title);

                if (eventItem is null)
                {
                    eventItem = new Event(
                        eventSeed.Title,
                        eventSeed.Description,
                        eventSeed.EventDate,
                        eventSeed.StartTime,
                        eventSeed.EndTime);

                    await context.Events.AddAsync(eventItem);
                    await context.SaveChangesAsync();
                }
                else
                {
                    eventItem.UpdateDetails(
                        eventSeed.Title,
                        eventSeed.Description,
                        eventSeed.EventDate,
                        eventSeed.StartTime,
                        eventSeed.EndTime);
                }

                await EnsureEventVenueAsync(context, eventItem.Id, venues[eventSeed.VenueName].Id);
                await EnsureEventActivitiesAsync(context, eventItem.Id, eventSeed.ActivityNames, activities);
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureEventVenueAsync(ApplicationDbContext context, int eventId, int venueId)
        {
            var exists = await context.EventVenues
                .AnyAsync(eventVenue => eventVenue.EventId == eventId && eventVenue.VenueId == venueId);

            if (!exists)
            {
                await context.EventVenues.AddAsync(new EventVenue(eventId, venueId));
            }
        }

        private static async Task EnsureEventActivitiesAsync(
            ApplicationDbContext context,
            int eventId,
            IEnumerable<string> activityNames,
            IReadOnlyDictionary<string, DomainActivity> activities)
        {
            foreach (var activityName in activityNames)
            {
                var activityId = activities[activityName].Id;
                var exists = await context.EventActivities
                    .AnyAsync(eventActivity => eventActivity.EventId == eventId && eventActivity.ActivityId == activityId);

                if (!exists)
                {
                    await context.EventActivities.AddAsync(new EventActivity(eventId, activityId));
                }
            }
        }

        private static void EnsureIdentityResultSucceeded(IdentityResult result, string message)
        {
            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"{message} {errors}");
        }

        private sealed record EventSeed(
            string Title,
            string Description,
            DateTime EventDate,
            TimeSpan StartTime,
            TimeSpan EndTime,
            string VenueName,
            IReadOnlyCollection<string> ActivityNames);
    }
}
