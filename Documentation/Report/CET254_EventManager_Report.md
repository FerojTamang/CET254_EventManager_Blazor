# Community Event Management System

## Title Page

**Project Title:** Community Event Management System  
**Module:** CET254 Advanced Programming  
**Technology Stack:** ASP.NET Core Blazor Web App, C#, Entity Framework Core, SQL Server LocalDB, ASP.NET Core Identity, Bootstrap, xUnit  

**Student Name:** [Insert Student Name]  
**Student ID:** [Insert Student ID]  
**Submission Date:** [Insert Submission Date]  

---

# 1. Introduction

The Community Event Management System is an ASP.NET Core Blazor Web App developed for the CET254 Advanced Programming assignment. The purpose of the system is to support the management and registration of community events. Administrators can manage events, venues, activities, participants, and registrations through Blazor components, while users can browse upcoming events, apply filters, view event details, register for events, and manage their own registrations.

The application demonstrates key advanced programming concepts required by the assignment rubric. These include object-oriented programming, inheritance, encapsulation, interfaces, polymorphism, method overloading, design patterns, database relationships, validation, exception handling, and automated testing. The system also uses ASP.NET Core Identity to support authentication and role-based access control. ASP.NET Core Identity Razor Pages are retained for authentication pages such as login, register, and logout. The main event management workflows are implemented using Blazor components.

---

# 2. Requirement Analysis

## 2.1 Actors

The system has two main actors:

- **Admin:** An authenticated user with the `Admin` role. The admin manages system data and registration status.
- **User / Participant:** A public or authenticated user who browses events and registers for community events.

## 2.2 Admin Requirements

The admin side of the system supports the following requirements:

- Log in as an administrator.
- View the admin dashboard.
- Manage events.
- Assign venues to events.
- Assign activities to events.
- Manage venues.
- Manage activities.
- View participants.
- View registrations.
- Confirm registrations.
- Cancel registrations.

## 2.3 User Requirements

The user side of the system supports the following requirements:

- Register and log in.
- View upcoming events.
- Filter events by date, venue, activity, activity type, and search term.
- View event details.
- Register for an event.
- View personal registrations.
- Cancel personal registrations.

## 2.4 Required Entities

The main domain entities are:

- `Event`
- `Participant`
- `Venue`
- `Activity`
- `Registration`

The system also includes junction models:

- `EventVenue`
- `EventActivity`

## 2.5 Many-to-Many Relationships

The system includes three important many-to-many relationships:

- **Event-Participant:** Implemented through `Registration`.
- **Event-Venue:** Implemented through `EventVenue`.
- **Event-Activity:** Implemented through `EventActivity`.

The `Registration` entity is more than a simple junction table because it also stores `RegistrationDate` and `Status`.

---

# 3. System Architecture

The application uses a layered architecture:

```text
Browser
        |
Presentation Layer: Blazor components
        |
Authentication Layer: ASP.NET Core Identity Razor Pages
        |
Business Logic Layer: Services
        |
Data Access Layer: Repository pattern with EF Core DbContext
        |
Domain Layer: Models, validation, and custom exceptions
        |
Testing Layer: xUnit tests
        |
SQL Server LocalDB
```

Blazor components do not access `ApplicationDbContext` directly. Instead, components depend on service interfaces such as `IEventService`, `IRegistrationService`, `IVenueService`, `IActivityService`, and `IParticipantService`. This improves testability and maintains separation of concerns.

ASP.NET Core Identity is used for authentication and role-based authorization. Identity Razor Pages are retained only for authentication pages such as login, register, and logout. The main user and admin workflows are implemented in Blazor components under `Components/Pages` and `Components/Pages/Admin`. The system includes `Admin` and `User` roles. Bootstrap is used to provide a responsive and consistent user interface. xUnit tests target the service layer and use the EF Core InMemory provider.

Architecture diagram source: `Documentation/UML/Diagrams/architecture-diagram.mmd`.

---

# 4. UML Diagrams

## 4.1 Use Case Diagram

Use case diagram source: `Documentation/UML/Diagrams/use-case-diagram.mmd`.

