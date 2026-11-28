# ArabClinic

ArabClinic is a server-rendered ASP.NET Core MVC application organized with clear dependency boundaries.

## Structure

```text
src/
  ArabClinic.Web/             MVC controllers, Razor views, and application composition
  ArabClinic.Application/     Use cases and application contracts
  ArabClinic.Domain/          Entities and domain rules
  ArabClinic.Infrastructure/  Persistence and external integrations
tests/
  ArabClinic.UnitTests/       Fast domain and application tests
```

Dependencies point inward: `Web -> Application/Infrastructure`,
`Infrastructure -> Application/Domain`, and `Application -> Domain`.

## Local development

Install the .NET SDK version selected by `global.json`. ArabClinic requires a
SQL Server connection string named `ConnectionStrings:ArabClinic` during
startup. The repository intentionally does not contain one because connection
strings may include credentials. If it is missing, startup stops with:

```text
Connection string 'ArabClinic' is not configured.
```

### Windows and Visual Studio

For SQL Server LocalDB, run these commands from the repository root in
PowerShell or a terminal:

```powershell
dotnet user-secrets set "ConnectionStrings:ArabClinic" "Server=(localdb)\MSSQLLocalDB;Database=MediCare;Trusted_Connection=True;TrustServerCertificate=True;" --project src/ArabClinic.Web
dotnet tool restore
dotnet ef database update --project src/ArabClinic.Infrastructure --startup-project src/ArabClinic.Web
```

For SQL Server Express, use this connection string instead:

```powershell
dotnet user-secrets set "ConnectionStrings:ArabClinic" "Server=.\SQLEXPRESS;Database=MediCare;Trusted_Connection=True;TrustServerCertificate=True;" --project src/ArabClinic.Web
```

Restart Visual Studio after setting user secrets, then select
`ArabClinic.Web` as the startup project.

### macOS, Linux, or Docker Desktop

On macOS/Linux, SQL Server LocalDB is not available. Create the local Docker
configuration, replace the example password, then start SQL Server. It is
exposed on `localhost:1433`.

```bash
cp .env.example .env
docker compose up -d db
```

On Windows PowerShell, copy the example with:

```powershell
Copy-Item .env.example .env
```

Edit `.env` and replace the placeholder with a strong local password:

```dotenv
MSSQL_SA_PASSWORD=replace-with-a-strong-local-password
```

`.env.example` is a committed template used by Docker Compose. The real `.env`
file is ignored by Git. Visual Studio and `dotnet run` do not automatically
load `.env`; use .NET User Secrets for those workflows.

To run the web application directly while SQL Server runs in Docker, configure
the matching connection string:

```bash
dotnet user-secrets set "ConnectionStrings:ArabClinic" \
  "Server=localhost,1433;Database=MediCare;User Id=sa;Password=<the password from .env>;Encrypt=False;TrustServerCertificate=True" \
  --project src/ArabClinic.Web
dotnet tool restore
dotnet ef database update \
  --project src/ArabClinic.Infrastructure \
  --startup-project src/ArabClinic.Web
```

### Build and run

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet run --project src/ArabClinic.Web
```

The web application is available at the root URL. The health endpoint is
available at `/health`.

Authentication uses Patient, Doctor, and Admin roles. Configure the first admin
with user secrets for local development:

```bash
dotnet user-secrets set "AdminSeed:Email" "admin@arabclinic.com" --project src/ArabClinic.Web
dotnet user-secrets set "AdminSeed:Password" "<a strong unique password>" --project src/ArabClinic.Web
dotnet user-secrets set "AdminSeed:FullName" "Clinic Administrator" --project src/ArabClinic.Web
```

To run in a container:

```bash
docker compose up --build
```

The web application is then available at `http://localhost:8080`.

## Production deployment requirements

The hosting environment must provide:

- .NET 9 ASP.NET Core Hosting Bundle (IIS) or a .NET 9 container runtime.
- SQL Server reachable by the web application.
- `ConnectionStrings__ArabClinic` with the production SQL Server connection.
- `AdminSeed__Email`, `AdminSeed__Password`, and optionally
  `AdminSeed__FullName` for initial admin access. Remove the admin password from
  the host configuration after the account has been created.
- Persistent ASP.NET Core Data Protection keys when running more than one
  instance, so authentication cookies survive restarts and load balancing.

The application applies EF Core migrations and seeds reference data during
startup, so the database login needs schema-change permissions during a
deployment. Restrict those permissions afterward if migrations are managed by
a separate release process. Never commit production connection strings or
admin credentials to this repository.

The Somee workflow additionally requires GitHub repository secrets
`FTP_SERVER`, `FTP_USERNAME`, `FTP_PASSWORD`, `FTP_SERVER_DIR`, and
`DB_CONNECTION_STRING`. The deployment job generates
`appsettings.Production.json` from `DB_CONNECTION_STRING` only after downloading
the verified package, so the secret is not committed or stored in the retained
build artifact. Optional repository variables are `FTP_PROTOCOL`, `FTP_PORT`,
`SITE_BASE_URL`, `DEPLOY_VERIFY_RETRIES`, and `DEPLOY_VERIFY_DELAY_SECONDS`.

## UI template

The MVC presentation adapts the Clinic Bootstrap template by BootstrapMade to
the ArabClinic project modules and Razor routing. The original template source
and license notice are retained in `Clinic-v1.0.0/Readme.txt`.

## CI/CD

- The single `Build, Test, and Deploy` workflow runs quality checks on pull
  requests and pushes. It restores locked dependencies and repository tools,
  checks formatting, builds, tests, verifies EF migrations, validates the IIS
  package, and uploads diagnostic logs.
- Successful pushes to `main` deploy the verified package to Somee and publish
  the multi-architecture container image to `ghcr.io/<owner>/<repository>`.
- Semantic version tags such as `v1.2.3` publish the container image without a
  Somee production deployment.
- **Dependabot** checks NuGet packages, GitHub Actions, and base images weekly.

After the first image is published, set its package visibility and deployment
permissions in GitHub Packages. Complete logs and test reports are available in
the workflow's `pipeline-diagnostics-*` artifact.
