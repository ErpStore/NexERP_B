using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Rejection_Module;
using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.Mappings.MasterMapping
{
    public class RejectionMapping : Profile
    {
        public RejectionMapping()
        {
            CreateMap<RejectionMaster, RejectionMasterVM>();

            // ViewModel -> Entity
            CreateMap<RejectionMasterVM, RejectionMaster>();
        }
    }
}
