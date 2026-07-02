# Sequence Diagram: Register For Event

## Purpose

This document describes the user registration workflow implemented in the Blazor-first application.

## Participants

- User
- Browser / Blazor component
- `Components/Pages/EventDetails.razor`
- `UserManager<IdentityUser>`
- `ParticipantService`
- `RegistrationService`
- `Repository<Event>`
- `Repository<Participant>`
- `Repository<Registration>`
- Database

## Main Flow

1. User opens `Components/Pages/EventDetails.razor` at `/events/{id:int}`.
2. User clicks **Register for Event**.
3. `EventDetails.razor` gets the current authentication state.
4. `EventDetails.razor` uses `UserManager<IdentityUser>` to get the current logged-in Identity user.
5. `EventDetails.razor` calls `ParticipantService.GetParticipantByApplicationUserIdAsync`.
6. If no participant profile exists:
   - The component creates a `Participant` using the Identity user's email.
   - The component calls `ParticipantService.CreateParticipantAsync`.
   - Participant data is saved through `Repository<Participant>`.
7. The component calls `RegistrationService.RegisterAsync(eventId, participantId)`.
8. `RegistrationService` checks that the event exists through `Repository<Event>`.
9. `RegistrationService` checks that the participant exists through `Repository<Participant>`.
10. `RegistrationService` checks that the event is not in the past.
11. `RegistrationService` checks that there is no duplicate active registration.
12. `RegistrationService` checks venue capacity.
13. `RegistrationService` creates a new `Registration`.
14. `Repository<Registration>` adds the registration and saves changes.
15. `EventDetails.razor` reloads event details and displays a success message.

## Alternate Flows

### Duplicate Registration

- If an active registration already exists for the same event and participant:
  - `RegistrationService` throws `DuplicateRegistrationException`.
  - `EventDetails.razor` catches `AppException`.
  - The component displays a friendly registration error message.

### Past Event

- If the event date is before today:
  - `RegistrationService` throws `PastEventRegistrationException`.
  - The component displays a friendly registration error message.

### Venue Full

- If the event has venue assignments and confirmed registrations meet the smallest venue capacity:
  - `RegistrationService` throws `VenueCapacityExceededException`.
  - The component displays a friendly registration error message.

## Mermaid Sequence Sketch

```mermaid
sequenceDiagram
    actor User
    participant Browser as Browser / Blazor UI
    participant Component as EventDetails.razor
    participant UserManager as UserManager<IdentityUser>
    participant ParticipantService
    participant RegistrationService
    participant EventRepo as Repository<Event>
    participant ParticipantRepo as Repository<Participant>
    participant RegistrationRepo as Repository<Registration>
    participant DB as Database

    User->>Browser: Click Register for Event
    Browser->>Component: Invoke RegisterForEventAsync()
    Component->>Component: Get authentication state
    Component->>UserManager: Get current Identity user
    UserManager-->>Component: IdentityUser

    Component->>ParticipantService: GetParticipantByApplicationUserIdAsync(user.Id)
    ParticipantService->>ParticipantRepo: Query participant profile
    ParticipantRepo->>DB: SELECT Participant
    DB-->>ParticipantRepo: Participant or null
    ParticipantRepo-->>ParticipantService: Result
    ParticipantService-->>Component: Participant or null

    alt Participant profile does not exist
        Component->>ParticipantService: CreateParticipantAsync(participant)
        ParticipantService->>ParticipantRepo: AddAsync(participant)
        ParticipantRepo->>DB: INSERT Participant
        ParticipantService->>ParticipantRepo: SaveChangesAsync()
        ParticipantRepo->>DB: COMMIT
        ParticipantService-->>Component: Created participant
    end

    Component->>RegistrationService: RegisterAsync(eventId, participantId)
    RegistrationService->>EventRepo: Check event exists
    EventRepo->>DB: SELECT Event
    DB-->>EventRepo: Event

    RegistrationService->>ParticipantRepo: Check participant exists
    ParticipantRepo->>DB: SELECT Participant exists
    DB-->>ParticipantRepo: Exists result

    RegistrationService->>RegistrationService: Check event is not in the past
    RegistrationService->>RegistrationRepo: Check duplicate active registration
    RegistrationRepo->>DB: SELECT Registration
    DB-->>RegistrationRepo: Duplicate result

    RegistrationService->>EventRepo: Load EventVenues and Venue capacity
    EventRepo->>DB: SELECT Event + Venues
    DB-->>EventRepo: Capacity data

    RegistrationService->>RegistrationRepo: Count confirmed registrations
    RegistrationRepo->>DB: SELECT COUNT
    DB-->>RegistrationRepo: Confirmed count

    RegistrationService->>RegistrationService: Create Registration(Pending)
    RegistrationService->>RegistrationRepo: AddAsync(registration)
    RegistrationRepo->>DB: INSERT Registration
    RegistrationService->>RegistrationRepo: SaveChangesAsync()
    RegistrationRepo->>DB: COMMIT
    RegistrationService-->>Component: Registration
    Component->>Component: Reload details and show success message
    Component-->>Browser: Updated event details

    alt Duplicate registration
        RegistrationService-->>Component: DuplicateRegistrationException
        Component-->>Browser: Show friendly error message
    else Past event
        RegistrationService-->>Component: PastEventRegistrationException
        Component-->>Browser: Show friendly error message
    else Venue full
        RegistrationService-->>Component: VenueCapacityExceededException
        Component-->>Browser: Show friendly error message
    end
```
