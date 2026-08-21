using V.SMART.Api.Middleware;
using V.SMART.Shared.Data.Master.Admin;

namespace V.SMART.Api.Auth
{
    /// <summary>
    /// One account-level refusal: the RFC 7807 <c>type</c> that makes it distinguishable, and the
    /// <c>title</c>, which is the Blazor message <b>verbatim</b> (ADR-002 §4).
    /// </summary>
    /// <remarks>
    /// <b>The messages are product UX and are copied byte for byte from <c>Login.razor</c>.</b> They
    /// are not reworded, prefixed or genericised, and none of them collapses into the generic 401 the
    /// login already returns for bad credentials: a user locked out by an expired trial and a user
    /// who mistyped a password need different next actions, and a support desk that cannot tell them
    /// apart resets passwords for expired trials all day.
    /// </remarks>
    public sealed record AccountGateRefusal(string ProblemType, string Title);

    /// <summary>
    /// The trial / expiry gate, ported from
    /// <c>V.SMART/V.SMART.Shared/Pages/Master_Module_pages/Identity_Pages/Login.razor:271-275</c>
    /// (M2-A08, Q-06, BR-AUTH-001).
    ///
    /// <para><b>This gate is enforced today, in Razor only.</b> The API login performed no trial
    /// check at all before M2-A08, so every SPA login bypassed a gate the Blazor login honours. The
    /// port is therefore behaviour-preserving for the migration, not a new rule
    /// (KB-108 §4, decision P3).</para>
    ///
    /// <para><b>All three carve-outs are ported unchanged, because each one is currently the reason
    /// some population can log in.</b> "Cleaning up" any of them locks real users out of a live ERP
    /// on the day it ships.</para>
    /// </summary>
    public static class TrialGate
    {
        /// <summary>
        /// <c>Login.razor:273</c>, byte for byte.
        /// </summary>
        public const string ExpiredMessage = "Your trial period has expired. Please contact Administrator.";

        /// <summary>
        /// The configuration key <c>Login.razor:224</c> reads to decide whether the host is the
        /// desktop build: <c>private bool IsDesktop =&gt; Configuration["AppEnvironment"] == "Desktop";</c>.
        /// Note what this is <b>not</b>: it is a property of the <i>host process</i>, not of the user,
        /// and it is unrelated to <c>User.IsDesktop</c> (<c>User.cs:88</c>), which the device gate reads.
        /// </summary>
        public const string HostEnvironmentKey = "AppEnvironment";

        /// <summary>The value of <see cref="HostEnvironmentKey"/> that means "desktop host".</summary>
        public const string DesktopHostValue = "Desktop";

        /// <summary>
        /// Returns the refusal, or <c>null</c> to let the login proceed.
        ///
        /// <para>The condition is <c>Login.razor:271</c> transcribed:
        /// <c>!IsDesktop &amp;&amp; user.UserId&gt;1 &amp;&amp; user != null &amp;&amp; user.TrialDays &gt; 0
        /// &amp;&amp; user.ExpiryDate.HasValue &amp;&amp; DateTime.Today &gt; user.ExpiryDate.Value.Date</c>.
        /// The <c>user != null</c> term is the one thing dropped: it is dead there — the page already
        /// returned for a null user at <c>Login.razor:263-268</c> — and it is tested <i>after</i>
        /// <c>user.UserId</c> has been dereferenced, so it can never be false. That defect is recorded
        /// in KB-060, not fixed in the Blazor file, which M2-A08 must not touch.</para>
        ///
        /// <para><c>DateTime.Today</c> and <c>.Date</c> are both kept: the comparison is date-only, so a
        /// user whose <c>ExpiryDate</c> is today is <b>not</b> expired.</para>
        /// </summary>
        /// <param name="user">The authenticated user. <c>LoginAsync</c> has already returned non-null.</param>
        /// <param name="hostIsDesktop">
        /// <c>Login.razor:224</c>'s <c>IsDesktop</c>, resolved from configuration by the caller.
        /// <b>Carve-out (a), <c>Login.razor:271</c>:</b> the desktop host is exempt entirely.
        /// Whether that exemption is deliberate licensing policy or an oversight is not determinable
        /// from source — KB-108 decision P3 carries it as an open question for the repository owner.
        /// </param>
        /// <param name="today">
        /// Injectable for tests only. Production passes <c>DateTime.Today</c>, which is what
        /// <c>Login.razor:271</c> uses — server-local, not UTC.
        /// </param>
        public static AccountGateRefusal? Evaluate(User user, bool hostIsDesktop, DateTime today)
        {
            ArgumentNullException.ThrowIfNull(user);

            // Carve-out (a) — Login.razor:271, "!IsDesktop".
            if (hostIsDesktop)
            {
                return null;
            }

            // Carve-out (b) — Login.razor:271, "user.UserId>1". User 1 is never trial-gated.
            if (user.UserId <= 1)
            {
                return null;
            }

            // Carve-out (c) — Login.razor:271, "user.TrialDays > 0". A user with TrialDays == 0 is
            // exempt EVEN IF ExpiryDate is in the past. This is not an accident of ordering: the
            // write path only derives an ExpiryDate when TrialDays > 0 (RegisterUpsert.razor:1062-1068),
            // so a zero-trial user with a stale ExpiryDate is a non-trial account and stays in.
            if (user.TrialDays <= 0)
            {
                return null;
            }

            // Login.razor:271 — "user.ExpiryDate.HasValue". A null expiry is not an expired one.
            if (!user.ExpiryDate.HasValue)
            {
                return null;
            }

            // Login.razor:271 — "DateTime.Today > user.ExpiryDate.Value.Date". Date-only, strictly
            // greater: expiry day itself is still allowed.
            if (today.Date > user.ExpiryDate.Value.Date)
            {
                return new AccountGateRefusal(ProblemTypes.TrialExpired, ExpiredMessage);
            }

            return null;
        }
    }

