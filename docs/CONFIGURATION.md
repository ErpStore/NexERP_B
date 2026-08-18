# Configuration — where secrets come from

**No credential is ever committed to this repository — not in an active line, and not in a
commented-out line.** `appsettings.json` declares configuration *shape* only: every secret
key is present with an empty value so the shape is discoverable, and the value is supplied
at run time by .NET user-secrets (developer machines) or environment variables (servers and
CI).

Established by task **M0-03-01** and extended to C# source by **M0-03-02**; rotation of the
already-exposed values is M0-04; purging them from git history is M0-05.

## Keys, and which host needs which

| Key | `V.SMART.Web` (Blazor Server) | `V.SMART.Api` (Web API) | Secret? |
|---|---|---|---|
| `ConnectionStrings:MasterDb` | Required | Required | **Yes** |
| `Jwt:Secret` | — | Required | **Yes** |
| `Jwt:Issuer` | — | Required | No — committed value `V.SMART.Api` |
| `Jwt:Audience` | — | Required | No — committed value `V.SMART.Angular` |
| `Jwt:ExpiresMinutes` | — | Required | No — committed value `480` |

`ConnectionStrings:MasterDb` is the **master** database only. Per-tenant connection strings
are read from the master database's `Tenants` table at run time and are not configuration
(see KB-014, `docs/kb/architecture/multi-tenancy.md`).

Both hosts read these through `WebApplication.CreateBuilder(args)`, which already registers
the user-secrets provider in the `Development` environment and the environment-variable
provider in every environment. No `Program.cs` change is required to use either source.

## Developer machines — user-secrets

Both projects carry a `UserSecretsId`, so `dotnet user-secrets` works out of the box. Run
from the repository root:

```bash
dotnet user-secrets set "ConnectionStrings:MasterDb" "<your local master connection string>" --project V.SMART/V.SMART.Web/V.SMART.Web.csproj

dotnet user-secrets set "ConnectionStrings:MasterDb" "<your local master connection string>" --project V.SMART/V.SMART.Api/V.SMART.Api.csproj
dotnet user-secrets set "Jwt:Secret"                 "<a locally generated 32+ byte random string>" --project V.SMART/V.SMART.Api/V.SMART.Api.csproj
dotnet user-secrets set "Jwt:Issuer"                 "V.SMART.Api"      --project V.SMART/V.SMART.Api/V.SMART.Api.csproj
dotnet user-secrets set "Jwt:Audience"               "V.SMART.Angular"  --project V.SMART/V.SMART.Api/V.SMART.Api.csproj
dotnet user-secrets set "Jwt:ExpiresMinutes"         "480"              --project V.SMART/V.SMART.Api/V.SMART.Api.csproj
```

`Jwt:Issuer`, `Jwt:Audience` and `Jwt:ExpiresMinutes` are **not** secrets and keep their
committed values in `appsettings.json`; the three lines above are only needed if you want to
override them locally.

Generate your own `Jwt:Secret`. Never reuse a value found in this repository or in its
history — every such value is treated as compromised (KB-060 R-02).

List what is set (values are printed, so do not paste the output anywhere):

```bash
dotnet user-secrets list --project V.SMART/V.SMART.Api/V.SMART.Api.csproj
```

User-secrets are stored per user, outside the repository, under
`%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json` on Windows.

## Servers and CI — environment variables

.NET's hierarchical key separator in an environment-variable name is a **double
underscore**:

| Key | Environment variable |
|---|---|
| `ConnectionStrings:MasterDb` | `ConnectionStrings__MasterDb` |
| `Jwt:Secret` | `Jwt__Secret` |
| `Jwt:Issuer` | `Jwt__Issuer` |
| `Jwt:Audience` | `Jwt__Audience` |
| `Jwt:ExpiresMinutes` | `Jwt__ExpiresMinutes` |

```bash
# Linux / container
export ConnectionStrings__MasterDb="<master connection string>"
export Jwt__Secret="<32+ byte random string>"
```

```powershell
# Windows
$env:ConnectionStrings__MasterDb = "<master connection string>"
$env:Jwt__Secret = "<32+ byte random string>"
```

Note: the .NET MAUI head (`V.SMART/V.SMART`) does **not** read environment variables by
default — `MauiAppBuilder.Configuration` has no environment-variable provider unless one is
added explicitly. `MauiProgram.cs` therefore reads `ConnectionStrings__MasterDb` directly
from the process environment (falling back to `builder.Configuration`), and throws naming
the variable if neither supplies a value.

## Design-time keys — `dotnet ef` only

The two `IDesignTimeDbContextFactory` implementations in
`V.SMART/V.SMART.Shared/Data/MigrationData/` are used **only** by the `dotnet ef` tooling,
never by a running host. They read their connection string through
`DesignTimeConnectionString.Resolve` — the environment first, then this project's
user-secrets — and **throw** if neither supplies one. There is no default value.

