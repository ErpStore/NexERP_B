using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// Refuses to start the host if an action serves a <b>scoped entity</b> without saying whether
    /// it scopes (M2-A08, KB-108 §5; the structural guard the task's test 15 requires).
    ///
    /// <para><b>Why a startup sweep and not a code review.</b> The Blazor row scope failed as a
    /// design because scoping was decided per call site and nothing noticed when a call site decided
    /// wrong: the unscoped <c>GetAllLeadsAsync()</c> ended up with four call sites against the scoped
    /// method's one (<c>LeadsList.razor:398</c>, <c>:476</c>; <c>LeadsUpsert.razor:1275</c>,
    /// <c>:1287</c>). Reproducing the same "remember to scope" convention in the API and trusting
    /// review to catch it would reproduce the same outcome. This makes forgetting a build-and-boot
    /// failure instead.</para>
    ///
    /// <para><b>What it cannot see, stated so nobody over-reads it.</b> It matches on the entity
    /// types appearing in an action's return type, including inside <c>Task&lt;&gt;</c>,
    /// <c>ActionResult&lt;&gt;</c>, <c>PagedResult&lt;&gt;</c> and any other generic wrapper. An
    /// action that returns a hand-written DTO which happens to carry lead fields is invisible to it,
    /// and always will be. It is a floor, not a proof.</para>
    ///
    /// <para>Same shape as <see cref="ScreenRightStartupValidator"/> — one
    /// <see cref="InvalidOperationException"/> naming every offender at once, with the remediation.</para>
    /// </summary>
    public static class RowScopeStartupValidator
    {
        /// <summary>
        /// Throws when any of the following holds.
        /// <list type="number">
        /// <item>An action's return type mentions a <see cref="ScopedEntityCatalogue"/> entity and it
        /// declares neither <see cref="RowScopedAttribute"/> nor <see cref="NoRowScopeAttribute"/>.</item>
        /// <item>An action declares both — the two are contradictory and the resolution must be
        /// made by a person, not by a precedence rule nobody will remember.</item>
        /// <item>An action declares <c>[RowScoped(typeof(X))]</c> for an <c>X</c> that is not
        /// registered in <see cref="ScopedEntityCatalogue"/>, so no predicate could be applied to it.</item>
        /// </list>
        /// <para>
        /// <c>[AllowAnonymous]</c> is <b>not</b> an exemption here, unlike in the screen-right sweep.
        /// An anonymous endpoint has no caller to resolve a scope for, so it cannot legitimately
        /// serve a scoped entity at all; it must say <c>[NoRowScope]</c> and justify itself.
        /// </para>
        /// </summary>
        public static void Validate(IServiceProvider services)
        {
            ArgumentNullException.ThrowIfNull(services);

            var actions = services
                .GetRequiredService<IActionDescriptorCollectionProvider>()
                .ActionDescriptors
                .Items
                .OfType<ControllerActionDescriptor>();

            var problems = new List<string>();

            foreach (var action in actions)
            {
                var metadata = action.EndpointMetadata ?? Array.Empty<object>();
                var scoped = metadata.OfType<RowScopedAttribute>().LastOrDefault();
                var opted = metadata.OfType<NoRowScopeAttribute>().LastOrDefault();
                var name = $"{action.ControllerName}Controller.{action.ActionName}";

                if (scoped is not null && opted is not null)
                {
                    problems.Add(
                        $"{name} declares both [RowScoped] and [NoRowScope]. Remove one; they cannot both be true.");
                    continue;
                }

                if (scoped is not null && !ScopedEntityCatalogue.IsScoped(scoped.EntityType))
                {
                    problems.Add(
                        $"{name} declares [RowScoped(typeof({scoped.EntityType.Name}))], which is not registered in " +
                        "ScopedEntityCatalogue, so no scope predicate exists for it. Register the entity with its " +
                        "state selector, or remove the attribute.");
                    continue;
                }

                if (scoped is not null || opted is not null)
                {
                    continue;
                }

                var entity = ScopedEntitiesIn(action.MethodInfo?.ReturnType).FirstOrDefault();
                if (entity is not null)
                {
                    problems.Add(
                        $"{name} returns {entity.Name}, which is row-scoped (ScopedEntityCatalogue), but declares " +
                        $"neither [RowScoped(typeof({entity.Name}))] nor [NoRowScope(\"<justification>\")]. " +
                        "Apply the caller's scope with IQueryable<T>.ApplyRowScope(scope) and declare it, or opt out " +
                        "explicitly and say why.");
                }
            }

            if (problems.Count == 0)
            {
                return;
            }

            var message = new StringBuilder()
                .AppendLine(
                    "Row-scope annotations are invalid. The API will not start until every item below is fixed " +
                    "(M2-A08, KB-108 §5):");

            foreach (var problem in problems.Distinct(StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal))
            {
                message.Append("  - ").AppendLine(problem);
            }

            throw new InvalidOperationException(message.ToString());
        }

        /// <summary>
        /// Every scoped entity type mentioned by <paramref name="type"/>, unwrapping generic
        /// arguments (<c>Task&lt;ActionResult&lt;PagedResult&lt;Leads&gt;&gt;&gt;</c> → <c>Leads</c>)
        /// and array element types. Depth-limited only by the type graph, which is finite.
        /// </summary>
        internal static IEnumerable<Type> ScopedEntitiesIn(Type? type)
        {
            if (type is null)
            {
                yield break;
            }

            if (ScopedEntityCatalogue.IsScoped(type))
            {
                yield return type;
            }

            if (type.IsArray)
            {
                foreach (var found in ScopedEntitiesIn(type.GetElementType()))
                {
                    yield return found;
                }
            }

            if (!type.IsGenericType)
            {
                yield break;
            }

            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var found in ScopedEntitiesIn(argument))
                {
                    yield return found;
                }
            }
        }
    }
}