    /// <summary>
    /// The device identity a client asserts about itself. Both members come from the browser in the
    /// Blazor original — <c>deviceHelper.getDeviceId</c> and <c>deviceHelper.isMobile</c>
    /// (<c>Login.razor:279-280</c>) — and the server derives neither.
    /// </summary>
    /// <remarks>
    /// <b>This is a convenience lock, not an authentication factor.</b> Anything a client asserts, a
    /// client can assert falsely. Documented as such rather than presented as security it is not
    /// (KB-108 §4, claim 30).
    /// </remarks>
    public sealed record DeviceIdentity(string? DeviceId, bool IsMobile);

    /// <summary>
    /// The device-binding gate, ported from <c>Login.razor:277-322</c> (M2-A08, Q-07).
    ///
    /// <para><b>NOT WIRED INTO THE API LOGIN PATH, deliberately (KB-108 decision P4 — deferred,
    /// unanswered).</b> The evaluator is implemented and tested here so that the behaviour is
    /// pinned and the eventual owner inherits a specification rather than a Razor page, but
    /// <c>AuthController.Login</c> does not call it. Two reasons, both recorded:</para>
    /// <list type="number">
    /// <item>Enforcing it needs a client-supplied device identity, and designing a spoofable input
    /// and presenting it as a lock is a product decision the task explicitly refuses to take for the
    /// owner ("Escalate — no safe default").</item>
    /// <item>The trust-on-first-use half cannot be ported at all as it stands:
    /// <c>UserService.UpdateUserDeviceAsync</c> (<c>UserService.cs:713-757</c>) obtains the device
    /// identity by calling <c>IJSRuntime</c> from inside a business service (<c>:722-725</c>), so it
    /// cannot run in an API request. A first login through the API would refuse where Blazor would
    /// claim the binding — a behaviour change, not a port.</item>
    /// </list>
    /// </summary>
    public static class DeviceGate
    {
        /// <summary><c>Login.razor:286</c>, byte for byte.</summary>
        public const string MobileNotAllowedMessage = "Mobile login is not allowed.";

        /// <summary><c>Login.razor:306</c>, byte for byte.</summary>
        public const string DesktopNotAllowedMessage = "Desktop login is not allowed.";

        /// <summary><c>Login.razor:298</c>, byte for byte.</summary>
        public const string OtherMobileDeviceMessage = "This account is already registered on another mobile device.";

        /// <summary><c>Login.razor:318</c>, byte for byte.</summary>
        public const string OtherDesktopMessage = "This account is already registered on another desktop.";

        /// <summary>
        /// Returns the refusal, or <c>null</c> to allow — with <paramref name="claimsBinding"/> set
        /// when this is a first login that would take the binding.
        ///
        /// <para><b>The gate is opt-in per user</b> — <c>Login.razor:277</c>,
        /// <c>user.UserId &gt; 1 &amp;&amp; (user.IsMobile || user.IsDesktop)</c>. A user with both
        /// flags false is completely unbound and is never checked; user 1 is exempt. Preserved
        /// exactly (claim 27).</para>
        /// </summary>
        /// <param name="claimsBinding">
        /// True when the stored device id is blank and the Blazor page would call
        /// <c>UpdateUserDeviceAsync</c> to claim it (<c>Login.razor:291-294</c>, <c>:311-314</c>).
        /// There is no admin approval step: the first device to log in wins. The caller — once there
        /// is one — must decide how to record that without <c>IJSRuntime</c>.
        /// </param>
        public static AccountGateRefusal? Evaluate(User user, DeviceIdentity device, out bool claimsBinding)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(device);

            claimsBinding = false;

            // Login.razor:277 — the opt-in switch, both halves.
            if (user.UserId <= 1 || !(user.IsMobile || user.IsDesktop))
            {
                return null;
            }

            if (device.IsMobile)
            {
                // Login.razor:284-288.
                if (!user.IsMobile)
                {
                    return new AccountGateRefusal(ProblemTypes.PlatformNotAllowed, MobileNotAllowedMessage);
                }

                // Login.razor:291-294 — trust on first use.
                if (string.IsNullOrWhiteSpace(user.MobileDeviceId))
                {
                    claimsBinding = true;
                    return null;
                }

                // Login.razor:296-300. Ordinal, as C#'s string != is.
                return string.Equals(user.MobileDeviceId, device.DeviceId, StringComparison.Ordinal)
                    ? null
                    : new AccountGateRefusal(ProblemTypes.DeviceNotRecognised, OtherMobileDeviceMessage);
            }

            // Login.razor:304-308.
            if (!user.IsDesktop)
            {
                return new AccountGateRefusal(ProblemTypes.PlatformNotAllowed, DesktopNotAllowedMessage);
            }

            // Login.razor:311-314 — trust on first use.
            if (string.IsNullOrWhiteSpace(user.DesktopDeviceId))
            {
                claimsBinding = true;
                return null;
            }

            // Login.razor:316-320.
            return string.Equals(user.DesktopDeviceId, device.DeviceId, StringComparison.Ordinal)
                ? null
                : new AccountGateRefusal(ProblemTypes.DeviceNotRecognised, OtherDesktopMessage);
        }
    }
}
