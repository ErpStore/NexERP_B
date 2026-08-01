using AutoMapper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.AccountsModule;

namespace V.SMART.Shared.Mappings.AccountsProfile.PaymentsProfile
{
    public class AdvanceAdjustMapping : Profile
    {
        public AdvanceAdjustMapping()
        {
            // ---------------------------
            // Payments → PaymentsVM
            // ---------------------------
            CreateMap<Advaceadjustment, AdvaceadjustmentVM>()
                .ForMember(d => d.ExpenseCode,
                    o => o.MapFrom(s => s.ExpenseCode))
                .ForMember(d => d.ExpenseName,
                    o => o.MapFrom(s => s.Expense != null ? s.Expense.ExpenseName : null))
                 .ForMember(d => d.IncomeCode,
                    o => o.MapFrom(s => s.IncomeCode))
                .ForMember(d => d.IncomeName,
                    o => o.MapFrom(s => s.Income != null ? s.Income.IncomeName : null))
                .ForMember(d => d.BankId,
                    o => o.MapFrom(s => s.BankId))
                .ForMember(d => d.PayToRefCode,
                    o => o.MapFrom(s => s.PayToRefCode))
                .ForMember(d => d.PayToName,
                    o => o.MapFrom(s => s.PayToName))   // ✅ AUTO MAP
                .ForMember(d => d.AdvaceadjustmentSubVM,
                    o => o.MapFrom(s => s.AdvaceadjustmentSubs));

            // ---------------------------
            // PaymentsVM → Payments
            // ---------------------------
            CreateMap<AdvaceadjustmentVM, Advaceadjustment>()
                .ForMember(d => d.Expense, o => o.Ignore())
                .ForMember(d => d.Income, o => o.Ignore())
                .ForMember(d => d.Banks, o => o.Ignore())
                .ForMember(d => d.PayToName,
                    o => o.MapFrom(s => s.PayToName))   // ✅ AUTO MAP
                .ForMember(d => d.AdvaceadjustmentSubs,
                    o => o.MapFrom(s => s.AdvaceadjustmentSubVM));

            // ---------------------------
            // Sub mappings
            // ---------------------------
            CreateMap<AdvaceadjustmentSub, AdvaceadjustmentSubVM>();

            CreateMap<AdvaceadjustmentSubVM, AdvaceadjustmentSub>()
                .ForMember(d => d.BillDate,
                    o => o.MapFrom(s =>
                        s.BillDate == default ? DateTime.Now : s.BillDate));



        }
    }
}
