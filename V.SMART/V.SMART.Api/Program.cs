using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using V.SMART.Api.Auth;
using V.SMART.Api.Authorization;
using V.SMART.Api.Middleware;
using V.SMART.Shared.DependencyInjection;
using V.SMART.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// M0-03-03 — fail fast on missing, empty or known-default secrets, before anything below
// consumes one (Jwt:Secret is read further down, and JwtTokenService is registered later).
// Throws InvalidOperationException naming the key and the remediation, never the value.
StartupConfigurationValidator.Validate(builder.Configuration, requireJwt: true);

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
// REMOVE THIS BLOCK once M2-B06 and M2-B08 supply IPathProvider / IFileUploadService /
// IFileOpener for this host. At that point the graph validates and the check must go back on.
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
// Deliberately still absent, and therefore not resolvable in this host: IPathProvider,
// IFileUploadService, IFileOpener and IJSRuntime have no V.SMART.Api implementation yet
// (M2-B08 and M2-B06). Exactly seven registrations therefore stay unresolvable here —
// measured by running this host with ValidateOnBuild = true, not assumed (M2-B07):
//     ReportService                 needs IPathProvider
//     IUserService                  needs IPathProvider + IJSRuntime
//     IGSTITCService                needs IPathProvider
//     IUserThemePreferenceService   needs IJSRuntime
//     ICompanyService               needs IFileUploadService
//     IItemService                  needs IFileUploadService
//     IEnquirySalesService          needs IPathProvider, transitively via ReportService
// That gap is expected and is not closed by this task. Injecting any of the seven into a
// controller still fails at activation time, exactly as it did before this task. The
// equivalent build-time guarantee for the shared graph is enforced instead by
// tests/V.SMART.Shared.Tests/DependencyInjection/AddVSmartDomainTests.cs, which validates the
// identical graph with the host seams supplied.
builder.Services.AddVSmartDomain(builder.Configuration);

// M2-A01-02 — server-side screen-right authorization (ADR-004, KB-105 §6.2). Both are scoped:
// the provider reaches IUnitOfWork, which AddVSmartDomain() registers scoped over the
// tenant-resolved ApplicationDbContext, and the filter resolves the provider per request.
// NO CACHE here on purpose — M2-A01-03 puts one behind IUserRightsProvider, and a cache added
// now would make it impossible to tell a denial by rule from a denial by stale entry.
// KB-105 §6.2 places these beside AddVSmartDomain() and notes that M2-B07's sibling
// AddVSmartApiAuthorization() extension may later absorb them; they must never move into
// V.SMART.Shared, where the Blazor and MAUI hosts would also receive them.
builder.Services.AddScoped<IUserRightsProvider, UserRightsProvider>();
builder.Services.AddScoped<ScreenRightAuthorizationFilter>();

builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthStateProvider>();
builder.Services.AddSingleton(new JwtTokenService(builder.Configuration));

var app = builder.Build();

// M2-A01-02 — a misannotated action is a property of the assembly, not of a request, so it is
// caught here rather than after someone has been denied (KB-105 D-4/D-6). Same shape as the
// M0-03-03 configuration check above: throw InvalidOperationException naming every offender.
// This host cannot rely on DI validation for it — ValidateOnBuild is off (see below).
ScreenRightStartupValidator.Validate(app.Services);

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

app.UseCors("AngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
