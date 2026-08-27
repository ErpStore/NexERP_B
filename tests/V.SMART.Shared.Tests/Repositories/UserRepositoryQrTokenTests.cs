using Microsoft.AspNetCore.Identity;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Repository.MasterRepository.AdminRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.Tests.Infrastructure;
using Xunit;

namespace V.SMART.Shared.Tests.Repositories;

/// <summary>
/// M2-A08 (Q-05 / R-16) — <c>UserRepository.GetUserByQrToken</c> must not return expired users.
///
/// <para><b>Why the fix is in the query.</b> Before M2-A08 the query filtered <c>QrToken</c>,
/// <c>IsQrEnabled</c> and <c>IsActive</c> but not <c>QrExpiryDate</c>, so it returned expired
/// users and correctness depended on the caller checking afterwards. Both Blazor callers do —
/// <c>QrLogin.razor:50-56</c> and <c>Login.razor:422-429</c> — and they keep their checks. Any
/// third caller did not, and that is the whole of R-16.</para>
///
/// <para><b>Why these tests live in the Shared suite and not in <c>V.SMART.Api.Tests</c></b>
/// (where the M2-A08 task file expected them): the code under test is a <c>V.SMART.Shared</c>
/// repository over <c>ApplicationDbContext</c>, and this project already carries the EF fixture
/// and the provider decision INV-031 made. A deviation, recorded rather than hidden.</para>
/// </summary>
public class UserRepositoryQrTokenTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task An_expired_QrExpiryDate_returns_null()
    {
        var token = Guid.NewGuid();
        var repository = Repository(Seed(token, expiry: DateTime.Now.AddDays(-1)));

        Assert.Null(await repository.GetUserByQrToken(token));
    }

    [Fact]
    public async Task A_null_QrExpiryDate_still_returns_the_user()
    {
        // Preserves current behaviour: a null expiry is not an expired one. Both callers guard on
        // HasValue first (Login.razor:422, QrLogin.razor:50), so tightening this would refuse
        // logins that succeed today — the opposite of the intended fix.
        var token = Guid.NewGuid();
        var repository = Repository(Seed(token, expiry: null));

        Assert.NotNull(await repository.GetUserByQrToken(token));
    }

    [Fact]
    public async Task A_future_QrExpiryDate_still_returns_the_user()
    {
        // RegisterUpsert.razor:1125 sets the expiry a year ahead when a token is issued, so this is
        // the ordinary case, not an edge one.
        var token = Guid.NewGuid();
        var repository = Repository(Seed(token, expiry: DateTime.Now.AddYears(1)));

        Assert.NotNull(await repository.GetUserByQrToken(token));
    }

    [Fact]
    public async Task The_pre_existing_filters_are_unchanged()
    {
        // IsQrEnabled and IsActive were already in the query and stay in it: M2-A08 adds a
        // predicate, it does not rewrite the method.
        var disabled = Guid.NewGuid();
        var inactive = Guid.NewGuid();
        var unknown = Guid.NewGuid();

        var db = _factory.CreateContext();
        db.Users.Add(NewUser(disabled, expiry: null, isQrEnabled: false, isActive: true));
        db.Users.Add(NewUser(inactive, expiry: null, isQrEnabled: true, isActive: false));
        await db.SaveChangesAsync();

        var repository = Repository(db);

        Assert.Null(await repository.GetUserByQrToken(disabled));
        Assert.Null(await repository.GetUserByQrToken(inactive));
        Assert.Null(await repository.GetUserByQrToken(unknown));
    }

    private ApplicationDbContext Seed(Guid token, DateTime? expiry)
    {
        var db = _factory.CreateContext();
        db.Users.Add(NewUser(token, expiry, isQrEnabled: true, isActive: true));
        db.SaveChanges();
        return db;
    }

    private static User NewUser(Guid token, DateTime? expiry, bool isQrEnabled, bool isActive)
        => new()
        {
            UserName = $"qr-{token:N}",
            UserPassword = "irrelevant",
            // Required on the entity (User.cs:33-34); the InMemory provider enforces it.
            Role = V.SMART.Shared.Data.Enum.UserRole.User,
            QrToken = token,
            IsQrEnabled = isQrEnabled,
            IsActive = isActive,
            QrExpiryDate = expiry
        };

    private static UserRepository Repository(ApplicationDbContext db)
        => new(
            db,
            new PasswordHasher<User>(),
            new FakeLoggingService(),
            new CurrentUserService(new TestAuthenticationStateProvider()));
}
