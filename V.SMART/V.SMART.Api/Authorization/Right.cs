namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The operation a controller action requires on its declared screen.
    /// <para>
    /// <b>Four members, not five.</b> <c>IsHide</c> is deliberately absent: it is a navigation
    /// affordance, never an operation gate (KB-105 B-5, T-4), so it must not be expressible as a
    /// required right. Evidence: <c>V.SMART/V.SMART.Shared/Shared/RightsHelper.cs:19-20</c> is
    /// read only by <c>BaseUserRightsComponent.cs:27</c>, independently of the four operation
    /// properties at <c>:23-26</c>.
    /// </para>
    /// </summary>
    public enum Right
    {
        View,
        Create,
        Edit,
        Delete
    }
}
