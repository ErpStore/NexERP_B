using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using System.Text;
using V.SMART.Api.Auth;
using V.SMART.Api.Authorization;
using V.SMART.Api.Caching;
using V.SMART.Api.HealthChecks;
using V.SMART.Api.Logging;
using V.SMART.Api.Middleware;
using V.SMART.Api.Reporting;
using V.SMART.Api.Services;
using V.SMART.Shared.DependencyInjection;
using V.SMART.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// M0-03-03 — fail fast on missing, empty or known-default secrets, before anything below
// consumes one (Jwt:Secret is read further down, and JwtTokenService is registered later).
// Throws InvalidOperationException naming the key and the remediation, never the value.
StartupConfigurationValidator.Validate(builder.Configuration, requireJwt: true);

// ─── M2-B11 — structured logging (R-23) ──────────────────────────────────────────────────
//
// Replaces the flat |-delimited text files of FileLoggingService for THIS HOST ONLY. The
// Blazor and MAUI hosts keep FileLoggingService; see the ILoggingService registration below
// for why, and KB-113 (docs/kb/architecture/observability.md) for the whole contract.
//
// The configuration is deliberately split in two:
//   * the "Serilog" section, read by ReadFrom.Configuration, is where a DEPLOYMENT adds a
//     second sink (Seq, Elasticsearch, OTLP, a file share) without a rebuild. The sink
//     decision itself is DEFERRED pending Q-16 (deployment topology) — KB-113 §4 records the
//     deferral and the criteria a sink must meet before it is chosen;
//   * the "Observability:Logging" section, bound below, carries the two RETENTION values,
//     because their inequality is a policy decision that has to be reviewable, not a sink
//     argument.
var loggingOptions =
    builder.Configuration.GetSection(AuditLoggingOptions.SectionName).Get<AuditLoggingOptions>()
    ?? new AuditLoggingOptions();

// R-23's action item requires audit retention INDEPENDENT OF and LONGER THAN diagnostic
// retention. Configuration can express the opposite, so it is refused at startup rather than
// discovered when someone asks what a user did last March and the file is gone. Same
// fail-fast shape as StartupConfigurationValidator above.
if (loggingOptions.AuditRetentionDays <= loggingOptions.DiagnosticRetentionDays)
{
    throw new InvalidOperationException(
        $"Observability:Logging:AuditRetentionDays ({loggingOptions.AuditRetentionDays}) must be " +
        $"greater than Observability:Logging:DiagnosticRetentionDays " +
        $"({loggingOptions.DiagnosticRetentionDays}). The user-action audit trail is a business " +
        "record and R-23 requires its retention to exceed diagnostic retention.");
}

var logDirectory = string.IsNullOrWhiteSpace(loggingOptions.Directory)
    ? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Logs")
    : loggingOptions.Directory;

