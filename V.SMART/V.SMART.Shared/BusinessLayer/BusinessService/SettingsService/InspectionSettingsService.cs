using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SettingsService
{
    public class InspectionSettingsService:IInspectionSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;

        public InspectionSettingsService(
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

        #region Save Inspection SettingsAsync
        public async Task SaveInspectionSettingsAsync(IEnumerable<InspectionSettings> settings)
        {
            if (settings == null || !settings.Any())
                throw new ArgumentException("No inspection settings to save.", nameof(settings));

            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var setting in settings)
                {
                    // ✅ Validation (Hard Rule)
                    if (setting.SamplesPerSheet != 10 && setting.SamplesPerSheet != 15)
                    {
                        throw new InvalidOperationException(
                            $"Invalid SamplesPerSheet value ({setting.SamplesPerSheet}). Only 10 or 15 are allowed.");
                    }

                    if (setting.Id == 0)
                    {
                        // 🔹 New Entry
                        await _unitOfWork.InspectSettings.CreateAsync(setting);

                        changes.AppendLine(
                            $"<tr><td colspan='3'><b>New Inspection Setting Created</b> - {setting.SettingName}, Samples: {setting.SamplesPerSheet}</td></tr>");
                    }
                    else
                    {
                        // 🔹 Existing Entry
                        var existing = await _unitOfWork.InspectSettings.GetAsync(setting.Id);
                        if (existing == null)
                            throw new InvalidOperationException($"Inspection Setting with Id {setting.Id} not found.");

                        // 🔹 Track Changes
                        if (existing.SamplesPerSheet != setting.SamplesPerSheet)
                        {
                            changes.AppendLine($@"
                        <tr>
                            <td>{setting.SettingName}</td>
                            <td>{existing.SamplesPerSheet}</td>
                            <td>{setting.SamplesPerSheet}</td>
                        </tr>");
                        }

                        // 🔹 Update
                        existing.SamplesPerSheet = setting.SamplesPerSheet;
                        await _unitOfWork.InspectSettings.UpdateAsync(existing);
                    }
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Failed to save inspection settings");
                throw new InvalidOperationException("Failed to save Inspection Settings. Please try again.");
            }
        }
        #endregion

        #region GettingAllInspectionSettings
        public async Task<List<InspectionSettings>> GetAllInspectionSettings()
        {
            try
            {
                var Inspects= await _unitOfWork.InspectSettings.GetAllAsync();
                return Inspects.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,"Error Occured in GetAllInspectionSettings()");
                return new List<InspectionSettings>();
            }
        }
        #endregion

    }
}
