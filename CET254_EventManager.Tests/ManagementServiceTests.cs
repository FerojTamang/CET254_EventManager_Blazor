using CET254_EventManager.Data;
using CET254_EventManager.Exceptions;
using CET254_EventManager.Models;
using CET254_EventManager.Services;
using CET254_EventManager.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using DomainActivity = CET254_EventManager.Models.Activity;

namespace CET254_EventManager.Tests;

public class ManagementServiceTests
{
    // Management tests cover safe create/update/delete rules for admin-facing services.
    [Fact]
    public async Task DeleteVenueAsync_WhenVenueAssignedToEvent_ThrowsAppException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var venue = new Venue("Main Hall", "1 Test Street", 100);
        var eventItem = CreateEvent("Venue Event", DateTime.Today.AddDays(2));
        await context.AddRangeAsync(venue, eventItem);
        await context.SaveChangesAsync();
        await context.EventVenues.AddAsync(new EventVenue(eventItem.Id, venue.Id));
        await context.SaveChangesAsync();
        var service = CreateVenueService(context);

        var exception = await Assert.ThrowsAsync<AppException>(() => service.DeleteVenueAsync(venue.Id));

        Assert.Equal("VENUE_IN_USE", exception.ErrorCode);
        Assert.True(await context.Venues.AnyAsync(v => v.Id == venue.Id));
    }

    [Fact]
    public async Task DeleteActivityAsync_WhenActivityAssignedToEvent_ThrowsAppException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var activity = new DomainActivity("Workshop", ActivityType.Workshop, "Workshop session");
        var eventItem = CreateEvent("Activity Event", DateTime.Today.AddDays(2));
        await context.AddRangeAsync(activity, eventItem);
        await context.SaveChangesAsync();
        await context.EventActivities.AddAsync(new EventActivity(eventItem.Id, activity.Id));
        await context.SaveChangesAsync();
        var service = CreateActivityService(context);

        var exception = await Assert.ThrowsAsync<AppException>(() => service.DeleteActivityAsync(activity.Id));

        Assert.Equal("ACTIVITY_IN_USE", exception.ErrorCode);
        Assert.True(await context.Activities.AnyAsync(a => a.Id == activity.Id));
    }

    [Fact]
    public async Task CreateParticipantAsync_WithValidParticipant_CreatesParticipant()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = CreateParticipantService(context);
        var participant = new Participant("Alex Green", "alex@example.com", "07123456789");

        var result = await service.CreateParticipantAsync(participant);

        Assert.Equal(participant.Id, result.Id);
        Assert.Single(await context.Participants.ToListAsync());
        Assert.Equal("alex@example.com", result.Email);
    }

    [Fact]
    public async Task CreateParticipantAsync_WithDuplicateEmail_ThrowsAppException()
    {
        using var context = TestDbContextFactory.CreateContext();
        await context.Participants.AddAsync(new Participant("Alex Green", "alex@example.com", "07123456789"));
        await context.SaveChangesAsync();
        var service = CreateParticipantService(context);
        var duplicate = new Participant("Sam Blue", "ALEX@example.com", "07987654321");

        var exception = await Assert.ThrowsAsync<AppException>(() => service.CreateParticipantAsync(duplicate));

        Assert.Equal("DUPLICATE_PARTICIPANT_EMAIL", exception.ErrorCode);
        Assert.Single(await context.Participants.ToListAsync());
    }

    [Fact]
    public async Task CreateParticipantAsync_WithInvalidEmail_ThrowsAppException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = CreateParticipantService(context);
        var participant = new Participant("Alex Green", "not-an-email", "07123456789");

        var exception = await Assert.ThrowsAsync<AppException>(() => service.CreateParticipantAsync(participant));

        Assert.Equal("INVALID_PARTICIPANT_EMAIL", exception.ErrorCode);
        Assert.Empty(await context.Participants.ToListAsync());
    }

    [Fact]
    public async Task CreateParticipantAsync_WithInvalidPhone_ThrowsAppException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = CreateParticipantService(context);
        var participant = new Participant("Alex Green", "alex@example.com", "not a phone number");

        var exception = await Assert.ThrowsAsync<AppException>(() => service.CreateParticipantAsync(participant));

        Assert.Equal("INVALID_PARTICIPANT_PHONE", exception.ErrorCode);
        Assert.Empty(await context.Participants.ToListAsync());
    }

    [Fact]
    public async Task UpdateParticipantAsync_WithValidData_UpdatesParticipant()
    {
        using var context = TestDbContextFactory.CreateContext();
        var participant = new Participant("Alex Green", "alex@example.com", "07123456789");
        await context.Participants.AddAsync(participant);
        await context.SaveChangesAsync();
        var service = CreateParticipantService(context);

        participant.UpdateDetails("Alex Updated", "alex.updated@example.com", "07000000000");
        await service.UpdateParticipantAsync(participant);

        var updatedParticipant = await context.Participants.FindAsync(participant.Id);
        Assert.NotNull(updatedParticipant);
        Assert.Equal("Alex Updated", updatedParticipant.FullName);
        Assert.Equal("alex.updated@example.com", updatedParticipant.Email);
        Assert.Equal("07000000000", updatedParticipant.PhoneNumber);
    }

    [Fact]
    public async Task DeleteParticipantAsync_WhenParticipantHasRegistrations_ThrowsAppException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var participant = new Participant("Alex Green", "alex@example.com", "07123456789");
        var eventItem = CreateEvent("Registered Event", DateTime.Today.AddDays(2));
        await context.AddRangeAsync(participant, eventItem);
        await context.SaveChangesAsync();
        await context.Registrations.AddAsync(new Registration(eventItem.Id, participant.Id));
        await context.SaveChangesAsync();
        var service = CreateParticipantService(context);

        var exception = await Assert.ThrowsAsync<AppException>(() => service.DeleteParticipantAsync(participant.Id));

        Assert.Equal("PARTICIPANT_HAS_REGISTRATIONS", exception.ErrorCode);
        Assert.True(await context.Participants.AnyAsync(existing => existing.Id == participant.Id));
    }

    [Fact]
    public async Task DeleteParticipantAsync_WhenNoRegistrations_DeletesParticipant()
    {
        using var context = TestDbContextFactory.CreateContext();
        var participant = new Participant("Alex Green", "alex@example.com", "07123456789");
        await context.Participants.AddAsync(participant);
        await context.SaveChangesAsync();
        var service = CreateParticipantService(context);

        await service.DeleteParticipantAsync(participant.Id);

        Assert.False(await context.Participants.AnyAsync(existing => existing.Id == participant.Id));
    }

    private static VenueService CreateVenueService(ApplicationDbContext context)
    {
        return new VenueService(TestRepositoryFactory.Create<Venue>(context));
    }

    private static ActivityService CreateActivityService(ApplicationDbContext context)
    {
        return new ActivityService(TestRepositoryFactory.Create<DomainActivity>(context));
    }

    private static ParticipantService CreateParticipantService(ApplicationDbContext context)
    {
        return new ParticipantService(TestRepositoryFactory.Create<Participant>(context));
    }

    private static Event CreateEvent(string title, DateTime eventDate)
    {
        return new Event(title, $"{title} description", eventDate, TimeSpan.FromHours(10), TimeSpan.FromHours(12));
    }
}
