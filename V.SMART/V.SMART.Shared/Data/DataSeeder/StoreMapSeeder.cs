using V.SMART.Shared.Data.GeneralSettingsMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.DataSeeder
{
    public static class StoreMapSeeder
    {
        public static List<StoreMap> GetDefaultStoreMaps()
        {
            int defaultStoreId = 1;
            int slNo = 1;

            return new List<StoreMap>
            {
                new StoreMap { Id = 1,  SlNo = slNo++, ScreenName = "GRN - Purchase",FormName = "GRNForm",StoreId = 2 },
                new StoreMap { Id = 2,  SlNo = slNo++, ScreenName = "Store Credit Note - Quality",FormName = "SCNQualityForm",StoreId = defaultStoreId },

                new StoreMap { Id = 3,  SlNo = slNo++, ScreenName = "Produced Material - Issue",FormName = "PMIssueForm",StoreId = defaultStoreId },
                new StoreMap { Id = 4,  SlNo = slNo++, ScreenName = "Produced Material - Return",FormName = "PMReturnForm",StoreId = 5 },
                new StoreMap { Id = 5,  SlNo = slNo++, ScreenName = "Store Credit Note - Production",FormName = "SCNProductionForm",StoreId = defaultStoreId },

                new StoreMap { Id = 6,  SlNo = slNo++, ScreenName = "Sales DC Outwards - Manufacturing",FormName = "SalesDCForm",StoreId = defaultStoreId },

                new StoreMap { Id = 7,  SlNo = slNo++, ScreenName = "Labour DC Inwards",FormName = "LabourDCInForm",StoreId = 4 },
                new StoreMap { Id = 8,  SlNo = slNo++, ScreenName = "Labour DC Outwards",FormName = "LabourDCOutForm",StoreId = defaultStoreId },
                new StoreMap { Id = 9, SlNo = slNo++, ScreenName = "Store Credit Note - Labour",FormName = "SCNLabourForm",StoreId = defaultStoreId },


                new StoreMap { Id = 10,  SlNo = slNo++, ScreenName = "SubContract DC Outwards",FormName = "SubContractDCForm",      StoreId = defaultStoreId },
                new StoreMap { Id = 11, SlNo = slNo++, ScreenName = "SubCon GRN",FormName = "SubConGRNForm",StoreId = 3 },
                new StoreMap { Id = 12, SlNo = slNo++, ScreenName = "SubCon Store Credit Note",FormName = "SubConSCNForm",StoreId = defaultStoreId },

                new StoreMap { Id = 13, SlNo = slNo++, ScreenName = "Store Credit Note - General",FormName = "SCNGeneralForm",StoreId = defaultStoreId },
                new StoreMap { Id = 14, SlNo = slNo++, ScreenName = "Material Issue Note",FormName = "MaterialIssueForm",StoreId = defaultStoreId },
                new StoreMap { Id = 15, SlNo = slNo++, ScreenName = "Inter Store Transfer",FormName = "InterStoreTransferForm",StoreId = defaultStoreId },

                new StoreMap { Id = 16, SlNo = slNo++, ScreenName = "Tool Crib Issue Note",FormName = "ToolCribIssueForm",StoreId = defaultStoreId },
                new StoreMap { Id = 17, SlNo = slNo++, ScreenName = "Tool Crib Return Note",FormName = "ToolCribReturnForm",StoreId = defaultStoreId },

                new StoreMap { Id = 18, SlNo = slNo++, ScreenName = "Purchase Requisition",FormName = "PurchaseReqForm",StoreId = defaultStoreId },


                new StoreMap { Id = 19, SlNo = slNo++, ScreenName = "Produced Material Issue (IWO) Assy",FormName = "PMIssueAssyForm",StoreId = defaultStoreId },
                new StoreMap { Id = 20, SlNo = slNo++, ScreenName = "Produced Material Return (GRN) Assy",FormName = "PMReturnAssyForm",StoreId = 5 },
                new StoreMap { Id = 21, SlNo = slNo++, ScreenName = "Store Credit Note - Production (GRN) Assy", FormName = "SCNProductionAssyForm", StoreId = defaultStoreId },

                new StoreMap { Id = 22, SlNo = slNo++, ScreenName = "Production Log Issue Store",FormName = "ProdLogIssueForm",StoreId = defaultStoreId },
                new StoreMap { Id = 23, SlNo = slNo++, ScreenName = "Production Log Return Store",FormName = "ProdLogReturnForm",StoreId = defaultStoreId }
            };
        }
    }

}
