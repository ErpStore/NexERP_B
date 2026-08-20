namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// Declares the single right an action requires on its controller's screen (KB-105 §2.3).
    /// </summary>
    /// <remarks>
    /// <c>AllowMultiple = false</c>: <c>RightsHelper</c> offers no combining rule, so any AND/OR
    /// semantics across two rights would be invented rather than preserved (KB-105 D-4).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class RequireRightAttribute : Attribute
    {
        public RequireRightAttribute(Right right)
        {
            Right = right;
        }

        public Right Right { get; }
    }
}
