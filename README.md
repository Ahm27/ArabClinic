# ArabClinic

ArabClinic is an ASP.NET Core Web API organized with clear dependency boundaries.

## Structure

```text
src/
  ArabClinic.Api/             HTTP endpoints and application composition
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

The health endpoint is available at `/health`. OpenAPI is available at
`/openapi/v1.json` in the Development environment.

To run in a container:

```bash
docker compose up --build
```

The API is then available at `http://localhost:8080`.

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
