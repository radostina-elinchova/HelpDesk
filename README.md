# HelpDesk

HelpDesk is an ASP.NET Core MVC application for managing support projects and tickets. It provides role-based workflows for administrators and clients, project membership, ticket assignment, favorites, ticket following, persistent notifications, and real-time updates with SignalR.

The project was developed as an individual assignment for the **ASP.NET Advanced** course at SoftUni and demonstrates layered architecture, ASP.NET Core Identity, Entity Framework Core, MVC Areas, server-side validation, authorization, pagination, filtering, custom error handling, and real-time communication.

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
- xUnit, Moq, Coverlet — planned test stack

## Main functionality

### Authentication and authorization

- User registration and login with ASP.NET Core Identity.
- Newly registered users receive the `Client` role.
- The application supports `Administrator` and `Client` roles.
- Administrative functionality is separated through an MVC `Admin` Area.
- Controller actions are protected with authentication, role authorization, and anti-forgery validation.
- Unauthorized and forbidden requests are handled through custom error pages.

### Projects

- List projects available to the current user.
- Search projects and display paginated results.
- Create, edit, view, and delete projects.
- Restrict project creation, editing, deletion, and membership management to administrators.
- Assign users to projects and remove them from projects.
- Prevent deletion of projects that contain tickets.
- Mark projects as favorites and view a dedicated Favorite Projects page.
- Automatically remove the favorite state when a user is removed from a project.

### Tickets

- List tickets available through the current user's project memberships.
- Search and filter tickets by project, status, and search term.
- Display paginated ticket results.
- Create tickets only in projects accessible to the current user.
- Create, view, edit, and delete tickets.
- Validate the selected category and subcategory relationship.
- Assign or unassign a project member to a ticket through Ticket Edit.
- Prevent clients from changing administrator-controlled fields through a modified POST request.
- Allow administrators to change ticket status.
- Remove ticket assignments when an assigned user is removed from the corresponding project.

### Favorites and followers

- Clients can add accessible projects to Favorites.
- Clients can remove projects from Favorites.
- Users can follow or unfollow accessible tickets independently of ticket assignment.
- A dedicated Following page displays followed tickets.
- When a user is removed from a project, follower records for that project's tickets are removed.

### Notifications and SignalR

- Ticket followers receive real-time SignalR notifications when a ticket is updated.
- Ticket followers receive notifications when ticket status changes.
- Notifications are persisted in the database and can be read later.
- The navigation displays the number of unread notifications.
- A user can mark only their own notifications as read.
- Failed real-time delivery is logged without losing the persisted notification or failing the main ticket operation.

### Administration

The `Admin` Area provides:

- an administration dashboard;
- project management;
- ticket status management;
- user search, role filtering, and pagination;
- user details;
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
| Edit own ticket | Yes | Yes |
| Create and manage projects | No | Yes |
| Assign users to projects | No | Yes |
| Assign or unassign ticket assignees | No | Yes |
| Change ticket status from the Admin Area | No | Yes |
| Add projects to favorites | Yes | No |
| Follow accessible tickets | Yes | Yes |
| Manage application users | No | Yes |

## Architecture

