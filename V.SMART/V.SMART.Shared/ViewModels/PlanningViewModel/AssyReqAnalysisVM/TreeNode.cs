using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.AssyReqAnalysisVM
{
    public class TreeNode
    {
        public int Id { get; set; }

        public string ItemCode { get; set; }

        public string ItemName { get; set; }

        public string OrderNo { get; set; } = "";
        public String UOM { get; set; }
        public string Category { get; set; }

        public decimal QtyPerParent { get; set; }

        public int Level { get; set; }

        public bool IsExpanded { get; set; }

        public List<TreeNode> Children { get; set; } = new();
    }
}
