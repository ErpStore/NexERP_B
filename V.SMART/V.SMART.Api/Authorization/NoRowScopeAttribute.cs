namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The explicit, auditable opt-out: this action returns a scoped entity <b>unscoped</b>, on
    /// purpose (M2-A08, KB-108 §5).
    ///
    /// <para>It exists so that unscoping is a <i>reviewable act</i> rather than an omission. In the
    /// Blazor code the equivalent act is invisible — <c>LeadsList.razor:398</c> and
    /// <c>LeadsUpsert.razor:1275,:1287</c> call the unscoped <c>GetAllLeadsAsync()</c> and nothing
    /// records that a decision was taken, or by whom. The mandatory justification argument makes
    /// every such decision greppable:
    /// <c>git grep NoRowScope V.SMART/V.SMART.Api</c> lists the whole set.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NoRowScopeAttribute : Attribute
    {
        public NoRowScopeAttribute(string justification)
        {
            Justification = justification ?? throw new ArgumentNullException(nameof(justification));
        }

        public string Justification { get; }
    }
}
