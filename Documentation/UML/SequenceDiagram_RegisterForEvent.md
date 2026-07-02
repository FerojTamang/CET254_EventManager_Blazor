# Sequence Diagram: Register For Event

## Purpose

This document describes the user registration workflow implemented in the MVC application.

## Participants

- Browser / Razor View
- `EventsController`
- `UserManager<IdentityUser>`
- `ParticipantService`
- `RegistrationService`
- `Repository<Event>`
- `Repository<Participant>`
- `Repository<Registration>`
- Database

## Main Flow

1. User opens an event details page.
2. User clicks **Register**.
3. Browser posts the registration form to `EventsController.Register`.
4. `EventsController` uses `UserManager<IdentityUser>` to get the current logged-in Identity user.
5. `EventsController` calls `ParticipantService.GetParticipantByApplicationUserIdAsync`.
6. If no participant profile exists:
   - Controller creates a `Participant` using the Identity user's email.
   - Controller calls `ParticipantService.CreateParticipantAsync`.
   - Participant is saved through `Repository<Participant>`.
7. Controller calls `RegistrationService.RegisterAsync(eventId, participantId)`.
8. `RegistrationService` checks that the event exists through `Repository<Event>`.
9. `RegistrationService` checks that the participant exists through `Repository<Participant>`.
10. `RegistrationService` checks that the event is not in the past.
11. `RegistrationService` checks that there is no duplicate active registration.
12. `RegistrationService` checks venue capacity.
13. `RegistrationService` creates a new `Registration`.
14. `Repository<Registration>` adds the registration and saves changes.
15. Controller sets a success message and redirects to **My Registrations**.

## Alternate Flows

### Duplicate Registration

- If an active registration already exists for the same event and participant:
  - `RegistrationService` throws `DuplicateRegistrationException`.
  - Controller catches `AppException`.
  - Controller stores a friendly error in `TempData["Error"]`.
  - User is redirected back to event details.

### Past Event

- If the event date is before today:
  - `RegistrationService` throws `PastEventRegistrationException`.
  - Controller shows a friendly error message.

### Venue Full

- If the event has venue assignments and confirmed registrations meet the smallest venue capacity:
  - `RegistrationService` throws `VenueCapacityExceededException`.
  - Controller shows a friendly error message.

## Mermaid Sequence Sketch

```mermaid
sequenceDiagram
    actor User
    participant Browser as Browser / Razor View
    participant Controller as EventsController
    participant UserManager as UserManager<IdentityUser>
    participant ParticipantService
    participant RegistrationService
    participant EventRepo as Repository<Event>
    participant ParticipantRepo as Repository<Participant>
    participant RegistrationRepo as Repository<Registration>
    participant DB as Database

    User->>Browser: Click Register
    Browser->>Controller: POST Register(eventId)
    Controller->>UserManager: Get current user
    UserManager-->>Controller: IdentityUser

    Controller->>ParticipantService: GetParticipantByApplicationUserIdAsync(user.Id)
    ParticipantService->>ParticipantRepo: Query participant
    ParticipantRepo->>DB: SELECT Participant
    DB-->>ParticipantRepo: Participant or null
    ParticipantRepo-->>ParticipantService: Result
    ParticipantService-->>Controller: Participant or null

    alt Participant does not exist
        Controller->>ParticipantService: CreateParticipantAsync(participant)
        ParticipantService->>ParticipantRepo: AddAsync + SaveChangesAsync
        ParticipantRepo->>DB: INSERT Participant
        DB-->>ParticipantRepo: Saved participant
    end

    Controller->>RegistrationService: RegisterAsync(eventId, participantId)
    RegistrationService->>EventRepo: GetByIdAsync(eventId)
    EventRepo->>DB: SELECT Event
    DB-->>EventRepo: Event

    RegistrationService->>ParticipantRepo: ExistsAsync(participantId)
    ParticipantRepo->>DB: SELECT Participant exists
    DB-->>ParticipantRepo: Exists result

    RegistrationService->>RegistrationRepo: Check duplicate active registration
    RegistrationRepo->>DB: SELECT Registration
    DB-->>RegistrationRepo: Duplicate result

    RegistrationService->>EventRepo: Load venues for capacity check
    EventRepo->>DB: SELECT Event + EventVenues + Venues
    DB-->>EventRepo: Event capacity data

    RegistrationService->>RegistrationRepo: Count confirmed registrations
    RegistrationRepo->>DB: SELECT COUNT
    DB-->>RegistrationRepo: Count

    RegistrationService->>RegistrationRepo: AddAsync(new Registration)
    RegistrationRepo->>DB: INSERT Registration
    RegistrationService->>RegistrationRepo: SaveChangesAsync
    RegistrationRepo->>DB: COMMIT

    RegistrationService-->>Controller: Registration
    Controller-->>Browser: Redirect with success message

    alt Duplicate registration / Past event / Venue full
        RegistrationService-->>Controller: Throw AppException
        Controller-->>Browser: Redirect with friendly error message
    end
```
