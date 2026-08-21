using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace V.SMART.Shared.Tests.Architecture;

/// <summary>
/// Architecture guard installed by task M2-B04.
///
/// V.SMART.Shared is a single assembly holding both the domain (BusinessLayer/, Data/,
/// Repository/, Mappings/, ViewModels/, E_Invoice/) and the Blazor UI (Pages/). Because
/// they share an assembly, the compiler does not stop domain code depending on the UI —
/// and before M2-B04, 16 `using V.SMART.Shared.Pages...` directives across 15 non-UI
/// files did exactly that. These tests fail if one is reintroduced.
///
/// The two facts below are complementary and BOTH are needed:
///
///   * <see cref="Domain_Types_Do_Not_Reference_Pages_Types"/> reflects over the compiled
///     assembly. It catches a real, load-bearing dependency (a field, property, parameter,
///     return type, base type or implemented interface whose type lives under
///     V.SMART.Shared.Pages). It CANNOT see an *unused* `using` directive — an unused
///     using leaves no trace in metadata whatsoever.
///
///   * <see cref="Domain_Source_Files_Contain_No_Pages_Using_Directives"/> scans the source
///     text, which is the only way to catch that unused-using case. It is the same check as
///     the task's grep:
///         grep -rn "V.SMART.Shared.Pages" --include="*.cs" V.SMART/V.SMART.Shared/ | grep -v "/Pages/"
///     It is skipped (not failed) when the source tree cannot be located, so the suite stays
///     runnable from a published test bundle; see <see cref="TryFindRepositoryRoot"/>.
///
/// Both assert on a COLLECTED list of violations rather than the first one, so a failure
/// message names every offender at once.
/// </summary>
public class NoPagesReferenceFromDomainTests
{
    /// <summary>Namespace root of the Blazor UI inside V.SMART.Shared.</summary>
    private const string PagesNamespaceRoot = "V.SMART.Shared.Pages";

    /// <summary>
    /// The six non-UI namespace roots named by M2-B04's Target Result item 4.
    /// </summary>
    private static readonly string[] DomainNamespaceRoots =
    {
        "V.SMART.Shared.BusinessLayer",
        "V.SMART.Shared.Data",
        "V.SMART.Shared.Repository",
        "V.SMART.Shared.Mappings",
        "V.SMART.Shared.ViewModels",
        "V.SMART.Shared.E_Invoice",
    };

