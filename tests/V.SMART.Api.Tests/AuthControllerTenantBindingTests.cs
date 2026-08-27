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
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using V.SMART.Shared.Services.MultiCompanyService;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A05 — ADR-002 §5's <c>{ tenant, username, password }</c> body, and the ordering it
    /// forces: <c>ITenantProvider.SetTenant(request.Tenant)</c> must run, and
    /// <c>GetCurrentTenant()</c> must resolve, before any tenant-scoped service
    /// (<c>IUnitOfWork</c>, <c>IRefreshTokenService</c>, <c>IUserRightService</c>) is resolved
    /// from <c>_serviceProvider</c> — otherwise a cross-origin SPA's login can never bind its
    /// own tenant database before <c>UserRepository</c> needs one (the "genuine
    /// chicken-and-egg" this task's own KB describes).
    /// </summary>
    public class AuthControllerTenantBindingTests
    {
        [Fact]
        public async Task Login_binds_the_request_s_tenant_before_doing_anything_else()
        {
            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns(new TenantInfo { Id = 9 });

            var controller = Controller(tenantProvider.Object, ServiceProviderWithEverything());

            await controller.Login(new AuthController.LoginRequest("acme-tenant", "u", "p"));

            tenantProvider.Verify(p => p.SetTenant("acme-tenant"), Times.Once);
        }

        [Fact]
        public async Task Refresh_binds_the_request_s_tenant_before_doing_anything_else()
        {
            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns(new TenantInfo { Id = 9 });

            var controller = Controller(tenantProvider.Object, ServiceProviderWithEverything());

            await controller.Refresh(new AuthController.RefreshRequest("acme-tenant", "some-token"));

            tenantProvider.Verify(p => p.SetTenant("acme-tenant"), Times.Once);
        }

        [Fact]
        public async Task Logout_binds_the_request_s_tenant_before_doing_anything_else()
        {
            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns(new TenantInfo { Id = 9 });

            var controller = Controller(tenantProvider.Object, ServiceProviderWithEverything());

            await controller.Logout(new AuthController.LogoutRequest("acme-tenant", "some-token"));

            tenantProvider.Verify(p => p.SetTenant("acme-tenant"), Times.Once);
        }

        /// <summary>
        /// The ordering proof, stated as a negative: an unresolved tenant must refuse the
        /// request without ever touching a service that only a bound tenant makes safe to
        /// resolve. A completely unconfigured <see cref="IServiceProvider"/> mock — nothing
        /// wired for any of the three tenant-scoped services — makes this observable: if
        /// <c>Login</c> resolved <c>IUnitOfWork</c> before checking the tenant, this test would
        /// fail with an unhandled exception from <c>GetRequiredService</c>'s own null check,
        /// not with the clean 400 the assertions below expect.
        /// </summary>
        [Fact]
        public async Task Login_never_touches_the_service_provider_when_the_tenant_is_unresolved()
        {
            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns((TenantInfo?)null);

            var controller = Controller(tenantProvider.Object, Mock.Of<IServiceProvider>());

            var result = await controller.Login(new AuthController.LoginRequest("unknown", "u", "p"));

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
            var problem = Assert.IsAssignableFrom<ProblemDetails>(obj.Value);
            Assert.Equal(ProblemTypes.TenantUnresolved, problem.Type);
        }

        [Fact]
        public async Task Refresh_never_touches_the_service_provider_when_the_tenant_is_unresolved()
        {
            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns((TenantInfo?)null);

            var controller = Controller(tenantProvider.Object, Mock.Of<IServiceProvider>());

            var result = await controller.Refresh(new AuthController.RefreshRequest("unknown", "some-token"));

            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
            var problem = Assert.IsAssignableFrom<ProblemDetails>(obj.Value);
            Assert.Equal(ProblemTypes.TenantUnresolved, problem.Type);
        }

        [Fact]
        public async Task Logout_never_touches_the_service_provider_when_the_tenant_is_unresolved()
        {
            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns((TenantInfo?)null);

            var controller = Controller(tenantProvider.Object, Mock.Of<IServiceProvider>());

            var result = await controller.Logout(new AuthController.LogoutRequest("unknown", "some-token"));

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
            var problem = Assert.IsAssignableFrom<ProblemDetails>(obj.Value);
            Assert.Equal(ProblemTypes.TenantUnresolved, problem.Type);
        }

        // ---- harness ---------------------------------------------------------------------------

        private static IServiceProvider ServiceProviderWithEverything()
        {
            var users = new Mock<IUserRepository>();
            users.Setup(r => r.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((User?)null);
            users.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.Users).Returns(users.Object);

            var refreshTokenService = new Mock<IRefreshTokenService>();
            refreshTokenService.Setup(s => s.RotateAsync(It.IsAny<string>()))
                .ReturnsAsync(RefreshRotationResult.Failure(RefreshOutcome.NotFound));
            refreshTokenService.Setup(s => s.RevokeAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            return ErrorContractTestContext.ServiceProvider(
                unitOfWork: unitOfWork.Object,
                refreshTokenService: refreshTokenService.Object,
                userRightService: Mock.Of<IUserRightService>());
        }

        private static AuthController Controller(ITenantProvider tenantProvider, IServiceProvider serviceProvider)
        {
            return new AuthController(
                serviceProvider,
                null!,
                tenantProvider,
                new ConfigurationBuilder().Build(),
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
