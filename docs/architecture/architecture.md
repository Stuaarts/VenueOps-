# VenueOps Architecture

```mermaid
flowchart LR
  subgraph Client["User Browser"]
    React["React + Vite Frontend"]
  end

  subgraph Api["ASP.NET Core Web API"]
    Auth["JWT Authentication"]
    RBAC["Role-based Authorization"]
    Controllers["REST Controllers"]
    EF["Entity Framework Core"]
    Seeder["Demo Data Seeder"]
  end

  subgraph Data["PostgreSQL"]
    Users["Users"]
    Clients["Clients"]
    Venues["Venue Rooms"]
    Bookings["Event Bookings"]
    Assignments["Staff Assignments"]
    Notes["Shift Notes"]
  end

  subgraph Docker["Local Docker Compose"]
    WebContainer["frontend container"]
    ApiContainer["backend container"]
    DbContainer["postgres container"]
  end

  subgraph CI["GitHub Actions"]
    BackendCI["dotnet build + tests"]
    FrontendCI["npm lint + tests + build"]
    DockerCI["Docker image builds"]
  end

  React -- "POST /api/auth/login" --> Auth
  Auth -- "signed JWT" --> React
  React -- "Bearer token REST calls" --> Controllers
  Controllers --> RBAC
  Controllers --> EF
  EF --> Users
  EF --> Clients
  EF --> Venues
  EF --> Bookings
  EF --> Assignments
  EF --> Notes
  Seeder --> EF

  WebContainer --> ApiContainer
  ApiContainer --> DbContainer

  CI --> BackendCI
  CI --> FrontendCI
  CI --> DockerCI
```

## Auth Flow

1. A demo account signs in through `POST /api/auth/login`.
2. The API validates the BCrypt password hash and returns a JWT.
3. The frontend stores the token in memory for the active session.
4. API requests include `Authorization: Bearer <token>`.
5. Controllers enforce role-based access for Admin, Manager, Staff, and Demo users.

## Local Runtime

`docker compose up --build` starts PostgreSQL, the ASP.NET Core API, and the Nginx-hosted React app. The API applies EF Core migrations on startup and seeds demo data when `SeedDemoData=true`.
