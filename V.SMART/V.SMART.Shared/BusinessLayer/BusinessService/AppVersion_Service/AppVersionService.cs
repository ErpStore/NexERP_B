using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Services
{
    public class AppVersionService: IAppVersionService
    {
        private readonly ApplicationDbContext _unitOfWork;
        private readonly ILoggingService _logs;
    

        public AppVersionService(ApplicationDbContext unitOfWork,ILoggingService logs)
        {
                _unitOfWork = unitOfWork;
                _logs = logs;
        }
        public async Task<string?> GetLatestVersionAsync()
        {
            return await _unitOfWork.AppVersions
                .AsNoTracking()
                .Select(x => x.Version)
                .FirstOrDefaultAsync();
        }
    }
}
