using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.HumanResource.AppointmentLetter;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IAppointmentLetterRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.HumanResourceRepository.AppointmentLetterRepository
{
    public class AppointmentLetterRepository : Repository<AppointmentLetter>, IAppointmentLetterRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public AppointmentLetterRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService) : base(db, loggingService)
        {
            db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }
    }
}
