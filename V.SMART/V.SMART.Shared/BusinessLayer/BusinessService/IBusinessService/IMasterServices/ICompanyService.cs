using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels;
using Microsoft.AspNetCore.Components.Forms;
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
        Task<(bool success, string filePath, string fileUrl)> UploadFileAsync(IBrowserFile file, string target, long maxFileSize);
        Task<(bool success, string message)> DeleteFileAsync(string path);

        Task<(bool success, string message)> UpdateEwayUserAsync(Companydetails company);
    }
}
