namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// The body of a successful <c>POST api/v1/files</c> (M2-B06).
    ///
    /// <para><see cref="Id"/> is the <c>Correspondence.Id</c> the file was recorded under, and is
    /// the only thing <c>GET api/v1/files/{id}</c> accepts. It is deliberately <b>not</b> a path:
    /// an identifier a caller cannot turn into a directory traversal is the strongest form of the
    /// guarantee, and it also carries tenant isolation for free, because the row lives in the
    /// tenant database resolved from the JWT.</para>
    ///
    /// <para><see cref="RelativePath"/> is the web-relative location on disk, in the same shape
    /// <c>WebFileUploadService.cs:104</c> returns, so a record written through the API and one
    /// written through Blazor are indistinguishable to every existing reader of
    /// <c>Correspondence.FilePath</c>.</para>
    /// </summary>
    /// <param name="Id">The stored file's identifier — the <c>{id}</c> of the download endpoint.</param>
    /// <param name="FileName">The original client file name, unmodified.</param>
    /// <param name="RelativePath">Web-relative path of the stored file.</param>
    /// <param name="ContentType">The content type the API resolved from the extension and will serve.</param>
    /// <param name="Size">Size in bytes, as stored.</param>
    public sealed record FileUploadResponse(
        int Id,
        string FileName,
        string RelativePath,
        string ContentType,
        long Size);
}
