using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ICreditNote_Repository
{
    public interface ICreditNoteRepository:IRepository<CreditNote>
    {
        Task<string> GetLastCreditNoAsync(string suffix);
    }
}
