# Enterprise Management System

A full-stack work-management portfolio application built with **ASP.NET Core 8**, **Entity Framework Core**, **SQL Server**, **React 18**, and **TypeScript**.

## Implemented

- REST API for task creation, listing, editing, status changes and deletion.
- JWT bearer authentication with account registration and login.
- Password hashing via ASP.NET Core Identity's password hasher.
- Role-based authorization with default `User` and privileged `Admin` roles.
- Admin-only user listing endpoint; public registration cannot set or escalate roles.
- SQL Server persistence using EF Core.
- User-scoped task access (a user cannot read or modify another user's tasks).
- API request validation, Problem Details error handling and Swagger/OpenAPI in Development.
- Responsive React + TypeScript interface with login/register, task filters, counters and CRUD.
- Backend domain and integration tests, including role escalation and authorization checks.
- GitHub Actions workflow for backend restore/test/build and frontend lint/typecheck/test/build.

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
  src/             React UI, typed API client and tests
.github/workflows/ci.yml
```

This first slice is intentionally a modular monolith. Microservices and BFF extraction should follow a demonstrated need, not be added as decorative complexity.

## Requirements

- .NET 8 SDK
- Node.js 20+
- SQL Server or SQL Server LocalDB

## Run locally

1. Copy `src/EnterpriseManagement.Api/appsettings.example.json` to `src/EnterpriseManagement.Api/appsettings.Development.json`.
2. Replace `Jwt:Key` with a random secret of at least 32 bytes. Keep local secrets out of Git.
3. Set `ConnectionStrings:DefaultConnection` to your SQL Server instance.
4. Start the API: `dotnet run --project src/EnterpriseManagement.Api/EnterpriseManagement.Api.csproj --urls http://localhost:5080`.
5. In another terminal: `cd frontend && npm install && npm run dev`.

Swagger is available at `http://localhost:5080/swagger` in Development. The Vite dev server proxies `/api` to the API on port 5080.

## Database setup and administrator provisioning

Apply reviewed migrations from the repository root:

```bash
dotnet ef database update --project src/EnterpriseManagement.Api --startup-project src/EnterpriseManagement.Api
```

New accounts always receive the `User` role. There is deliberately no public role-assignment endpoint and no default administrator password. To grant the first administrator, use a trusted database-operator session after verifying the person's identity:

```sql
UPDATE dbo.Users
SET Role = N'Admin'
WHERE Email = N'verified-admin@example.com';
```

Replace the email with the verified account. Existing JWTs do not gain new claims automatically: the promoted user must sign in again to receive an Admin-role token. Treat database write access as privileged and audit administrator changes operationally.

For production, apply reviewed migrations as a deployment step. Do not run schema changes automatically from multiple app instances.

## Tests and quality checks

```bash
dotnet test src/EnterpriseManagement.Tests/EnterpriseManagement.Tests.csproj --configuration Release
cd frontend
npm install
npm run lint
npm run typecheck
npm test
npm run build
```

The CI workflow runs these checks on pushes and pull requests to `main`. A green workflow is the evidence of a successful CI run; local or CI execution must not be inferred from source code alone.

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
| GET | `/health` | No | Health check |

## Security and known limitations

- Passwords are hashed; plaintext passwords are never persisted.
- JWT key is configuration-only and must not be committed.
- Role claims are signed into JWTs at login; role changes take effect on the next login.
- Admin privileges are assigned through a trusted database operator, not self-service.
- Tasks remain scoped to the authenticated user's ID, including for administrators.
- The current implementation uses short-lived bearer tokens without refresh-token rotation.
- Email verification, token revocation, audit events, SQL Server integration tests, observability, deployment and production hardening remain planned.

## Roadmap

- [x] Initial API and React application
- [x] Registration/login and JWT-protected task CRUD
- [x] Unit tests and CI pipeline definition
- [x] Rate limiting and role-based authorization
- [ ] EF Core migration and SQL Server integration tests
- [ ] Frontend component tests and browser end-to-end tests
- [ ] Structured logging, tracing and deployment pipeline

## License

MIT
