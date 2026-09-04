# HelpDesk

HelpDesk is an ASP.NET Core MVC application for managing support projects and tickets. It provides role-based workflows for administrators and clients, project membership, ticket assignment, favorites, ticket following, persistent notifications, and real-time updates with SignalR.

The project was developed as an individual assignment for the **ASP.NET Advanced** course at SoftUni. It demonstrates layered architecture, ASP.NET Core Identity, Entity Framework Core, MVC Areas, validation, authorization, pagination, filtering, custom error handling, and real-time communication.

## Technology stack

- .NET 8
- ASP.NET Core MVC
- ASP.NET Core Identity
- Entity Framework Core 8
- Microsoft SQL Server
- SignalR
- Razor Views
- Bootstrap
- JavaScript
- NUnit
- Moq


## Main functionality

### Authentication and authorization

- User registration and login with ASP.NET Core Identity.
- Newly registered users receive the `Client` role.
- The application supports `Administrator` and `Client` roles.
- Administrative functionality is separated through an MVC `Admin` Area.
- Controller actions are protected with authentication and role authorization.
- Unauthorized and forbidden requests are handled through custom error pages.

### Projects

- Display projects available to the current user.
- Search projects by name or description.
- Display paginated project results.
- Create, edit, view, and delete projects.
- Restrict project creation, editing, deletion, and membership management to administrators.
- Assign users to projects.
- Remove users from projects.
- Create a project without initially assigning users.
- Prevent deletion of projects that contain tickets.
- Mark accessible projects as favorites.
- Display favorite projects on a dedicated page.
- Remove the favorite state when a user is removed from a project.

### Tickets

- Display tickets available through the current user's project memberships.
- Search tickets by title.
- Filter tickets by project and status.
- Display paginated ticket results.
- Create tickets only in projects accessible to the current user.
- View, edit, and delete tickets according to the permissions of the current user.
- Validate the selected category and subcategory relationship.
- Assign or unassign a project member to a ticket.
- Restrict assignee management to administrators.
- Prevent clients from changing administrator-controlled fields through modified POST requests.
- Change ticket status through the Admin Area.
- Remove ticket assignments when an assigned user is removed from the corresponding project.

### Favorites and followers

- Clients can add accessible projects to Favorites.
- Clients can remove projects from Favorites.
- Users can follow accessible tickets independently of ticket assignment.
- Users can unfollow tickets.
- A dedicated Following page displays followed tickets.
- Duplicate follower records are prevented.
- When a user is removed from a project, follower records for tickets in that project are removed.

### Notifications and SignalR

- Ticket followers receive real-time SignalR notifications when a ticket is updated.
- Ticket followers receive notifications when a ticket status changes.
- Notifications are persisted in the database.
- Offline users can read stored notifications later.
- The navigation displays the number of unread notifications.
- Users can mark their notifications as read.
- A user can modify only notifications that belong to their account.
- Failed real-time delivery does not remove the stored notification or interrupt the main ticket operation.

### Administration

The `Admin` Area provides:

- an administration dashboard;
- project management;
- ticket status management;
- user management;
- user search;
- role filtering;
- user pagination;
- user details;
- project, created ticket, assigned ticket, and followed ticket counts;
- safe user deletion restrictions;
- protection against deleting the currently logged-in administrator;
- protection against deleting administrator accounts;
- preservation of ticket history by preventing deletion of users who created tickets.

## Roles and permissions

| Functionality | Client | Administrator |
|---|:---:|:---:|
| Register and sign in | Yes | Yes |
| View assigned projects | Yes | Yes |
| View tickets from accessible projects | Yes | Yes |
| Create a ticket in an accessible project | Yes | Yes |
| Edit an allowed ticket | Yes | Yes |
| Create and manage projects | No | Yes |
| Assign users to projects | No | Yes |
| Remove users from projects | No | Yes |
| Assign or unassign ticket assignees | No | Yes |
| Change ticket status from the Admin Area | No | Yes |
| Add projects to favorites | Yes | No |
| Follow accessible tickets | Yes | Yes |
| View stored notifications | Yes | Yes |
| Manage application users | No | Yes |

## Architecture

The solution uses layered architecture, dependency injection, and repository abstractions.

