using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.ContractReview;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.GeneralMasterProfile
{
    public class ContractReviewMasterMapping : Profile
    {
        public ContractReviewMasterMapping()
        {
            // Entity -> VM
            CreateMap<ContractReviewMaster, ContractReviewMasterVM>();

            // VM -> Entity
            CreateMap<ContractReviewMasterVM, ContractReviewMaster>();
        }
    }
}
