namespace V.SMART.Api.Services
{
    /// <summary>
    /// The pure path rules shared by <see cref="ApiFileUploadService"/> and the download endpoint
    /// (M2-B06). Static and dependency-free so the traversal guard can be tested without a host.
    ///
    /// <para>Every rule here is a transcription of
    /// <c>V.SMART/V.SMART.Web/Services/WebFileUploadService.cs</c>, cited line by line, so that a
    /// file written through the API is indistinguishable from one written through Blazor.</para>
    /// </summary>
    public static class UploadPaths
    {
        /// <summary>
        /// <c>WebFileUploadService.cs:37</c>, <c>:80-81</c>, <c>:163-164</c> — strip every
        /// <see cref="Path.GetInvalidFileNameChars"/> character and lowercase. Reproduced exactly,
        /// including the fact that it is <b>not</b> a traversal guard on its own: it removes no
        /// dot, so "<c>..</c>" survives it. Traversal is prevented structurally instead — by the
        /// <see cref="Guid"/> prefix on the file name and by <see cref="IsInsideRoot"/>.
        /// </summary>
        public static string Sanitise(string? value)
            => string.Concat((value ?? string.Empty).Where(c => !Path.GetInvalidFileNameChars().Contains(c))).ToLower();

        /// <summary>
        /// <c>WebFileUploadService.cs:40-44</c> — the logo/iso-logo pair. <c>"logo"</c> (trimmed,
        /// lowercased) is the only recognised value; everything else, <c>null</c> included, is the
        /// ISO logo folder.
        /// </summary>
        public static string LogoBaseFolder(string? target) => target?.Trim().ToLower() switch
        {
            "logo" => "uploads/Logos",
            _ => "uploads/IsoLogos"
        };

        /// <summary>
        /// <c>WebFileUploadService.cs:86-90</c> and <c>:178-182</c> — the drawing/correspondence
        /// pair, with the same "anything that is not <c>drawing</c>" default.
        /// </summary>
        public static string DocumentBaseFolder(string? docType) => docType?.Trim().ToLower() switch
        {
            "drawing" => "uploads/drawings",
            _ => "uploads/correspondences"
        };

        /// <summary>
        /// <c>WebFileUploadService.cs:78</c>, <c>:160</c> — an empty reference type becomes
        /// <c>ManualUpload</c> before sanitisation.
        /// </summary>
        public static string RefTypeOrDefault(string? refType)
            => string.IsNullOrWhiteSpace(refType) ? "ManualUpload" : refType.Trim();

        /// <summary>
        /// <c>WebFileUploadService.cs:77</c>, <c>:159</c> — an empty tenant host name becomes
        /// <c>DefaultCompany</c> before sanitisation.
        /// </summary>
        public static string CompanyOrDefault(string? hostname)
            => string.IsNullOrWhiteSpace(hostname) ? "DefaultCompany" : hostname.Trim();

        /// <summary>
        /// <c>WebFileUploadService.cs:84</c>, <c>:174-175</c> — <c>{guid}_{originalName}</c>. The
        /// GUID prefix is what prevents collisions and what stops a user-supplied name from being
        /// used verbatim; <see cref="Path.GetFileName(string)"/> additionally drops any directory
        /// part a client tries to smuggle in ("<c>../../evil.exe</c>" becomes "<c>evil.exe</c>").
        /// </summary>
        public static string UniqueFileName(string fileName)
            => $"{Guid.NewGuid()}_{Path.GetFileName(fileName ?? string.Empty)}";

        /// <summary>
        /// The download-side traversal guard (M2-B06 security requirement). Answers whether
        /// <paramref name="candidate"/> — after full canonicalisation — really sits inside
        /// <paramref name="root"/>. A stored path containing <c>..</c>, an absolute path outside
        /// the root, or a URL-decoded traversal all fail here, so a poisoned
        /// <c>Correspondence.FilePath</c> row cannot be turned into an arbitrary file read.
        /// </summary>
        public static bool IsInsideRoot(string root, string candidate)
        {
            if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(candidate))
                return false;

            var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
            string fullCandidate;
            try
            {
                fullCandidate = Path.GetFullPath(candidate);
            }
            catch (Exception)
            {
                // A malformed path (illegal characters, a bare volume, an over-long name) is not
                // inside the root by definition. Fail closed rather than propagate.
                return false;
            }

            return fullCandidate.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(fullCandidate, fullRoot, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Resolves a value stored in <c>Correspondence.FilePath</c> to an absolute path under
        /// <paramref name="root"/>, or <c>null</c> if it escapes the root.
        ///
        /// <para>Both shapes seen in the data are accepted, because both are written today: the
        /// web-relative form "<c>/uploads/correspondences/…</c>" returned by
        /// <c>WebFileUploadService.cs:104</c>, and an absolute local path (the <c>FullPath</c>
        /// half of the pipe composite at <c>:57</c>). <c>DeleteFileAsync</c> in the Web
        /// implementation discriminates them the same way (<c>:126-135</c>).</para>
        /// </summary>
        public static string? ResolveStoredPath(string root, string? storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                return null;

            string candidate;
            if (storedPath.Contains("uploads/", StringComparison.OrdinalIgnoreCase)
                && !storedPath.Contains(":\\") && !storedPath.Contains(":/"))
            {
                var relative = storedPath
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
                    .Replace("\\", Path.DirectorySeparatorChar.ToString())
                    .TrimStart(Path.DirectorySeparatorChar);
                candidate = Path.Combine(root, relative);
            }
            else
            {
                candidate = storedPath;
            }

            return IsInsideRoot(root, candidate) ? Path.GetFullPath(candidate) : null;
        }
    }
}
