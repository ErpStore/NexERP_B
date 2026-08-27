using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using V.SMART.Api.Auth;
using V.SMART.Api.Controllers;
using V.SMART.Api.Tests.Infrastructure;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService;
using V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AdminService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IMasterSettings;
using V.SMART.Shared.Services;
using V.SMART.Shared.Services.MultiCompanyService;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A10 — administrator rights seeding on <c>POST /api/v1/auth/login</c>, mirroring
    /// <c>Login.razor:345-349</c>.
    ///
    /// <para><b>The negative test is the important one.</b> <c>SyncRightsForUserAsync</c> writes
    /// <c>CanView</c>, <c>CanCreate</c>, <c>CanEdit</c> and <c>CanDelete</c> all <c>true</c>
    /// (<c>UserRightService.cs:67-70</c>) for every screen the user lacks a row for, so invoking it
    /// for any user other than <c>UserId == 1</c> is a silent privilege escalation — the option B
    /// the owner rejected in KB-109 on 2026-08-24. <c>Non_administrator_login_does_not_invoke…</c>
    /// asserts on the <b>absence of the call</b>, not merely on the absence of rows, so the
    /// rejected option cannot arrive later by accident.</para>
    ///
    /// <para>Controller-level, like every other test in this project: no host, no HTTP wire, no
    /// database beyond the EF InMemory provider (R-43 still open).</para>
    /// </summary>
    public class AuthControllerRightsSeedingTests : IDisposable
    {
        private const int AdministratorUserId = 1;
        private const int OrdinaryUserId = 2;

        private readonly ApplicationDbContext _db;

        public AuthControllerRightsSeedingTests()
        {
            _db = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                    .Options);

            _db.Database.EnsureCreated();
        }

        public void Dispose() => _db.Dispose();

        // ---- criterion 1: the gate, proven by absence of the call ---------------------------

        [Theory]
        [InlineData(OrdinaryUserId)]
        [InlineData(7)]
        [InlineData(150)]
        public async Task Non_administrator_login_does_not_invoke_the_rights_seeder(int userId)
        {
            var seeder = new Mock<IUserRightService>(MockBehavior.Strict);

            var controller = Controller(User(userId), seeder.Object);

            var result = await controller.Login(new AuthController.LoginRequest("acme", "u", "p"));

            // The login itself must be unaffected...
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var body = Assert.IsType<AuthController.LoginResponse>(ok.Value);
            Assert.Equal(userId, body.UserId);

            // ...and the seeder must never have been touched. MockBehavior.Strict means any call
            // at all would already have thrown; this states the intent explicitly.
            seeder.Verify(s => s.SyncRightsForUserAsync(It.IsAny<int>()), Times.Never);
            seeder.VerifyNoOtherCalls();
        }

        // ---- criterion 2: it IS invoked for UserId == 1 -------------------------------------

        [Fact]
        public async Task Administrator_login_invokes_the_rights_seeder_exactly_once_for_user_1()
        {
            var seeder = new Mock<IUserRightService>();

            var controller = Controller(User(AdministratorUserId), seeder.Object);

            var result = await controller.Login(new AuthController.LoginRequest("acme", "admin", "p"));

            Assert.IsType<OkObjectResult>(result.Result);
            seeder.Verify(s => s.SyncRightsForUserAsync(AdministratorUserId), Times.Once);
            seeder.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Criterion 2's second half: the rows the login produces are the rows the <b>real</b>
        /// <c>UserRightService</c> writes — one per screen the administrator has no row for, all
        /// four operation flags <c>true</c>, <c>IsHide</c> false, <c>CreatedBy = "System"</c>
        /// (<c>UserRightService.cs:62-75</c>). The service is not mocked here; it is the live one.
        /// </summary>
        [Fact]
        public async Task Administrator_login_writes_the_rows_SyncRightsForUserAsync_writes()
        {
            // The screens come from the model's own HasData seed (ApplicationDbContext.cs:1150) —
            // adding more here collides with it on the primary key. The test therefore works off
            // whatever the model seeds, which keeps it independent of the exact catalogue size.
            var allScreenIds = await _db.Screens.Select(s => s.Id).OrderBy(id => id).ToListAsync();
            Assert.True(allScreenIds.Count > 2, "the seeded screen catalogue should not be empty");

            var expectedMissing = allScreenIds.Take(2).ToArray();

            // Idempotence: the administrator already holds a row for every other screen, so only
            // the two withheld ones may be created (UserRightService.cs:52-54).
            foreach (var screenId in allScreenIds.Skip(2))
            {
                _db.UserRights.Add(new UserRight
                {
                    UserId = AdministratorUserId,
                    ScreenId = screenId,
                    CanView = true
                });
            }

            await _db.SaveChangesAsync();

            var created = new List<UserRight>();
            var saves = 0;

            var screens = new Mock<IScreenRepository>();
            screens.Setup(r => r.GetQueryable()).Returns(() => _db.Screens);

            var userRights = new Mock<IUserRightsRepository>();
            userRights.Setup(r => r.GetQueryable()).Returns(() => _db.UserRights);
            userRights
                .Setup(r => r.CreateRangeAsync(It.IsAny<IEnumerable<UserRight>>()))
                .Callback<IEnumerable<UserRight>>(rows => created.AddRange(rows))
                .Returns(Task.CompletedTask);

            var users = new Mock<IUserRepository>();
            users.Setup(r => r.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(User(AdministratorUserId));
            users.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                 .ReturnsAsync(true);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.Users).Returns(users.Object);
            unitOfWork.SetupGet(u => u.Screens).Returns(screens.Object);
            unitOfWork.SetupGet(u => u.UserRights).Returns(userRights.Object);
            unitOfWork.Setup(u => u.SaveAsync()).Callback(() => saves++).ReturnsAsync(1);

            // The real service, not a stand-in. CurrentUserService and ICommonService are unused by
            // SyncRightsForUserAsync (UserRightService.cs:32-87) and CurrentUserService's constructor
            // performs a DNS lookup, so they are left null deliberately.
            var seeder = new UserRightService(
                unitOfWork.Object,
                Mock.Of<ILoggingService>(),
                null!,
                null!);

            var controller = Controller(unitOfWork, seeder);

            var result = await controller.Login(new AuthController.LoginRequest("acme", "admin", "p"));

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(1, saves);

            Assert.Equal(expectedMissing, created.Select(r => r.ScreenId).OrderBy(id => id).ToArray());
            Assert.All(created, row =>
            {
                Assert.Equal(AdministratorUserId, row.UserId);
                Assert.True(row.CanView);
                Assert.True(row.CanCreate);
                Assert.True(row.CanEdit);
                Assert.True(row.CanDelete);
                Assert.False(row.IsHide);
                Assert.Equal("System", row.CreatedBy);
            });
        }

        // ---- criterion 3: a seeding failure does not fail the login -------------------------

        /// <summary>
        /// The deliberate choice, stated in <c>AuthController.SeedAdministratorRightsAsync</c>: the
        /// exception is logged and swallowed, and the login returns its normal 200. The credential
        /// check and the account gates have already passed, so the caller is authenticated; failing
        /// here would turn a transient database fault into a total lockout of the one account that
        /// could repair it, and continuing grants nothing that seeding would not have granted.
        /// <c>Login.razor</c> reaches the same outcome. Its seeding call at <c>:345-349</c> sits
        /// inside the page try/catch, but <c>MarkUserAsAuthenticated</c> has already run at
        /// <c>:337</c>, so the catch at <c>:357-362</c> leaves the user signed in and only skips
        /// <c>NavigateTo("/dashboard")</c> (Confirmed). The divergence is narrow: Blazor loses the
        /// navigation, the API returns its normal 200. Blazor is left byte-unchanged.
        /// </summary>
        [Fact]
        public async Task Login_still_returns_200_when_the_rights_seeder_throws()
        {
            var seeder = new Mock<IUserRightService>();
            seeder.Setup(s => s.SyncRightsForUserAsync(AdministratorUserId))
                  .ThrowsAsync(new InvalidOperationException("seeding blew up"));

            var controller = Controller(User(AdministratorUserId), seeder.Object);

            var result = await controller.Login(new AuthController.LoginRequest("acme", "admin", "p"));

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

            var body = Assert.IsType<AuthController.LoginResponse>(ok.Value);
            Assert.Equal(AdministratorUserId, body.UserId);
            Assert.Equal("admin", body.Username);
            Assert.False(string.IsNullOrWhiteSpace(body.Token));

            seeder.Verify(s => s.SyncRightsForUserAsync(AdministratorUserId), Times.Once);
        }

        // ---- harness ------------------------------------------------------------------------

        /// <summary>
        /// A user who passes every gate the login applies before seeding: active, and with no trial
        /// expiry so <c>TrialGate.Evaluate</c> returns null (<c>AccountGates.cs</c>).
        /// </summary>
        private static User User(int userId) => new()
        {
            UserId = userId,
            UserName = userId == AdministratorUserId ? "admin" : $"user{userId}",
            TrialDays = 0,
            ExpiryDate = null,
            IsActive = true
        };

        private static AuthController Controller(User user, IUserRightService userRightService)
        {
            var users = new Mock<IUserRepository>();
            users.Setup(r => r.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(user);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.Users).Returns(users.Object);

            return Controller(unitOfWork, userRightService);
        }

        private static AuthController Controller(Mock<IUnitOfWork> unitOfWork, IUserRightService userRightService)
        {
            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns(new TenantInfo { Id = 1 });

            // A real JwtTokenService: the success path mints a token, so the null stand-in the
            // failure-path tests use is not enough here. The secret is test-only and satisfies
            // StartupConfigurationValidator's 32-byte minimum.
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = "m2-a10-test-only-secret-value-not-a-real-one",
                    ["Jwt:Issuer"] = "v.smart.tests",
                    ["Jwt:Audience"] = "v.smart.tests"
                })
                .Build();

            return new AuthController(
                // M2-A05 — IUnitOfWork, IRefreshTokenService and IUserRightService all resolve
                // from here now, not by direct constructor injection.
                ErrorContractTestContext.ServiceProvider(
                    unitOfWork: unitOfWork.Object,
                    // M2-A04 — every test in this file reaches Login's success path, which now
                    // also issues a refresh token. A stub is enough: nothing here asserts on the
                    // refresh token's value, only on rights-seeding behaviour.
                    refreshTokenService: Mock.Of<IRefreshTokenService>(s =>
                        s.IssueAsync(It.IsAny<int>()) ==
                        Task.FromResult(new IssuedRefreshToken("test-refresh-token", DateTime.UtcNow.AddDays(14)))),
                    userRightService: userRightService),
                new JwtTokenService(configuration),
                tenantProvider.Object,
                configuration,
                NullLogger<AuthController>.Instance)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = ErrorContractTestContext.Create("/api/v1/auth/login", "POST")
                }
            };
        }
    }
}
