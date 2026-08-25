using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Repository.MasterRepository.AdminRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.Tests.Infrastructure;
using Xunit;

namespace V.SMART.Shared.Tests.Data;

/// <summary>
/// Task M0-06 (risk R-09) — pins the removal of the default "Administrator" seed and proves
/// BR-AUTH-001 still holds afterwards.
///
/// Provider note (INV-031, KB-003): the fixture is the InMemory provider and
/// <c>EnsureCreated()</c> DOES apply the model's HasData seeds — Screens, UOM, Category,
/// Store and the rest all materialise. So <see cref="FreshDatabase_HasNoDefaultAdministrator"/>
/// is a real database assertion, not a model assertion standing in for one. The seeds that
/// remain are asserted here too, so that a future change which removes the seed block
/// wholesale (rather than just the User row) fails loudly.
/// </summary>
public sealed class SeedDataTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    /// <summary>
    /// The EF model must carry no HasData seed for <see cref="User"/> at all. Asserting on the
    /// model (not just on a created database) is what stops the seed being reintroduced with a
    /// different hash: any User seed fails this test, whatever its password.
    /// </summary>
    [Fact]
    public void Model_HasNoSeededUserWithKnownPasswordHash()
    {
        using var context = _factory.CreateContext();

        // The runtime model is read-optimised and throws on GetSeedData(); the design-time
        // model is the one that carries HasData. EF Core 9 message, verbatim, when the runtime
        // model is used: "The requested configuration is not stored in the read-optimized
        // model, please use 'DbContext.GetService<IDesignTimeModel>().Model'."  The interface
        // lives in Microsoft.EntityFrameworkCore.Metadata, NOT .Infrastructure (EF Core 9.0.5);
        // it is fully qualified below so the two Infrastructure namespaces in scope cannot be
        // confused for it.
        var designTimeModel = context.GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>().Model;
        var userEntityType = designTimeModel.FindEntityType(typeof(User));
        Assert.NotNull(userEntityType);

        var seeded = userEntityType!.GetSeedData().ToList();

        Assert.Empty(seeded);
    }

    /// <summary>
    /// A database created from the current model contains no users whatsoever — and in
    /// particular no account named "Administrator". Per INV-031 the HasData seeds ARE applied
    /// by EnsureCreated() under this provider, so an empty Users table here means the seed is
    /// genuinely gone rather than merely unapplied.
    /// </summary>
    [Fact]
    public async Task FreshDatabase_HasNoDefaultAdministrator()
    {
        using var context = _factory.CreateContext();

        Assert.Empty(await context.Users.ToListAsync());
        Assert.False(await context.Users.AnyAsync(u => u.UserName == "Administrator"));
    }

    /// <summary>
    /// The Screens catalogue and the other master seeds are NOT part of what M0-06 removes.
    /// They are the permission catalogue (BR-AUTH-002) and ScreenCode literals depend on them.
    /// </summary>
    [Fact]
    public async Task FreshDatabase_StillSeedsTheScreensCatalogue()
    {
        using var context = _factory.CreateContext();

        Assert.Equal(152, await context.Screens.CountAsync());
    }

    /// <summary>
    /// BR-AUTH-001 is unbroken. A user created with the same IPasswordHasher&lt;User&gt;
    /// implementation production registers (PasswordHasher&lt;User&gt; — V.SMART.Api/Program.cs:106,
    /// V.SMART.Web/Program.cs:270, V.SMART/MauiProgram.cs:267) still authenticates through
    /// UserRepository.LoginAsync, which this task did not modify.
    /// </summary>
    [Fact]
    public async Task Login_WithRealHash_StillSucceeds()
    {
        using var context = _factory.CreateContext();
        var hasher = new PasswordHasher<User>();

        var user = new User
        {
            UserName = "m0-06-real-user",
            IsActive = true,
            Role = UserRole.Administrator,
        };
        user.UserPassword = hasher.HashPassword(user, "a-real-password");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context, hasher);

        var result = await repository.LoginAsync("m0-06-real-user", "a-real-password");

        Assert.NotNull(result);
        Assert.Equal(user.UserId, result!.UserId);
    }

    /// <summary>
    /// The wrong password must not authenticate — the VerifyHashedPassword half of BR-AUTH-001.
    /// </summary>
    [Fact]
    public async Task Login_WithWrongPassword_Fails()
    {
        using var context = _factory.CreateContext();
        var hasher = new PasswordHasher<User>();

        var user = new User
        {
            UserName = "m0-06-wrong-password",
            IsActive = true,
            Role = UserRole.Administrator,
        };
        user.UserPassword = hasher.HashPassword(user, "a-real-password");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context, hasher);

        Assert.Null(await repository.LoginAsync("m0-06-wrong-password", "not-the-password"));
    }

    /// <summary>
    /// The IsActive half of BR-AUTH-001: LoginAsync filters on IsActive before it ever verifies
    /// the hash (UserRepository.cs:37-38), so a deactivated account cannot log in even with the
    /// correct password.
    /// </summary>
    [Fact]
    public async Task Login_WithInactiveUser_Fails()
    {
        using var context = _factory.CreateContext();
        var hasher = new PasswordHasher<User>();

        var user = new User
        {
            UserName = "m0-06-inactive-user",
            IsActive = false,
            Role = UserRole.Administrator,
        };
        user.UserPassword = hasher.HashPassword(user, "a-real-password");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = CreateRepository(context, hasher);

        Assert.Null(await repository.LoginAsync("m0-06-inactive-user", "a-real-password"));
    }

    private static UserRepository CreateRepository(
        V.SMART.Shared.Data.ApplicationDbContext context,
        IPasswordHasher<User> hasher)
        => new(
            context,
            hasher,
            new FakeLoggingService(),
            new CurrentUserService(new TestAuthenticationStateProvider()));
}
