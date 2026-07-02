using CET254_EventManager.Data;
using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.Services;
using CET254_EventManager.Strategies;
using CET254_EventManager.Tests.TestHelpers;
using CET254_EventManager.ViewModels;
using Microsoft.EntityFrameworkCore;
using DomainActivity = CET254_EventManager.Models.Activity;

namespace CET254_EventManager.Tests;

public class EventServiceTests
{
    // Service-layer tests cover normal paths and edge cases without MVC controllers.
    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsOnlyFutureEvents()
    {
        using var context = TestDbContextFactory.CreateContext();
        await context.Events.AddRangeAsync(
            CreateEvent("Past Event", DateTime.Today.AddDays(-1)),
            CreateEvent("Future Event 2", DateTime.Today.AddDays(10)),
            CreateEvent("Future Event 1", DateTime.Today.AddDays(3)));
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetUpcomingEventsAsync();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, eventItem => eventItem.Title == "Past Event");
        Assert.Equal("Future Event 1", result[0].Title);
        Assert.Equal("Future Event 2", result[1].Title);
    }

    [Fact]
    public async Task GetEventDetailsAsync_WithMissingEvent_ThrowsEventNotFoundException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = CreateService(context);

        await Assert.ThrowsAsync<EventNotFoundException>(() => service.GetEventDetailsAsync(999));
    }

    [Fact]
    public async Task GetFilteredUpcomingEventsAsync_FilterByDate_ReturnsMatchingEvents()
    {
        using var context = TestDbContextFactory.CreateContext();
        var targetDate = DateTime.Today.AddDays(5);
        await context.Events.AddRangeAsync(
            CreateEvent("Target Event", targetDate),
            CreateEvent("Other Event", DateTime.Today.AddDays(8)));
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetFilteredUpcomingEventsAsync(new EventFilterViewModel { Date = targetDate });

        Assert.Single(result);
        Assert.Equal("Target Event", result[0].Title);
    }

    [Fact]
    public async Task GetFilteredUpcomingEventsAsync_FilterByVenue_ReturnsMatchingEvents()
    {
        using var context = TestDbContextFactory.CreateContext();
        var venueA = new Venue("Main Hall", "1 Test Street", 100);
        var venueB = new Venue("Library Room", "2 Test Street", 50);
        var eventA = CreateEvent("Venue A Event", DateTime.Today.AddDays(2));
        var eventB = CreateEvent("Venue B Event", DateTime.Today.AddDays(3));
        await context.AddRangeAsync(venueA, venueB, eventA, eventB);
        await context.SaveChangesAsync();
        await context.EventVenues.AddRangeAsync(
            new EventVenue(eventA.Id, venueA.Id),
            new EventVenue(eventB.Id, venueB.Id));
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetFilteredUpcomingEventsAsync(new EventFilterViewModel { VenueId = venueA.Id });

        Assert.Single(result);
        Assert.Equal("Venue A Event", result[0].Title);
    }

    [Fact]
    public async Task GetFilteredUpcomingEventsAsync_FilterByActivity_ReturnsMatchingEvents()
    {
        using var context = TestDbContextFactory.CreateContext();
        var workshop = new DomainActivity("Workshop Activity", ActivityType.Workshop, "Practical session");
        var talk = new DomainActivity("Talk Activity", ActivityType.Talk, "Talk session");
        var eventA = CreateEvent("Workshop Event", DateTime.Today.AddDays(2));
        var eventB = CreateEvent("Talk Event", DateTime.Today.AddDays(3));
        await context.AddRangeAsync(workshop, talk, eventA, eventB);
        await context.SaveChangesAsync();
        await context.EventActivities.AddRangeAsync(
            new EventActivity(eventA.Id, workshop.Id),
            new EventActivity(eventB.Id, talk.Id));
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetFilteredUpcomingEventsAsync(new EventFilterViewModel { ActivityId = workshop.Id });

        Assert.Single(result);
        Assert.Equal("Workshop Event", result[0].Title);
    }

    [Fact]
    public async Task GetFilteredUpcomingEventsAsync_FilterByActivityType_ReturnsMatchingEvents()
    {
        using var context = TestDbContextFactory.CreateContext();
        var workshop = new DomainActivity("Workshop Activity", ActivityType.Workshop, "Practical session");
        var talk = new DomainActivity("Talk Activity", ActivityType.Talk, "Talk session");
        var eventA = CreateEvent("Workshop Event", DateTime.Today.AddDays(2));
        var eventB = CreateEvent("Talk Event", DateTime.Today.AddDays(3));
        await context.AddRangeAsync(workshop, talk, eventA, eventB);
        await context.SaveChangesAsync();
        await context.EventActivities.AddRangeAsync(
            new EventActivity(eventA.Id, workshop.Id),
            new EventActivity(eventB.Id, talk.Id));
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetFilteredUpcomingEventsAsync(new EventFilterViewModel { ActivityType = ActivityType.Workshop.ToString() });

        Assert.Single(result);
        Assert.Equal("Workshop Event", result[0].Title);
    }

    [Fact]
    public async Task DeleteEventAsync_WhenEventHasRegistrations_ThrowsAppException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var eventItem = CreateEvent("Registered Event", DateTime.Today.AddDays(2));
        var participant = new Participant("Alex Green", "alex@example.com", "Not provided");
        await context.AddRangeAsync(eventItem, participant);
        await context.SaveChangesAsync();
        await context.Registrations.AddAsync(new Registration(eventItem.Id, participant.Id));
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<AppException>(() => service.DeleteEventAsync(eventItem.Id));

        Assert.Equal("EVENT_HAS_REGISTRATIONS", exception.ErrorCode);
        Assert.True(await context.Events.AnyAsync(e => e.Id == eventItem.Id));
    }

    [Fact]
    public async Task CreateEventAsync_WithInvalidEvent_ThrowsAppException()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = CreateService(context);
        var invalidEvent = new Event(string.Empty, "Missing title", DateTime.Today.AddDays(1), TimeSpan.FromHours(12), TimeSpan.FromHours(11));

        var exception = await Assert.ThrowsAsync<AppException>(() => service.CreateEventAsync(invalidEvent));

        Assert.Equal("INVALID_EVENT", exception.ErrorCode);
        Assert.False(await context.Events.AnyAsync());
    }

    private static EventService CreateService(ApplicationDbContext context)
    {
        var strategies = new IEventFilterStrategy[]
        {
            new DateFilterStrategy(),
            new VenueFilterStrategy(),
            new ActivityFilterStrategy(),
            new SearchTermFilterStrategy()
        };

        return new EventService(
            TestRepositoryFactory.Create<Event>(context),
            TestRepositoryFactory.Create<Venue>(context),
            TestRepositoryFactory.Create<DomainActivity>(context),
            strategies);
    }

    private static Event CreateEvent(string title, DateTime eventDate)
    {
        return new Event(title, $"{title} description", eventDate, TimeSpan.FromHours(10), TimeSpan.FromHours(12));
    }
}
