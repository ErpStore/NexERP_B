using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;

namespace V.SMART.Api.Tests.PermissionMatrix
{
    /// <summary>
    /// M2-A03 — the reflection sweep the whole harness is built on. It enumerates every public
    /// controller action in the <b>real</b> <c>V.SMART.Api</c> assembly, so a controller added
    /// tomorrow appears here with no edit to any test file. That property is the harness's only
    /// job (M2-A03 Target Result §5): if a new controller can merge without appearing in these
    /// cases, the harness has failed.
    ///
    /// <para><b>Why reflection over the assembly and not MVC's own
    /// <c>IActionDescriptorCollectionProvider</c>.</b> The provider needs a built host, and this
    /// project has none — no <c>Microsoft.AspNetCore.Mvc.Testing</c>, no
    /// <c>WebApplicationFactory</c> (R-43, KB-060). The sweep below reproduces the part of MVC's
    /// discovery this harness depends on and nothing more; where it could diverge, it is
    /// deliberately <i>wider</i> than MVC (it does not require an <c>[HttpGet]</c>-family
    /// attribute), so a genuinely routable action can never be missed by being filtered out.
    /// <see cref="EndpointDiscoveryTests.The_sweep_agrees_with_the_production_startup_validator"/>
    /// then feeds the result back through the production
    /// <see cref="ScreenRightStartupValidator"/>, which is the code that actually gates the host,
    /// so the two never drift apart silently.</para>
    ///
    /// <para><b>Attribute order matters and is not negotiable.</b>
    /// <see cref="ApiEndpoint.EndpointMetadata"/> is composed controller-attributes-first, then
    /// action-attributes, exactly as MVC composes <c>ActionDescriptor.EndpointMetadata</c>. Both
    /// <see cref="ScreenRightAuthorizationFilter"/> (filter, <c>:55-56</c>) and
    /// <see cref="ScreenRightStartupValidator"/> (<c>:70-71</c>) take
    /// <c>LastOrDefault()</c> from that sequence to get the nearest declaration. A helper that
    /// read <c>MethodInfo.GetCustomAttribute&lt;RequireScreenAttribute&gt;()</c> alone would miss
    /// every controller-level screen and fail the entire API.</para>
    /// </summary>
    internal static class ApiEndpointDiscovery
    {
        /// <summary>
        /// The assembly under test, taken from a production type rather than named as a string,
        /// so a rename cannot silently reduce the sweep to zero endpoints.
        /// </summary>
        public static Assembly ApiAssembly { get; } = typeof(ScreenRightAuthorizationFilter).Assembly;

        private static readonly Lazy<IReadOnlyList<ApiEndpoint>> LazyAll = new(Discover);

        /// <summary>Every discovered action, in a stable order (controller, then action).</summary>
        public static IReadOnlyList<ApiEndpoint> All => LazyAll.Value;

        /// <summary>
        /// The actions this harness must drive a full permission matrix over: everything that is
        /// neither anonymous nor <c>[NoScreenRight]</c>-exempt. Note it is <b>not</b> filtered to
        /// "has a screen and a right" — an action that is missing either one still belongs here,
        /// and the discovery tests must fail on it rather than quietly drop it from the matrix.
        /// </summary>
        public static IReadOnlyList<ApiEndpoint> Guarded { get; } =
            All.Where(e => !e.IsAnonymous && !e.IsScreenRightExempt).ToList();

        /// <summary>Controllers found, for the "the sweep is not accidentally empty" assertion.</summary>
        public static IReadOnlyList<Type> ControllerTypes { get; } =
            All.Select(e => e.ControllerType).Distinct().OrderBy(t => t.Name, StringComparer.Ordinal).ToList();

        private static IReadOnlyList<ApiEndpoint> Discover()
            => DiscoverFrom(ApiAssembly.GetTypes().Where(IsController));

        /// <summary>
        /// The same sweep over an explicit set of types. Used by
        /// <see cref="HarnessSelfTests"/> to run the real rules over deliberately misannotated
        /// stand-in controllers, so the harness's ability to fail is itself a permanent,
        /// automated test rather than a one-off manual experiment recorded in prose.
        /// </summary>
        public static IReadOnlyList<ApiEndpoint> DiscoverFrom(IEnumerable<Type> controllerTypes)
        {
            var controllers = controllerTypes.OrderBy(t => t.Name, StringComparer.Ordinal);

            var endpoints = new List<ApiEndpoint>();

            foreach (var controller in controllers)
            {
                // Controller-level attributes, resolved with inherit: true so an attribute placed
                // on a future base controller is seen exactly as MVC would see it.
                var controllerAttributes = controller.GetCustomAttributes(inherit: true);

                foreach (var method in Actions(controller))
                {
                    var actionAttributes = method.GetCustomAttributes(inherit: true);

                    // Controller first, then action: MVC's own order (see the class remark).
                    var metadata = controllerAttributes.Concat(actionAttributes).ToList();

                    endpoints.Add(new ApiEndpoint(
                        controller,
                        method,
                        metadata));
                }
            }

            return endpoints
                .OrderBy(e => e.ControllerName, StringComparer.Ordinal)
                .ThenBy(e => e.ActionName, StringComparer.Ordinal)
                .ToList();
        }