| Key | Environment variable | Used by | Points at |
|---|---|---|---|
| `ConnectionStrings:MasterDb` | `ConnectionStrings__MasterDb` | `MasterDbContextFactory` | the master database (same key the hosts use) |
| `ConnectionStrings:DesignTimeTenantDb` | `ConnectionStrings__DesignTimeTenantDb` | `ApplicationDbContextFactory` | *any* reachable tenant database — `ApplicationDbContext` is per tenant, so it deliberately does not overload the master key |

```bash
dotnet user-secrets set "ConnectionStrings:MasterDb"           "<master connection string>" --project V.SMART/V.SMART.Shared/V.SMART.Shared.csproj
dotnet user-secrets set "ConnectionStrings:DesignTimeTenantDb" "<a tenant connection string>" --project V.SMART/V.SMART.Shared/V.SMART.Shared.csproj
```

Running the tooling (verified 2026-08-18). `--framework` is required because
`V.SMART.Shared` multi-targets, and the startup project must be `V.SMART.Web` —
`V.SMART.Api` does not reference `Microsoft.EntityFrameworkCore.Design`:

```bash
dotnet ef dbcontext info \
  --project V.SMART/V.SMART.Shared/V.SMART.Shared.csproj \
  --startup-project V.SMART/V.SMART.Web/V.SMART.Web.csproj \
  --context MasterDbContext --framework net9.0
```

Per-tenant connection strings used at **run time** still come from the master database's
`Tenants` table, not from configuration (KB-014). Nothing here changes that.

## The hosts refuse to start when configuration is wrong — this is deliberate

Added by task **M0-03-03**. Both `V.SMART.Api` and `V.SMART.Web` validate their configuration
in `StartupConfigurationValidator.Validate(builder.Configuration, requireJwt: …)`
(`V.SMART/V.SMART.Shared/Services/StartupConfigurationValidator.cs`), called immediately after
`WebApplication.CreateBuilder(args)` and before anything consumes a secret. A failed check
throws `InvalidOperationException` and the process stops.

A host that starts with no secret configured, or with the published default JWT secret, is
worse than one that will not start: the first silently issues forgeable tokens, the second
tells you exactly what to fix.

| Key | Checked in | Rejected when |
|---|---|---|
| `ConnectionStrings:MasterDb` | both hosts | missing, empty, whitespace, or equal to a known pre-rotation default |
| `Jwt:Secret` | `V.SMART.Api` | missing, empty, whitespace, fewer than 32 bytes in UTF-8, or equal to the known default |
| `Jwt:Issuer`, `Jwt:Audience` | `V.SMART.Api` | missing, empty, or whitespace |

The message you will see takes this form (`<key>` is the failing key, `<KEY>` its
environment-variable spelling):

```
Unhandled exception. System.InvalidOperationException: Startup configuration is invalid:
<key> is missing, empty, or whitespace. This host deliberately refuses to start (task
M0-03-03); see docs/CONFIGURATION.md.
  Environment variable (servers, CI): <KEY>=<value>
  User-secrets (developer machines): dotnet user-secrets set "<key>" "<value>" --project <this host's .csproj>
  This message never repeats the offending value, because exception messages reach log files.
```

The middle clause varies with the reason: `is missing, empty, or whitespace`, `is shorter
than the required 32 bytes in UTF-8, which is what SymmetricSecurityKey with HMAC-SHA256
needs`, or `matches a known pre-rotation / published default value (see
docs/kb/risks/technical-debt-register.md R-01 / R-02)`. Fix it by setting the key as
described above and starting the host again.

**Two things this is not.** The known-defaults list is stored as SHA-256 digests of the
leaked values, never as plaintext, and it is a **tripwire, not a security control** — it
catches only exact reuse of a value already known to be compromised, not a weak new one. And
the message never echoes the value that failed, because this application's logger writes
plaintext log files per user per day.

**CI note.** A build agent with no secrets configured will hit this the moment it *runs* a
host. Keep CI build-only, or give it dummy values that are not on the deny-list — a valid
`ConnectionStrings:MasterDb` shape pointing at nothing, and a `Jwt:Secret` of at least 32
bytes.

## Rules

- Never put a credential in `appsettings.json` or `appsettings.Development.json`, in any
  environment, including inside a comment. Both `appsettings.Development.json` files
  currently contain a `Logging` section only, and must stay that way.
- Never commit a `secrets.json`.
- A `UserSecretsId` is **not** a secret — it is only a folder name, and it is committed
  deliberately.
- If a credential does reach the working tree, treat it as compromised: rotate it, do not
  merely delete it.
