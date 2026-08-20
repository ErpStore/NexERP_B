namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// Declares that an action returns rows of a scoped entity <b>and applies the caller's row
    /// scope</b> (M2-A08, KB-108 §5).
    ///
    /// <para>The attribute is a <i>declaration</i>, not the enforcement: the filtering happens in
    /// <see cref="RowScopeQueryExtensions.ApplyRowScope{T}"/>, at the query. What the attribute buys
    /// is that <see cref="RowScopeStartupValidator"/> can tell an action that scopes from one that
    /// forgot — the failure mode that made the Blazor design unsafe, where scoping was a per-call-site
    /// choice with nothing to notice when a call site chose wrong (claim 7).</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class RowScopedAttribute : Attribute
    {
        public RowScopedAttribute(Type entityType)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        }

        /// <summary>The scoped entity, which must be registered in <see cref="ScopedEntityCatalogue"/>.</summary>
        public Type EntityType { get; }
    }
}
