# Enterprise Management System

A full-stack work-management application built with **ASP.NET Core 8**, **Entity Framework Core**, **SQL Server**, **React 18**, and **TypeScript**.

## Implemented

- REST API for task creation, listing, editing, status changes and deletion.
- JWT bearer authentication, password hashing, request validation and authentication rate limits.
- Role-based authorization: public registration always creates a `User`; privileged `Admin` access is assigned only by a trusted database operator.
- Admin-only account listing endpoint. Tasks remain scoped to their owner, including for administrators.
- SQL Server persistence with versioned EF Core migrations.
- Responsive React dashboard with login/register and task CRUD.
- xUnit integration tests using relational SQLite, frontend Vitest tests, and Playwright browser E2E against a real SQL Server service in CI.
- Liveness and database readiness endpoints, JSON console logs, and GitHub Actions quality gates.

## Architecture

```text
src/
  EnterpriseManagement.Api/
    Controllers/  REST endpoints, authentication and admin operations
    Contracts/    Request and response models
    Data/         EF Core DbContext and SQL Server migrations
    Domain/       User and task entities
    Services/     Token and task application services
  EnterpriseManagement.Tests/
frontend/
  src/             React UI, typed API client and unit tests
  e2e/             Browser E2E scenarios
.github/workflows/ci.yml
```

This is intentionally a modular monolith. Microservices should be introduced only when operational requirements justify the added complexity.

## Requirements

- .NET 8 SDK
- Node.js 22+
- SQL Server 2022 or SQL Server LocalDB

## Local setup

1. Copy `src/EnterpriseManagement.Api/appsettings.example.json` to `src/EnterpriseManagement.Api/appsettings.Development.json`.
2. Replace `Jwt:Key` with a random secret of at least 32 bytes. Keep local secrets out of Git.
3. Set `ConnectionStrings:DefaultConnection` to your SQL Server instance.
4. Apply migrations:
   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.33
   dotnet ef database update --project src/EnterpriseManagement.Api --startup-project src/EnterpriseManagement.Api
   ```
5. Start the API:
   ```bash
   dotnet run --project src/EnterpriseManagement.Api --urls http://localhost:5080
   ```
6. In another terminal, start the frontend:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

Swagger is available at `http://localhost:5080/swagger` in Development. The Vite dev server proxies `/api` to the API on port 5080.

## Database migrations and administrator provisioning

Migrations are applied as a deployment step, not automatically by application instances. Review migration SQL before applying it to production.

New accounts always receive the `User` role. There is no public role-assignment endpoint and no default administrator password. To grant the first administrator, use a trusted database-operator session after verifying the person's identity:

```sql
UPDATE dbo.Users
SET Role = N'Admin'
WHERE Email = N'verified-admin@example.com';
```

Replace the email with the verified account. Existing JWTs do not gain new claims automatically; the promoted user must sign in again. Database write access is privileged and administrator changes should be audited operationally.

## Health, readiness and logs

- `GET /health`: process liveness; does not require the database.
- `GET /health/ready`: checks database connectivity and returns HTTP 503 when unavailable.
- ASP.NET Core logs are emitted as structured JSON to standard output. Production hosting should collect stdout/stderr centrally, redact secrets and configure retention/alerting.

## Tests and quality gates

Run backend tests:

```bash
dotnet test src/EnterpriseManagement.Tests/EnterpriseManagement.Tests.csproj --configuration Release
```

Run frontend checks:

```bash
cd frontend
npm install
npm run lint
npm run typecheck
npm test
npm run build
npx playwright install chromium
npm run test:e2e
```

The Playwright scenario uses the live API and SQL Server. The GitHub Actions workflow provisions an ephemeral SQL Server 2022 service, applies EF migrations, starts API and frontend, and exercises registration plus task create/update/delete through Chromium. CI also runs independent backend and frontend jobs.

For local E2E, start SQL Server, apply migrations, start the API on port 5080 and the Vite frontend on port 5173 before running Playwright.

## API

| Method | Endpoint | Auth | Purpose |
|---|---|---:|---|
| POST | `/api/auth/register` | No | Register as User and receive a JWT |
| POST | `/api/auth/login` | No | Authenticate and receive a JWT with role claim |
| GET | `/api/tasks` | User/Admin | List the current user's tasks |
| GET | `/api/tasks/{id}` | User/Admin | Get one owned task |
| POST | `/api/tasks` | User/Admin | Create a task |
| PUT | `/api/tasks/{id}` | User/Admin | Update an owned task |
| DELETE | `/api/tasks/{id}` | User/Admin | Delete an owned task |
| GET | `/api/admin/users` | Admin | List user account metadata |
| GET | `/health` | No | Liveness |
| GET | `/health/ready` | No | Database readiness |

## Security and known limitations

- Passwords are hashed; plaintext passwords are never persisted.
- JWT signing key is configuration-only and must not be committed.
- Role claims are signed into JWTs at login; role changes take effect on the next login.
- Admin privileges are assigned through a trusted database operator, not self-service.
- Task ownership is enforced server-side and is not bypassed by the Admin role.
- Bearer tokens are short-lived; refresh-token rotation and revocation are not implemented.
- Email verification, MFA, distributed rate-limit storage, centralized log aggregation, metrics/tracing, backups, disaster recovery and production deployment infrastructure remain operational follow-up work.
- CI validates migrations and browser flows against an ephemeral SQL Server container; this does not replace staging/production validation with real infrastructure and secrets.

## Roadmap

- [x] Authentication, role authorization and user-scoped task CRUD
- [x] Relational integration tests and frontend unit tests
- [x] SQL Server migration smoke test in CI
- [x] Playwright browser E2E against SQL Server
- [x] Liveness/readiness endpoints and structured JSON logging
- [ ] Production hosting, secrets management, monitoring/alerting, backups and disaster-recovery validation

## License

MIT