The use case diagram shows the main interactions between the two actors and the system. The admin manages events, venues, activities, participants, and registrations. The user browses upcoming events, filters event lists, views event details, registers for events, and cancels personal registrations.

## 4.2 Class Diagram

Class diagram source: `Documentation/UML/Diagrams/class-diagram.mmd`.

The class diagram shows the object-oriented structure of the system. The domain classes inherit from `BaseEntity`, services implement service interfaces, and the generic repository implements `IRepository<T>`. The diagram also shows polymorphism through `IEventFilterStrategy` and its concrete strategy implementations.

## 4.3 Entity Relationship Diagram

ERD source: `Documentation/UML/Diagrams/erd.mmd`.

The ERD shows the database structure. It includes domain tables, Identity tables, primary keys, foreign keys, and junction tables. The `Registrations` table links events and participants and stores registration-specific data such as registration date and status.

## 4.4 Sequence Diagram: Register for Event

Sequence diagram source: `Documentation/UML/Diagrams/sequence-register-event.mmd`.

The sequence diagram explains the registration process. `Components/Pages/EventDetails.razor` gets the current Identity user, finds or creates the linked participant profile, and calls `RegistrationService.RegisterAsync`. The service checks event existence, participant existence, past event rules, duplicate registrations, and venue capacity before creating the registration.

Alternate flows include duplicate registration, past event registration, and venue capacity exceeded.

---

# 5. Database Design

The database is implemented using Entity Framework Core and SQL Server LocalDB. The `ApplicationDbContext` inherits from `IdentityDbContext`, preserving ASP.NET Core Identity tables while adding the event-management domain model.

## 5.1 Main Tables

### Events

Stores event information:

- `Id`
- `Title`
- `Description`
- `EventDate`
- `StartTime`
- `EndTime`
- `CreatedAt`
- `UpdatedAt`

### Participants

Stores participant profile information:

- `Id`
- `FullName`
- `Email`
- `PhoneNumber`
- `ApplicationUserId`
- `CreatedAt`
- `UpdatedAt`

### Venues

Stores venue details:

- `Id`
- `Name`
- `Address`
- `Capacity`
- `CreatedAt`
- `UpdatedAt`

The database includes a check constraint to ensure venue capacity is greater than zero.

### Activities

Stores event activity details:

- `Id`
- `Name`
- `ActivityType`
- `Description`
- `CreatedAt`
- `UpdatedAt`

### Registrations

Stores event registration records:

- `Id`
- `EventId`
- `ParticipantId`
- `RegistrationDate`
- `Status`
- `CreatedAt`
- `UpdatedAt`

`RegistrationStatus` supports:

- `Pending`
- `Confirmed`
- `Cancelled`

The database includes a unique composite index on `EventId + ParticipantId` to help prevent duplicate registrations.

### EventVenues

Junction table for events and venues:

- Composite key: `EventId + VenueId`
- Foreign key to `Events`
- Foreign key to `Venues`

### EventActivities

Junction table for events and activities:

- Composite key: `EventId + ActivityId`
- Foreign key to `Events`
- Foreign key to `Activities`

## 5.2 Delete Behaviour and Protection

The database uses restricted delete behavior to avoid accidental cascade deletes. The service layer also adds friendly delete protections:

- Events with registrations cannot be deleted.
- Venues assigned to events cannot be deleted.
- Activities assigned to events cannot be deleted.

These protections prevent database exceptions from reaching the user interface and allow Blazor components to display friendly error messages.

---

# 6. Object-Oriented Implementation

The project demonstrates object-oriented implementation through classes, constructors, encapsulation, inheritance, interfaces, polymorphism, overloading, and composition.

## 6.1 Classes and Objects

The system is structured around domain classes such as `Event`, `Participant`, `Venue`, `Activity`, and `Registration`. Each class represents a real concept in the event management domain.

## 6.2 BaseEntity Inheritance

The domain classes inherit from `BaseEntity`. This avoids duplication by providing common properties:

- `Id`
- `CreatedAt`
- `UpdatedAt`

