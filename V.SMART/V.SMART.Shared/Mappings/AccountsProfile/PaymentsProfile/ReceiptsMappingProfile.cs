using AutoMapper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.ViewModels.AccountsViewModel;

namespace V.SMART.Shared.Mappings.AccountsProfile.PaymentsProfile
{
    public class ReceiptsMappingProfile : Profile
    {
        public ReceiptsMappingProfile()
        {
            // ---------------------------
            // Payments → PaymentsVM
            // ---------------------------
            CreateMap<Receipts, ReceiptsVM>()
                .ForMember(d => d.IncomeCode,
                    o => o.MapFrom(s => s.IncomeCode))
                .ForMember(d => d.InComeName,
                    o => o.MapFrom(s => s.Income != null ? s.Income.IncomeName : null))
                .ForMember(d => d.BankId,
                    o => o.MapFrom(s => s.BankId))
                .ForMember(d => d.PayFromRefCode,
                    o => o.MapFrom(s => s.PayFromRefCode))
                .ForMember(d => d.PayFromName,
                    o => o.MapFrom(s => s.PayFromName))   // ✅ AUTO MAP
                .ForMember(d => d.ReceiptsSubVMs,
                    o => o.MapFrom(s => s.ReceiptsSubs));

            // ---------------------------
            // PaymentsVM → Payments
            // ---------------------------
            CreateMap<ReceiptsVM, Receipts>()
                .ForMember(d => d.Income, o => o.Ignore())
                .ForMember(d => d.Banks, o => o.Ignore())
                .ForMember(d => d. PayFromName ,
                    o => o.MapFrom(s => s.PayFromName))   // ✅ AUTO MAP
                .ForMember(d => d.ReceiptsSubs,
                    o => o.MapFrom(s => s.ReceiptsSubVMs));

            // ---------------------------
            // Sub mappings
            // ---------------------------
            CreateMap<ReceiptsSub, ReceiptsSubVM>();

            CreateMap<ReceiptsSubVM, ReceiptsSub>()
                .ForMember(d => d.BillDate,
                    o => o.MapFrom(s =>
                        s.BillDate == default ? DateTime.Now : s.BillDate));



        }

    }
}
