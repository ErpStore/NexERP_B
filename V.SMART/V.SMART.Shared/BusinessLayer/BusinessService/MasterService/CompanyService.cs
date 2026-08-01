using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CurrentUserService _userService;
        private readonly ILoggingService _loggingService;
        private readonly IFileUploadService _fileUploadService;
        private readonly HttpClient _httpClient;

        public CompanyService(IUnitOfWork unitOfWork, CurrentUserService userService, ILoggingService loggingService, IFileUploadService fileUploadService, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _loggingService = loggingService;
            _fileUploadService = fileUploadService;
            _httpClient = httpClient;
        }

        public async Task<string?> GetCompanyLogoUrlAsync()
        {
            var logo = await _unitOfWork.Companies.GetQueryable()
                                        .Select(c => c.LogoUrl)
                                        .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(logo) ? null : logo;
        }


        // New methods to load data
        public async Task<Companydetails> GetCompanyDetailsAsync()
        {
            return (await _unitOfWork.Companies.GetAllAsync()).FirstOrDefault();
        }

        // Method to handle all business logic for saving/updating a company
        public async Task<(bool success, string message)> UpsertCompanyAsync(Companydetails company)
        {
            try
            {
                var isDuplicate = await _unitOfWork.Companies.ExistsByNameAsync("CompanyName", company.CompanyName, "CompanyId", company.CompanyId == 0 ? null : company.CompanyId);

                if (isDuplicate)
                {
                    await _loggingService.LogUserAction(
                        UserName: await _userService.GetUsernameAsync(),
                        Machine: _userService.MachineName,
                        IP_Address: _userService.IpAddress,
                        screen: "Company",
                        action: $"Duplicate company attempted: {company.CompanyName}",
                        additionalInfo: $"CompanyName: {company.CompanyName}");
                    return (false, "Company Name already exists.");
                }

                if (company.CompanyId == 0)
                {
                    company.CreatedBy = await _userService.GetUsernameAsync();
                    company.CreatedDate = DateTime.Now;
                    await _unitOfWork.Companies.CreateAsync(company);
                    await _loggingService.LogUserAction(
                        UserName: await _userService.GetUsernameAsync(),
                        Machine: _userService.MachineName,
                        IP_Address: _userService.IpAddress,
                        screen: "Company",
                        action: $"Created new Company: {company.CompanyName}");
                }
                else
                {
                    company.ModifiedBy = await _userService.GetUsernameAsync();
                    company.ModifiedDate = DateTime.Now;
                    await _unitOfWork.Companies.UpdateAsync(company);
                    await _loggingService.LogUserAction(
                        UserName: await _userService.GetUsernameAsync(),
                        Machine: _userService.MachineName,
                        IP_Address: _userService.IpAddress,
                        screen: "Company",
                        action: $"Updated Company: {company.CompanyName}");
                }

                await _unitOfWork.SaveAsync();
                var successMessage = company.CompanyId == 0 ? "Company Created Successfully" : "Company Updated Successfully";
                return (true, successMessage);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error occurred during UpsertCompany.");
                return (false, "An unexpected error occurred.");
            }
        }

        // Method to fetch bank details from external API
        public async Task<BankDetailsResponse> GetBankDetailsByIFSCAsync(string ifscCode)
        {
            if (string.IsNullOrWhiteSpace(ifscCode))
                return null;

            try
            {
                var response = await _httpClient.GetAsync($"https://ifsc.razorpay.com/{ifscCode}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<BankDetailsResponse>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Failed to fetch bank details by IFSC.");
                return null;
            }
        }

        // Method to handle file uploads
        public async Task<(bool success, string filePath, string fileUrl)> UploadFileAsync(IBrowserFile file, string target, long maxFileSize)
        {
            try
            {
                if (file.Size > maxFileSize)
                {
                    return (false, $"File size is too large. Max size is {maxFileSize / 1024} KB.", null);
                }

                using var stream = file.OpenReadStream(maxAllowedSize: maxFileSize);
                var result = await _fileUploadService.SaveFileAsync(file.Name, stream, target);
                var parts = result.Split('|');

                if (parts.Length < 2)
                {
                    return (false, "Invalid file upload response.", null);
                }

                return (true, parts[1], parts[0]); // Returns success, path, and URL
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Failed to upload file.");
                return (false, "Failed to upload file due to an error.", null);
            }
        }

        // Method to delete files
        public async Task<(bool success, string message)> DeleteFileAsync(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    await _fileUploadService.DeleteFileAsync(path);
                    return (true, "Logo removed successfully.");
                }
                return (false, "Path is null or empty.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Failed to delete file.");
                return (false, "Failed to delete file.");
            }
        }

        public async Task<(bool success, string message)> UpdateEwayUserAsync(Companydetails company)
        {
            try
            {
                await _unitOfWork.Companies.UpdateAsync(company);
                await _unitOfWork.SaveAsync();
                var successMessage = company.CompanyId == 0 ? "Company Created Successfully" : "Company Updated Successfully";
                return (true, successMessage);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