```text
HelpDeskApp.sln
├── HelpDeskApp
│   ├── Areas
│   │   ├── Admin
│   │   └── Identity
│   ├── Controllers
│   ├── Hubs
│   ├── Services
│   ├── Views
│   └── wwwroot
├── HelpDeskApp.Core
│   ├── Contracts
│   └── Services
├── HelpDeskApp.Infrastructure
│   ├── Data
│   ├── Migrations
│   └── Repositories
├── HelpDeskApp.ViewModels
│   └── Models
├── HelpDeskApp.Common
└── HelpDeskApp.Tests
    ├── Services
    │   ├── ProjectServiceTests.cs
    │   ├── ProjectFavoriteServiceTests.cs
    │   ├── TicketServiceTests.cs
    │   └── TicketFollowerServiceTests.cs
    └── HelpDeskApp.Services.Tests.csproj
```

### Layer responsibilities

- **HelpDeskApp** - MVC controllers, Razor Views, Identity pages, SignalR Hub, middleware configuration, and web-specific notification delivery.
- **HelpDeskApp.Core** - application contracts, business rules, entity-to-view-model mapping, authorization-related checks, pagination normalization, and service orchestration.
- **HelpDeskApp.Infrastructure** - EF Core entities, database context, migrations, repositories, and data seeding.
- **HelpDeskApp.ViewModels** - models used by views and form binding without exposing database entities directly to the user interface.
- **HelpDeskApp.Common** - shared validation constants.
- **HelpDeskApp.Tests** - unit tests for the main service-layer business logic.

Controllers depend on service interfaces. Services depend on repository interfaces. Concrete implementations are registered through the built-in ASP.NET Core dependency-injection container.

## Domain model

The application contains the following entities:

- `ApplicationUser`
- `Project`
- `UserProject`
- `Ticket`
- `TicketFollower`
- `TicketStatus`
- `Category`
- `SubCategory`
- `Notification`

### Main relationships

- A user can participate in many projects.
- A project can contain many users.
- The many-to-many relationship between users and projects is implemented through `UserProject`.
- `UserProject.IsFavorite` stores whether a user marked a project as a favorite.
- A project can contain many tickets.
- A ticket has one creator.
- A ticket can have an optional assignee.
- A user can follow many tickets.
- A ticket can have many followers.
- The many-to-many follower relationship is implemented through `TicketFollower`.
- A category can contain many subcategories.
- A ticket belongs to one subcategory.
- A ticket has one status.
- A notification belongs to one user.
- A notification can reference a ticket.

Delete behavior is configured to preserve ticket history and prevent unsafe cascade deletion.

Projects that contain tickets cannot be deleted. Ticket creator and assignee relationships use restricted deletion. Notification-to-ticket deletion uses `SetNull`.

## Business rules

- A client can access only projects to which they are assigned.
- A client can create a ticket only in an accessible project.
- A project can be created without initially assigned users.
- A ticket assignee must be a member of the ticket's project.
- Only an administrator can change a ticket assignee.
- Only an administrator can change a ticket status.
- Clients cannot change ticket project, status, or assignee through parameter tampering.
- Ticket following is independent of ticket assignment.
- A user must have access to a ticket before following it.
- Removing a user from a project clears ticket assignments related to that project.
- Removing a user from a project removes follower records for tickets in that project.
- Removing a `UserProject` record also removes the favorite state for that project.
- A project containing tickets cannot be deleted.
- A user who created tickets cannot be deleted because ticket history must be preserved.
- An administrator cannot delete their own account.
- Administrator accounts cannot be deleted through User Management.
- A user can mark only their own notifications as read.

## Search, filtering, and pagination

Search, filtering, and pagination are implemented for:

- projects;
- tickets;
- administrator user management.

Project functionality supports:

- search by project information;
- favorite-only filtering;
- page sizes of 6, 12, and 24 records.

Ticket functionality supports:

- search by ticket information;
- filtering by project;
- filtering by status;
- page sizes of 6, 12, and 24 records.

User Management supports:

- search by name, username, or email;
- filtering by role;
- page sizes of 5, 10, and 20 records.

Query view models normalize search terms, validate supported page sizes, keep the current page within the available range, and return paged view-model results.

## Validation and security

The project includes:

