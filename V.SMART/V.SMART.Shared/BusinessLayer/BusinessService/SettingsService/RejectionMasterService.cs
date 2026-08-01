using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.Data.Master.Rejection_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IMasterSettings;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SettingsService
{
    public class RejectionMasterService : IRejectionMasterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;

        public RejectionMasterService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            ICommonService commonService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
            _mapper = mapper;
        }
     
        public async Task<List<RejectionMasterVM>> GetAllAsync()
        {
            var entities = _unitOfWork.RejectionMasters.GetQueryable()
                        .OrderBy(x => x.RejectionId)
                        .ToList();

            return _mapper.Map<List<RejectionMasterVM>>(entities);
        }

        public async Task<RejectionMasterVM?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.RejectionMasters
                .FirstOrDefaultAsync(x => x.RejectionId == id);

            if (entity == null)
                return null;

            return _mapper.Map<RejectionMasterVM>(entity);
        }

        public async Task<RejectionMasterVM> Upsertrejection(RejectionMasterVM RejectionMasterVMs)
        {
            if (RejectionMasterVMs == null)
                throw new ArgumentNullException(nameof(RejectionMasterVMs));

            var now = DateTime.Now;

            var currentUser =
                await _currentUserService.GetUsernameAsync();

            var changes = new StringBuilder();

            using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                RejectionMaster entity;


                if (RejectionMasterVMs.RejectionId == 0)
                {
               

                    var exists = await _unitOfWork.RejectionMasters
                        .AnyAsync(x =>
                            x.RejectionCode ==
                            RejectionMasterVMs.RejectionCode);

                    if (exists)
                    {
                        throw new Exception(
                            "Rejection Code already exists.");
                    }

                    entity = _mapper.Map<RejectionMaster>(
                        RejectionMasterVMs);

                    entity.CreatedBy = currentUser;

                    entity.CreatedDate = now;

                    entity.IsActive = true;

                    await _unitOfWork.RejectionMasters
                        .CreateAsync(entity);

                    changes.AppendLine(
                        "Rejection Created.");
                }


                else
                {

                    if (await IsCodeExistsAsync(
                        RejectionMasterVMs.RejectionCode,
                        RejectionMasterVMs.RejectionId))
                    {
                        throw new InvalidOperationException(
                            $"Rejection code '{RejectionMasterVMs.RejectionCode}' already exists.");
                    }

                    entity = await _unitOfWork.RejectionMasters
                        .GetQueryable()
                        .FirstOrDefaultAsync(x =>
                            x.RejectionId ==
                            RejectionMasterVMs.RejectionId)

                        ?? throw new InvalidOperationException(
                            "Rejection not found.");

                    var exists = await _unitOfWork.RejectionMasters
                        .AnyAsync(x =>
                            x.RejectionCode ==  RejectionMasterVMs.RejectionCode &&   x.RejectionId != RejectionMasterVMs.RejectionId
                         );

                    if (exists)
                    {
                        throw new Exception(
                            "Rejection Code already exists.");
                    }


                    entity.RejectionCode =
                        RejectionMasterVMs.RejectionCode;

                    entity.RejectionDescription =
                        RejectionMasterVMs
                            .RejectionDescription;

                    entity.IsActive =
                        RejectionMasterVMs.IsActive;

                    entity.ModifiedBy = currentUser;

                    entity.ModifiedDate = now;

                    await _unitOfWork.RejectionMasters
                        .UpdateAsync(entity);

                    changes.AppendLine(
                        "Rejection Updated.");
                }
                await _unitOfWork.SaveAsync();


                await transaction.CommitAsync();

                await LogChangesAsync(
                    changes,
                    RejectionMasterVMs.RejectionId == 0
                        ? "Rejection Created"
                        : "Rejection Updated");


                var savedEntity = await _unitOfWork
                    .RejectionMasters
                    .GetQueryable()
                    .FirstOrDefaultAsync(x =>
                        x.RejectionId ==
                        entity.RejectionId);

                return _mapper.Map<RejectionMasterVM>(
                    savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggingService.LogDeveloperError(
                    ex,
                    $"Failed to save Rejection : " +
                    $"{RejectionMasterVMs.RejectionCode}");

                throw new InvalidOperationException(
                    "Failed to save Rejection.");
            }
        }
      
        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _loggingService.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Item",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<RejectionMasterVM?> UpdateAsync(RejectionMasterVM model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var now = DateTime.Now;

            var currentUser =
                await _currentUserService.GetUsernameAsync();

            var changes = new StringBuilder();

            using var transaction =   await _unitOfWork.BeginTransactionAsync();
            try
            {
                var entity = await _unitOfWork.RejectionMasters
               .FirstOrDefaultAsync(
                   x => x.RejectionId == model.RejectionId);

                if (entity == null)
                    return null;

                if (string.IsNullOrWhiteSpace(model.RejectionCode))
                {
                    throw new Exception("Rejection Code is required.");
                }

                model.RejectionCode = model.RejectionCode.Trim();

                // DUPLICATE CHECK

                if (await IsCodeExistsAsync(
                    model.RejectionCode,
                    model.RejectionId))
                {
                    throw new InvalidOperationException(
                        $"Rejection code '{model.RejectionCode}' already exists.");
                }

                // UPDATE

                entity.RejectionCode = model.RejectionCode;

                entity.RejectionDescription =
                    model.RejectionDescription;

                entity.IsActive = model.IsActive;

                entity.ModifiedDate = DateTime.Now;

                entity.ModifiedBy = "Admin";

                await _unitOfWork.SaveAsync();

                return _mapper.Map<RejectionMasterVM>(entity);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggingService.LogDeveloperError(ex, $"Failed to save Rejection : " + $"{model.RejectionCode}");

                throw new InvalidOperationException("Failed to save Rejection.");
            }


           
        }
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _unitOfWork.RejectionMasters
                 .FirstOrDefaultAsync(x => x.RejectionId == id);

                    if (entity == null)
                        return false;

                    await _unitOfWork.RejectionMasters.DeleteAsync(entity);

                    await _unitOfWork.SaveAsync();

                return true;
            }
            catch (Exception)
            {

                throw;
            }
         
        }
        public async Task<string?> CheckRejectionUsageAsync(int rejectionId)
        {
            try
            {
                if (await _unitOfWork.PurchaseSCNSubs.AnyAsync(x => x.RejectionId == rejectionId))
                {
                    return "Purchase SCN";
                }
                 
                if (await _unitOfWork.LabourSCNSubs.AnyAsync(x => x.RejectionId == rejectionId))
                {
                    return "Labour SCN";
                }
                
                if (await _unitOfWork.SubConSCNSubs.AnyAsync(x => x.RejectionId == rejectionId))
                {
                    return "SubContract SCN";
                }
                
                if (await _unitOfWork.ProductionSCNAssySubs.AnyAsync(x => x.RejectionId == rejectionId))
                {
                    return "Production Assembly SCN";
                }

                if (await _unitOfWork.ProductionSCNCompSubs.AnyAsync(x => x.RejectionId == rejectionId))
                {
                    return "Production Componet SCN";
                }
                   
                return null;

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to CheckRejectionUsageAsync : " + $"{rejectionId}");
                return null;
            }
        }


        public async Task<bool> IsCodeExistsAsync(string code,int? excludeId = null)
        {
            var query = _unitOfWork.RejectionMasters.GetQueryable()
                .Where(x => x.RejectionCode == code);

            if (excludeId.HasValue)
            {
                query = query.Where(
                    x => x.RejectionId != excludeId.Value);
            }

            return query.Any();
        }


    }
}
