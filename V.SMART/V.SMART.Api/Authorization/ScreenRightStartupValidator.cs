using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The startup half of KB-105 D-4 and D-6: a misannotated action is a property of the
    /// assembly, not of any request, so it is caught before the host serves anything. Same shape
    /// as <c>StartupConfigurationValidator.Validate</c> (<c>Program.cs:16</c>, M0-03-03) - throw
    /// <see cref="InvalidOperationException"/> naming every offender at once, with the remediation.
    /// <para>
    /// This is defence in depth, not the only guard: <see cref="ScreenRightAuthorizationFilter"/>
    /// fails closed for the same conditions at request time, because a dynamically registered
    /// application part would never be seen by this sweep.
    /// </para>
    /// </summary>
    public static class ScreenRightStartupValidator
    {
        /// <summary>
        /// Sweeps every controller action known to MVC and throws if any of the following holds.
        /// <list type="number">
        /// <item>An action carries <c>[RequireRight]</c> while its controller carries no
        /// <c>[RequireScreen]</c> (KB-105 D-4, T-11).</item>
        /// <item>A controller carries <c>[RequireScreen]</c> while one of its actions carries
        /// neither <c>[RequireRight]</c> nor <c>[NoScreenRight]</c> and is not anonymous
        /// (KB-105 D-4, T-12).</item>
        /// <item>A declared screen name is absent from <see cref="ScreenCatalogue.SeededScreenNames"/>
        /// and the attribute did not set <c>Seeded = false</c> (KB-105 D-6). Ordinal comparison -
        /// the seed's own misspellings are the canonical strings (F-8).</item>
        /// </list>
        /// <para>
        /// <b>Not checked here, deliberately (M2-A01-02).</b> KB-105 D-4 also requires that an
        /// authenticated action on a controller with <i>no</i> <c>[RequireScreen]</c> at all be an
        /// error. That direction is not enabled yet: no controller is annotated in this task, so
        /// enabling it would refuse to start the host over <c>CurrencyController</c>'s five
        /// existing endpoints, which M2-A01-02 requires to behave exactly as before. M2-A02
        /// annotates the first controller and turns this direction on, together with the matching
        /// request-time branch in <see cref="ScreenRightAuthorizationFilter"/>. Until then R-03
        /// stays open for unannotated controllers.
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
                var metadata = action.EndpointMetadata;

                if (metadata.OfType<IAllowAnonymous>().Any())
                {
                    continue;
                }

                if (metadata.OfType<NoScreenRightAttribute>().Any())
                {
                    continue;
                }

                var screenAttribute = metadata.OfType<RequireScreenAttribute>().LastOrDefault();
                var rightAttribute = metadata.OfType<RequireRightAttribute>().LastOrDefault();

                var name = $"{action.ControllerName}Controller.{action.ActionName}";

                if (rightAttribute is not null && screenAttribute is null)
                {
                    problems.Add(
                        $"{name} declares [RequireRight({rightAttribute.Right})] but its controller declares no [RequireScreen]. " +
                        "Add [RequireScreen(\"<seeded screen name>\")] to the controller.");
                    continue;
                }

                if (screenAttribute is null)
                {
                    // Unannotated action on an unannotated controller: not this task's to reject
                    // (see the remark above). Left for M2-A02.
                    continue;
                }

                if (rightAttribute is null)
                {
                    problems.Add(
                        $"{name} is on a controller declaring [RequireScreen(\"{screenAttribute.ScreenName}\")] " +
                        "but declares neither [RequireRight] nor [NoScreenRight]. Add one of them.");
                }

                if (screenAttribute.Seeded &&
                    !ScreenCatalogue.SeededScreenNames.Contains(screenAttribute.ScreenName))
                {
                    problems.Add(
                        $"{name} declares [RequireScreen(\"{screenAttribute.ScreenName}\")], which is not one of the 150 seeded " +
                        "Screens.ScreenName values (ordinal comparison; see KB-105 Appendix A). Fix the string, or set " +
                        "Seeded = false with written justification at review.");
                }
            }

            if (problems.Count == 0)
            {
                return;
            }

            var message = new StringBuilder()
                .AppendLine("Screen-right annotations are invalid. The API will not start until every item below is fixed (ADR-004, KB-105 D-4/D-6):");

            foreach (var problem in problems.Distinct(StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal))
            {
                message.Append("  - ").AppendLine(problem);
            }

            throw new InvalidOperationException(message.ToString());
        }
    }
}
