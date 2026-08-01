using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IReportExecutor
    {
        Task<List<T>> ExecuteAsync<T>(
            string procedureName,
            params SqlParameter[] parameters) where T : class;

        Task<DataTable> ExecuteDataTableAsync(string procedureName, params SqlParameter[] parameters);
    }
}
