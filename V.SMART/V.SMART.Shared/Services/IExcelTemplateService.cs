using ClosedXML.Excel;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    public interface IExcelTemplateService
    {
        Task<byte[]> CreateTemplateAsync(
            string sheetName,
            string[] headers,
            IEnumerable<object>? sampleData = null,
            bool freezeHeader = true,
            XLColor? headerColor = null,
            IEnumerable<UOM>? uomList = null,
            IEnumerable<string>? categoryList = null);

        Task<string[]> LoadHeadings(string uploadType);

        Task<byte[]> DownloadTemplate(string uploadType);

        //Task<ExcelUploadResult<T>> UploadExcelFile<T>(IBrowserFile file, string uploadType);

        Task<HSNMaster> EnsureHsnExists(string HSN);

        Task<Dictionary<string, string>> ItemValidation(ExcelUploadVM Model);

        Task<Category> EnsureCategoryExists(string Category);

        Task<Item> GetOrCreateItemByCodeAsync(string itemCode, string itemName, string speification, string uom, string hsn,
                                              string scn, string itemType, int categorycode = 0, decimal weight = 0, decimal Area = 0, string Remarks = "");

        Task AssignItemToCustomer(int customerId, int itemId);

        Task<List<Item>> GetItemsByCodesAsync(List<string> itemCodes);

        Task<Dictionary<string, int>> GetItemIdsByItemCodesAsync(List<string> itemCodes);

        Task<List<ItemVM>> GetItemVMsByItemIdsAsync(List<int> itemIds);

        Task<Dictionary<string, byte[]>> ExtractImagesFromExcelAsync(Stream excelStream, string sheetName);

    }


}


