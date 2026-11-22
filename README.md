# Jobby

## Current status

Legacy ASP.NET Core / Entity Framework application. The web entry point is `WebUI/WebUI.csproj`, with `Application`, `Domain`, and `Infrastructure` projects. This codebase currently targets **.NET 7 (end of support)**. A successful compilation does not establish production readiness.

## Build (no production data)

```sh
dotnet restore WebUI/WebUI.csproj
dotnet build WebUI/WebUI.csproj --configuration Release --no-restore
```

The repository's `global.json` pins the SDK used for legacy validation. A framework and NuGet migration to a supported .NET release is still required, followed by integration and regression testing.

## Required runtime configuration

Supply independently generated secrets through the deployment environment or protected secret store, not Git:

- `JWT_SIGNING_KEY`: a random ASCII signing secret of at least 32 characters; previously committed signing material must not be reused.
- `BOOTSTRAP_ADMIN_PASSWORD`: a separately generated password of at least 12 characters, needed when a fresh database must create the bootstrap administrator.
- Database connection and any external integration settings: configure privately for the intended environment; **never use a production database for experiments**.

The initialiser may create or initialise the database at application startup. Review and back up the deployment database before enabling startup. Authentication, database seeding, file uploads and permissions have **not** been integration tested.

See [SECURITY.md](SECURITY.md) for required key rotation, security limitations and known release blockers.

## Contribution and licensing

Changes should be proposed through reviewed pull requests and checked with CI before merging. No open-source reuse license has been selected.
