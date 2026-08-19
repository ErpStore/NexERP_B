using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using V.SMART.Shared.Data;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.DependencyInjection;
using V.SMART.Shared.Services;
using V.SMART.Shared.Tests.Infrastructure;
using Xunit;

namespace V.SMART.Shared.Tests.DependencyInjection;

/// <summary>
/// DI-graph validation for the shared composition root introduced by M2-B07.
///
/// A green build does not prove the object graph resolves — the failure mode this task
/// exists to eliminate is a controller that compiles and then throws at activation time.
/// <c>BuildServiceProvider(validateScopes: true, validateOnBuild: true)</c> walks every
/// descriptor that has an implementation type and verifies each constructor parameter is
/// registered, so it turns that runtime failure into a test failure.
///
/// The tests deliberately register test doubles for the *host seams*
/// (<see cref="IPathProvider"/>, <see cref="IFileUploadService"/>, <see cref="IFileOpener"/>,
/// <see cref="IJSRuntime"/>, a bare <see cref="HttpClient"/> and
/// <see cref="AuthenticationStateProvider"/>). Those are per-host by design and
/// <c>AddVSmartDomain()</c> must not register them — see
/// <see cref="AddVSmartDomain_DoesNotRegisterHostSeams"/>, which pins that seam, and
/// <see cref="AddVSmartDomain_WithoutHostSeams_FailsValidation"/>, which pins the
/// consequence for <c>V.SMART.Api</c> (M2-B06 / M2-B08).
/// </summary>
public class AddVSmartDomainTests
{
    // A syntactically valid connection string. No connection is ever opened: UseSqlServer
    // only records the string, and ValidateOnBuild constructs no DbContext instance.
    private const string MasterDbConnectionString =
        "Server=(local);Database=VSmartMaster_Test;Trusted_Connection=True;TrustServerCertificate=True";

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MasterDb"] = MasterDbConnectionString
            })
            .Build();

    /// <summary>
    /// Registers the host-provided seams that <c>AddVSmartDomain()</c> deliberately omits.
    /// A real host supplies these; the API host does not yet (M2-B06 / M2-B08).
    /// </summary>
    private static void AddHostSeamDoubles(IServiceCollection services)
    {
        services.AddScoped(_ => Mock.Of<IPathProvider>());
        services.AddScoped(_ => Mock.Of<IFileUploadService>());
        services.AddScoped(_ => Mock.Of<IFileOpener>());
        services.AddScoped(_ => Mock.Of<IJSRuntime>());
        services.AddScoped(_ => new HttpClient());

        // AddHttpClient() is a host-platform registration (V.SMART.Web/Program.cs:232,
        // V.SMART.Api/Program.cs). Four business services take IHttpClientFactory.
        services.AddHttpClient();

        // CurrentUserService takes AuthenticationStateProvider, and every host registers a
        // different implementation (Web/MAUI: CustomAuthStateProvider; API: ApiAuthStateProvider,
        // V.SMART/V.SMART.Api/Program.cs:89). It is therefore a host seam too, and the one
        // seam V.SMART.Api already supplies.
        services.AddScoped<AuthenticationStateProvider>(_ => new TestAuthenticationStateProvider());
    }

    [Fact]
    public void AddVSmartDomain_WithHostSeams_BuildsAndValidatesTheWholeGraph()
    {
        var services = new ServiceCollection();
        AddHostSeamDoubles(services);

        services.AddVSmartDomain(BuildConfiguration());

        // Throws AggregateException listing every unconstructable descriptor if the graph
        // has a hole. No assertion is needed beyond "does not throw".
        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        Assert.NotNull(provider);
    }

    [Fact]
    public void AddVSmartDomain_DoesNotRegisterHostSeams()
    {
        var services = new ServiceCollection();

        services.AddVSmartDomain(BuildConfiguration());

        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IPathProvider));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IFileOpener));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IFileUploadService));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IJSRuntime));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(HttpClient));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(AuthenticationStateProvider));
    }

    /// <summary>
    /// Characterises the documented gap: without the host seams — which is exactly
    /// <c>V.SMART.Api</c>'s situation today — validation fails. This is expected, not a
    /// defect, and it is closed by M2-B06 (<c>IFileUploadService</c>/<c>IFileOpener</c>) and
    /// M2-B08 (<c>IPathProvider</c>).
    /// </summary>
    [Fact]
    public void AddVSmartDomain_WithoutHostSeams_FailsValidation()
    {
        var services = new ServiceCollection();

        services.AddVSmartDomain(BuildConfiguration());

        var ex = Assert.Throws<AggregateException>(() => services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }));

        Assert.Contains(nameof(IPathProvider), ex.ToString());
    }

    [Fact]
    public void AddVSmartDomain_RegistersTheOpenGenericRepository()
    {
        var services = new ServiceCollection();

        services.AddVSmartDomain(BuildConfiguration());

        Assert.Contains(services, d => d.ServiceType == typeof(IRepository<>));
    }

    /// <summary>
    /// <c>ApplicationDbContext</c> is tenant-resolved through
    /// <c>ITenantDbContextFactory.CreateDbContext()</c>. Registering it with
    /// <c>AddDbContext</c> instead would silently break database-per-tenant isolation, so
    /// the descriptor must carry an implementation *factory*, not an implementation type.
    /// </summary>
    [Fact]
    public void ApplicationDbContext_IsRegisteredAsATenantFactoryDelegate()
    {
        var services = new ServiceCollection();

        services.AddVSmartDomain(BuildConfiguration());

        var descriptor = Assert.Single(
            services, d => d.ServiceType == typeof(ApplicationDbContext));

        Assert.NotNull(descriptor.ImplementationFactory);
        Assert.Null(descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