        private static bool IsController(Type type)
        {
            if (!type.IsClass || type.IsAbstract || !type.IsPublic)
            {
                return false;
            }

            if (type.IsDefined(typeof(NonControllerAttribute), inherit: true))
            {
                return false;
            }

            // Both halves of MVC's own rule: derive from ControllerBase, or end in "Controller"
            // and carry [ApiController]/[Controller]. The API uses only the first form today; the
            // second is here so a future POCO controller is not invisible to the gate.
            return typeof(ControllerBase).IsAssignableFrom(type)
                || (type.Name.EndsWith("Controller", StringComparison.Ordinal)
                    && type.IsDefined(typeof(ApiControllerAttribute), inherit: true));
        }

        /// <summary>
        /// Public, instance, declared on the controller itself, not a property accessor, not
        /// <c>[NonAction]</c>, not inherited framework plumbing. Constructors are excluded by
        /// <see cref="Type.GetMethods(BindingFlags)"/>, which never returns them.
        /// </summary>
        private static IEnumerable<MethodInfo> Actions(Type controller)
            => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Where(m => !m.IsDefined(typeof(NonActionAttribute), inherit: true))
                .OrderBy(m => m.Name, StringComparer.Ordinal);
    }

    /// <summary>
    /// One discovered controller action and everything the authorization mechanism can say about
    /// it. Every field is read from the real type — nothing here is hand-maintained.
    /// </summary>
    internal sealed class ApiEndpoint
    {
        public ApiEndpoint(Type controllerType, MethodInfo method, IReadOnlyList<object> metadata)
        {
            ControllerType = controllerType;
            Method = method;
            EndpointMetadata = metadata;

            // MVC strips the "Controller" suffix for ControllerActionDescriptor.ControllerName;
            // the same name is used here so failure messages match the production validator's.
            ControllerName = controllerType.Name.EndsWith("Controller", StringComparison.Ordinal)
                ? controllerType.Name[..^"Controller".Length]
                : controllerType.Name;

            ActionName = method.Name;

            IsAnonymous = metadata.OfType<IAllowAnonymous>().Any();
            ScreenRightExemption = metadata.OfType<NoScreenRightAttribute>().LastOrDefault();

            // LastOrDefault, not First and not MethodInfo.GetCustomAttribute: the nearest
            // declaration wins, which is what the filter and the startup validator both do.
            Screen = metadata.OfType<RequireScreenAttribute>().LastOrDefault();
            Right = metadata.OfType<RequireRightAttribute>().LastOrDefault();

            RightAttributeCount = method.GetCustomAttributes<RequireRightAttribute>(inherit: true).Count();
            Route = RouteDescription(controllerType, method);
        }

        public Type ControllerType { get; }

        public MethodInfo Method { get; }

        public IReadOnlyList<object> EndpointMetadata { get; }

        public string ControllerName { get; }

        public string ActionName { get; }

        /// <summary><c>ControllerController.Action</c> — the key the allow-list uses.</summary>
        public string Key => $"{ControllerType.Name}.{ActionName}";

        /// <summary>Best-effort <c>METHOD /route</c>, for human-readable failure messages only.</summary>
        public string Route { get; }

        public bool IsAnonymous { get; }

        public NoScreenRightAttribute? ScreenRightExemption { get; }

        public bool IsScreenRightExempt => ScreenRightExemption is not null;

        public RequireScreenAttribute? Screen { get; }

        public RequireRightAttribute? Right { get; }

        /// <summary>
        /// How many <c>[RequireRight]</c> attributes the action itself declares. The attribute is
        /// <c>AllowMultiple = false</c>, so this can only be 0 or 1 today — it is counted rather
        /// than assumed so that "exactly one" is an assertion, not a compiler side effect that
        /// could change with the attribute's declaration.
        /// </summary>
        public int RightAttributeCount { get; }

        public string ScreenName => Screen?.ScreenName ?? "(undeclared)";

        public string RightName => Right?.Right.ToString() ?? "(undeclared)";

        /// <summary>
        /// The string every failure message in this harness carries. M2-A03 acceptance: a bare
        /// assertion failure across hundreds of cases is unusable, so controller, action, screen
        /// and right are always named.
        /// </summary>
        public string Describe()
            => $"{ControllerType.Name}.{ActionName} [{Route}] screen='{ScreenName}' right='{RightName}'";

        public override string ToString() => Key;

        private static string RouteDescription(Type controller, MethodInfo method)
        {
            var prefix = controller
                .GetCustomAttributes<RouteAttribute>(inherit: true)
                .FirstOrDefault()?.Template?.Trim('/') ?? string.Empty;

            var httpMethodAttribute = method
                .GetCustomAttributes(inherit: true)
                .OfType<IActionHttpMethodProvider>()
                .FirstOrDefault();

            var verb = httpMethodAttribute?.HttpMethods?.FirstOrDefault() ?? "ANY";
            var suffix = (httpMethodAttribute as IRouteTemplateProvider)?.Template?.Trim('/');

            var path = string.IsNullOrEmpty(suffix) ? prefix : $"{prefix}/{suffix}";

            return $"{verb} /{path}";
        }
    }
}
