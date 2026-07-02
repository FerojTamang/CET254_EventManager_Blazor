using CET254_EventManager.Data;
using CET254_EventManager.Exceptions;
using CET254_EventManager.Models;
using CET254_EventManager.Services;
using CET254_EventManager.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace CET254_EventManager.Tests;

public class RegistrationServiceTests
{
    // Registration tests cover business rules and exception paths.
    [Fact]
    public async Task RegisterAsync_WithValidEventAndParticipant_CreatesPendingRegistration()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Future Event", DateTime.Today.AddDays(7));
        var participant = CreateParticipant("Alex Green", "alex@example.com");
        await context.AddRangeAsync(eventItem, participant);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var registration = await service.RegisterAsync(eventItem.Id, participant.Id);

        Assert.Equal(eventItem.Id, registration.EventId);
        Assert.Equal(participant.Id, registration.ParticipantId);
        Assert.Equal(RegistrationStatus.Pending, registration.Status);
        Assert.Single(await context.Registrations.ToListAsync());
    }

    [Fact]
    public async Task RegisterAsync_WithMissingEvent_ThrowsEventNotFoundException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var participant = CreateParticipant("Alex Green", "alex@example.com");
        await context.Participants.AddAsync(participant);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<EventNotFoundException>(() => service.RegisterAsync(999, participant.Id));
    }

    [Fact]
    public async Task RegisterAsync_WithMissingParticipant_ThrowsParticipantNotFoundException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Future Event", DateTime.Today.AddDays(7));
        await context.Events.AddAsync(eventItem);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ParticipantNotFoundException>(() => service.RegisterAsync(eventItem.Id, 999));
    }

    [Fact]
    public async Task RegisterAsync_ForPastEvent_ThrowsPastEventRegistrationException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Past Event", DateTime.Today.AddDays(-2));
        var participant = CreateParticipant("Alex Green", "alex@example.com");
        await context.AddRangeAsync(eventItem, participant);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<PastEventRegistrationException>(() => service.RegisterAsync(eventItem.Id, participant.Id));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateActiveRegistration_ThrowsDuplicateRegistrationException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Future Event", DateTime.Today.AddDays(7));
        var participant = CreateParticipant("Alex Green", "alex@example.com");
        await context.AddRangeAsync(eventItem, participant);
        await context.SaveChangesAsync();
        await context.Registrations.AddAsync(new Registration(eventItem.Id, participant.Id));
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<DuplicateRegistrationException>(() => service.RegisterAsync(eventItem.Id, participant.Id));

        Assert.Equal("You have already registered for Future Event.", exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_WhenCancelledRegistrationExists_ReactivatesExistingRegistration()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Future Event", DateTime.Today.AddDays(7));
        var participant = CreateParticipant("Alex Green", "alex@example.com");
        await context.AddRangeAsync(eventItem, participant);
        await context.SaveChangesAsync();
        var cancelledRegistration = new Registration(eventItem.Id, participant.Id);
        cancelledRegistration.Cancel();
        var originalRegistrationDate = cancelledRegistration.RegistrationDate;
        await context.Registrations.AddAsync(cancelledRegistration);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var registration = await service.RegisterAsync(eventItem.Id, participant.Id);
        var registrations = await context.Registrations.ToListAsync();

        Assert.Equal(cancelledRegistration.Id, registration.Id);
        Assert.Single(registrations);
        Assert.Equal(RegistrationStatus.Pending, registration.Status);
        Assert.True(registration.RegistrationDate >= originalRegistrationDate);
    }

    [Fact]
    public async Task RegisterAsync_WhenVenueCapacityFull_ThrowsVenueCapacityExceededException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Small Event", DateTime.Today.AddDays(7));
        var venue = new Venue("Small Venue", "1 Test Street", 1);
        var registeredParticipant = CreateParticipant("Alex Green", "alex@example.com");
        var secondParticipant = CreateParticipant("Sam Blue", "sam@example.com");
        await context.AddRangeAsync(eventItem, venue, registeredParticipant, secondParticipant);
        await context.SaveChangesAsync();
        await context.EventVenues.AddAsync(new EventVenue(eventItem.Id, venue.Id));
        var existingRegistration = new Registration(eventItem.Id, registeredParticipant.Id);
        existingRegistration.Confirm();
        await context.Registrations.AddAsync(existingRegistration);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<VenueCapacityExceededException>(() => service.RegisterAsync(eventItem.Id, secondParticipant.Id));
    }

    [Fact]
    public async Task CancelRegistrationAsync_WithValidOwner_ChangesStatusToCancelled()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Future Event", DateTime.Today.AddDays(7));
        var participant = CreateParticipant("Alex Green", "alex@example.com");
        await context.AddRangeAsync(eventItem, participant);
        await context.SaveChangesAsync();
        var registration = new Registration(eventItem.Id, participant.Id);
        await context.Registrations.AddAsync(registration);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await service.CancelRegistrationAsync(registration.Id, participant.Id);

        Assert.Equal(RegistrationStatus.Cancelled, registration.Status);
        Assert.NotNull(registration.UpdatedAt);
    }

    [Fact]
    public async Task ConfirmRegistrationAsync_WhenAlreadyConfirmed_DoesNotThrow()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Future Event", DateTime.Today.AddDays(7));
        var participant = CreateParticipant("Alex Green", "alex@example.com");
        await context.AddRangeAsync(eventItem, participant);
        await context.SaveChangesAsync();
        var registration = new Registration(eventItem.Id, participant.Id);
        registration.Confirm();
        await context.Registrations.AddAsync(registration);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await service.ConfirmRegistrationAsync(registration.Id);

        Assert.Equal(RegistrationStatus.Confirmed, registration.Status);
    }

    [Fact]
    public async Task ConfirmRegistrationAsync_WhenCancelled_ThrowsInvalidRegistrationStatusException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Future Event", DateTime.Today.AddDays(7));
        var participant = CreateParticipant("Alex Green", "alex@example.com");
        await context.AddRangeAsync(eventItem, participant);
        await context.SaveChangesAsync();
        var registration = new Registration(eventItem.Id, participant.Id);
        registration.Cancel();
        await context.Registrations.AddAsync(registration);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidRegistrationStatusException>(() => service.ConfirmRegistrationAsync(registration.Id));

        Assert.Equal(RegistrationStatus.Cancelled, registration.Status);
    }

    [Fact]
    public async Task CancelRegistrationAsync_WithWrongParticipant_ThrowsAppException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Future Event", DateTime.Today.AddDays(7));
        var owner = CreateParticipant("Alex Green", "alex@example.com");
        var otherParticipant = CreateParticipant("Sam Blue", "sam@example.com");
        await context.AddRangeAsync(eventItem, owner, otherParticipant);
        await context.SaveChangesAsync();
        var registration = new Registration(eventItem.Id, owner.Id);
        await context.Registrations.AddAsync(registration);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<AppException>(() => service.CancelRegistrationAsync(registration.Id, otherParticipant.Id));

        Assert.Equal("UNAUTHORISED_REGISTRATION_ACCESS", exception.ErrorCode);
        Assert.Equal(RegistrationStatus.Pending, registration.Status);
    }

    private static RegistrationService CreateService(ApplicationDbContext context)
    {
        return new RegistrationService(
            TestRepositoryFactory.Create<Registration>(context),
            TestRepositoryFactory.Create<Event>(context),
            TestRepositoryFactory.Create<Participant>(context));
    }

    private static Event CreateEvent(string title, DateTime eventDate)
    {
        return new Event(title, $"{title} description", eventDate, TimeSpan.FromHours(10), TimeSpan.FromHours(12));
    }

    private static Participant CreateParticipant(string name, string email)
    {
        return new Participant(name, email, "Not provided");
    }
}
