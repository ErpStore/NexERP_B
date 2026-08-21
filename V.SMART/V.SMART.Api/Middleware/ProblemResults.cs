using Microsoft.AspNetCore.Mvc;

namespace V.SMART.Api.Middleware
{
    /// <summary>
    /// The controller-side half of the error contract.
    ///
    /// <para><b>M2-A06 decision — how a 409 is produced. Every future controller copies this.</b></para>
    ///
    /// The business services do <b>not</b> throw on a business-rule refusal; they <b>return</b> it.
    /// The pervasive convention is the two-element delete guard
    /// <c>(bool CanDelete, string Message)</c> — 79 methods across 61 service files in
    /// <c>V.SMART.Shared/BusinessLayer</c> — with three near-identical variants
    /// (<c>(bool CanItemCancel, string Message)</c>, <c>(bool IsValid, string Message)</c> on
    /// <c>ValidateDeleteAsync</c>, and one nested <c>ServiceResult</c> in <c>StoreService</c>).
    /// <c>CurrencyService</c>'s three-element create/update tuple
    /// <c>(bool Success, string Message, T? Entity)</c> is currently unique to that one service.
    ///
    /// <para>
    /// A returned value is invisible to middleware, so the middleware <b>cannot</b> map it. The
    /// controller must, and it does so here — via these extension methods, deliberately <b>not</b>
    /// by introducing a domain exception type into <c>V.SMART.Shared</c>. Two reasons: that
    /// library is live under Blazor Server and this task must not change it, and 1,107
    /// <c>InvalidOperationException</c> throw sites in <c>BusinessLayer</c> already mix business
    /// refusals with rethrown infrastructure faults using the identical type, so exceptions are
    /// not a trustworthy refusal channel here.
    /// </para>
    ///
    /// <para>
    /// The API therefore treats the <i>boolean</i> as the signal and passes the message through
    /// untouched: it never parses, reclassifies or rewords the string. That is what keeps
    /// BR-SO-001 honest — but it also means a refusal tuple that actually carries not-found
    /// semantics (<c>CurrencyService.cs:197</c> "Currency not found or already removed.") or a
    /// caught infrastructure fault (<c>CurrencyService.cs:208-211</c>) is reported as 409.
    /// Recorded as an open question rather than guessed at; see
    /// <c>docs/kb/open-questions.md</c> (Q-34).
    /// </para>
    /// </summary>
    public static class ProblemResults
    {
        /// <summary><b>409</b> with the service's message verbatim in <c>title</c> (BR-SO-001).</summary>
        public static ObjectResult BusinessRuleProblem(this ControllerBase controller, string message)
            => ApiProblems.ToResult(ApiProblems.BusinessRuleRefusal(controller.HttpContext, message));

        /// <summary><b>404</b> in the canonical shape.</summary>
        public static ObjectResult NotFoundProblem(this ControllerBase controller, string message)
            => ApiProblems.ToResult(ApiProblems.NotFound(controller.HttpContext, message));

        /// <summary><b>401</b>, no more informative than the pre-M2-A06 <c>{ message }</c> body.</summary>
        public static ObjectResult UnauthenticatedProblem(this ControllerBase controller, string title)
            => ApiProblems.ToResult(ApiProblems.Unauthenticated(controller.HttpContext, title));

        /// <summary><b>403</b> — the single producer of the KB-105 §7.1 body.</summary>
        public static ObjectResult ScreenRightDeniedProblem(this ControllerBase controller, string screen, string right)
            => ApiProblems.ToResult(ApiProblems.ScreenRightDenied(controller.HttpContext, screen, right));

        /// <summary>Tenant could not be resolved. Never carries a connection string (R-01).</summary>
        public static ObjectResult TenantUnresolvedProblem(this ControllerBase controller, int status, string title)
            => ApiProblems.ToResult(ApiProblems.TenantUnresolved(controller.HttpContext, status, title));

        /// <summary><b>413</b> — the upload exceeds the configured maximum (M2-B06).</summary>
        public static ObjectResult PayloadTooLargeProblem(this ControllerBase controller, string title)
            => ApiProblems.ToResult(ApiProblems.PayloadTooLarge(controller.HttpContext, title));

        /// <summary><b>400</b> with the <c>errors</c> dictionary keyed by field.</summary>
        public static ObjectResult ValidationProblemResult(this ControllerBase controller)
            => ApiProblems.ToResult(ApiProblems.Validation(controller.HttpContext, controller.ModelState));
    }
}
