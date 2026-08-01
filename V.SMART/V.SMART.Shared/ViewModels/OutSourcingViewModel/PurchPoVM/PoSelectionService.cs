using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM
{
    public class PoSelectionService
    {
        public List<RateComparisonVM> SelectedItems { get; set; } = new();
        public Dictionary<int, List<RateComparisonVM>> GetVendorGroups()
        {
            return SelectedItems
                .GroupBy(x => x.VendorCode)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public void Clear() => SelectedItems.Clear();
    }
}
