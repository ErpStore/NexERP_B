using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using V.SMART.Api.Auth;
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
    /// M2-A08 — the account gates that live in <c>Login.razor</c> today (Q-06, Q-07).
    ///
    /// <para>Every carve-out below is currently the reason some population can log in, so each one
    /// gets a test of its own. A carve-out "cleaned up" during the port is a production incident,
    /// and a test is the only thing that makes that statement enforceable.</para>
    /// </summary>
    public class AccountGateTests
    {
        private static readonly DateTime Today = new(2026, 8, 20);

        // ---- the trial gate: refusal ------------------------------------------------------

        [Fact]
        public void An_expired_trial_is_refused_with_the_Login_razor_message_byte_for_byte()
        {
            var refusal = TrialGate.Evaluate(
                User(userId: 7, trialDays: 30, expiry: Today.AddDays(-1)),
                hostIsDesktop: false,
                Today);

            Assert.NotNull(refusal);

            // Login.razor:273. Compared as a literal here on purpose: if TrialGate.ExpiredMessage
            // is ever "tidied", this test fails rather than the constant quietly drifting.
            Assert.Equal("Your trial period has expired. Please contact Administrator.", refusal!.Title);
            Assert.Equal(ProblemTypes.TrialExpired, refusal.ProblemType);
        }

        [Fact]
        public void The_expiry_day_itself_is_still_allowed()
        {
            // Login.razor:271 — "DateTime.Today > user.ExpiryDate.Value.Date", strictly greater and
            // date-only. A user expiring today logs in today.
            Assert.Null(TrialGate.Evaluate(
                User(userId: 7, trialDays: 30, expiry: Today),
                hostIsDesktop: false,
                Today));

            // ...including when the stored value carries a time of day, because both sides are .Date.
            Assert.Null(TrialGate.Evaluate(
                User(userId: 7, trialDays: 30, expiry: Today.AddHours(1)),
                hostIsDesktop: false,
                Today));
        }

        // ---- the trial gate: the three carve-outs, each proven ----------------------------

        [Fact]
        public void Carve_out_a_the_desktop_host_is_exempt_entirely()
        {
            // Login.razor:271, "!IsDesktop", where IsDesktop is Configuration["AppEnvironment"] ==
            // "Desktop" (Login.razor:224) — a property of the host, not of the user. KB-108 P3 flags
            // this one for the owner: it may be deliberate licensing policy or an oversight, and the
            // source cannot say which. It is preserved until someone says otherwise.
            Assert.Null(TrialGate.Evaluate(
                User(userId: 7, trialDays: 30, expiry: Today.AddDays(-1)),
                hostIsDesktop: true,
                Today));
        }

        [Fact]
        public void Carve_out_b_user_1_logs_in_with_an_expired_trial()
        {
            // Login.razor:271, "user.UserId>1".
            Assert.Null(TrialGate.Evaluate(
                User(userId: 1, trialDays: 30, expiry: Today.AddDays(-365)),
                hostIsDesktop: false,
                Today));
        }

        [Fact]
        public void Carve_out_c_TrialDays_zero_logs_in_even_with_a_past_ExpiryDate()
        {
            // Login.razor:271, "user.TrialDays > 0". The write path only derives an ExpiryDate when
            // TrialDays > 0 (RegisterUpsert.razor:1062-1068), so a zero-trial account with a stale
            // ExpiryDate is a non-trial account and stays in.
            Assert.Null(TrialGate.Evaluate(
                User(userId: 7, trialDays: 0, expiry: Today.AddDays(-1)),
                hostIsDesktop: false,
                Today));
        }

        [Fact]
        public void A_null_ExpiryDate_is_not_an_expired_one()
        {
            // Login.razor:271, "user.ExpiryDate.HasValue".
            Assert.Null(TrialGate.Evaluate(
                User(userId: 7, trialDays: 30, expiry: null),
                hostIsDesktop: false,
                Today));
        }

        // ---- the trial gate on the wire ---------------------------------------------------

        [Fact]
        public async Task Login_refuses_an_expired_trial_with_a_403_that_is_not_the_401_bad_credentials_get()
        {
            var controller = Controller(User(userId: 7, trialDays: 30, expiry: DateTime.Today.AddDays(-1)));

            var result = Assert.IsType<ObjectResult>(
                (await controller.Login(new AuthController.LoginRequest("u", "p"))).Result);

            // Distinguishable, and deliberately NOT a 401: the password was correct, so re-prompting
            // for it — which is what a 401 tells a client to do — cannot resolve this.
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
            Assert.Contains(ApiProblems.ContentType, result.ContentTypes);

            var problem = Assert.IsAssignableFrom<ProblemDetails>(result.Value);
            Assert.Equal(TrialGate.ExpiredMessage, problem.Title);
            Assert.Equal(ProblemTypes.TrialExpired, problem.Type);
            Assert.NotEqual(ProblemTypes.Unauthenticated, problem.Type);
            Assert.True(problem.Extensions.ContainsKey("traceId"));

            // R-01 and general hygiene: no connection string, no hash, no token, no device id.
            var raw = ErrorContractTestContext.Serialize(problem).GetRawText();
            Assert.DoesNotContain("Server=", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Password", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("QrToken", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("DeviceId", raw, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Login_lets_a_live_trial_through_to_the_token()
        {
            var controller = Controller(User(userId: 7, trialDays: 30, expiry: DateTime.Today.AddDays(5)));

            // JwtTokenService is a null stand-in here (as it is in the M2-A06 tests), so "the gate
            // let this login through" is observed as execution reaching token issuance and throwing
            // there. The assertion that matters is the negative one: no ObjectResult, so no refusal.
            // A 200 test needs a host, which this project deliberately has no harness for (see
            // PagedContractTests' note on Microsoft.AspNetCore.Mvc.Testing).
            await Assert.ThrowsAsync<NullReferenceException>(
                () => controller.Login(new AuthController.LoginRequest("u", "p")));
        }

        [Fact]
        public async Task Bad_credentials_still_get_the_401_they_got_before_this_task()
        {
            var controller = Controller(user: null);

            var result = Assert.IsType<ObjectResult>(
                (await controller.Login(new AuthController.LoginRequest("u", "p"))).Result);

            Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
            Assert.Equal(
                ProblemTypes.Unauthenticated,
                Assert.IsAssignableFrom<ProblemDetails>(result.Value).Type);
        }

        // ---- the device gate: ported, tested, NOT wired (KB-108 P4, deferred) --------------

        [Fact]
        public void An_unbound_user_is_never_device_gated()
        {
            // Login.razor:277 — "(user.IsMobile || user.IsDesktop)". Both false means completely
            // unbound, which is the majority case and must stay untouched.
            var user = User(userId: 7);
            user.IsMobile = false;
            user.IsDesktop = false;

            Assert.Null(DeviceGate.Evaluate(user, new DeviceIdentity("anything", IsMobile: true), out var claimed));
            Assert.False(claimed);
        }

        [Fact]
        public void User_1_is_never_device_gated_either()
        {
            // Login.razor:277 — "user.UserId > 1".
            var user = User(userId: 1);
            user.IsDesktop = true;
            user.DesktopDeviceId = "the-one-true-desktop";

            Assert.Null(DeviceGate.Evaluate(user, new DeviceIdentity("some-other-desktop", IsMobile: false), out _));
        }

        [Fact]
        public void A_mismatched_mobile_device_is_refused_verbatim()
        {
            var user = User(userId: 7);
            user.IsMobile = true;
            user.MobileDeviceId = "phone-a";

            var refusal = DeviceGate.Evaluate(user, new DeviceIdentity("phone-b", IsMobile: true), out _);

            // Login.razor:298.
            Assert.Equal("This account is already registered on another mobile device.", refusal!.Title);
            Assert.Equal(ProblemTypes.DeviceNotRecognised, refusal.ProblemType);
        }

        [Fact]
        public void A_mismatched_desktop_is_refused_verbatim()
        {
            var user = User(userId: 7);
            user.IsDesktop = true;
            user.DesktopDeviceId = "box-a";

            var refusal = DeviceGate.Evaluate(user, new DeviceIdentity("box-b", IsMobile: false), out _);

            // Login.razor:318.
            Assert.Equal("This account is already registered on another desktop.", refusal!.Title);
            Assert.Equal(ProblemTypes.DeviceNotRecognised, refusal.ProblemType);
        }

        [Fact]
        public void A_wrong_platform_is_refused_verbatim_and_distinguishably()
        {
            var mobileOnly = User(userId: 7);
            mobileOnly.IsMobile = true;

            var desktopOnly = User(userId: 7);
            desktopOnly.IsDesktop = true;

            // Login.razor:306 — a desktop trying to use a mobile-only account.
            var desktopRefusal = DeviceGate.Evaluate(mobileOnly, new DeviceIdentity("box", IsMobile: false), out _);
            Assert.Equal("Desktop login is not allowed.", desktopRefusal!.Title);

            // Login.razor:286 — a phone trying to use a desktop-only account.
            var mobileRefusal = DeviceGate.Evaluate(desktopOnly, new DeviceIdentity("phone", IsMobile: true), out _);
            Assert.Equal("Mobile login is not allowed.", mobileRefusal!.Title);

            // A wrong platform and an unknown device are different problems with different fixes.
            Assert.Equal(ProblemTypes.PlatformNotAllowed, desktopRefusal.ProblemType);
            Assert.Equal(ProblemTypes.PlatformNotAllowed, mobileRefusal.ProblemType);
            Assert.NotEqual(ProblemTypes.DeviceNotRecognised, mobileRefusal.ProblemType);
        }

        [Fact]
        public void A_blank_stored_device_id_is_a_first_login_that_claims_the_binding()
        {
            // Login.razor:291-294 and :311-314 — trust on first use, no admin approval step.
            var user = User(userId: 7);
            user.IsMobile = true;
            user.MobileDeviceId = null;

            Assert.Null(DeviceGate.Evaluate(user, new DeviceIdentity("phone-a", IsMobile: true), out var claimed));
            Assert.True(claimed);
        }

        [Fact]
        public void Each_gate_refusal_carries_its_own_problem_type()
        {
            // Test 13 — none of them collapses into another, and none into the generic 401.
            var types = new[]
            {
                ProblemTypes.TrialExpired,
                ProblemTypes.DeviceNotRecognised,
                ProblemTypes.PlatformNotAllowed
            };

            Assert.Equal(types.Length, types.Distinct(StringComparer.Ordinal).Count());
            Assert.DoesNotContain(ProblemTypes.Unauthenticated, types);
            Assert.All(types, t => Assert.StartsWith(ProblemTypes.Base, t, StringComparison.Ordinal));
        }

        // ---- fixtures ----------------------------------------------------------------------

        private static User User(int userId, int trialDays = 0, DateTime? expiry = null)
            => new()
            {
                UserId = userId,
                UserName = "u",
                TrialDays = trialDays,
                ExpiryDate = expiry
            };

        private static AuthController Controller(User? user)
        {
            var users = new Mock<IUserRepository>();
            users.Setup(r => r.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(user);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.Users).Returns(users.Object);

            var tenantProvider = new Mock<ITenantProvider>();
            tenantProvider.Setup(p => p.GetCurrentTenant()).Returns(new TenantInfo { Id = 1 });

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
