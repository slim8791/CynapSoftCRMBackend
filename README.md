# CynaPharm — Pharmaceutical CRM (Web & Mobile)

A CRM platform for the pharmaceutical industry, built as a .NET 9 microservices solution with an Angular back-office and a .NET MAUI field application. It covers medical visits, orders, sample inventory and commercial documents.

**Stack** — ASP.NET Core 9 · Entity Framework Core · SQL Server · Ocelot API Gateway · RabbitMQ · Docker · Angular · .NET MAUI (MVVM) · SQLite · JWT / ASP.NET Core Identity

---

## Table of contents

- [Architecture](#architecture)
- [Repository layout](#repository-layout)
- [Quick start (local)](#quick-start-local)
- [Docker Compose](#docker-compose)
- [Configuration](#configuration)
- [Database migrations](#database-migrations)
- [Troubleshooting](#troubleshooting)
- [License](#license)

---

## Architecture

Clients talk to a single Ocelot gateway, which routes each request to the service that owns the corresponding domain. Every service is an independent ASP.NET Core project with its own database and its own Dockerfile.

```
┌──────────────────┐     ┌────────────────────┐
│  Angular         │     │  .NET MAUI         │
│  back-office     │     │  field app         │
└────────┬─────────┘     └─────────┬──────────┘
         │                         │  (offline mode, SQLite sync)
         └───────────┬─────────────┘
                     ▼
          ┌─────────────────────┐
          │  Ocelot API Gateway │
          └──────────┬──────────┘
                     ▼
   ┌──────┬─────────┬───────────┬───────┬──────┬───────┐
   │ Auth │ Product │ Inventory │ Order │ Doc  │ Field │
   └──────┴─────────┴───────────┴───────┴──────┴───────┘
                     │
                  SQL Server
```

| Service | Responsibility |
|---|---|
| `AuthAPI` | Authentication, users and roles (ASP.NET Core Identity, JWT) |
| `ProductAPI` | Product catalogue |
| `InventoryAPI` | Sample stock management |
| `OrderAPI` | Orders |
| `DocAPI` | Commercial documents |
| `FieldAPI` | Medical visits and field activity |

## Repository layout

```
CynapCRM.Gateway/               Ocelot API gateway
CynapCRM.Services.AuthAPI/      Authentication service
CynapCRM.Services.ProductAPI/   Product service
CynapCRM.Services.InventoryAPI/ Inventory service
CynapCRM.Services.OrderAPI/     Order service
CynapCRM.Services.DocAPI/       Document service
CynapCRM.Services.FieldAPI/     Field activity service
Cynapharm/                      Angular back-office
Cynapharm-Mobile/               .NET MAUI field application
CynapCRM.sln                    Backend solution file
```

## Quick start (local)

**Prerequisites** — .NET 9 SDK, SQL Server (or a SQL Server container), Node.js and the Angular CLI for the web client, Docker (optional).

```bash
git clone https://github.com/slim8791/CynapSoftCRMBackend.git
cd CynapSoftCRMBackend
dotnet restore CynapCRM.sln
dotnet build CynapCRM.sln -c Debug
```

Run a service and the gateway in two terminals:

```bash
# Terminal A — Auth service
dotnet run --project CynapCRM.Services.AuthAPI --urls "http://localhost:5001"

# Terminal B — Gateway
dotnet run --project CynapCRM.Gateway --urls "http://localhost:5000"
```

`CynapCRM.Gateway/ocelot.json` points to the hosted environment by default. For local development, change the `DownstreamHostAndPorts` entries to `localhost` and the ports you started the services on.

Angular back-office:

```bash
cd Cynapharm
npm install
ng serve
```

## Docker Compose

Create a `docker-compose.yml` at the repository root:

```yaml
services:
  gateway:
    build:
      context: ./CynapCRM.Gateway
      dockerfile: Dockerfile
    ports:
      - "5000:80"
    depends_on:
      - auth
      - product
    volumes:
      - ./CynapCRM.Gateway/ocelot.json:/app/ocelot.json

  auth:
    build:
      context: ./CynapCRM.Services.AuthAPI
      dockerfile: Dockerfile
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=${AUTH_DB_CONN}
      - ApiSettings__JwtOptions__Secret=${JWT_SECRET}

  product:
    build:
      context: ./CynapCRM.Services.ProductAPI
      dockerfile: Dockerfile
    ports:
      - "5002:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=${PRODUCT_DB_CONN}
      - ApiSettings__Secret=${JWT_SECRET}
```

Then:

```bash
# create a .env file with AUTH_DB_CONN, PRODUCT_DB_CONN and JWT_SECRET
docker compose up --build
```

## Configuration

Each service reads its own `appsettings.json`. Override these values with environment variables or a `.env` file — never commit real credentials.

| Variable | Used by | Purpose |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | every service | SQL Server connection string |
| `ApiSettings__JwtOptions__Secret` | Auth, Gateway | Signing key for JWT tokens |
| `ApiSettings__Secret` | downstream services | Key used to validate incoming tokens |
| `EmailSettings__SmtpServer` | Auth | SMTP host for transactional e-mails |
| `EmailSettings__Port` | Auth | SMTP port |
| `EmailSettings__SenderEmail` | Auth | Sender address |
| `EmailSettings__SenderPassword` | Auth | SMTP password |

A `.env.example` file with empty placeholders is the right place to document these.

## Database migrations

Each service runs its pending EF Core migrations at startup (`applyMigrations()` in `Program.cs`) when the database is reachable. To run them manually:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project CynapCRM.Services.AuthAPI
```

## Troubleshooting

**Migrations fail at startup** — check `ConnectionStrings__DefaultConnection`: wrong server name or credentials is the usual cause. The startup code catches the exception and logs it instead of crashing, so check the console output.

**Gateway returns 404 or 502** — the `DownstreamHostAndPorts` entries in `ocelot.json` still point to an unreachable host. Set them to the local host and port of the running service.

**401 on every downstream call** — the JWT secret configured in the Auth service and in the downstream service must be identical.

## License

Copyright © CynapSoft. All rights reserved.