builder.Host.UseSerilog((context, _, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    // Without this, ANY "{@Tenant}" anywhere in the codebase serialises
    // TenantInfo.ConnectionString — credentials — into a searchable, retained sink. See
    // TenantInfoDestructuringPolicy.
    .Destructure.With(new TenantInfoDestructuringPolicy())
    .WriteTo.Console()
    // The audit stream. Filtered on the EventType discriminator that
    // StructuredLoggingService stamps on every LogUserAction event, which is what makes the
    // audit trail SEPARABLE from diagnostics — a distinct file set, a distinct retention, and
    // a query surface that does not have to sift diagnostics first.
    //
    // CompactJsonFormatter, not a text template: UserName, Machine, IpAddress, Screen, Action
    // and AdditionalInfo land as named JSON fields, so "what did user X do on the Sales Order
    // screen in March" is three predicates over audit-2026-03-*.json rather than a grep for
    // "| Screen: ". That question is the acceptance test for this task.
    .WriteTo.Logger(audit => audit
        .Filter.ByIncludingOnly(Matching.WithProperty<string>(
            "EventType", value => value == StructuredLoggingService.UserActionEventType))
        .WriteTo.File(
            formatter: new CompactJsonFormatter(),
            path: Path.Combine(logDirectory, "audit-.json"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: loggingOptions.AuditRetentionDays,
            fileSizeLimitBytes: loggingOptions.FileSizeLimitBytes,
            rollOnFileSizeLimit: true,
            shared: true))
    // Everything else. Same format, much shorter retention.
    .WriteTo.Logger(diagnostics => diagnostics
        .Filter.ByExcluding(Matching.WithProperty<string>(
            "EventType", value => value == StructuredLoggingService.UserActionEventType))
        .WriteTo.File(
            formatter: new CompactJsonFormatter(),
            path: Path.Combine(logDirectory, "diagnostics-.json"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: loggingOptions.DiagnosticRetentionDays,
            fileSizeLimitBytes: loggingOptions.FileSizeLimitBytes,
            rollOnFileSizeLimit: true,
            shared: true)));

// M2-A01-02 — the screen-right filter is registered GLOBALLY (KB-105 §6.2) rather than per
// controller, so that an unannotated controller is still swept rather than silently skipped.
// It decides only for endpoints declaring [RequireScreen] + [RequireRight]; every other
// endpoint, including [AllowAnonymous] ones, passes through untouched. No controller declares
// them yet — M2-A02 does that — so all six existing endpoints are unaffected by this task.
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ScreenRightAuthorizationFilter>();
});

// M2-A06 — one error contract for the whole API (ADR-002 §4). Registers ProblemDetails
// services and replaces MVC's automatic 400 body with the canonical one, so the
// [ApiController] model-state short-circuit and an explicit controller return produce the
// identical shape.
builder.Services.AddErrorContract();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "V.SMART API", Version = "v1" });

    // M2-B10 — the operation id becomes the generated TypeScript method name, so it is declared
    // explicitly on every action as the route name (`[HttpGet("…", Name = "getCurrencies")]`)
    // instead of being left to Swashbuckle's default. Renaming one is a breaking change to every
    // call site in the SPA, so it is a deliberate, reviewable edit rather than a by-product of
    // renaming a C# method. KB-112 records the naming rule; KB-114 §11 makes it a template
    // obligation, and OpenApiConformanceTests fails the build when an action omits it.
    c.CustomOperationIds(api => api.ActionDescriptor.AttributeRouteInfo?.Name);

    // M2-B10 — the XML <summary> on each action becomes the operation description in the
    // document and the doc comment on the generated client method. Requires
    // <GenerateDocumentationFile> in the csproj (set there, with CS1591 suppressed).
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "V.SMART.Api.xml");
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Already validated above (M0-03-03): StartupConfigurationValidator is the single code path
// that decides whether Jwt:Secret is acceptable — null, empty, whitespace, under 32 UTF-8
// bytes and known-default values all threw InvalidOperationException before this line.
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

// M2-B07 — host-platform registration, kept out of AddVSmartDomain() on purpose. Four
// business services take IHttpClientFactory (BankService, PaymentsService, ReceiptsService,
// AdvaceAdjustmentService), which only AddHttpClient() supplies. V.SMART.Web has had this
// since before this task (Program.cs:232); the API did not, so those four would have stayed
// unresolvable here.
builder.Services.AddHttpClient();

// M2-B07 — this host must not build-validate its service graph, and that has to be said
// explicitly rather than assumed. WebApplicationBuilder switches ValidateOnBuild AND
// ValidateScopes on by itself whenever the hosting environment is Development
// (HostApplicationBuilder -> HostingHostBuilderExtensions.CreateDefaultServiceProviderOptions),
// and Development is exactly what both launch profiles set
// (Properties/launchSettings.json:9,18). ValidateOnBuild eagerly builds a call site for every
// descriptor that has an implementation type, so the seven registrations listed below — which
// depend on host seams V.SMART.Api deliberately does not have until M2-B06 / M2-B08 — abort
// startup with AggregateException("Some services are not able to be constructed") before the
// host ever reaches a request. ValidateScopes is left at the framework's own default (on in
// Development, off elsewhere): captive-dependency detection is not what has to be relaxed.
//
// M2-B08 update: this block does NOT come off yet. M2-B06 and M2-B08 between them supply
// IPathProvider and IFileUploadService, but IUserService and IUserThemePreferenceService still
// need IJSRuntime — a Blazor concept with no meaningful API implementation (M2-B08.md
// §Prerequisites) — so two of the original seven registrations remain genuinely unresolvable.
// This block comes off only when something supplies IJSRuntime for this host, or those two
// services are refactored not to need it. Re-run with ValidateOnBuild = true first to confirm
// the count before removing it — do not assume from this comment alone.
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateOnBuild = false;
    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
});