    [Fact]
    public void Domain_Types_Do_Not_Reference_Pages_Types()
    {
        // ApplicationDbContext is a stable, always-present anchor into V.SMART.Shared.
        var assembly = typeof(global::V.SMART.Shared.Data.ApplicationDbContext).Assembly;
        Assert.Equal("V.SMART.Shared", assembly.GetName().Name);

        var violations = new List<string>();

        foreach (var type in GetLoadableTypes(assembly))
        {
            var ns = type.Namespace;
            if (ns is null || !IsInDomain(ns))
            {
                continue;
            }

            foreach (var (referenced, member) in GetReferencedTypes(type))
            {
                if (IsPagesType(referenced))
                {
                    violations.Add($"{type.FullName} -> {member} -> {referenced.FullName}");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            BuildFailureMessage(
                "Domain types must not depend on any type under " + PagesNamespaceRoot + ".",
                violations));
    }

    [Fact]
    public void Domain_Source_Files_Contain_No_Pages_Using_Directives()
    {
        if (!TryFindRepositoryRoot(out var repoRoot))
        {
            // Nothing to scan: the compiled assembly is present but the source tree is not
            // (for example a published test bundle). Reflection above still ran. Do not
            // fail on an absent input - that would be a false alarm, not a violation.
            return;
        }

        var sharedRoot = Path.Combine(repoRoot, "V.SMART", "V.SMART.Shared");
        Assert.True(
            Directory.Exists(sharedRoot),
            $"Located repository root '{repoRoot}' but '{sharedRoot}' does not exist.");

        var pagesDirectory = Path.Combine(sharedRoot, "Pages") + Path.DirectorySeparatorChar;
        var violations = new List<string>();

        foreach (var file in Directory.EnumerateFiles(sharedRoot, "*.cs", SearchOption.AllDirectories))
        {
            // The UI itself is allowed to live in - and name - its own namespace.
            if (file.StartsWith(pagesDirectory, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // obj/ and bin/ hold generated Razor C# that legitimately names the UI namespace.
            if (IsBuildOutput(file, sharedRoot))
            {
                continue;
            }

            var lineNumber = 0;
            foreach (var line in File.ReadLines(file))
            {
                lineNumber++;
                if (line.Contains(PagesNamespaceRoot, StringComparison.Ordinal))
                {
                    violations.Add(
                        $"{Path.GetRelativePath(repoRoot, file).Replace('\\', '/')}:{lineNumber}: {line.Trim()}");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            BuildFailureMessage(
                "Non-UI source under V.SMART/V.SMART.Shared/ must not name " + PagesNamespaceRoot + ".",
                violations));
    }

    private static bool IsBuildOutput(string file, string sharedRoot)
    {
        var relative = Path.GetRelativePath(sharedRoot, file)
            .Replace('\\', '/');

        return relative.StartsWith("obj/", StringComparison.OrdinalIgnoreCase)
            || relative.StartsWith("bin/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInDomain(string ns)
    {
        foreach (var root in DomainNamespaceRoots)
        {
            if (ns.Equals(root, StringComparison.Ordinal)
                || ns.StartsWith(root + ".", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPagesType(Type type)
    {
        var ns = type.Namespace;
        if (ns is null)
        {
            return false;
        }

        return ns.Equals(PagesNamespaceRoot, StringComparison.Ordinal)
            || ns.StartsWith(PagesNamespaceRoot + ".", StringComparison.Ordinal);
    }

    /// <summary>
    /// Every type a domain type mentions in its own signature surface, paired with a label
    /// describing where the mention is. Generic arguments, arrays, by-ref and pointer types
    /// are unwrapped so that, for example, <c>List&lt;SomePage&gt;</c> is caught.
    /// </summary>
    private static IEnumerable<(Type Referenced, string Member)> GetReferencedTypes(Type type)
    {
        const BindingFlags All = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        var results = new List<(Type, string)>();

        try
        {
            if (type.BaseType is { } baseType)
            {
                AddUnwrapped(results, baseType, "base type");
            }

            foreach (var iface in type.GetInterfaces())
            {
                AddUnwrapped(results, iface, "implemented interface");
            }

            foreach (var argument in type.GetGenericArguments())
            {
                AddUnwrapped(results, argument, "generic argument");
            }

            foreach (var field in type.GetFields(All))
            {
                AddUnwrapped(results, field.FieldType, $"field {field.Name}");
            }

            foreach (var property in type.GetProperties(All))
            {
                AddUnwrapped(results, property.PropertyType, $"property {property.Name}");
            }

            foreach (var method in type.GetMethods(All))
            {
                AddUnwrapped(results, method.ReturnType, $"return type of {method.Name}()");

                foreach (var parameter in method.GetParameters())
                {
                    AddUnwrapped(
                        results,
                        parameter.ParameterType,
                        $"parameter {parameter.Name} of {method.Name}()");
                }
            }

            foreach (var constructor in type.GetConstructors(All))
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    AddUnwrapped(
                        results,
                        parameter.ParameterType,
                        $"constructor parameter {parameter.Name}");
                }
            }
        }
        catch (Exception ex) when (ex is TypeLoadException or FileNotFoundException or FileLoadException)
        {
            // A member whose type lives in an assembly this test project does not reference
            // cannot be inspected. It also cannot be a V.SMART.Shared.Pages type, because
            // those live in the very assembly being scanned - so skipping is safe here.
        }

        return results;
    }

    private static void AddUnwrapped(List<(Type, string)> results, Type type, string member)
    {
        foreach (var unwrapped in Unwrap(type))
        {
            results.Add((unwrapped, member));
        }
    }

    private static IEnumerable<Type> Unwrap(Type type)
    {
        var current = type;

        while (current.IsArray || current.IsByRef || current.IsPointer)
        {
            var element = current.GetElementType();
            if (element is null)
            {
                break;
            }

            current = element;
        }

        yield return current;

        if (current.IsGenericType)
        {
            foreach (var argument in current.GetGenericArguments())
            {
                foreach (var nested in Unwrap(argument))
                {
                    yield return nested;
                }
            }
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }

    /// <summary>
    /// Finds the repository root by walking up from this file's compile-time location, then
    /// from the test binaries' location. Returns false rather than throwing when neither
    /// works, so the source scan degrades to a skip instead of a spurious failure.
    /// </summary>
    private static bool TryFindRepositoryRoot(
        out string repositoryRoot,
        [CallerFilePath] string callerFilePath = "")
    {
        var candidates = new[]
        {
            string.IsNullOrEmpty(callerFilePath) ? null : Path.GetDirectoryName(callerFilePath),
            AppContext.BaseDirectory,
            Directory.GetCurrentDirectory(),
        };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrEmpty(candidate))
            {
                continue;
            }

            var directory = new DirectoryInfo(candidate);
            while (directory is not null)
            {
                var marker = Path.Combine(
                    directory.FullName, "V.SMART", "V.SMART.Shared", "V.SMART.Shared.csproj");

                if (File.Exists(marker))
                {
                    repositoryRoot = directory.FullName;
                    return true;
                }

                directory = directory.Parent;
            }
        }

        repositoryRoot = string.Empty;
        return false;
    }

    private static string BuildFailureMessage(string headline, List<string> violations)
    {
        if (violations.Count == 0)
        {
            return headline;
        }

        var builder = new StringBuilder();
        builder.Append(headline);
        builder.Append(' ');
        builder.Append(violations.Count);
        builder.AppendLine(" violation(s) found:");

        foreach (var violation in violations.Distinct().OrderBy(v => v, StringComparer.Ordinal))
        {
            builder.Append("  - ");
            builder.AppendLine(violation);
        }

        return builder.ToString();
    }
}
