using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using V.SMART.Api.Authorization;
using V.SMART.Api.Controllers;
using V.SMART.Api.Middleware;
using V.SMART.Api.Services;
using V.SMART.Api.Tests.Infrastructure;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Utilities.Correspondence;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IUtilitiesRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.Services.MultiCompanyService;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-B06 — the seven negative tests the task requires, each reported individually, plus the
    /// byte-identity round trip.
    ///
    /// <para><b>What these prove and what they do not.</b> N1–N5 and the round trip are
    /// behavioural: they call the real controller and the real <see cref="ApiFileUploadService"/>
    /// and assert what comes back. <b>N6 and N7 are policy-level</b> — they prove the endpoints
    /// carry the attributes that produce 401 and 403, and
    /// <see cref="ScreenRightAuthorizationFilterTests"/> separately proves the filter those
    /// attributes drive denies with 403. Neither is an end-to-end HTTP assertion against a running
    /// host with two real tenant databases; that needs a tenant-DB credential (Q-14 / R-01 / Q-32)
    /// and is the same residual risk M2-B09 recorded in KB-124 §6. The gap is stated, not papered
    /// over.</para>
    /// </summary>
    public class FileEndpointSecurityTests
    {
        // =====================================================================================
        // N1 — traversal in the identifier is refused
        // =====================================================================================

        /// <summary>
        /// The download route is <c>{id:int}</c>, so a traversal string is not a "rejected id" —
        /// it fails to match the route and never reaches the action at all. This asserts the
        /// constraint is actually on the template, because deleting it would silently reopen the
        /// whole class of attack.
        /// </summary>
        [Fact]
        public void N1_download_route_is_int_constrained_so_traversal_never_binds()
        {
            var method = typeof(FilesController).GetMethod(nameof(FilesController.Download))!;
            var route = method.GetCustomAttribute<HttpGetAttribute>()!;

            Assert.Equal("{id:int}", route.Template);
        }

        /// <summary>
        /// Defence in depth behind the route constraint: even a <i>stored</i> path that escapes the
        /// uploads root — a poisoned <c>Correspondence.FilePath</c> row, not a URL — resolves to
        /// null rather than to a file outside the root.
        /// </summary>
        [Theory]
        [InlineData("../../../../Windows/System32/drivers/etc/hosts")]
        [InlineData("/uploads/correspondences/../../../../Windows/win.ini")]
        [InlineData("uploads/../../secrets.txt")]
        [InlineData("%2e%2e%2f%2e%2e%2fsecrets.txt")]
        [InlineData("..\\..\\secrets.txt")]
        [InlineData("C:\\Windows\\win.ini")]
        public void N1_stored_paths_that_escape_the_uploads_root_are_refused(string storedPath)
        {
            var root = NewTempRoot();
            try
            {
                Assert.Null(UploadPaths.ResolveStoredPath(root, storedPath));
            }
            finally { Cleanup(root); }
        }

        /// <summary>
        /// Percent-encoded traversal is <b>not</b> refused by the resolver, and that is the correct
        /// behaviour rather than a hole. <c>ResolveStoredPath</c> takes a value out of the database,
        /// not off the wire, so it must not URL-decode: <c>%2f</c> is a legal character in a file
        /// name. The encoded form therefore stays a single, harmless path segment inside the root —
        /// it escapes nothing — and the file it names does not exist, so the endpoint answers the
        /// same 404 as any other unknown path.
        ///
        /// <para>On the wire the sequence never reaches here at all: ASP.NET Core decodes the path
        /// before routing, and <c>{id:int}</c> then refuses to bind it (see
        /// <see cref="N1_download_route_is_int_constrained_so_traversal_never_binds"/>). This test
        /// records that division of labour so a future change does not "fix" the resolver by
        /// decoding, which would <i>introduce</i> the traversal it appears to prevent.</para>
        /// </summary>
        [Fact]
        public void N1_percent_encoded_traversal_is_a_literal_name_and_still_cannot_escape()
        {
            var root = NewTempRoot();
            try
            {
                const string stored = "/uploads/correspondences/..%2f..%2fsecrets.txt";

                var resolved = UploadPaths.ResolveStoredPath(root, stored);

                Assert.NotNull(resolved);
                Assert.True(UploadPaths.IsInsideRoot(root, resolved!));
                Assert.StartsWith(Path.GetFullPath(root), resolved!, StringComparison.OrdinalIgnoreCase);
                Assert.False(File.Exists(resolved!));
            }
            finally { Cleanup(root); }
        }

        /// <summary>A path genuinely inside the root still resolves — the guard is not "refuse everything".</summary>
        [Fact]
        public void N1_a_path_inside_the_uploads_root_still_resolves()
        {
            var root = NewTempRoot();
            try
            {
                var resolved = UploadPaths.ResolveStoredPath(
                    root, "/uploads/correspondences/acme/manualupload/x.pdf");

                Assert.NotNull(resolved);
                Assert.StartsWith(Path.GetFullPath(root), resolved!, StringComparison.OrdinalIgnoreCase);
            }
            finally { Cleanup(root); }
        }

        /// <summary>
        /// The upload side of the same attack: a client-supplied name carrying a directory part
        /// cannot become a directory part on disk.
        /// </summary>
        [Theory]
        [InlineData("../../evil.exe", "evil.exe")]
        [InlineData("..\\..\\evil.exe", "evil.exe")]
        [InlineData("/etc/passwd", "passwd")]
        public void N1_a_traversing_file_name_is_reduced_to_its_leaf(string supplied, string expectedLeaf)
        {
            var unique = UploadPaths.UniqueFileName(supplied);

            Assert.EndsWith("_" + expectedLeaf, unique);
            Assert.DoesNotContain("..", unique);
            Assert.DoesNotContain("/", unique);
            Assert.DoesNotContain("\\", unique);
        }

        // =====================================================================================
        // N2 — a tenant-A file requested with a tenant-B token is refused, without revealing
        //      whether the id exists
        // =====================================================================================

        /// <summary>
        /// Isolation is structural: <c>IUnitOfWork.Correspondances</c> is scoped to the
        /// tenant-resolved <c>ApplicationDbContext</c>, so tenant B's query for tenant A's id
        /// returns nothing. What this test pins is the part that is a <i>choice</i> and could
        /// regress — that "not yours" and "no such id" produce identical responses, so the endpoint
        /// cannot be used as a cross-tenant existence oracle.
        /// </summary>
        [Fact]
        public async Task N2_cross_tenant_and_unknown_id_are_indistinguishable()
        {
            var root = NewTempRoot();
            try
            {
                // Tenant B asking for tenant A's id 4242: its own scoped repository has no such row.
                var crossTenant = await DownloadWith(root, id: 4242, record: null);
                // The same tenant asking for an id that exists nowhere.
                var unknownId = await DownloadWith(root, id: 999999, record: null);

                var a = Assert.IsType<ObjectResult>(crossTenant);
                var b = Assert.IsType<ObjectResult>(unknownId);
                var pa = Assert.IsType<ProblemDetails>(a.Value);
                var pb = Assert.IsType<ProblemDetails>(b.Value);

                Assert.Equal(StatusCodes.Status404NotFound, a.StatusCode);
                Assert.Equal(StatusCodes.Status404NotFound, b.StatusCode);
                Assert.Equal(pa.Status, pb.Status);
                Assert.Equal(pa.Title, pb.Title);
                Assert.Equal(pa.Type, pb.Type);
                Assert.Equal(pa.Detail, pb.Detail);

                // Nothing in the body may hint that the id exists somewhere else.
                Assert.Equal("File not found.", pa.Title);
                Assert.DoesNotContain("4242", pa.Title);
            }
            finally { Cleanup(root); }
        }

        // =====================================================================================
        // N3 — a file above the size limit is refused with 413
        // =====================================================================================

        [Fact]
        public async Task N3_an_upload_above_the_configured_limit_is_refused_with_413()
        {
            var root = NewTempRoot();
            try
            {
                var options = new FileStorageOptions { Root = root, MaxUploadBytes = 1024 };
                var controller = NewController(root, options, Mock.Of<IUnitOfWork>());

                var result = await controller.Upload(
                    new FakeFormFile("report.pdf", new byte[2048]),
                    refType: "ManualUpload",
                    docType: "Correspondence");

                var objectResult = Assert.IsType<ObjectResult>(result.Result);
                var problem = Assert.IsType<ProblemDetails>(objectResult.Value);

                Assert.Equal(StatusCodes.Status413PayloadTooLarge, objectResult.StatusCode);
                Assert.Equal(StatusCodes.Status413PayloadTooLarge, problem.Status);
                Assert.Equal(ProblemTypes.PayloadTooLarge, problem.Type);
            }
            finally { Cleanup(root); }
        }

        /// <summary>
        /// The framework-level ceiling that stops an oversize body before the action runs, and its
        /// documented relationship to <c>WebFileUploadService.cs:101</c>'s 20 MB.
        /// </summary>
        [Fact]
        public void N3_the_framework_request_ceiling_matches_the_blazor_20MB_limit()
        {
            var method = typeof(FilesController).GetMethod(nameof(FilesController.Upload))!;

            Assert.Equal(20L * 1024 * 1024, FileStorageOptions.DefaultMaxUploadBytes);

            // RequestSizeLimitAttribute exposes no getter for its ceiling, so the constructor
            // argument is read off the metadata directly.
            var sizeLimit = method
                .GetCustomAttributesData()
                .Single(a => a.AttributeType == typeof(RequestSizeLimitAttribute));
            Assert.Equal(
                FileStorageOptions.DefaultMaxUploadBytes,
                (long)sizeLimit.ConstructorArguments[0].Value!);

            Assert.Equal(
                FileStorageOptions.DefaultMaxUploadBytes,
                method.GetCustomAttribute<RequestFormLimitsAttribute>()!.MultipartBodyLengthLimit);
        }

        // =====================================================================================
        // N4 — a disallowed extension / content type is refused with 400
        // =====================================================================================

        [Theory]
        [InlineData("payload.exe")]
        [InlineData("payload.dll")]
        [InlineData("payload.ps1")]
        [InlineData("payload.sh")]
        [InlineData("payload.bat")]
        [InlineData("payload")]
        [InlineData("archive.pdf.exe")]
        public async Task N4_a_disallowed_extension_is_refused_with_400(string fileName)
        {
            var root = NewTempRoot();
            try
            {
                var controller = NewController(
                    root, new FileStorageOptions { Root = root }, Mock.Of<IUnitOfWork>());

                var result = await controller.Upload(
                    new FakeFormFile(fileName, new byte[16]),
                    refType: "ManualUpload",
                    docType: "Correspondence");

                var objectResult = Assert.IsType<ObjectResult>(result.Result);

                Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
                Assert.False(UploadContentTypes.IsAllowed(fileName));
            }
            finally { Cleanup(root); }
        }

        /// <summary>
        /// The content type served is this API's own mapping, never the client's. An <c>.svg</c>
        /// stays uploadable — the Blazor list allows it — but is served opaquely rather than as
        /// <c>image/svg+xml</c>, which would make the endpoint a stored-XSS vector.
        /// </summary>
        [Fact]
        public void N4_the_served_content_type_is_the_api_s_own_mapping()
        {
            Assert.True(UploadContentTypes.TryResolve("drawing.svg", out var svg));
            Assert.Equal(UploadContentTypes.Fallback, svg);

            Assert.True(UploadContentTypes.TryResolve("report.pdf", out var pdf));
            Assert.Equal("application/pdf", pdf);
        }

        // =====================================================================================
        // N5 — an unknown id is refused with 404
        // =====================================================================================

        [Fact]
        public async Task N5_an_unknown_id_is_refused_with_404()
        {
            var root = NewTempRoot();
            try
            {
                var result = await DownloadWith(root, id: 987654, record: null);

                var objectResult = Assert.IsType<ObjectResult>(result);
                var problem = Assert.IsType<ProblemDetails>(objectResult.Value);

                Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
                Assert.Equal("File not found.", problem.Title);
            }
            finally { Cleanup(root); }
        }

        /// <summary>
        /// A row that exists but whose bytes are gone — an empty <c>Image</c> column plus a missing
        /// file, which is exactly the state a Blazor-uploaded file is in because of the
        /// <c>WebFileUploadService.cs:102</c> defect — is the same 404, not a 500.
        /// </summary>
        [Fact]
        public async Task N5_a_row_whose_bytes_are_missing_is_the_same_404()
        {
            var root = NewTempRoot();
            try
            {
                var record = new Correspondence
                {
                    Id = 7,
                    FileName = "gone.pdf",
                    FilePath = "/uploads/correspondences/acme/manualupload/gone.pdf",
                    Image = Array.Empty<byte>(),
                    DocumentType = "Correspondence"
                };

                var result = await DownloadWith(root, id: 7, record: record);

                var objectResult = Assert.IsType<ObjectResult>(result);
                var problem = Assert.IsType<ProblemDetails>(objectResult.Value);

                Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
                Assert.Equal("File not found.", problem.Title);
            }
            finally { Cleanup(root); }
        }

        // =====================================================================================
        // N6 — an unauthenticated request is refused with 401
        // =====================================================================================

        /// <summary>
        /// Policy level, not end-to-end: every file endpoint sits under <c>[Authorize]</c> and
        /// nothing below it opts out with <c>[AllowAnonymous]</c>, so an unauthenticated request is
        /// refused by the authentication middleware with 401 before any action runs. An
        /// unauthenticated download endpoint over a per-tenant folder tree is a cross-tenant data
        /// leak, so the actions are checked individually as well as the class.
        /// </summary>
        [Theory]
        [InlineData(typeof(FilesController))]
        [InlineData(typeof(CurrencyExcelController))]
        public void N6_every_file_endpoint_requires_authentication(Type controller)
        {
            Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
            Assert.Null(controller.GetCustomAttribute<AllowAnonymousAttribute>());

            foreach (var action in Actions(controller))
                Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>());
        }

        // =====================================================================================
        // N7 — an authenticated request without the screen right is refused with 403
        // =====================================================================================

        /// <summary>
        /// Policy level, not end-to-end: each controller carries <c>[RequireScreen]</c> and every
        /// action carries a <c>[RequireRight]</c>.
        /// <see cref="ScreenRightAuthorizationFilterTests"/> proves the filter those attributes
        /// drive answers 403 when the right is absent; this proves the file endpoints are actually
        /// wired into it. Writes require <c>Create</c>, reads require <c>View</c>.
        /// </summary>
        [Theory]
        [InlineData(typeof(FilesController), "Correspondences")]
        [InlineData(typeof(CurrencyExcelController), "Currency")]
        public void N7_every_file_endpoint_is_gated_by_a_screen_right(Type controller, string expectedScreen)
        {
            var screen = controller.GetCustomAttribute<RequireScreenAttribute>();
            Assert.NotNull(screen);
            Assert.Equal(expectedScreen, screen!.ScreenName);

            var actions = Actions(controller).ToList();
            Assert.NotEmpty(actions);

            foreach (var action in actions)
            {
                var right = action.GetCustomAttribute<RequireRightAttribute>();
                Assert.True(right is not null, $"{controller.Name}.{action.Name} carries no [RequireRight].");

                var writes = action.GetCustomAttribute<HttpPostAttribute>() is not null;
                Assert.Equal(writes ? Right.Create : Right.View, right!.Right);
            }
        }

        // =====================================================================================
        // Round trip — a file uploaded through the API is byte-identical when read back
        // =====================================================================================

        /// <summary>
        /// The positive control for the whole task, and the direct proof that
        /// <c>ApiFileUploadService</c> does not reproduce the <c>WebFileUploadService.cs:102</c>
        /// zero-byte defect: the bytes on disk are the bytes that went in, and the folder layout is
        /// the one <c>WebFileUploadService.cs:86-93</c> produces.
        /// </summary>
        [Fact]
        public async Task RoundTrip_stored_bytes_are_identical_and_the_layout_matches_blazor()
        {
            var root = NewTempRoot();
            try
            {
                var original = new byte[64 * 1024];
                new Random(20260821).NextBytes(original);

                var service = NewUploadService(root, new FileStorageOptions { Root = root }, hostname: "Acme Ltd");

                using var source = new MemoryStream(original, writable: false);
                var (relativePath, fullPath) = await service.SaveCorrespondenceFileAsync(
                    source, "quarterly report.pdf", refType: "Purchase Order", docType: "Correspondence");

                // The bytes survived. This is the assertion WebFileUploadService would fail.
                var stored = await File.ReadAllBytesAsync(fullPath);
                Assert.NotEmpty(stored);
                Assert.Equal(original.Length, stored.Length);
                Assert.Equal(original, stored);

                // The layout is Blazor's: uploads/correspondences/{safeCompany}/{safeRefType}/{guid}_{name}
                Assert.StartsWith("/uploads/correspondences/acme ltd/purchase order/", relativePath);
                Assert.EndsWith("_quarterly report.pdf", relativePath);
                Assert.DoesNotContain("\\", relativePath);

                // And it is inside the root, so the download guard will serve it.
                Assert.Equal(Path.GetFullPath(fullPath), UploadPaths.ResolveStoredPath(root, relativePath));
            }
            finally { Cleanup(root); }
        }

        /// <summary>
        /// The Blazor-only entry point must not be reachable from the API host: an
        /// <c>IBrowserFile</c> cannot exist in an HTTP request, and silently accepting one would
        /// hide a wiring mistake.
        /// </summary>
        [Fact]
        public async Task The_IBrowserFile_overload_is_refused_by_the_api_implementation()
        {
            var root = NewTempRoot();
            try
            {
                var service = NewUploadService(root, new FileStorageOptions { Root = root });

                await Assert.ThrowsAsync<NotSupportedException>(
                    () => service.SaveCorresFileAsync(null!, "ref", "doc"));
            }
            finally { Cleanup(root); }
        }

        // =====================================================================================
        // Helpers
        // =====================================================================================

        private static IEnumerable<MethodInfo> Actions(Type controller)
            => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);

        private static string NewTempRoot()
        {
            var path = Path.Combine(Path.GetTempPath(), "vsmart-m2b06-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void Cleanup(string root)
        {
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch (IOException)
            {
                // A temp directory that will not delete must not fail an otherwise passing test.
            }
        }

        private static ApiFileUploadService NewUploadService(
            string root, FileStorageOptions options, string hostname = "Acme Ltd")
        {
            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.WebRootPath).Returns(root);

            var tenants = new Mock<ITenantProvider>();
            tenants
                .Setup(t => t.GetCurrentTenant())
                .Returns(new TenantInfo
                {
                    Id = 1,
                    Name = hostname,
                    Hostname = hostname,
                    ConnectionString = "not-used-by-these-tests"
                });

            return new ApiFileUploadService(
                env.Object, Mock.Of<ILoggingService>(), tenants.Object, Options.Create(options));
        }

        private static FilesController NewController(
            string root, FileStorageOptions options, IUnitOfWork unitOfWork)
            => new FilesController(NewUploadService(root, options), unitOfWork, Options.Create(options))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = ErrorContractTestContext.Create("/api/v1/files", "POST")
                }
            };

        /// <summary>Runs Download against a repository that returns <paramref name="record"/>.</summary>
        private static async Task<IActionResult> DownloadWith(string root, int id, Correspondence? record)
        {
            var repository = new Mock<ICorrespondanceRepository>();
            repository.Setup(r => r.GetAsync(id)).ReturnsAsync(record!);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.Correspondances).Returns(repository.Object);

            var controller = NewController(root, new FileStorageOptions { Root = root }, unitOfWork.Object);
            controller.ControllerContext.HttpContext =
                ErrorContractTestContext.Create($"/api/v1/files/{id}", "GET");

            return await controller.Download(id);
        }

        /// <summary>A minimal <see cref="IFormFile"/> over a byte array.</summary>
        private sealed class FakeFormFile : IFormFile
        {
            private readonly byte[] _content;

            public FakeFormFile(string fileName, byte[] content)
            {
                FileName = fileName;
                _content = content;
            }

            public string ContentType { get; set; } = "application/octet-stream";

            public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{FileName}\"";

            public IHeaderDictionary Headers { get; } = new HeaderDictionary();

            public long Length => _content.LongLength;

            public string Name => "file";

            public string FileName { get; }

            public void CopyTo(Stream target) => target.Write(_content, 0, _content.Length);

            public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
                => target.WriteAsync(_content, 0, _content.Length, cancellationToken);

            public Stream OpenReadStream() => new MemoryStream(_content, writable: false);
        }
    }
}
