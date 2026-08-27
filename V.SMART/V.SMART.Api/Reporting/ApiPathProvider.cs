using V.SMART.Shared.Services;

namespace V.SMART.Api.Reporting
{
    /// <summary>
    /// The <see cref="IPathProvider"/> the API host was missing (M2-B07, deferred here). Mirrors
    /// <c>V.SMART.Web/Services/WebPathProvider.cs</c> byte-for-byte in resolution order — same
    /// two candidate paths, same fallback order, same exception on total failure — because
    /// nothing about the resolution logic is host-specific; only the concrete
    /// <see cref="IWebHostEnvironment"/> instance differs between hosts.
    /// </summary>
    /// <remarks>
    /// M2-B08 §Target Result item 1: the assets landing under
    /// <c>_content/V.SMART.Shared/...</c> for <c>V.SMART.Api</c> specifically (a
    /// <c>Microsoft.NET.Sdk.Web</c> project referencing the <c>Microsoft.NET.Sdk.Razor</c>
    /// <c>V.SMART.Shared</c>) was <b>Inferred</b>, not observed, before this task ran the API and
    /// checked. See the task's Testing §2 record for the observed result.
    /// </remarks>
    public sealed class ApiPathProvider : IPathProvider
    {
        private readonly IWebHostEnvironment _env;

        public ApiPathProvider(IWebHostEnvironment env)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public string GetReportTemplatePath()
        {
            // 1. Published/static web assets path (matches WebPathProvider's PROD candidate).
            var prodPath = Path.Combine(_env.WebRootPath, "_content", "V.SMART.Shared", "templates");
            if (Directory.Exists(prodPath))
                return prodPath;

            // 2. Dev fallback: walk up from the API's own content root to the sibling
            //    V.SMART.Shared project and read its wwwroot directly.
            var devPath = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "V.SMART.Shared", "wwwroot", "templates"));
            if (Directory.Exists(devPath))
                return devPath;

            throw new DirectoryNotFoundException(
                "Report template folder not found in either the published static-assets path " +
                $"('{prodPath}') or the dev fallback ('{devPath}').");
        }
    }
}