// M2-B07 — the whole domain graph (repositories, the IRepository<> open generic, UnitOfWork,
// ~285 business services, MasterDbContext, the tenant-resolved ApplicationDbContext and
// AutoMapper) now comes from the single shared composition root in V.SMART.Shared.
// Before this, the API registered exactly one business service (ICurrencyService) and no
// IRepository<> open generic, so any second controller compiled fine and then failed at
// activation time with a DI resolution error.
//
// Originally (M2-B07): IPathProvider, IFileUploadService, IFileOpener and IJSRuntime had no
// V.SMART.Api implementation, leaving exactly seven registrations unresolvable here — measured
// by running this host with ValidateOnBuild = true, not assumed:
//     ReportService                 needs IPathProvider
//     IUserService                  needs IPathProvider + IJSRuntime
//     IGSTITCService                needs IPathProvider
//     IUserThemePreferenceService   needs IJSRuntime
//     ICompanyService               needs IFileUploadService
//     IItemService                  needs IFileUploadService
//     IEnquirySalesService          needs IPathProvider, transitively via ReportService
// M2-B06 supplied IFileUploadService (below), resolving ICompanyService/IItemService.
// M2-B08 supplies IPathProvider (ApiPathProvider, below), resolving ReportService/
// IGSTITCService/IEnquirySalesService. That leaves exactly two still unresolvable —
// IUserService and IUserThemePreferenceService, both needing IJSRuntime, a Blazor concept with
// no meaningful web-API implementation (M2-B08.md §Prerequisites) — so ValidateOnBuild stays
// off. Injecting either of those two into a controller still fails at activation time. The
// equivalent build-time guarantee for the shared graph is enforced instead by
// tests/V.SMART.Shared.Tests/DependencyInjection/AddVSmartDomainTests.cs, which validates the
// identical graph with the host seams supplied.
builder.Services.AddVSmartDomain(builder.Configuration);

// M2-B06 — the API host's file storage seam. IFileUploadService is host-specific by design and
// AddVSmartDomain() deliberately omits it (M2-B07); these three lines are the API's supply of it,
// and they belong HERE, beside that call, never inside it — V.SMART.Web and the MAUI head have
// their own implementations and must keep them.
//
// Registering the concrete type and forwarding the interface to the same instance is deliberate:
// FilesController needs ApiFileUploadService's two API-only members (SaveCorrespondenceFileAsync,
// which takes a Stream because IBrowserFile cannot exist in an HTTP request, and RootPath), while
// CompanyService and ItemService need the interface. One scoped instance serves both.
//
// Consequence for the M2-B07 note above: two of the seven unresolvable registrations —
// ICompanyService and IItemService — resolve from here on. The remaining five still need
// IPathProvider / IJSRuntime (M2-B08), so ValidateOnBuild stays off.
builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection(FileStorageOptions.SectionName));
builder.Services.AddScoped<ApiFileUploadService>();
builder.Services.AddScoped<IFileUploadService>(sp => sp.GetRequiredService<ApiFileUploadService>());

// M2-B08 — the API host's report-template path seam. IPathProvider is host-specific by design
// (WebPathProvider/DesktopPathProvider each resolve a different IWebHostEnvironment/AppContext
// root) and AddVSmartDomain() deliberately omits it for the same reason IFileUploadService is
// omitted above. Resolves ReportService, IGSTITCService and IEnquirySalesService, none of which
// were constructible in this host before this task — see the ValidateOnBuild note above.
builder.Services.AddScoped<IPathProvider, ApiPathProvider>();

// M2-A01-02 — server-side screen-right authorization (ADR-004, KB-105 §6.2). Both are scoped:
// the provider reaches IUnitOfWork, which AddVSmartDomain() registers scoped over the
// tenant-resolved ApplicationDbContext, and the filter resolves the provider per request.
// KB-105 §6.2 places these beside AddVSmartDomain() and notes that M2-B07's sibling
// AddVSmartApiAuthorization() extension may later absorb them; they must never move into
// V.SMART.Shared, where the Blazor and MAUI hosts would also receive them.
//
// M2-A01-03 — the provider now resolves through a memory cache (KB-105 §8). The cache itself is
// the framework singleton: it has to outlive the request to be a cache at all, while the provider
// stays scoped because IUnitOfWork is. A scoped consumer of a singleton is not a captive
// dependency; the reverse would be.
//
// No SizeLimit is set on this shared cache, and that is deliberate. Setting one obliges EVERY
// future IMemoryCache consumer in this host to set MemoryCacheEntryOptions.Size or Set() throws at
// runtime — a trap exported to code that has no idea this task exists. The rights entries set
// Size = 1 anyway, so a cap can be imposed later without touching the provider. KB-105 §8.2.
builder.Services.AddMemoryCache();

