# ArabClinic deployment to Somee

ArabClinic is deployed as an ASP.NET Core MVC application to Somee Windows/IIS
hosting. The deployment workflow is `.github/workflows/deploy.yml`.

The workflow runs for every push to `main` and can also be started manually. It
restores locked NuGet dependencies, builds the complete solution, runs all
tests, publishes only `ArabClinic.Api`, validates the publish output, uploads a
rollback artifact, deploys the publish directory over FTP or FTPS, and checks
the live health and CSS endpoints.

The workflow does not run Entity Framework migrations. Database migrations are
a separate, explicit operation.

## Required GitHub secrets

Configure these under **Repository settings → Secrets and variables → Actions →
Secrets** (or as secrets on a protected `production` environment if the
workflow is later updated to use that environment):

| Secret | Purpose |
| --- | --- |
| `FTP_SERVER` | Somee FTP host or IP address, for example `155.254.246.30` |
| `FTP_USERNAME` | Somee FTP username |
| `FTP_PASSWORD` | Somee FTP password |
| `FTP_SERVER_DIR` | Verified FTP-relative directory containing the deployed `web.config` |

Do not commit any of these values. The server directory is a secret because it
can expose account structure even though it is not usually a credential.

## Optional GitHub variables

Configure these under **Repository settings → Secrets and variables → Actions →
Variables**:

| Variable | Default | Supported values or purpose |
| --- | --- | --- |
| `FTP_PROTOCOL` | `ftp` | `ftp`, `ftps-explicit`, or `ftps-implicit` |
| `FTP_PORT` | `21` | FTP/FTPS port supplied by Somee |
| `SITE_BASE_URL` | `http://arabclinic.somee.com` | Public site URL used after deployment |
| `DEPLOY_VERIFY_RETRIES` | `12` | Number of health/static-file verification attempts |
| `DEPLOY_VERIFY_DELAY_SECONDS` | `10` | Delay between verification attempts |

`FTP_PROTOCOL=ftps-explicit` uses explicit TLS on an FTP connection.
`ftps-implicit` uses an implicit FTPS URL and normally a provider-supplied FTPS
port. Certificate verification remains enabled. Do not disable certificate
verification to work around a host-name mismatch.

## Verify FTP or FTPS support

An FTP endpoint does not prove that FTPS is supported. Check the Somee control
panel/documentation or test with FileZilla, WinSCP, or `lftp` before changing
`FTP_PROTOCOL`.

For an FTPS test, verify all of the following:

1. Somee explicitly identifies the connection as explicit or implicit FTPS.
2. The documented port works.
3. The certificate is valid for the configured host name. An IP address may not
   match a certificate issued to a DNS name.
4. A directory listing and a harmless test-file upload both succeed.

If Somee only supports plain FTP, credentials and files travel without
transport encryption. Use a unique hosting password, restrict access to the
GitHub repository, rotate the password after suspected exposure, and migrate to
FTPS as soon as the provider supports it.

## Identify the correct Somee FTP root

Do not assume `/www.arabclinic.somee.com/` is correct. Some FTP accounts are
already chrooted into the website root, so `/`, `/www.arabclinic.somee.com/`,
and a similarly named subdirectory can refer to different locations.

Before enabling automatic deployment:

1. Connect using FileZilla, WinSCP, or `lftp` with the Somee credentials.
2. Run/list `pwd` and inspect the initial login directory.
3. Locate the directory containing the currently running application's
   `web.config` and `ArabClinic.Api.dll`.
4. Confirm that the same directory contains `wwwroot`, not that it is itself
   `wwwroot`.
5. Set `FTP_SERVER_DIR` to that exact directory.

The correct remote layout after deployment is:

```text
<FTP_SERVER_DIR>/
  web.config
  ArabClinic.Api.dll
  ArabClinic.Api.deps.json
  ArabClinic.Api.runtimeconfig.json
  wwwroot/
    assets/
      css/
        main.css
```

The workflow logs `pwd` and the directory listing before uploading. Review the
first deployment log carefully. The workflow mirrors the *contents* of the
local publish directory into `FTP_SERVER_DIR`; it does not upload an additional
`publish/` directory.

## Safe deployment behavior

- Deployments use a single `somee-production` concurrency group and do not
  overlap.
- Build or test failures stop the job before deployment.
- Only `dotnet publish` output is uploaded.
- `appsettings.Development.json` is removed before upload.
- `appsettings.Production.json` is excluded and an existing remote copy is not
  deleted or overwritten.
- Remote files not present in the new publish output are not deleted. This
  preserves server-managed directories and unrelated files.
