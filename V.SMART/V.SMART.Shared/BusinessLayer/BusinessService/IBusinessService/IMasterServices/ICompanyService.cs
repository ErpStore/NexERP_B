using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices
{
    public interface ICompanyService
    {
        Task<string?> GetCompanyLogoUrlAsync();
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<(bool success, string message)> UpsertCompanyAsync(Companydetails company);
        Task<BankDetailsResponse> GetBankDetailsByIFSCAsync(string ifscCode);
        /// <summary>
        /// Uploads a company logo. <b>M2-B06:</b> this took Blazor's <c>IBrowserFile</c> until
        /// 2026-08-21. A UI type in the business layer made the method uncallable from
        /// <c>V.SMART.Api</c>, where an HTTP request yields <c>IFormFile</c> and no adapter between
        /// the two shapes exists. It now takes the stream plus the two facts it actually used off
        /// the file - the name and the size - so that every host can call it.
        /// </summary>
        /// <param name="content">The file's bytes. <b>The caller owns this stream and disposes it</b>;
        /// this method reads it and does not close it. It is not read at all when the size check fails.</param>
        /// <param name="fileName">The original file name, used for its extension.</param>
        /// <param name="fileSize">Byte length, checked against <paramref name="maxFileSize"/> before a
        /// byte is read - the same order as the <c>IBrowserFile</c> version.</param>
        Task<(bool success, string filePath, string fileUrl)> UploadFileAsync(Stream content, string fileName, long fileSize, string target, long maxFileSize);
        Task<(bool success, string message)> DeleteFileAsync(string path);

        Task<(bool success, string message)> UpdateEwayUserAsync(Companydetails company);
    }
}
