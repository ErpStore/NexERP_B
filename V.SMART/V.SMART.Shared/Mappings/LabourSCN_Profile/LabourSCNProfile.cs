using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourSCN_VM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.LabourSCN_Profile
{
    public class LabourSCNProfile:Profile
    {
        public LabourSCNProfile()
        {
            // ============== Entity → ViewModel Mapping ==============
            CreateMap<LabourSCN, LabourSCNVM>()
                // Vendor Details
                .ForMember(dest => dest.CustName,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
                .ForMember(dest => dest.CustAddress,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustAddr : string.Empty))
                .ForMember(dest => dest.CustGst,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GSTNo : string.Empty))
                .ForMember(dest => dest.CustContactNo,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.ContactNo : string.Empty))

                // Store Details
                .ForMember(dest => dest.AddStoreName,
                    opt => opt.MapFrom(src => src.StoreAdd != null ? src.StoreAdd.StoreName : string.Empty))

                .ForMember(dest => dest.IssueStoreName,
                    opt => opt.MapFrom(src => src.StoreIssue != null ? src.StoreIssue.StoreName : string.Empty))
                // Child Collection
                .ForMember(dest => dest.LabourSCNSubVMs, opt => opt.MapFrom(src => src.LabourSCNSubs));

            // ============== ViewModel → Entity Mapping ==============
            CreateMap<LabourSCNVM, LabourSCN>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.StoreAdd, opt => opt.Ignore())
                .ForMember(dest => dest.StoreIssue, opt => opt.Ignore())
                .ForMember(dest => dest.LabourSCNSubs, opt => opt.Ignore());
        }
    }
}