It also provides common behaviour:

- `Validate()`
- `Touch()`

## 6.3 Constructors

Domain classes include constructors for required data. For example:

- `Event` requires title, description, date, start time, and end time.
- `Participant` requires name, email, and phone number.
- `Registration` requires event ID and participant ID and automatically sets `RegistrationDate` and `Pending` status.

Protected parameterless constructors are also provided for EF Core.

## 6.4 Encapsulation

Domain properties use private setters where appropriate. Updates are performed through methods such as:

- `Event.UpdateDetails(...)`
- `Venue.UpdateDetails(...)`
- `Activity.UpdateDetails(...)`
- `Participant.UpdateDetails(...)`
- `Registration.Confirm()`
- `Registration.Cancel()`

This prevents uncontrolled changes and keeps domain behaviour inside the domain model.

## 6.5 Interfaces

The project uses interfaces to support abstraction and testability:

- `IRepository<T>`
- `IEventService`
- `IRegistrationService`
- `IVenueService`
- `IActivityService`
- `IParticipantService`
- `IAdminDashboardService`
- `IEventFilterStrategy`

Blazor components depend on service interfaces rather than concrete classes.

## 6.6 Polymorphism

Polymorphism is demonstrated through `IEventFilterStrategy`. The event service receives multiple strategy implementations and applies them through the same interface:

- `DateFilterStrategy`
- `VenueFilterStrategy`
- `ActivityFilterStrategy`
- `SearchTermFilterStrategy`

Each strategy has its own filtering algorithm but shares the same `Apply(...)` method signature.

## 6.7 Method Overloading

`EventService` demonstrates method overloading:

- `GetUpcomingEventsAsync()`
- `GetUpcomingEventsAsync(DateTime fromDate)`
- `CreateEventAsync(Event eventItem)`
- `CreateEventAsync(Event eventItem, IEnumerable<int> venueIds, IEnumerable<int> activityIds)`
- `UpdateEventAsync(Event eventItem)`
- `UpdateEventAsync(Event eventItem, IEnumerable<int> venueIds, IEnumerable<int> activityIds)`

## 6.8 Composition

Composition is used through navigation collections:

- `Event` contains `Registrations`.
- `Event` contains `EventVenues`.
- `Event` contains `EventActivities`.
- `Participant` contains `Registrations`.
- `Venue` contains `EventVenues`.
- `Activity` contains `EventActivities`.

---

# 7. Design Patterns, Data Structures, and Algorithms

## 7.1 Blazor Component-Based Presentation

The application uses Blazor components for the main presentation layer:

- `Components/Pages/Events.razor` renders upcoming events and filtering.
- `Components/Pages/EventDetails.razor` renders event details and registration.
- `Components/Pages/MyRegistrations.razor` renders participant registrations.
- `Components/Pages/Admin/Dashboard.razor` renders the admin dashboard.
- `Components/Pages/Admin/Events.razor` manages event CRUD and venue/activity assignment.
- `Components/Pages/Admin/Registrations.razor` manages registration confirmation and cancellation.
- `Components/Pages/Admin/Participants.razor` manages participants.
- `Components/Pages/Admin/Venues.razor` manages venues.
- `Components/Pages/Admin/Activities.razor` manages activities.

## 7.2 Repository Pattern

The generic repository abstracts EF Core data access:

- `IRepository<T>`
- `Repository<T>`

This allows services to access data without directly depending on `ApplicationDbContext`.

## 7.3 Service Layer Pattern

The service layer contains business rules and validation. Examples include:

- `EventService`
- `RegistrationService`
- `VenueService`
- `ActivityService`
- `ParticipantService`

This keeps Blazor components focused on UI flow while services contain business rules.

## 7.4 Strategy Pattern

The Strategy Pattern is used for event filtering. `EventService` applies all registered `IEventFilterStrategy` implementations to an `IQueryable<Event>`.

This supports maintainability because new filters can be added without rewriting the Blazor event listing component.

## 7.5 Dependency Injection

ASP.NET Core Dependency Injection is used to register:

