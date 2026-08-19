using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using V.SMART.Api.Auth;
using V.SMART.Shared.DependencyInjection;
using V.SMART.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// M0-03-03 — fail fast on missing, empty or known-default secrets, before anything below
// consumes one (Jwt:Secret is read further down, and JwtTokenService is registered later).
// Throws InvalidOperationException naming the key and the remediation, never the value.
StartupConfigurationValidator.Validate(builder.Configuration, requireJwt: true);

builder.Services.AddControllers();
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

// M2-B07 — the whole domain graph (repositories, the IRepository<> open generic, UnitOfWork,
// ~285 business services, MasterDbContext, the tenant-resolved ApplicationDbContext and
// AutoMapper) now comes from the single shared composition root in V.SMART.Shared.
// Before this, the API registered exactly one business service (ICurrencyService) and no
// IRepository<> open generic, so any second controller compiled fine and then failed at
// activation time with a DI resolution error.
//
// Deliberately still absent, and therefore not resolvable in this host: IPathProvider,
// IFileOpener and IFileUploadService have no V.SMART.Api implementation yet (M2-B08 and
// M2-B06), so ReportService, IUserService, IGSTITCService and IUserThemePreferenceService
// remain unresolvable here. That gap is expected and is not closed by this task.
builder.Services.AddVSmartDomain(builder.Configuration);

builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthStateProvider>();
builder.Services.AddSingleton(new JwtTokenService(builder.Configuration));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
