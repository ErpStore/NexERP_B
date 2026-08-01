using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InspectionService
{
    public class DefectInfoService: IDefectInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;

        public DefectInfoService(IUnitOfWork unitOfWork, ILoggingService loggingService, CurrentUserService currentUserService, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
        }

        // Get All Active Defects
        public async Task<List<DefectInfo>> GetAllActiveDefectsAsync()
            => await _commonService.GetAllActiveDefectsAsync();


        #region Saving Method
        public async Task<string> UpsertDefect(DefectInfo DefectInfos)
        {
            try
            { 
                // Duplicate Defect Name check
                bool isDuplicate = await _unitOfWork.DefectInfos
                    .ExistsByNameAsync(
                        "DefectName",
                        DefectInfos.DefectName!,
                        "DefectCode",
                        DefectInfos.DefectCode == 0 ? null : DefectInfos.DefectCode
                    );

                if (isDuplicate)
                {
                    await _loggingService.LogUserAction(
                        UserName: await _currentUserService.GetUsernameAsync(),
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Defect",
                        action: $"Duplicate Defect creation attempt: {DefectInfos.DefectName}",
                        additionalInfo: $"DefectName: {DefectInfos.DefectName}"
                    );

                    
                    return "Defect Already Exits";
                }

                if (DefectInfos.DefectCode == 0)
                {
                    // CREATE
                    DefectInfos.CreatedBy = await _currentUserService.GetUsernameAsync();
                    DefectInfos.CreatedDate = DateTime.Now;

                    await _unitOfWork.DefectInfos.CreateAsync(DefectInfos);
                    await _unitOfWork.SaveAsync();


                    await _loggingService.LogUserAction(
                        UserName: DefectInfos.CreatedBy,
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Defect",
                        action: $"Created Defect: {DefectInfos.DefectName}",
                        additionalInfo: $"DefectNo: {DefectInfos.DefectNo}"
                    );
                    return "Defect created successfully";
                }
                else
                {
                    // UPDATE
                    DefectInfos.ModifiedBy = await _currentUserService.GetUsernameAsync();
                    DefectInfos.ModifiedDate = DateTime.Now;

                    await _unitOfWork.DefectInfos.UpdateAsync(DefectInfos);
                    await _unitOfWork.SaveAsync();
                    await _loggingService.LogUserAction(
                        UserName: DefectInfos.ModifiedBy,
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Defect",
                        action: $"Updated Defect: {DefectInfos.DefectName}",
                        additionalInfo: $"DefectCode: {DefectInfos.DefectCode}"
                    );
                    return "Defect updated successfully";
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,"Error in UpsertDefect() - Defect");
                return "Error occurred while processing the Defect";
            }
        }
        #endregion

        #region Delete Method
        public async Task<bool> DeleteDefect(int DefectCode)
        {
            try
            {
                var defect = await _unitOfWork.DefectInfos.GetAsync(DefectCode);
                if (defect == null)
                    return false;
                var deleted = await _unitOfWork.DefectInfos.DeleteAsync(DefectCode);
                if (deleted)
                {
                    await _unitOfWork.SaveAsync();
                    await _loggingService.LogUserAction(
                      UserName: await _currentUserService.GetUsernameAsync(),
                      Machine: _currentUserService.MachineName,
                      IP_Address: _currentUserService.IpAddress,
                      screen: "DefectList",
                      action: $"Deleted Defect: {defect.DefectName}",
                      additionalInfo: $"DefectCode: {defect.DefectCode}"
                    );
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error deleting Defect");
                return false;
            }
        }
        #endregion

    }
}
