# 🚀 HelpDesk

> Odoo-inspired HelpDesk system built with **ASP.NET Core MVC**,
> demonstrating real-world web development concepts including layered
> architecture, authentication, and database management.

![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)\
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-blue)\
![License](https://img.shields.io/badge/license-MIT-green)

------------------------------------------------------------------------

## 📖 About the Project

**HelpDesk** is a web-based ticket management system built with ASP.NET
Core MVC.\
It allows users to register, log in, and manage support requests in a
structured and secure environment.

The project demonstrates:

-   Clean layered architecture\
-   Authentication & authorization with ASP.NET Identity\
-   Entity Framework Core with Code-First approach\
-   Separation of business logic and presentation layer

------------------------------------------------------------------------

## 🛠️ Technologies Used

-   ASP.NET Core MVC (.NET 8)
-   Entity Framework Core
-   ASP.NET Core Identity
-   SQL Server / LocalDB
-   Razor Views
-   C#
-   Bootstrap (if used)

------------------------------------------------------------------------

## 🚀 Getting Started

### Clone the repository

``` bash
git clone https://github.com/radostina-elinchova/HelpDesk.git
cd HelpDesk
```

### Restore dependencies

``` bash
dotnet restore
```

### Apply database migrations

``` bash
dotnet ef database update
```

### Run the application

``` bash
dotnet run
```

The application will be available at:

-   https://localhost:5001
-   http://localhost:5000

------------------------------------------------------------------------

## 📁 Project Structure

    HelpDesk/
    │
    ├── HelpDesk.Core/            
    ├── HelpDesk.Infrastructure/  
    ├── HelpDesk.Web/             
    │   ├── Controllers/
    │   ├── Views/
    │   ├── wwwroot/
    │   ├── appsettings.json
    │   └── Program.cs

------------------------------------------------------------------------

## ✨ Features

-   User registration and login\
-   Role-based authorization\
-   Ticket creation and management\
-   Entity Framework Core integration\
-   MVC architecture\
-   Error handling middleware

------------------------------------------------------------------------

## 🗄️ Database Setup

Example connection string in `appsettings.json`:

``` json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=HelpDeskAppDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true",
}
```

To create migrations:

``` bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

------------------------------------------------------------------------

## 📄 License

This project is licensed under the **MIT License**.
