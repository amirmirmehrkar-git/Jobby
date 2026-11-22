# Security configuration and maintenance

## Required before deployment

1. Generate a new independent signing secret for this application using a cryptographic generator, e.g. `openssl rand -base64 48`.
2. Set `JWT_SIGNING_KEY` in the deployment secret store or process environment. Never put the value in a tracked file or in a PR comment.
3. Restart every token issuer and verifier with the new key. Previously issued tokens must become invalid; users must sign in again. The old committed key is compromised and must not be reused.
4. Verify missing/short/non-ASCII keys prevent startup, new tokens validate, and old tokens are rejected.

The application now fails at startup without an ASCII signing key of at least 32 characters. Length alone does not ensure randomness.
Generic error responses replace developer exception details outside Development.
New passwords require at least 12 characters. Existing password hashes are not changed.
Uploads are limited to 20 MiB (21 MiB total request); form values to 1 MiB and in-memory buffering to 64 KiB.
Unrestricted cross-origin access has been removed. Configure an explicit CORS allowlist only if a reviewed cross-origin client needs it.

## Remaining release blockers

- The project targets .NET 7, which is out of support. Migrate the framework and related EF/Identity packages together to a supported release, with database and authentication integration tests.
- Review current NuGet advisories, including System.Linq.Dynamic.Core when present. A dependency upgrade is not validated by a textual framework change.
- Review production database initialization/seeding, authorization, issuer/audience validation and file upload content validation before deployment.
- Remove tracked bin/obj artifacts in a separate reviewed cleanup commit; .gitignore does not remove files already committed.
- A source-level change cannot invalidate deployed secrets or erase Git history. Rotate first; any history rewrite needs a coordinated plan.
- Project licensing remains undecided; no new reuse license is granted by these changes.

## Validation status

No .NET SDK/database is available in the audit environment. This change needs a build and authentication integration validation before merge.
