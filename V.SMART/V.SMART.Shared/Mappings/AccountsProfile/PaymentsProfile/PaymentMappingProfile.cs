using AutoMapper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.ViewModels.AccountsViewModel;

namespace IQSMART.Shared.Mappings.AccountsProfile.PaymentsProfile
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            // ---------------------------
            // Payments → PaymentsVM
            // ---------------------------
            CreateMap<Payments, PaymentsVM>()
                .ForMember(d => d.ExpenseCode,
                    o => o.MapFrom(s => s.ExpenseCode))
                .ForMember(d => d.ExpenseName,
                    o => o.MapFrom(s => s.Expense != null ? s.Expense.ExpenseName : null))
                .ForMember(d => d.BankId,
                    o => o.MapFrom(s => s.BankId))
                .ForMember(d => d.PayToRefCode,
                    o => o.MapFrom(s => s.PayToRefCode))
                .ForMember(d => d.PayToName,
                    o => o.MapFrom(s => s.PayToName))   // ✅ AUTO MAP
                .ForMember(d => d.PaymentSubVM,
                    o => o.MapFrom(s => s.PaymentsSubs));
               
            // ---------------------------
            // PaymentsVM → Payments
            // ---------------------------
            CreateMap<PaymentsVM, Payments>()
                .ForMember(d => d.Expense, o => o.Ignore())
                .ForMember(d => d.Banks, o => o.Ignore())
                .ForMember(d => d.PayToName,
                    o => o.MapFrom(s => s.PayToName))   // ✅ AUTO MAP
                .ForMember(d => d.PaymentsSubs,
                    o => o.MapFrom(s => s.PaymentSubVM));

            // ---------------------------
            // Sub mappings
            // ---------------------------
            CreateMap<PaymentsSub, PaymentSubVM>();

            CreateMap<PaymentSubVM, PaymentsSub>()
                .ForMember(d => d.BillDate,
                    o => o.MapFrom(s =>
                        s.BillDate == default ? DateTime.Now : s.BillDate));



        }

    }
}