- `app_offline.htm` is uploaded before binaries to release IIS file locks.
- An exit trap attempts to remove `app_offline.htm` after success or failure.
  Failure to remove it fails the job and produces a manual-cleanup instruction.
- No database migration is executed automatically.
- A downloadable publish artifact is retained for 30 days for rollback.

Because the workflow intentionally avoids remote deletion, obsolete application
files can remain after dependency removals. Review and remove a confirmed stale
file manually during a controlled maintenance window; do not enable broad FTP
mirror deletion.

## Trigger a deployment

### Automatic

Push a commit to `main`. The deploy workflow starts after GitHub receives the
push.

### Manual

1. Open the repository on GitHub.
2. Select **Actions**.
3. Select **Deploy to Somee**.
4. Select **Run workflow** and choose `main`.

Only deploy a commit after `FTP_SERVER_DIR` and the protocol have been verified.

## Inspect logs and diagnose failures

Open **Actions → Deploy to Somee → the failed run**, then expand the failed
step.

- **Restore/build/test failure:** reproduce with `dotnet restore --locked-mode`,
  `dotnet build --configuration Release`, and
  `dotnet test --configuration Release`.
- **Publish verification failure:** inspect the `Publish MVC application` and
  `Verify published IIS application` steps. A valid publish must contain
  `web.config`, `ArabClinic.Api.dll`, and
  `wwwroot/assets/css/main.css`.
- **FTP login failure:** confirm server, username, password, protocol, port, IP
  allow-listing, and account status.
- **FTP directory failure:** reconnect interactively and verify `pwd`. Do not
  guess the path.
- **Locked DLL/upload failure:** check whether `app_offline.htm` was accepted in
  the IIS application root and whether Somee allows overwriting running files.
- **Site remains offline:** remove `app_offline.htm` manually from the exact
  deployment directory, then investigate the failed workflow step.
- **HTTP verification failure:** inspect the printed response status, headers,
  content type, and first 500 response bytes. The workflow retries to allow IIS
  startup time.

## Static-file troubleshooting

The repository and local publish output have the expected static-file setup:

- `Program.cs` calls `UseStaticFiles()` before routing.
- `_Layout.cshtml` references `~/assets/css/main.css`.
- The source asset is
  `src/ArabClinic.Api/wwwroot/assets/css/main.css`.
- `dotnet publish` produces
  `<publish-root>/wwwroot/assets/css/main.css`.
- The generated IIS `web.config` is at the publish root.

Therefore CI/CD alone does not prove or automatically repair the current
production 404. If `/health` works but `/assets/css/main.css` returns 404:

1. Confirm that the deployed `web.config` and `ArabClinic.Api.dll` are in the
   same directory used as the Somee IIS application root.
2. Confirm that `wwwroot` is directly below that directory. A common bad layout
   is `<site-root>/publish/wwwroot/...` while IIS starts the DLL from
   `<site-root>`.
3. Confirm exact case and name: `wwwroot/assets/css/main.css`.
4. Download the remote CSS file and compare its size/content with the publish
   artifact.
5. Check that the Somee IIS application identity can read the file.
6. Check Somee/IIS static-content and MIME configuration for `.css` (`text/css`).
7. Check for a parent or server-managed `web.config` that overrides static-file
   handling or changes the application root.
8. Use the workflow verification diagnostics to determine whether the response
   is CSS or an HTML error page.

Do not remove or reorder `UseStaticFiles()` without evidence of an application
middleware problem.

## Roll back

1. Open a previously successful GitHub Actions run.
2. Download its `arabclinic-publish-<commit-sha>` artifact.
3. Confirm the artifact contains `web.config`, `ArabClinic.Api.dll`, and
   `wwwroot` at its root.
4. Upload `app_offline.htm` to the verified `FTP_SERVER_DIR`.
5. Upload the contents of the previous artifact over the application files.
6. Remove `app_offline.htm` even if the rollback upload fails.
7. Verify `/health` and `/assets/css/main.css`.

A file rollback does not roll back database schema or data. This workflow never
applies database migrations, so any separately applied database change requires
its own reviewed rollback plan.

## Security notes

- Prefer FTPS when Somee confirms support and certificate validation succeeds.
- Never print or place FTP credentials in workflow YAML, repository files,
  artifacts, issue comments, or logs.
- Keep production connection strings outside the deployed repository, ideally
  in Somee/IIS environment configuration or a protected remote
  `appsettings.Production.json`.
- Restrict who can edit workflows or access repository Actions secrets.
- Rotate FTP credentials periodically and immediately after suspected exposure.
- Consider a protected GitHub `production` environment with required reviewers
  before enabling deployments for additional collaborators.
