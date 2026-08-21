namespace V.SMART.Api.Services
{
    /// <summary>
    /// Where this host writes uploaded files, and how large an upload it accepts (M2-B06).
    ///
    /// <para><b>Why a configured root instead of <c>IWebHostEnvironment.WebRootPath</c> alone.</b>
    /// <c>WebFileUploadService</c> writes under <c>_env.WebRootPath ?? "wwwroot"</c>
    /// (<c>V.SMART/V.SMART.Web/Services/WebFileUploadService.cs:39,83,166</c>). Resolving the same
    /// expression in this host resolves to <c>V.SMART/V.SMART.Api/wwwroot</c>, which is a
    /// <i>different directory</i> from the Blazor host's <c>V.SMART/V.SMART.Web/wwwroot</c>
    /// (Confirmed 2026-08-21: the API's wwwroot contains only <c>config/tenant.json</c>; the live
    /// <c>uploads/</c> tree is under the Web host). Mirroring the folder logic without a shared
    /// root would therefore produce a second, disjoint store, and M2-B06's requirement that
    /// "either host can read the other's files" could not hold. The root is configuration, so a
    /// deployment that wants one store points both hosts at it:
    /// <code>"FileStorage": { "Root": "C:\vsmart\uploads-root" }</code>
    /// When <c>Root</c> is unset the behaviour is byte-identical to <c>WebFileUploadService</c>'s
    /// — <c>WebRootPath ?? "wwwroot"</c> — so nothing changes implicitly.</para>
    ///
    /// <para>Whether either root is durable in the target deployment is <b>Unknown</b> and is
    /// tracked as Q-16 in <c>docs/kb/open-questions.md</c>. This task deliberately designs no
    /// blob-storage migration.</para>
    /// </summary>
    public sealed class FileStorageOptions
    {
        /// <summary>The configuration section these options are bound from.</summary>
        public const string SectionName = "FileStorage";

        /// <summary>
        /// The default maximum accepted upload, in bytes. 20 MB — the same ceiling
        /// <c>WebFileUploadService.cs:101</c> passes to <c>IBrowserFile.OpenReadStream</c>, so the
        /// HTTP path is no more permissive than the Blazor path.
        /// </summary>
        public const long DefaultMaxUploadBytes = 20 * 1024 * 1024;

        /// <summary>
        /// Absolute or relative path of the uploads root. <c>null</c> or whitespace means
        /// "<c>WebRootPath ?? wwwroot</c>", exactly as the Blazor host resolves it.
        /// </summary>
        public string? Root { get; set; }

        /// <summary>Maximum accepted upload in bytes. Defaults to <see cref="DefaultMaxUploadBytes"/>.</summary>
        public long MaxUploadBytes { get; set; } = DefaultMaxUploadBytes;
    }
}
