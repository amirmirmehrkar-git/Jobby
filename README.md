# Jobby

## Current status

Legacy ASP.NET Core / Entity Framework application. The web entry point is `WebUI/WebUI.csproj`, with `Application`, `Domain`, and `Infrastructure` projects. This codebase currently targets **.NET 7 (end of support)**. A successful compilation does not establish production readiness.

## Build (no production data)

```sh
python3 scripts/validate-dependency-versions.py
dotnet tool restore --tool-manifest WebUI/.config/dotnet-tools.json
dotnet restore WebUI/WebUI.csproj
dotnet build WebUI/WebUI.csproj --configuration Release --no-restore
dotnet run --project tests/DependencySmoke/DependencySmoke.csproj --configuration Release
```

The repository's `global.json` pins the SDK used for legacy validation. A framework and NuGet migration to a supported .NET release is still required, followed by integration and regression testing.

## Dependency maintenance

ASP.NET Core and Entity Framework packages are aligned at `7.0.20`; the local `dotnet-ef` tool uses the same release. Shared package references must move together across the four projects. CI rejects mismatched versions, restores local tools, checks `dotnet ef --version`, and builds the web project. The smoke checks exercise the application's JWT decoder with valid, altered, expired and differently signed tokens, require exact role membership, and verify that dynamic table sorting accepts property names and rejects method expressions. They do not start the application or connect to a database.

Protected endpoints compare complete comma-separated role names with case-sensitive matching. An authenticated user needs at least one role listed by the endpoint. Empty role claims, partial names, missing tokens and empty role requirements are forbidden; whitespace around role names is ignored. None of the existing protected endpoints uses an empty role requirement. Regression checks cover both permitted multi-role access and invalid signed role claims.

The obsolete, unused `ICSharpCode.SharpZipLib.dll` reference has been removed. The Dynamic LINQ dependency uses the patched parser's default restrictions; do not enable arbitrary method calls in order to accept untrusted sort input.

The unused RestSharp dependency and historical payment-provider configuration have also been removed. Payment integration remains unimplemented: generating a checkout URL reports missing configuration, and verification rejects every unverified transaction. The smoke checks cover this behavior. Azure Identity, MSAL and Duende IdentityServer use compatible security patches. Remaining AutoMapper and shared-runtime security findings require the supported-framework migration described in `SECURITY.md`.

Dependabot checks all four manifest directories in one configuration and groups copies of the same package into a single version-update PR. Routine updates are limited to minor and patch versions using `allow.update-types`; security updates remain eligible, including fixes that require a major upgrade. Security PRs are grouped separately. No update is automatically merged. AutoMapper/IdentityServer and .NET major upgrades need a coordinated migration with integration tests.

References: [Dependabot configuration](https://docs.github.com/en/code-security/reference/supply-chain-security/dependabot-options-reference), [EF tool/runtime compatibility](https://learn.microsoft.com/en-us/ef/core/get-started/overview/install), [Dynamic LINQ security defaults](https://github.com/zzzprojects/System.Linq.Dynamic.Core#breaking-changes).

## Required runtime configuration

Supply independently generated secrets through the deployment environment or protected secret store, not Git:

- `JWT_SIGNING_KEY`: a random ASCII signing secret of at least 32 characters; previously committed signing material must not be reused.
- `BOOTSTRAP_ADMIN_PASSWORD`: a separately generated password of at least 12 characters, needed when a fresh database must create the bootstrap administrator.
- Database connection and any external integration settings: configure privately for the intended environment; **never use a production database for experiments**.

The initialiser may create or initialise the database at application startup. Review and back up the deployment database before enabling startup. Authentication, database seeding, file uploads and permissions have **not** been integration tested.

See [SECURITY.md](SECURITY.md) for required key rotation, security limitations and known release blockers.

## Contribution and licensing

Changes should be proposed through reviewed pull requests and checked with CI before merging. No open-source reuse license has been selected.