- Repositories
- Services
- Filter strategies
- Identity services
- EF Core DbContext

## 7.6 Data Structures

The project uses several data structures:

- `List<T>` for ViewModel collections and query results.
- `ICollection<T>` for navigation collections.
- `IQueryable<T>` for efficient EF Core query composition.
- `Dictionary<RegistrationStatus, int>` for dashboard registration summaries.
- Enums for `RegistrationStatus` and `ActivityType`.

## 7.7 Algorithms and Business Rules

Implemented algorithms include:

- Upcoming event filtering by date.
- Event filtering by date, venue, activity, activity type, and search term.
- Duplicate active registration checking.
- Past event registration blocking.
- Venue capacity checking using the smallest assigned venue capacity.
- Registration status grouping for dashboard summaries.
- Safe delete checks for events, venues, and activities.

---

# 8. Validation and Exception Handling

## 8.1 ViewModel Data Annotations

ViewModels use Data Annotations to validate user input:

- `[Required]`
- `[StringLength]`
- `[Range]`
- `[EmailAddress]`
- `[Phone]`
- `[DataType]`
- `[Display]`

Examples include:

- `EventCreateViewModel`
- `EventEditViewModel`
- `VenueViewModel`
- `ActivityViewModel`
- `ParticipantViewModel`
- `EventFilterViewModel`

## 8.2 FutureDateAttribute

`FutureDateAttribute` is a custom validation attribute. It validates that an event date is today or in the future. It returns valid for null values so `[Required]` can handle required fields separately.

## 8.3 Blazor Form Validation

Blazor admin forms use `EditForm`, `DataAnnotationsValidator`, and `ValidationSummary` to validate ViewModels before calling services. Service-level validation still runs after form validation so business rules are enforced outside the UI.

## 8.4 Custom Exceptions

The application includes a custom exception hierarchy:

- `AppException`
- `EventNotFoundException`
- `ParticipantNotFoundException`
- `VenueNotFoundException`
- `ActivityNotFoundException`
- `RegistrationNotFoundException`
- `DuplicateRegistrationException`
- `VenueCapacityExceededException`
- `PastEventRegistrationException`
- `InvalidRegistrationStatusException`

## 8.5 Friendly Error Messages

Blazor components catch `AppException` and display friendly messages using component state such as:

- `_errorMessage`
- `_successMessage`
- `_registrationErrorMessage`

This avoids exposing raw stack traces to users.

## 8.6 Service-Level Validation

Services validate business rules before performing operations. Examples include:

- Invalid event details are rejected.
- Duplicate registrations are blocked.
- Registration for past events is blocked.
- Venue capacity is checked before registration confirmation.
- Linked events, venues, and activities are protected from unsafe deletion.

---

# 9. Testing Documentation

## 9.1 Testing Framework

The project uses xUnit for automated unit testing. The tests are located in the `CET254_EventManager.Tests` project.

## 9.2 Test Database

Tests use the EF Core InMemory provider rather than SQL Server LocalDB. Each test uses a unique database name to avoid cross-test pollution.

The tests use real service classes and the real generic repository where practical.

## 9.3 Service Layer Test Coverage

The automated tests focus on:

- `EventService`
- `RegistrationService`
- Management service delete protections
- Strategy Pattern filtering

## 9.4 Automated Test Table

