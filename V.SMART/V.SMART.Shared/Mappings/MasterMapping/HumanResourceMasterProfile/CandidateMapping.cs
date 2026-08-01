using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.Mappings.MasterMapping.HumanResourceMasterProfile
{
    public class CandidateMapping : Profile
    {
        public CandidateMapping()
        {
            CreateMap<Candidate, CandidateVM>();

            // VM -> Entity
            CreateMap<CandidateVM, Candidate>()
                .ForMember(dest => dest.CandidateID, opt => opt.Ignore());
                
        }
       
    }
}