// TTL is resolved and validated HERE, before builder.Build(), rather than through the options
// pattern: ValidateOnBuild is off in this host (see the block above), so a lazy IValidateOptions
// failure would surface on the first authorized request instead of at startup. KB-105 §8.4
// consequence 2 requires a TTL above 300 s to fail startup — it is the only guard against someone
// "reducing database load" by widening the window in which a revoked right still works.
builder.Services.AddSingleton(UserRightsCacheOptions.FromConfiguration(builder.Configuration));

builder.Services.AddScoped<IUserRightsProvider, UserRightsProvider>();
builder.Services.AddScoped<ScreenRightAuthorizationFilter>();

// M2-A08 — row scope, the SECOND authorization axis (KB-108). Screen rights answer "may this user
// open the Leads screen"; row scope answers "which Leads rows may this user see", and ADR-004
// covers only the first. Scoped for the same reason the rights provider is (it reaches IUnitOfWork)
// and sharing the same IMemoryCache and the same TTL, under its own key prefix.
//
// There is no global filter for it, deliberately: a row scope is not a yes/no decision that can be
// taken before the action runs, it is a predicate that has to be composed into the query. The
// enforcement lives in RowScopeQueryExtensions.ApplyRowScope; RowScopeStartupValidator below is what
// makes forgetting to call it a startup failure rather than a silent leak.
builder.Services.AddScoped<IRowScopeProvider, RowScopeProvider>();

builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthStateProvider>();
builder.Services.AddSingleton(new JwtTokenService(builder.Configuration));

// M2-B11 — this host resolves the STRUCTURED ILoggingService; the other two do not.
//
// AddVSmartDomain() above registers ILoggingService -> FileLoggingService
// (V.SMART/V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs), and this line
// deliberately overrides it for V.SMART.Api only — last registration wins for a
// GetRequiredService resolution.
//
// WHY NOT CHANGE IT INSIDE AddVSmartDomain(), which is where M2-B11's task file points by
// default: that extension is the composition root for ALL THREE hosts. Neither V.SMART.Web nor
// the MAUI head configures a Serilog sink, so changing it there would route a LIVE audit trail
// — 494 LogUserAction call sites across 202 files — into whatever ILogger providers those
// hosts happen to have, which is a console and a debug window. That deletes the audit trail
// instead of restructuring it, and R-23's action item is explicitly "PRESERVE the user-action
// audit trail as a first-class feature". The Blazor host therefore keeps writing
// App_Data/Logs/UserLogs/*.txt exactly as before, unchanged by this task, and continues to be
// the durable audit record for its own traffic until it is retired.
//
// StructuredLoggingService takes IHttpContextAccessor as an OPTIONAL constructor argument
// (the TenantProvider.cs:18 pattern), so the class itself is safe to register in the MAUI
// host the day that host gains a sink. That is the explicit resolution of the
// "IHttpContextAccessor does not exist in MAUI" question; recorded in KB-113 §6.
builder.Services.AddScoped<ILoggingService, StructuredLoggingService>();

// M2-B11 — health checks (KB-041 item C2). Built-in
// Microsoft.Extensions.Diagnostics.HealthChecks, no third-party stack: it ships in the shared
// framework, so this adds no package.
//
// Only readiness checks are registered, and both carry the "ready" tag. /health/live matches
// NO check at all (see MapHealthChecks below) — that is what keeps liveness free of any
// database dependency, rather than relying on a check being fast.
builder.Services.Configure<HealthProbeOptions>(
    builder.Configuration.GetSection(HealthProbeOptions.SectionName));