| Test ID | Test Name | Purpose | Expected Result | Status |
|---|---|---|---|---|
| UT-01 | GetUpcomingEventsAsync_ReturnsOnlyFutureEvents | Checks that only future events are returned and ordered correctly. | Past event excluded; future events ordered by date. | Passed |
| UT-02 | GetEventDetailsAsync_WithMissingEvent_ThrowsEventNotFoundException | Checks missing event exception handling. | `EventNotFoundException` thrown. | Passed |
| UT-03 | GetFilteredUpcomingEventsAsync_FilterByDate_ReturnsMatchingEvents | Checks date filtering. | Only matching event date returned. | Passed |
| UT-04 | GetFilteredUpcomingEventsAsync_FilterByVenue_ReturnsMatchingEvents | Checks venue filtering through `EventVenue`. | Only events assigned to selected venue returned. | Passed |
| UT-05 | GetFilteredUpcomingEventsAsync_FilterByActivity_ReturnsMatchingEvents | Checks activity filtering through `EventActivity`. | Only events assigned to selected activity returned. | Passed |
| UT-06 | GetFilteredUpcomingEventsAsync_FilterByActivityType_ReturnsMatchingEvents | Checks enum-based activity type filtering. | Only matching activity type returned. | Passed |
| UT-07 | CreateEventAsync_WithInvalidEvent_ThrowsAppException | Checks event validation. | `AppException` with `INVALID_EVENT` thrown. | Passed |
| UT-08 | RegisterAsync_WithValidEventAndParticipant_CreatesPendingRegistration | Checks successful registration. | Pending registration created. | Passed |
| UT-09 | RegisterAsync_WithMissingEvent_ThrowsEventNotFoundException | Checks missing event during registration. | `EventNotFoundException` thrown. | Passed |
| UT-10 | RegisterAsync_WithMissingParticipant_ThrowsParticipantNotFoundException | Checks missing participant during registration. | `ParticipantNotFoundException` thrown. | Passed |
| UT-11 | RegisterAsync_ForPastEvent_ThrowsPastEventRegistrationException | Checks past event blocking. | `PastEventRegistrationException` thrown. | Passed |
| UT-12 | RegisterAsync_DuplicateActiveRegistration_ThrowsDuplicateRegistrationException | Checks duplicate registration blocking. | `DuplicateRegistrationException` thrown. | Passed |
| UT-13 | RegisterAsync_WhenVenueCapacityFull_ThrowsVenueCapacityExceededException | Checks venue capacity rule. | `VenueCapacityExceededException` thrown. | Passed |
| UT-14 | CancelRegistrationAsync_WithValidOwner_ChangesStatusToCancelled | Checks participant can cancel own registration. | Status changes to `Cancelled`. | Passed |
| UT-15 | CancelRegistrationAsync_WithWrongParticipant_ThrowsAppException | Checks user cannot cancel another user's registration. | `AppException` with access error code thrown. | Passed |
| UT-16 | ConfirmRegistrationAsync_WhenAlreadyConfirmed_DoesNotThrow | Checks idempotent confirm behavior. | No exception thrown. | Passed |
| UT-17 | ConfirmRegistrationAsync_WhenCancelled_ThrowsInvalidRegistrationStatusException | Checks cancelled registration cannot be confirmed. | `InvalidRegistrationStatusException` thrown. | Passed |
| UT-18 | DeleteEventAsync_WhenEventHasRegistrations_ThrowsAppException | Checks event delete protection. | `AppException` with `EVENT_HAS_REGISTRATIONS` thrown. | Passed |
| UT-19 | DeleteVenueAsync_WhenVenueAssignedToEvent_ThrowsAppException | Checks venue delete protection. | `AppException` with `VENUE_IN_USE` thrown. | Passed |
| UT-20 | DeleteActivityAsync_WhenActivityAssignedToEvent_ThrowsAppException | Checks activity delete protection. | `AppException` with `ACTIVITY_IN_USE` thrown. | Passed |

Screenshot evidence still required: add or reference a current screenshot showing a successful test run. Do not mark this complete unless the screenshot is captured from the final project state.

---

# 10. Manual Testing Evidence

The following manual tests should be performed during final demonstration and screenshots.

