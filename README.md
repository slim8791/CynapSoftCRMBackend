# CynapSoft CRM Backend

This repository contains the CynapSoft CRM backend: an API gateway and multiple microservice APIs (Auth, Product, Inventory, Order, Doc, Field). This README was updated with concrete commands, environment variables and a docker-compose example to run the gateway and core services locally.

> NOTE: Confirm or update any credentials and ports before running — some sample values were taken from the repository's appsettings.json files for convenience, but secrets should be stored in a secure secrets store or .env files.

## Contents
- Overview
- Quick start (local)
- Docker (docker-compose)
- Services and ports
- Configuration (env vars & appsettings)
- Running migrations
- Troubleshooting
- Contributing
- License & contact

## Overview
CynapSoft CRM Backend is structured as a .NET 9 solution. An Ocelot API gateway (CynapCRM.Gateway) routes requests to multiple ASP.NET Core service projects. Each service has its own Dockerfile and appsettings.

## Quick start (local)
Prerequisites:
- .NET 9 SDK
- Docker & Docker Compose (optional)
- Git

1) Clone and restore

```bash
git clone https://github.com/slim8791/CynapSoftCRMBackend.git
cd CynapSoftCRMBackend
dotnet restore CynapCRM.sln
```

2) Build all projects

```bash
dotnet build CynapCRM.sln -c Debug
```

3) Run the Auth service and Gateway (example ports)

Open two terminals.

Terminal A — Auth service:
```bash
dotnet run --project CynapCRM.Services.AuthAPI --urls "http://localhost:5001"
```
Terminal B — Gateway:
```bash
dotnet run --project CynapCRM.Gateway --urls "http://localhost:5000"
```

The gateway is configured with ocelot.json and will forward matching requests to the downstream hosts defined there. For local development you can override downstream hosts by editing CynapCRM.Gateway/ocelot.json to use localhost ports where you run services.

## Docker & docker-compose
Here's a minimal docker-compose you can add as `docker-compose.yml` at repo root to bring up the gateway + auth + product services for local testing. Adjust images and environment variables as needed.

```yaml
version: '3.8'
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

networks:
  default:
    external: false
```

Start with:
```bash
# create a .env with AUTH_DB_CONN, PRODUCT_DB_CONN, JWT_SECRET
docker-compose up --build
```

## Services and default ports (from repo)
- Gateway: configured in ocelot.json, expected upstream (external) paths on the gateway (e.g., /auth/*, /products/*) and downstream hosts such as cynapharmauth.runasp.net (port 80). For local testing override these hosts to localhost:5001, localhost:5002, etc.
- Auth service: sample run port 5001 (use `--urls`) — reads ConnectionStrings:DefaultConnection and ApiSettings:JwtOptions in `CynapCRM.Services.AuthAPI/appsettings.json`.
- Product service: sample run port 5002 — reads its own `appsettings.json`.

## Configuration — env vars to set
From appsettings.json files, the services expect (examples):
- ConnectionStrings__DefaultConnection — SQL Server connection string for each service
- ApiSettings__JwtOptions__Secret (or ApiSettings__Secret) — JWT secret used by services and gateway
- Email settings used by Auth service: EmailSettings__SmtpServer, EmailSettings__Port, EmailSettings__SenderEmail, EmailSettings__SenderPassword

Create a `.env` file at repo root with variables referenced by docker-compose. Never commit secrets to git.

## Running migrations
Services call applyMigrations() at startup (see CynapCRM.Services.AuthAPI/Program.cs) which runs pending EF Core migrations if the database is reachable. To run manually:

```bash
# from repo root
dotnet tool install --global dotnet-ef
# Update database for Auth project
dotnet ef database update --project CynapCRM.Services.AuthAPI
```

## Troubleshooting
- If migrations fail: check ConnectionStrings__DefaultConnection for correct server and credentials. The app catches exceptions and logs a message in Program.cs for migration errors.
- If Ocelot routing fails: ensure ocelot.json DownstreamHostAndPorts point to reachable hosts/ports (for local: localhost and service ports).

## Contributing
- Fork, branch, PR.
- Add tests, update docs and update `NAVIGATE.md` / `MASTER_SUMMARY.md` when changing high-level flow.

## License
Add a LICENSE file to the repository and copy the license name here.
