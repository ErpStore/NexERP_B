using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using V.SMART.Api.Auth;
using V.SMART.Api.Controllers;
using V.SMART.Api.Middleware;
using V.SMART.Api.Tests.Infrastructure;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using V.SMART.Shared.Services.MultiCompanyService;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A04 — <c>POST /api/v1/auth/refresh</c> and <c>POST /api/v1/auth/logout</c>. Controller
    /// level, like every other test in this project: <c>IRefreshTokenService</c> is mocked (the
    /// store's own logic is <c>RefreshTokenServiceTests</c>'s job, against a real EF pipeline);
    /// this file is only about how the controller maps outcomes to responses.
    /// </summary>
    public class AuthControllerRefreshLogoutTests
    {
        private static User ActiveUser(int userId = 7) => new()
        {
            UserId = userId,
            UserName = "alice",
            UserPassword = "irrelevant-here",
            IsActive = true,
            Role = UserRole.Administrator
        };

        // ---- refresh: success -----------------------------------------------------------------

        [Fact]
        public async Task Valid_refresh_token_returns_200_with_a_new_pair()
        {
            var issued = new IssuedRefreshToken("new-raw-refresh-token", DateTime.UtcNow.AddDays(14));
            var refreshService = new Mock<IRefreshTokenService>();
            refreshService.Setup(s => s.RotateAsync("presented-token"))
                .ReturnsAsync(new RefreshRotationResult(RefreshOutcome.Success, 7, issued));

            var users = new Mock<IUserRepository>();
            users.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(ActiveUser());

            var controller = Controller(refreshService.Object, users.Object, tenant: new TenantInfo { Id = 5 });

            var result = await controller.Refresh(new AuthController.RefreshRequest("presented-token"));

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var body = Assert.IsType<AuthController.RefreshResponse>(ok.Value);
            Assert.Equal("new-raw-refresh-token", body.RefreshToken);
            Assert.False(string.IsNullOrWhiteSpace(body.Token));
            Assert.NotEqual(default, body.TokenExpiresAtUtc);
        }

        [Fact]
        public async Task Refresh_re_resolves_tenant_and_reports_400_if_unresolved()
        {
            var issued = new IssuedRefreshToken("new-raw-refresh-token", DateTime.UtcNow.AddDays(14));
            var refreshService = new Mock<IRefreshTokenService>();
            refreshService.Setup(s => s.RotateAsync(It.IsAny<string>()))
                .ReturnsAsync(new RefreshRotationResult(RefreshOutcome.Success, 7, issued));

            var users = new Mock<IUserRepository>();
            users.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync(ActiveUser());

            var controller = Controller(refreshService.Object, users.Object, tenant: null);

            var result = await controller.Refresh(new AuthController.RefreshRequest("presented-token"));

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
            var problem = Assert.IsAssignableFrom<ProblemDetails>(obj.Value);
            Assert.Equal(ProblemTypes.TenantUnresolved, problem.Type);
        }

        // ---- refresh: failure opacity (Target Result 5 / Testing item 10) --------------------

        [Theory]
        [InlineData(RefreshOutcome.NotFound)]
        [InlineData(RefreshOutcome.Revoked)]
        [InlineData(RefreshOutcome.Expired)]
        [InlineData(RefreshOutcome.UserInactive)]
        public async Task Every_rotation_failure_reason_produces_the_identical_401_body(RefreshOutcome outcome)
        {
            var refreshService = new Mock<IRefreshTokenService>();
            refreshService.Setup(s => s.RotateAsync(It.IsAny<string>()))
                .ReturnsAsync(RefreshRotationResult.Failure(outcome));

            var controller = Controller(refreshService.Object, Mock.Of<IUserRepository>(), tenant: new TenantInfo { Id = 1 });

            var result = await controller.Refresh(new AuthController.RefreshRequest("whatever"));

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status401Unauthorized, obj.StatusCode);
            var problem = Assert.IsAssignableFrom<ProblemDetails>(obj.Value);

            // The one body every reason maps to — an unknown token and an expired one must read
            // identically on the wire (Testing item 10).
            Assert.Equal("Invalid or expired refresh token.", problem.Title);
            Assert.Null(problem.Detail);
            Assert.Equal(ProblemTypes.Unauthenticated, problem.Type);
        }

        [Fact]
        public async Task A_user_who_vanishes_between_rotation_and_lookup_is_also_refused()
        {
            // RotateAsync itself already re-checks IsActive, but the controller does an
            // independent second read (defence in depth) — this proves that path also refuses
            // rather than trusting the first result blindly.
            var issued = new IssuedRefreshToken("new-raw-refresh-token", DateTime.UtcNow.AddDays(14));
            var refreshService = new Mock<IRefreshTokenService>();
            refreshService.Setup(s => s.RotateAsync(It.IsAny<string>()))
                .ReturnsAsync(new RefreshRotationResult(RefreshOutcome.Success, 999, issued));

            var users = new Mock<IUserRepository>();
            users.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            var controller = Controller(refreshService.Object, users.Object, tenant: new TenantInfo { Id = 1 });

            var result = await controller.Refresh(new AuthController.RefreshRequest("presented-token"));

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status401Unauthorized, obj.StatusCode);
        }

        // ---- logout: idempotent, opaque, never distinguishes ---------------------------------

        [Fact]
        public async Task Logout_revokes_the_presented_token_and_returns_204()
        {
            var refreshService = new Mock<IRefreshTokenService>();

            var controller = Controller(refreshService.Object, Mock.Of<IUserRepository>(), tenant: new TenantInfo { Id = 1 });

            var result = await controller.Logout(new AuthController.LogoutRequest("some-token"));

            Assert.IsType<NoContentResult>(result);
            refreshService.Verify(s => s.RevokeAsync("some-token"), Times.Once);
        }

        [Fact]
        public async Task Logout_of_an_unknown_token_still_returns_204_never_leaking_validity()
        {
            var refreshService = new Mock<IRefreshTokenService>();
            refreshService.Setup(s => s.RevokeAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            var controller = Controller(refreshService.Object, Mock.Of<IUserRepository>(), tenant: new TenantInfo { Id = 1 });

            var result = await controller.Logout(new AuthController.LogoutRequest("token-never-issued"));

            Assert.IsType<NoContentResult>(result);
        }

        // ---- harness ---------------------------------------------------------------------------

        private static AuthController Controller(
            IRefreshTokenService refreshTokenService,
            IUserRepository users,
            TenantInfo? tenant)
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.Users).Returns(users);

            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns(tenant!);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = "m2-a04-test-only-secret-value-not-a-real-one",
                    ["Jwt:Issuer"] = "v.smart.tests",
                    ["Jwt:Audience"] = "v.smart.tests"
                })
                .Build();

            return new AuthController(
                unitOfWork.Object,
                new JwtTokenService(configuration),
                refreshTokenService,
                tenantProvider.Object,
                configuration,
                Mock.Of<IUserRightService>(),
                NullLogger<AuthController>.Instance)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = ErrorContractTestContext.Create("/api/v1/auth/refresh", "POST")
                }
            };
        }
    }
}