| Test ID | Feature | Input / Action | Expected Result | Actual Result | Status | Screenshot Reference |
|---|---|---|---|---|---|---|
| MT-01 | Admin login | Log in with admin account. | Admin is authenticated and can access admin pages. | [To complete] | [To complete] | Screenshot-01-AdminLogin.png |
| MT-02 | Admin dashboard | Open admin dashboard. | Dashboard cards and recent registrations display. | [To complete] | [To complete] | Screenshot-02-AdminDashboard.png |
| MT-03 | Create event | Submit event create form with valid data. | Event is created. | [To complete] | [To complete] | Screenshot-03-CreateEvent.png |
| MT-04 | Create venue | Submit venue form with valid name, address, and capacity. | Venue is created. | [To complete] | [To complete] | Screenshot-04-CreateVenue.png |
| MT-05 | Create activity | Submit activity form with valid name and type. | Activity is created. | [To complete] | [To complete] | Screenshot-05-CreateActivity.png |
| MT-06 | Assign venue/activity to event | Select venue and activity checkboxes during event create/edit. | Event is linked to selected venue and activity. | [To complete] | [To complete] | Screenshot-06-AssignVenueActivity.png |
| MT-07 | User views upcoming events | Open Events page. | Upcoming events display. | [To complete] | [To complete] | Screenshot-07-UpcomingEvents.png |
| MT-08 | User filters events | Apply date, venue, or activity filter. | Matching events display. | [To complete] | [To complete] | Screenshot-08-FilterEvents.png |
| MT-09 | User registers for event | Click Register on event details page. | Registration is created. | [To complete] | [To complete] | Screenshot-09-RegisterEvent.png |
| MT-10 | Duplicate registration blocked | Attempt to register for same event again. | Friendly duplicate registration error shown. | [To complete] | [To complete] | Screenshot-10-DuplicateBlocked.png |
| MT-11 | My registrations page | Open My Registrations. | User's registrations display. | [To complete] | [To complete] | Screenshot-11-MyRegistrations.png |
| MT-12 | Cancel registration | Click Cancel on own registration. | Status changes to Cancelled. | [To complete] | [To complete] | Screenshot-12-CancelRegistration.png |
| MT-13 | Admin confirms registration | Admin clicks Confirm. | Registration status becomes Confirmed. | [To complete] | [To complete] | Screenshot-13-ConfirmRegistration.png |
| MT-14 | Admin delete protected linked event | Delete event with registrations. | Friendly protection error shown. | [To complete] | [To complete] | Screenshot-14-DeleteProtectedEvent.png |
| MT-15 | Admin delete protected linked venue/activity | Delete linked venue or activity. | Friendly protection error shown. | [To complete] | [To complete] | Screenshot-15-DeleteProtectedVenueActivity.png |

---

# 11. Demonstration Plan

The following 10-minute demonstration plan can be used for the final video:

| Time | Content |
|---|---|
| 0:00-0:40 | Introduce the Community Event Management System and technology stack. |
| 0:40-1:30 | Show architecture and UML overview. |
| 1:30-3:00 | Demonstrate admin dashboard and CRUD functionality. |
| 3:00-4:00 | Demonstrate event venue/activity assignment. |
| 4:00-5:30 | Demonstrate user event browsing and filtering. |
| 5:30-6:30 | Demonstrate registration and duplicate registration blocking. |
| 6:30-7:30 | Demonstrate validation and exception handling. |
| 7:30-8:40 | Explain code: OOP, interfaces, polymorphism, Strategy Pattern, Repository Pattern, and Service Layer. |
| 8:40-9:30 | Show unit tests and successful test run. |
| 9:30-10:00 | Final summary and future improvements. |

---

# 12. Conclusion

The Community Event Management System provides a working ASP.NET Core Blazor Web App for managing community events and registrations. The system supports admin workflows, user browsing, event filtering, registration, cancellation, dashboard reporting, validation, exception handling, and automated testing.

The project meets the main learning outcomes of CET254 Advanced Programming by demonstrating object-oriented design, inheritance, encapsulation, constructors, interfaces, polymorphism, method overloading, design patterns, database relationships, validation, custom exceptions, and unit testing.

Future improvements could include:

- Email notifications for registration confirmations.
- Calendar integration.
- Cloud deployment.
- Richer reporting and analytics.
- More detailed participant profile management.
- Additional integration tests and UI tests.

---

# 13. References

- Microsoft. ASP.NET Core Blazor Documentation. [Reference link required]
- Microsoft. Entity Framework Core Documentation. [Reference link required]
- Microsoft. ASP.NET Core Identity Documentation. [Reference link required]
- xUnit Documentation. [Reference link required]
- Mermaid Documentation. [Reference link required]
