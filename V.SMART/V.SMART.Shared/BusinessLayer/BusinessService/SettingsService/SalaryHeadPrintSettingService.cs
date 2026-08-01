using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SettingsService
{
    public class SalaryHeadPrintSettingService : ISalaryHeadPrintSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;

        public SalaryHeadPrintSettingService(
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

        public async Task<List<SalaryHeadPrintSetting>> GetAllSalaryPrintHeadSettingsAsync()
        {
            return (await _unitOfWork.SalaryHeadPrintSettings.GetAllAsync()).ToList();
        }

        public async Task<List<SalaryHeadPrintSetting>> GetAllAsync()
        {
            return await _unitOfWork.SalaryHeadPrintSettings.GetAllAsync();
        }

        // ✅ FIXED UPSERT (THIS IS YOUR FINAL SOLUTION)
        public async Task UpsertSalaryPrintHeadSettingAsync(List<SalaryHeadPrintSetting> settings)
        {
            await _unitOfWork.SalaryHeadPrintSettings.SaveAllAsync(settings);
        }
    }
}
