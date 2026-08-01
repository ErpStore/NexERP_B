using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.AppointmentLetterVM;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService
{
    public interface IAppointmentLetterService
    {
        Task<List<CandidateVM>> GetCandidatesAsync(int? CandidateId = null);
        Task<CandidateVM?> GetCandidateByIdAsync(int CandidateId);
        Task<IEnumerable<CandidateVM>> SearchCandidatesAsync(string searchText);

        Task<(List<AppointmentLetterVM> appointmentLetterVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<(bool CanDelete, string Message)> CanDeleteAppointmentletterAsync(int offerId);

        Task<bool> IsDocumentUploaded(int offerid);

        Task<bool> DeleteAppointmentIdByAppointmentIdAsync(int AppointmentId);
        Task<AppointmentLetterVM> GetAppointmentIdidByAppointmentIdAsync(int AppointmentId);

        Task DeleteOfferSubItemAndResequenceAsync(AppointmentLetterSubVM subitem, AppointmentLetterVM appointmentLetter);

        Task<List<AppointmentLetterSubVM>> GetAppointmentletterSubByAppointmentletterAsync(int AppointmentId);

        Task<AppointmentLetterVM> UpsertAppointmentLetterAsync(AppointmentLetterVM AppointmentLetterVM);

        Task<AppointmentLetterVM?> GetAppointmentLetterDetailsByAppointmentIdAsync(int OfferId);

        Task<List<string>> GetAppointmentLetterPositionsAsync();
        Task<AppointmentLetterVM> GetLastAppointmentLetterByPositionAsync(string position);

        Task<string> GetAppointmentLetterNoAsync();
    }
}
