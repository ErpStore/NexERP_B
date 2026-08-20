namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The explicit, auditable opt-out for an authenticated endpoint that legitimately has no
    /// screen (KB-105 §2.4). <b>Not</b> a substitute for <c>[AllowAnonymous]</c>: an endpoint
    /// marked this way still requires authentication, it merely carries no screen-right check.
    /// </summary>
    /// <remarks>
    /// The mandatory justification argument makes every opt-out self-documenting and greppable.
    /// It exists because <c>GET /api/v1/me</c> (M2-A07) must be reachable by every authenticated
    /// user regardless of screen rights — gating it on a screen right would deadlock login.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NoScreenRightAttribute : Attribute
    {
        public NoScreenRightAttribute(string justification)
        {
            Justification = justification ?? throw new ArgumentNullException(nameof(justification));
        }

        public string Justification { get; }
    }
}