- Data Annotation validation on input models;
- client-side validation through the standard validation scripts partial;
- server-side `ModelState` validation;
- database constraints;
- explicit EF Core relationship configuration;
- ASP.NET Core Identity authentication;
- role-based authorization;
- ownership checks;
- project-membership checks;
- anti-forgery validation on state-changing requests;
- parameter-tampering protection in service methods;
- parameterized EF Core queries;
- Razor output encoding for user-provided values;
- secure notification lookup by notification ID and current user ID;
- custom pages for bad requests, forbidden requests, missing resources, and server errors.

## Error handling

The application uses:

- `UseExceptionHandler` outside the Development environment;
- `UseStatusCodePagesWithReExecute` for HTTP status-code pages;
- a custom `400 Bad Request` page;
- a custom `403 Forbidden` page;
- a custom `404 Not Found` page;
- a custom `500 Server Error` page;
- service-level exceptions for missing resources and invalid business operations;
- validation messages returned to the relevant form where appropriate.

## Seeded data

On startup, the application ensures that the following data exists:

- `Administrator` and `Client` roles;
- a development administrator account;
- ticket statuses: Open, In Progress, Resolved, and Closed;
- Hardware category;
- Software category;
- Network and Connectivity category;
- Accounts and Access category;
- Email and Communication category;
- relevant subcategories for each category;
- seven support projects.

The seeded projects are:

1. Internal IT Support
2. Network Infrastructure
3. Hardware Maintenance
4. Software Support
5. Accounts and Access
6. Email and Collaboration
7. Remote Work Support

The development administrator account is intended only for local development and project demonstration.

## Getting started

### Prerequisites

The following software is required:

- .NET SDK 8.0 or later
- SQL Server or SQL Server LocalDB
- Visual Studio 2022, JetBrains Rider, or Visual Studio Code
- Git

### 1. Clone the repository

```bash
git clone https://github.com/radostina-elinchova/HelpDesk.git
cd HelpDesk/HelpDeskApp
```

### 2. Configure the database connection

Set `ConnectionStrings:DefaultConnection` in `HelpDeskApp/appsettings.Development.json` or use .NET user secrets.

Example:

```bash
dotnet user-secrets --project HelpDeskApp/HelpDeskApp.csproj set \
  "ConnectionStrings:DefaultConnection" \
  "Server=(localdb)\\MSSQLLocalDB;Database=HelpDeskApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Do not commit production credentials or secrets.

### 3. Restore dependencies

```bash
dotnet restore HelpDeskApp.sln
```

### 4. Apply database migrations

```bash
dotnet ef database update \
  --project HelpDeskApp.Infrastructure \
  --startup-project HelpDeskApp
```

### 5. Run the application

```bash
dotnet run --project HelpDeskApp
```

The HTTPS development profile uses:

```text
https://localhost:7155
```

## Testing

The solution contains 80 NUnit test cases for the main service-layer business logic.

The tested services are:

- `ProjectService`;
- `TicketService`;
- `ProjectFavoriteService`;
- `TicketFollowerService`.

The tests cover:

- successful operations;
- invalid input and missing entities;
- authorization-related business rules;
- project-membership restrictions;
- ticket creation and editing;
- ticket status changes;
- project favorites;
- ticket followers;
- entity-to-view-model mapping;
- pagination and query normalization.

Testing technologies:

- NUnit;
- Moq;
- Microsoft.NET.Test.Sdk.

Tests can be executed from Visual Studio Test Explorer or with:

```bash
dotnet test HelpDeskApp.Tests/HelpDeskApp.Services.Tests.csproj
```

A coverage report can be generated with:

```bash
dotnet test HelpDeskApp.Tests/HelpDeskApp.Services.Tests.csproj --collect:"XPlat Code Coverage"
```

## Deployment

The application is currently configured for local execution.

Before a public deployment:

1. Move the production connection string to secure configuration.
2. Remove or secure the development administrator credentials.
3. Apply the database migrations.
4. Configure HTTPS and production error handling.
5. Verify SignalR WebSocket connectivity.
6. Run the complete automated and manual test suites.

## Author

**Radostina Elinchova**

- GitHub: [radostina-elinchova](https://github.com/radostina-elinchova)
- Repository: [HelpDesk](https://github.com/radostina-elinchova/HelpDesk)

## License

This project was developed for educational purposes as part of the SoftUni ASP.NET Advanced course.