builder.Services.AddHealthChecks()
    .AddCheck<MasterDatabaseHealthCheck>(
        MasterDatabaseHealthCheck.Name,
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" })
    .AddCheck<TenantDatabaseHealthCheck>(
        TenantDatabaseHealthCheck.Name,
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" });

// M2-B09 — output caching for the /api/v1/reference route group only, never globally. The
// policy is hand-written (ReferenceCachePolicy -> TenantScopedOutputCachePolicy) because the
// framework's default policy declines to cache authenticated responses, and every endpoint in
// that group is [Authorize] — composing on the default would have produced a cache that
// silently stores nothing. Opting in means owning the key, so the key includes the TenantId
// claim and the policy fails closed when it cannot establish one.
builder.Services.AddOutputCache(options => ReferenceCachePolicy.Register(options, builder.Configuration));

var app = builder.Build();

// M2-A01-02 — a misannotated action is a property of the assembly, not of a request, so it is
// caught here rather than after someone has been denied (KB-105 D-4/D-6). Same shape as the
// M0-03-03 configuration check above: throw InvalidOperationException naming every offender.
// This host cannot rely on DI validation for it — ValidateOnBuild is off (see below).
ScreenRightStartupValidator.Validate(app.Services);

// M2-A08 — the same treatment for row scope: an action that serves a scoped entity without saying
// whether it scopes is a property of the assembly, so it stops the host here rather than leaking
// rows in production. Passes today because no endpoint returns a scoped entity yet — Leads has no
// controller. The point is that the first one cannot be added without deciding (KB-108 §5).
RowScopeStartupValidator.Validate(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// M2-A06 — first in the pipeline, before UseCors, so it wraps CORS, authentication,
// authorization and MVC: correlation id, then the global exception handler, then a
// status-code handler that gives framework-generated bodiless failures (a 401 challenge, an
// unmatched route's 404) the same application/problem+json shape. Do not move this below
// UseCors — an exception thrown by the CORS middleware would escape unhandled.
app.UseErrorContract();

// M2-B11 — push the M2-A06 correlation id into Serilog's LogContext for the whole request, so
// EVERY event emitted while handling it carries the same CorrelationId property: the request
// summary below, any diagnostic a service writes, and the user action the request performed.
// That property is the join key — it is the entire reason M2-A06 is this task's hard
// dependency. It sits immediately after UseErrorContract so that the id it publishes is the
// same one CorrelationId.For has already cached in HttpContext.Items and echoed in the
// X-Correlation-Id response header; a caller-supplied header is still ignored (Q-35).
app.Use(async (context, next) =>
{
    using (LogContext.PushProperty("CorrelationId", CorrelationId.For(context)))
    {
        await next();
    }
});

// One structured summary event per request instead of the framework's three unstructured
// ones. Health probes are demoted to Verbose: an orchestrator polls /health/ready every few
// seconds and at Information level it would dominate the diagnostic sink and push real events
// out of retention.
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, exception) =>
        exception != null ? LogEventLevel.Error
        : httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error
        : httpContext.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose
        : LogEventLevel.Information;
});

app.UseCors("AngularDev");
app.UseAuthentication();
app.UseAuthorization();

// M2-B09 — deliberately AFTER UseAuthentication and UseAuthorization, and the order is
// load-bearing in both directions:
//
//   * After UseAuthentication, because the cache key is the TenantId claim. Placed earlier,
//     HttpContext.User would still be anonymous when the policy builds the key, the policy
//     would fail closed on every request, and the cache would silently never store anything.
//   * After UseAuthorization, so a cache hit is still an authenticated, authorized request.
//     The cache short-circuits MVC, not the security pipeline — an expired or missing token
//     gets 401 whether or not a cached body exists for that tenant.
app.UseOutputCache();

app.MapControllers();

// M2-B11 — health endpoints. Deliberately OUTSIDE /api/v1: they are infrastructure, not part
// of the versioned API contract, and an orchestrator should not have to track an API version
// to know whether the process is alive.
//
// Both are ANONYMOUS, because an orchestrator cannot present a JWT. That is exactly why
// HealthResponseWriter emits an allow-list and never a description or an exception. If the
// deployment cannot restrict these two paths at the network layer, that is a recorded risk
// (KB-113 §3), not something the endpoint can compensate for.
//
// LIVENESS: Predicate = _ => false means the health service runs NO registered check and
// reports Healthy if the process can execute this line at all. This is not laziness. A
// liveness probe that touches the database gets the container KILLED whenever the database is
// slow, converting a database incident into a cluster-wide crash loop and taking away the very
// instances that would recover when it comes back.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthResponseWriter.WriteAsync
}).AllowAnonymous();

// READINESS: master DB + a configurable subset of tenant DBs (default: one), each reported
// individually — "Unhealthy" without naming which check failed is not actionable. The tenant
// check reads MasterDbContext.Tenants and connects directly; it cannot use ITenantProvider,
// which resolves from HttpContext.User (TenantProvider.cs:33-34) and a probe has no user.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthResponseWriter.WriteAsync
}).AllowAnonymous();

try
{
    app.Run();
}
finally
{
    // Flush buffered log events before the process exits. Without this, the last events
    // before a shutdown — often the interesting ones — are lost.
    Log.CloseAndFlush();
}
