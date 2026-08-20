using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using V.SMART.Api.Controllers;
using V.SMART.Api.Middleware;
using V.SMART.Api.Tests.Infrastructure;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using V.SMART.Shared.Services.MultiCompanyService;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A06 — <c>POST /api/auth/login</c>'s two failure paths. Both keep their pre-M2-A06 status
    /// and their message; only the body shape changes.
    /// </summary>
    public class AuthControllerErrorContractTests
    {
        [Fact]
        public async Task Unresolved_tenant_returns_400_problem_json_with_the_message_unchanged()
        {
            var controller = Controller(tenant: null, user: null);

            var result = Assert.IsType<ObjectResult>(
                (await controller.Login(new AuthController.LoginRequest("u", "p"))).Result);

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.Contains(ApiProblems.ContentType, result.ContentTypes);

            var problem = Assert.IsAssignableFrom<ProblemDetails>(result.Value);
            Assert.Equal(
                "Unable to resolve tenant. Check host or wwwroot/config/tenant.json.",
                problem.Title);
            Assert.Equal(ProblemTypes.TenantUnresolved, problem.Type);
            Assert.True(problem.Extensions.ContainsKey("traceId"));

            var raw = ErrorContractTestContext.Serialize(problem).GetRawText();
            Assert.DoesNotContain("Server=", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Password", raw, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Rejected_credentials_return_401_problem_json_that_says_no_more_than_before()
        {
            var controller = Controller(tenant: new TenantInfo { Id = 1 }, user: null);

            var result = Assert.IsType<ObjectResult>(
                (await controller.Login(new AuthController.LoginRequest("u", "p"))).Result);

            Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
            Assert.Contains(ApiProblems.ContentType, result.ContentTypes);

            var problem = Assert.IsAssignableFrom<ProblemDetails>(result.Value);

            // Identical to the pre-M2-A06 { message } string: one answer for every failure, so
            // an unknown user and a wrong password stay indistinguishable.
            Assert.Equal("Invalid username or password.", problem.Title);
            Assert.Null(problem.Detail);
            Assert.Equal(ProblemTypes.Unauthenticated, problem.Type);
        }

        private static AuthController Controller(TenantInfo? tenant, User? user)
        {
            var users = new Mock<IUserRepository>();
            users.Setup(r => r.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(user);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.Users).Returns(users.Object);

            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns(tenant!);

            // M2-A08 added the IConfiguration parameter (the trial gate reads Login.razor:224's
            // "AppEnvironment" key). An empty configuration means HostIsDesktop is false, which is
            // what a web-hosted API is.
            return new AuthController(
                unitOfWork.Object,
                null!,
                tenantProvider.Object,
                new ConfigurationBuilder().Build())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = ErrorContractTestContext.Create("/api/auth/login", "POST")
                }
            };
        }
    }
}
