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

Requires the .NET SDK version selected by `global.json`.

On macOS/Linux, SQL Server LocalDB is not available. Create the local Docker
configuration, replace the example password, then start SQL Server. It is
exposed on `localhost:1433`.

```bash
cp .env.example .env
docker compose up -d db
```

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet run --project src/ArabClinic.Web
```

The web application is available at the root URL. The health endpoint is
available at `/health`.

Patient management is backed by SQL Server through Entity Framework Core. Set
the `ConnectionStrings:ArabClinic` value with .NET user secrets or the
`ConnectionStrings__ArabClinic` environment variable before applying migrations.
For example:

```bash
dotnet user-secrets set "ConnectionStrings:ArabClinic" \
  "<your SQL Server connection string>" \
  --project src/ArabClinic.Web
dotnet tool restore
dotnet ef database update \
  --project src/ArabClinic.Infrastructure \
  --startup-project src/ArabClinic.Web
```

Authentication uses Patient, Doctor, and Admin roles. Configure the first admin
with `AdminSeed__Email`, `AdminSeed__Password`, and optionally
`AdminSeed__FullName` in environment variables or user secrets.

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
`FTP_SERVER`, `FTP_USERNAME`, `FTP_PASSWORD`, and `FTP_SERVER_DIR`. Optional
repository variables are `FTP_PROTOCOL`, `FTP_PORT`, `SITE_BASE_URL`,
`DEPLOY_VERIFY_RETRIES`, and `DEPLOY_VERIFY_DELAY_SECONDS`. Configure the
application settings above in the hosting control panel; the workflow does not
upload production secrets.

## UI template

The MVC presentation adapts the Clinic Bootstrap template by BootstrapMade to
the ArabClinic project modules and Razor routing. The original template source
and license notice are retained in `Clinic-v1.0.0/Readme.txt`.

## CI/CD

- **CI** runs on pull requests to `main` and pushes to `main` or `dev`. It
  restores locked dependencies, checks formatting, builds with warnings treated
  as errors, runs tests with coverage, and uploads test results.
- **CD** runs on `main`, semantic version tags such as `v1.2.3`, or manually. It
  builds a multi-architecture image and publishes it to
  `ghcr.io/<owner>/<repository>`.
- **Dependabot** checks NuGet packages, GitHub Actions, and base images weekly.

After the first image is published, set its package visibility and deployment
permissions in GitHub Packages. Add a deployment job to `cd.yml` once the target
runtime (for example Azure Container Apps, AWS ECS, or Kubernetes) is selected.
