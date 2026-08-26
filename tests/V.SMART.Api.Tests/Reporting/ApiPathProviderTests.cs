using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using V.SMART.Api.Reporting;
using Xunit;

namespace V.SMART.Api.Tests.Reporting
{
    /// <summary>
    /// M2-B08 Testing §2 / Acceptance Criteria item 1: <c>ApiPathProvider</c>'s resolved path
    /// must be <b>observed</b> to contain the templates directory, not assumed from reading the
    /// code — the task file calls the "assets actually land under
    /// <c>_content/V.SMART.Shared/...</c> for this project" claim <i>Inferred</i> until checked.
    /// This runs the real dev-fallback path-walk against the real repository layout (no fakes on
    /// the filesystem side), with a minimal <see cref="IWebHostEnvironment"/> stub standing in
    /// only for the two properties the provider reads.
    /// </summary>
    public class ApiPathProviderTests
    {
        [Fact]
        public void GetReportTemplatePath_resolves_to_a_real_directory_containing_default()
        {
            // Mirrors the real V.SMART.Api project's actual ContentRootPath at runtime — observed
            // live (task Testing §2) to be the PROJECT directory itself, not its bin output, when
            // run via `dotnet run`: "Content root path: .../V.SMART/V.SMART.Api". This test's own
            // assembly runs from tests/V.SMART.Api.Tests/bin/.../, so it walks up to the repo root
            // (the ancestor containing a "V.SMART" folder) and constructs the equivalent path,
            // rather than assuming its own AppContext.BaseDirectory is anywhere near the API
            // project's directory — it is not.
            var repoRoot = FindAncestorContaining(AppContext.BaseDirectory, "V.SMART")
                ?? throw new InvalidOperationException(
                    $"Could not find a repository root (an ancestor containing a 'V.SMART' folder) above '{AppContext.BaseDirectory}'.");
            var contentRoot = Path.Combine(repoRoot, "V.SMART", "V.SMART.Api");

            var env = new FakeWebHostEnvironment(webRootPath: string.Empty, contentRootPath: contentRoot);
            var provider = new ApiPathProvider(env);

            var resolved = provider.GetReportTemplatePath();

            Assert.True(Directory.Exists(resolved), $"Resolved path '{resolved}' does not exist.");
            Assert.True(
                Directory.Exists(Path.Combine(resolved, "default")),
                $"Resolved path '{resolved}' exists but has no 'default' subfolder — the templates directory this must be does.");
            // Sanity: the real default folder has dozens of .frx files (46 as of M2-B08), not
            // an empty or wrong directory that merely happens to be named "default".
            Assert.True(
                Directory.GetFiles(Path.Combine(resolved, "default"), "*.frx").Length > 10,
                "The resolved 'default' folder has too few .frx files to plausibly be the real templates directory.");
        }

        [Fact]
        public void GetReportTemplatePath_throws_when_neither_candidate_exists()
        {
            var env = new FakeWebHostEnvironment(
                webRootPath: Path.Combine(Path.GetTempPath(), "m2-b08-test-nonexistent-webroot-" + Guid.NewGuid()),
                contentRootPath: Path.Combine(Path.GetTempPath(), "m2-b08-test-nonexistent-contentroot-" + Guid.NewGuid()));
            var provider = new ApiPathProvider(env);

            Assert.Throws<DirectoryNotFoundException>(() => provider.GetReportTemplatePath());
        }

        private static string? FindAncestorContaining(string startPath, string childName)
        {
            var dir = new DirectoryInfo(startPath);
            while (dir is not null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, childName)))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        private sealed class FakeWebHostEnvironment : IWebHostEnvironment
        {
            public FakeWebHostEnvironment(string webRootPath, string contentRootPath)
            {
                WebRootPath = webRootPath;
                ContentRootPath = contentRootPath;
            }

            public string WebRootPath { get; set; }
            public IFileProvider WebRootFileProvider { get; set; } = null!;
            public string ContentRootPath { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; } = null!;
            public string ApplicationName { get; set; } = "V.SMART.Api.Tests";
            public string EnvironmentName { get; set; } = "Production";
        }
    }
}
