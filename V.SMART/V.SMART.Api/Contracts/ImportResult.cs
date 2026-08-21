namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// The body of a <c>POST api/v1/{resource}/import</c> (M2-B06). An import is not a
    /// success/failure: it is a per-row report, because a spreadsheet with one bad row must not
    /// discard the other 499 and must not silently accept them either.
    /// </summary>
    /// <param name="TotalRows">Data rows found in the sheet, excluding the header.</param>
    /// <param name="Accepted">Rows the business service created.</param>
    /// <param name="Rejected">Rows refused, each with a reason in <paramref name="Errors"/>.</param>
    /// <param name="Errors">One entry per rejected row, in sheet order.</param>
    public sealed record ImportResult(
        int TotalRows,
        int Accepted,
        int Rejected,
        IReadOnlyList<ImportRowError> Errors);

    /// <summary>
    /// One rejected row. <paramref name="Message"/> is the business service's own refusal message
    /// verbatim where there is one (BR-SO-001), or the DataAnnotations message from the resource's
    /// ViewModel — never a reworded API string.
    /// </summary>
    /// <param name="Row">1-based row number as it appears in the spreadsheet, header included.</param>
    /// <param name="Message">Why the row was refused.</param>
    public sealed record ImportRowError(int Row, string Message);
}
