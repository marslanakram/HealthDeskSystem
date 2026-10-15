# HealthDesk — Multi-Tenant Clinic Management System

**HealthDesk** is a web-based, multi-tenant clinic management system built with ASP.NET Core MVC. It enables multiple clinics to manage their operations through a centralized platform while keeping their data and permissions isolated.

This project demonstrates practical implementation of Clean Architecture, Entity Framework Core, ASP.NET Core Identity, tenant isolation, custom authorization, and audit logging.

**Live Demo:** [healthdesk.bsite.net](https://healthdesk.bsite.net/)

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Security and Multi-Tenancy](#security-and-multi-tenancy)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Key Implementation Details](#key-implementation-details)
- [What I Learned](#what-i-learned)
- [Future Improvements](#future-improvements)
- [License](#license)
- [Author](#author)

---

## Overview

HealthDesk follows a multi-tenant SaaS approach, allowing multiple clinics to operate within one application instance. A platform-level Super Admin manages tenants, while each clinic manages its own users, doctors, patients, appointments, prescriptions, departments, and permissions.

The system is designed around separation of concerns, maintainable application layers, and tenant-aware data access.

**Project Goals**

- Implement a multi-tenant application using ASP.NET Core.
- Apply Clean Architecture principles.
- Separate authentication from fine-grained authorization.
- Isolate clinic data and permissions.
- Implement audit logging and reliable database transactions.
- Build a maintainable foundation for a clinic management platform.

## Features

### Platform Administration — Super Admin

- Create and manage clinic tenants.
- Provision tenant administrators.
- Configure subscription tiers such as Basic, Pro, and Enterprise.
- View platform-level audit information.

### Clinic Administration — Admin

- Register and manage doctors and patients.
- Schedule and manage appointments.
- Manage prescriptions and departments.
- Manage clinic users and access permissions.
- Review audit logs with tracked changes.

### Multi-Tenant User Experience

- Dedicated administrative areas for platform and clinic operations.
- Role-based access for Super Admin, Admin, Doctor, Patient, Receptionist, and Nurse.
- Tenant-scoped access to clinic data.
- Independent permission configuration for each tenant.

### Security and Authorization

- ASP.NET Core Identity for user authentication.
- Tenant-aware authorization.
- Entity Framework Core global query filters for tenant-scoped data.
- Tenant identification through user claims and middleware.
- Centralized permission authorization.
- Audit logging for relevant application operations.

## Technology Stack

| Category | Technology |
|---|---|
| Backend | C# |
| Framework | ASP.NET Core MVC (.NET 8) |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Authentication | ASP.NET Core Identity |
| Architecture | Clean Architecture |
| Data Access | Repository Pattern and Unit of Work |
| Object Mapping | AutoMapper |
| Logging | Serilog |
| Frontend | Razor Views, HTML, CSS, JavaScript |
| UI Framework | Bootstrap 5 and Bootstrap Icons |
| Data Tables | DataTables |
| Dependency Management | Built-in Dependency Injection |
| Development Tools | Visual Studio, SQL Server Management Studio, Git, GitHub |

## Architecture

HealthDesk uses Clean Architecture to separate business logic, application services, infrastructure, and presentation concerns.

```text
HealthDeskSystem/
├── HealthDesk.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Constants/
│
├── HealthDesk.Application/
│   ├── Interfaces/
│   ├── Services/
│   └── DTOs/
│
├── HealthDesk.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   ├── Services/
│   ├── Authorization/
│   └── Seeds/
│
└── HealthDesk.Web/
    ├── Areas/
    │   ├── SuperAdmin/
    │   └── Admin/
    ├── Middleware/
    ├── Identity/
    ├── Controllers/
    └── Views/
```

### Layer Responsibilities

- **Domain:** Core entities, enums, constants, and business concepts.
- **Application:** Application services, interfaces, DTOs, and mapping definitions.
- **Infrastructure:** Entity Framework Core, repositories, Unit of Work, Identity integration, and supporting services.
- **Web:** MVC controllers, Razor views, middleware, dependency injection configuration, and web application behavior.

The intended dependency direction keeps the Domain layer independent of infrastructure details and allows Infrastructure to implement interfaces defined by the Application layer.

## Security and Multi-Tenancy

Tenant isolation is a core design consideration in HealthDesk.

### Tenant-Aware Data Access

Entity Framework Core global query filters help restrict tenant-scoped queries to the current tenant and exclude soft-deleted records where applicable.

```csharp
entity => !entity.IsDeleted &&
    (_currentTenantId == null ||
     entity.TenantId == _currentTenantId.Value)
```

The exact behavior depends on the entity configuration and the current tenant context. Platform-level operations require particular care because an unscoped query must not accidentally expose tenant data.

### Tenant Resolution

Tenant resolution middleware establishes the tenant context for requests. The application uses tenant information associated with authenticated users, including claims, to support tenant-aware operations.

### Custom Permission System

The application includes tenant-specific role permissions. This allows the same role, such as Doctor, to have different permissions in different clinics.

For example:

| Tenant | Role | Permission |
|---|---|---|
| Clinic 1 | Doctor | View patients |
| Clinic 1 | Doctor | View appointments |
| Clinic 2 | Doctor | View patients |

Permission checks are handled through a custom authorization system, with caching to reduce repeated permission lookups.

### Audit Logging

Audit logs help track application activity and relevant changes, supporting administrative oversight and troubleshooting.

> **Security note:** Global query filters and claims are useful safeguards, but they should be combined with authorization checks and security testing. Super Admin access and any tenant-bypassing operations must be explicitly controlled.

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or a compatible IDE
- Microsoft SQL Server or SQL Server Express
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/marslanakram/HealthDeskSystem.git
cd HealthDeskSystem
```

### 2. Open the Solution

Open the solution file in Visual Studio and restore the NuGet packages.

### 3. Configure the Database

Create a local SQL Server database or configure the application to use an existing development database.

Use .NET User Secrets or environment variables for local connection strings and sensitive configuration. Do not commit your personal `appsettings.json` files or passwords.

See [Configuration](#configuration) for the required settings.

### 4. Apply Database Migrations

If the project has migrations that need to be applied, run the following command from the solution root:

```bash
dotnet ef database update \
  --project HealthDesk.Infrastructure \
  --startup-project HealthDesk.Web
```

If the Entity Framework Core CLI tool is not installed, install it using the official .NET tooling instructions. Ensure the configured connection string points to your intended development database before applying migrations.

### 5. Configure Seed Settings

Configure the required Super Admin and Admin seed values using .NET User Secrets or environment variables.

The seeder requires these values to be present. Use credentials appropriate for your local development environment.

### 6. Run the Application

```bash
dotnet run --project HealthDesk.Web
```

Use the local URL printed by the application in the terminal.

The application may initialize its required seed data during startup, depending on the configured database and seeding logic.

## Configuration

The application requires the following configuration keys:

| Configuration Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection |
| `SeedSuperAdmin:Email` | Super Admin seed account email |
| `SeedSuperAdmin:Password` | Super Admin seed password |
| `SeedAdmin:Email` | Admin seed account email |
| `SeedAdmin:Password` | Admin seed password |
| `SeedAdmin:FullName` | Admin seed account name |

For local development, use the User Secrets feature of ASP.NET Core.

From the solution root, initialize User Secrets for the web project if they have not already been initialized:

```bash
dotnet user-secrets init --project HealthDesk.Web
```

Set the required values locally with commands like these, replacing the placeholders with your own development values:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<YOUR_LOCAL_CONNECTION_STRING>" --project HealthDesk.Web

dotnet user-secrets set "SeedSuperAdmin:Email" "<YOUR_SUPERADMIN_EMAIL>" --project HealthDesk.Web
dotnet user-secrets set "SeedSuperAdmin:Password" "<YOUR_SUPERADMIN_PASSWORD>" --project HealthDesk.Web

dotnet user-secrets set "SeedAdmin:Email" "<YOUR_ADMIN_EMAIL>" --project HealthDesk.Web
dotnet user-secrets set "SeedAdmin:Password" "<YOUR_ADMIN_PASSWORD>" --project HealthDesk.Web
dotnet user-secrets set "SeedAdmin:FullName" "<YOUR_ADMIN_FULL_NAME>" --project HealthDesk.Web
```

These commands are examples for local setup. Do not place real passwords in the README, source code, or committed configuration files.

## Key Implementation Details

### Multi-Tenancy

- Tenant-aware data access through EF Core global query filters.
- Tenant context resolution through middleware and authenticated-user claims.
- Tenant identifiers associated with users.
- Tenant-specific administration and data access.

### Custom Authorization

- Role-based access control through ASP.NET Core Identity.
- Fine-grained permission checks through custom authorization components.
- Per-tenant role permission configuration.
- Permission caching to reduce repeated lookups.

### Repository and Unit of Work

The Repository and Unit of Work patterns provide abstractions around database operations and help application services coordinate data access.

### Retry-Safe Transactions

Where database retry strategies are enabled, transactions need to be coordinated with the Entity Framework Core execution strategy. The project includes a Unit of Work transaction abstraction intended to support this workflow.

### Logging and Auditing

Serilog supports application logging, while audit records help administrators review relevant operations and data changes.

## What I Learned

Working on HealthDesk helped me develop practical experience with:

- Designing multi-tenant applications with ASP.NET Core.
- Structuring projects using Clean Architecture.
- Working with ASP.NET Core Identity and claims.
- Implementing role-based and permission-based authorization.
- Isolating tenant data using Entity Framework Core.
- Applying Repository and Unit of Work patterns.
- Coordinating database operations and retry-safe transactions.
- Managing application configuration and keeping secrets out of source control.
- Building administrative workflows for a clinic management system.

## Future Improvements

Potential areas for future development include:

- [ ] Tenant branding and clinic-specific themes.
- [ ] Subscription enforcement and plan expiry.
- [ ] PDF reports and document generation.
- [ ] Billing and invoicing.
- [ ] Pharmacy management.
- [ ] Email notifications.
- [ ] Enhanced dashboard analytics.

## License

No license is specified here. Add a license after confirming how you want others to use, modify, and distribute the project. If you choose the MIT License, include the corresponding `LICENSE` file in the repository.

## Author

**Muhammad Arslan Akram**

Software Engineer | ASP.NET Core / .NET Developer

- **GitHub:** [marslanakram](https://github.com/marslanakram)
- **LinkedIn:** [Muhammad Arslan Akram](https://linkedin.com/in/muhammad-arslan-akram/)
- **Portfolio:** [arsalanakram.com](https://arsalanakram.com/)

---

If you find the project interesting, feel free to explore the live demo and review the repository's implementation.
