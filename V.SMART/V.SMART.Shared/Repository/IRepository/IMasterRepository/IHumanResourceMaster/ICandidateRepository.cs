using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster
{
    public interface ICandidateRepository: IRepository<Candidate>
    {
        Task<Candidate?> GetCandidateDataAsync(int id);
       
    }
}
