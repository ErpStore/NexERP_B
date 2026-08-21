namespace V.SMART.Api.Services
{
    /// <summary>
    /// The upload allow-list (M2-B06). An <b>allow</b>-list, never a deny-list: a deny-list is a
    /// list of the attacks someone has already thought of.
    ///
    /// <para><b>Provenance.</b> The 24 extensions are copied verbatim from the only extension
    /// check that exists anywhere in the product today —
    /// <c>V.SMART/V.SMART.Shared/Pages/Utilities_Module_pages/Correspondance_pages/CorrespondenceUpload.razor:213-220</c>,
    /// enforced at <c>:284</c>. That check is inside a Razor page, so by this migration's own
    /// definition it is a <i>client-side</i> check with no server-side equivalent; moving the same
    /// list behind the endpoint is what makes the server authoritative for it. No extension was
    /// added and none was removed, so nothing a user can upload through Blazor today is refused by
    /// the API, and nothing new is permitted. (<c>.txt</c> appears twice in the source list; a set
    /// collapses it.)</para>
    ///
    /// <para><b>Content type is validated, not trusted.</b> The browser-supplied
    /// <c>Content-Type</c> is only consulted after the extension has been allowed, and the value
    /// stored and later served is this table's own mapping — so a client cannot get
    /// <c>text/html</c> (or anything else it chooses) echoed back to a future downloader.</para>
    /// </summary>
    public static class UploadContentTypes
    {
        /// <summary>Served when the extension is allowed but has no specific mapping below.</summary>
        public const string Fallback = "application/octet-stream";

        /// <summary>
        /// Extension (lowercase, leading dot) to the content type this API will serve it as.
        /// Membership in this dictionary <i>is</i> the allow-list.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> Map =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".doc"] = "application/msword",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".xls"] = "application/vnd.ms-excel",
                [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                [".xlsm"] = "application/vnd.ms-excel.sheet.macroEnabled.12",
                [".ppt"] = "application/vnd.ms-powerpoint",
                [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                [".txt"] = "text/plain",
                [".csv"] = "text/csv",
                [".rpt"] = Fallback,
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
                [".png"] = "image/png",
                [".bmp"] = "image/bmp",
                [".gif"] = "image/gif",
                [".tiff"] = "image/tiff",
                [".zip"] = "application/zip",
                [".rar"] = "application/vnd.rar",
                [".eml"] = "message/rfc822",
                [".msg"] = "application/vnd.ms-outlook",
                [".dwg"] = Fallback,
                [".dxf"] = "image/vnd.dxf",
                [".svg"] = Fallback
            };

        /// <summary>The allowed extensions, lowercase and dotted — for error messages and tests.</summary>
        public static IReadOnlyCollection<string> AllowedExtensions { get; } = Map.Keys.ToArray();

        /// <summary>True when <paramref name="fileName"/>'s extension is on the allow-list.</summary>
        public static bool IsAllowed(string? fileName)
            => TryResolve(fileName, out _);

        /// <summary>
        /// Resolves the content type this API will store and serve for
        /// <paramref name="fileName"/>. False when the extension is not allowed.
        /// </summary>
        /// <remarks>
        /// <c>.svg</c> maps to <see cref="Fallback"/> rather than <c>image/svg+xml</c> on purpose:
        /// an SVG is a script-bearing document, and serving one as its native type from the same
        /// origin as the SPA would make an upload endpoint a stored-XSS vector. It stays uploadable
        /// because the Blazor list allows it; it is served as an opaque download.
        /// <c>.dwg</c>/<c>.rpt</c> have no registered IANA type in use here.
        /// </remarks>
        public static bool TryResolve(string? fileName, out string contentType)
        {
            contentType = Fallback;

            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
                return false;

            if (!Map.TryGetValue(extension, out var resolved))
                return false;

            contentType = resolved;
            return true;
        }
    }
}