The solution uses a layered architecture with dependency injection and repository abstractions.

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
└── HelpDeskApp.Common
```

### Layer responsibilities

- **HelpDeskApp** — MVC controllers, Razor Views, Identity pages, SignalR Hub, middleware configuration, and web-specific notification delivery.
- **HelpDeskApp.Core** — application contracts, business rules, entity-to-view-model mapping, authorization-related checks, pagination normalization, and service orchestration.
- **HelpDeskApp.Infrastructure** — EF Core entities, database context, migrations, repositories, and data seeding.
- **HelpDeskApp.ViewModels** — models used by views and form binding without exposing database entities directly to the UI.
- **HelpDeskApp.Common** — shared validation constants.

Controllers depend on service interfaces, services depend on repository interfaces, and concrete implementations are registered through the built-in ASP.NET Core dependency-injection container.

## Domain model

The application contains the following main entities:

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

- A user can participate in many projects and a project can have many users through `UserProject`.
- `UserProject.IsFavorite` stores whether the user marked the project as a favorite.
- A project contains many tickets.
- A ticket has one creator and an optional assignee.
- A user can follow many tickets and a ticket can have many followers through `TicketFollower`.
- A category contains many subcategories.
- A ticket belongs to one subcategory and one status.
- A notification belongs to a user and can reference a ticket.

Delete behavior is configured to preserve ticket history and prevent unsafe cascade deletion. Projects containing tickets cannot be deleted, ticket creator and assignee relationships use restricted deletion, and notification-to-ticket deletion uses `SetNull`.

## Business rules

- A client can access only projects to which they are assigned.
- A client can create a ticket only in an accessible project.
- A ticket assignee must be a member of the ticket's project.
- Only an administrator can change a ticket assignee through Ticket Edit.
- Clients cannot change ticket project, status, or assignee through parameter tampering.
- Ticket following is independent of ticket assignment.
- Removing a user from a project clears their assignments and follower records for that project.
- Removing `UserProject` also removes the user's favorite state for that project.
- A project containing tickets cannot be deleted.
- A user who created tickets cannot be deleted because ticket history must be preserved.
- An administrator cannot delete their own account or another administrator account through User Management.
- A user can mark only notifications that belong to them as read.

## Search, filtering, and pagination

Search, filtering, and pagination are implemented for:

- projects;
- tickets;
- administrator user management.

Query view models normalize search terms, validate supported page sizes, keep the current page within the available range, and return paged view-model results.

## Validation and security

The project includes:

- Data Annotation validation on input models;
- client-side validation through the standard validation scripts partial;
- server-side `ModelState` validation;
- database constraints and explicit EF Core relationship configuration;
- ASP.NET Core Identity authentication;
- role-based authorization;
- ownership and project-membership checks;
- anti-forgery validation on state-changing requests;
- parameter-tampering protection in service methods;
- EF Core parameterized queries, which protect normal queries from SQL injection;
- Razor output encoding for user-provided values;
- secure notification lookup by both notification ID and current user ID;
- custom pages for bad requests, forbidden requests, missing resources, and server errors.

## Error handling

The application uses:

- `UseExceptionHandler` outside Development;
- `UseStatusCodePagesWithReExecute` for HTTP status-code pages;
- custom views for `400`, `403`, `404`, and `500` responses;
- service-level exceptions for missing resources and invalid business operations;
- validation messages returned to the relevant form where appropriate.

## Seeded data

On startup, the application ensures that the following data exists:

- `Administrator` and `Client` roles;
- a development administrator account;
- ticket statuses: Open, In Progress, Resolved, and Closed;
- Hardware and Software categories;
- PC/Laptop, Network, and OS Install subcategories;
- Internal Infrastructure and External Support projects.

> The seeded administrator credentials are intended only for local development and demonstration. Replace the hard-coded development credentials with secure configuration before public deployment.

## Getting started

### Prerequisites

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

Set `ConnectionStrings:DefaultConnection` in `HelpDeskApp/appsettings.Development.json` or use .NET user secrets:

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

### 4. Apply migrations

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

## Testing and code coverage

Unit tests are the next planned development step. The test suite will focus on the service layer and use:

- xUnit;
- Moq;
- Coverlet;
- ReportGenerator.

The target is at least **65% coverage of the business logic implemented in services**, as required by the course assignment.

Once the test project is added, tests and coverage will be run with:

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

An HTML coverage report can be generated with:

```bash
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"CoverageReport" \
  -reporttypes:Html
```

Current coverage: **pending test implementation**.

## Deployment

Public deployment is pending. After deployment, add the production URL here:

```text
Production URL: pending
```

Before deploying:

1. move credentials and the production connection string to secure configuration;
2. replace the development administrator seeding strategy;
3. apply production migrations;
4. configure forwarded headers and HTTPS;
5. verify custom error pages outside Development;
6. verify SignalR connectivity in the hosting environment;
7. run the complete automated test suite.

## Future improvements

- Add unit tests and reach at least 65% service-layer coverage.
- Deploy the application and document the public URL.
- Add screenshots and a short demonstration video.
- Add pagination to the notification history.
- Improve notification time-zone presentation for hosted environments.
- Optimize several multi-query repository workflows after functional verification.

## Author

**Radostina Elinchova**

- GitHub: [radostina-elinchova](https://github.com/radostina-elinchova)
- Repository: [HelpDesk](https://github.com/radostina-elinchova/HelpDesk)

## License

This project is intended for educational use as part of the SoftUni ASP.NET Advanced course.
