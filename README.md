# ArabClinic

ArabClinic is a server-rendered ASP.NET Core MVC application organized with clear dependency boundaries.

## Structure

```text
src/
  ArabClinic.Api/             MVC controllers, Razor views, and application composition
  ArabClinic.Application/     Use cases and application contracts
  ArabClinic.Domain/          Entities and domain rules
  ArabClinic.Infrastructure/  Persistence and external integrations
tests/
  ArabClinic.UnitTests/       Fast domain and application tests
```

Dependencies point inward: `Api -> Application/Infrastructure`,
`Infrastructure -> Application/Domain`, and `Application -> Domain`.

## Local development

Requires the .NET SDK version selected by `global.json`.

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet run --project src/ArabClinic.Api
```

The web application is available at the root URL. The health endpoint is
available at `/health`.

To run in a container:

```bash
docker compose up --build
```

The web application is then available at `http://localhost:8080`.

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
