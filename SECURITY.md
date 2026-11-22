# Security configuration and maintenance

## Required before deployment

1. Generate a new independent signing secret for this application using a cryptographic generator, e.g. `openssl rand -base64 48`.
2. Set `JWT_SIGNING_KEY` in the deployment secret store or process environment. Never put the value in a tracked file or in a PR comment.
3. On first-time database provisioning, set an independent random `BOOTSTRAP_ADMIN_PASSWORD` of at least 12 characters in the deployment secret store. It is only read if the configured bootstrap administrator does not yet exist. Do not commit or disclose this password.
4. Restart every token issuer and verifier with the new key. Previously issued tokens must become invalid; users must sign in again. The old committed key is compromised and must not be reused.
5. Verify missing/short/non-ASCII keys prevent startup, new tokens validate, and old tokens are rejected.

The application now fails at startup without an ASCII signing key of at least 32 characters. Length alone does not ensure randomness.
Generic error responses replace developer exception details outside Development.
New passwords require at least 12 characters. Existing password hashes are not changed.
Uploads are limited to 20 MiB per multipart body and 21 MiB total request. The 1 MiB ValueLengthLimit applies to URL-encoded form values; multipart text fields can be larger (up to the configured multipart body limit). In-memory buffering is 64 KiB. Individual upload and content-type restrictions require separate endpoint-level validation.
Unrestricted cross-origin access has been removed. Configure an explicit CORS allowlist only if a reviewed cross-origin client needs it.

## Remaining release blockers

- The project targets .NET 7, which is out of support. Migrate the framework and related EF/Identity packages together to a supported release, with database and authentication integration tests.
- Review current NuGet advisories, including System.Linq.Dynamic.Core when present. A dependency upgrade is not validated by a textual framework change.
- Review production database initialization/seeding, authorization, issuer/audience validation and file upload content validation before deployment.
- Remove tracked bin/obj artifacts in a separate reviewed cleanup commit; .gitignore does not remove files already committed.
- A source-level change cannot invalidate deployed secrets or erase Git history. Rotate first; any history rewrite needs a coordinated plan.
- Project licensing remains undecided; no new reuse license is granted by these changes.

## Validation status

A GitHub Actions .NET 7 Release build and dependency restore passed on the pull-request branch. Database, JWT issuer/verifier, bootstrap administrator and file-upload integration tests have NOT been completed. Do not deploy until these checks pass and production secrets are configured.

## Payment integration and remaining dependency risks

The payment service is an unimplemented integration. URL generation now reports that configuration is missing, and verification always rejects an unverified transaction. Do not re-enable it until the provider response is authenticated and checked against the expected order, amount, currency and payment state. Historical gateway configuration and signing material have been removed from the current service; any previously committed payment credentials must be revoked at their issuer before reuse of this integration.

Compatible Azure Identity, MSAL and Duende IdentityServer security patches are explicitly referenced while the legacy parent packages remain. These patches do not make .NET 7 supported. The transitive audit still requires a coordinated framework/IdentityServer/AutoMapper migration: AutoMapper 11 has no compatible patched version, and System.Text.Json / ASN.1 must be assessed together with the deployed shared runtime. Do not suppress these findings or downgrade packages to hide them. Database, external identity-provider and payment-provider integration tests remain required before release.
