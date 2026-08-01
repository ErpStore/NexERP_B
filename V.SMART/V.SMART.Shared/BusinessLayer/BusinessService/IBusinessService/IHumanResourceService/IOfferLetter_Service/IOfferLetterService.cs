using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.OfferLetter;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService
{
    public interface IOfferLetterService
    {
        Task<List<CandidateVM>> GetCandidatesAsync(int? CandidateId = null);
        Task<CandidateVM?> GetCandidateByIdAsync(int CandidateId);
        Task<IEnumerable<CandidateVM>> SearchCandidatesAsync(string searchText);

        Task<(List<OfferLetterVM> OfferVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<(bool CanDelete, string Message)> CanDeleteOfferLetterAsync(int offerId);

        Task<bool> IsDocumentUploaded(int offerid);

        Task<bool> DeleteOfferidByOfferidAsync(int offerid);
        Task<OfferLetterVM> GetOfferidByOfferidAsync(int offerid);

        Task DeleteOfferSubItemAndResequenceAsync(OfferLetterSubVM subitem, OfferLetterVM OfferLetter);

        Task<List<OfferLetterSubVM>> GetOfferSubByOfferLetterIdAsync(int OfferId);

        Task<OfferLetterVM> UpsertOfferLetterAsync(OfferLetterVM OfferLetterVM);

        Task<OfferLetterVM?> GetOfferDetailsByOfferIdAsync(int OfferId);

        Task<List<string>> GetOfferLetterPositionsAsync();
        Task<OfferLetterVM> GetLastOfferLetterByPositionAsync(string position);

        Task<string> GetOfferNoAsync();






    }
}
