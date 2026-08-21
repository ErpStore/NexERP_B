namespace V.SMART.Api.Logging
{
    /// <summary>
    /// M2-B11 — the file-sink half of the logging configuration, bound from the
    /// <c>Observability:Logging</c> section of <c>appsettings.json</c>.
    /// <para>
    /// It is separate from Serilog's own <c>Serilog</c> configuration section (which
    /// <c>ReadFrom.Configuration</c> consumes, and which a deployment uses to add a second
    /// sink) because these three values encode a <b>policy decision</b>, not a sink choice:
    /// the audit stream and the diagnostic stream have deliberately different retention, and
    /// R-23 requires the audit retention to be independent of, and longer than, the diagnostic
    /// one. Putting them here makes that inequality visible and reviewable rather than buried
    /// in two sink argument lists.
    /// </para>
    /// </summary>
    public sealed class AuditLoggingOptions
    {
        public const string SectionName = "Observability:Logging";

        /// <summary>
        /// Root directory for the rolling log files. Defaults to <c>App_Data/Logs</c> under the
        /// content root, which matches where <c>FileLoggingService</c> wrote
        /// (<c>FileLoggingService.cs:14</c>) so nothing has to move on day one.
        /// <para>
        /// <b>Set this to a mounted volume in any container deployment.</b> "Lost on container
        /// restart" is one of R-23's four recorded impacts and it is the one this task does not
        /// fix in code — it cannot; it is a deployment property. The default keeps behaviour
        /// identical to today, which is honest, not adequate.
        /// </para>
        /// </summary>
        public string? Directory { get; set; }

        /// <summary>
        /// Days of <b>audit</b> (<c>EventType = UserAction</c>) retention. Default 3650 — ten
        /// years. An ERP audit trail is a business record, not a rolling diagnostic buffer, and
        /// its retention question is a statutory one; ten years is chosen as the longest span
        /// any of the retention periods commonly cited for accounting records, so the default
        /// errs toward keeping. Shorten it only against a written retention policy.
        /// </summary>
        public int AuditRetentionDays { get; set; } = 3650;

        /// <summary>
        /// Days of <b>diagnostic</b> retention. Default 14 — long enough to investigate an
        /// incident from the following Monday, short enough to bound growth. It must stay
        /// strictly below <see cref="AuditRetentionDays"/>; startup fails if it does not (see
        /// <c>Program.cs</c>).
        /// </summary>
        public int DiagnosticRetentionDays { get; set; } = 14;

        /// <summary>Size cap per file before it rolls, in bytes. Default 64 MB.</summary>
        public long FileSizeLimitBytes { get; set; } = 64L * 1024 * 1024;
    }
}
