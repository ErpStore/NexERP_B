using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class ReportExecutor : IReportExecutor
    {
        private readonly ApplicationDbContext _context;

        public ReportExecutor(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<T>> ExecuteAsync<T>(string procedureName,params SqlParameter[] parameters) where T : class
        {
            try
            {
                _context.Database.SetCommandTimeout(300);

                string sql = $"EXEC dbo.{procedureName}";

                if (parameters != null && parameters.Length > 0)
                {
                    sql += " " + string.Join(", ",
                        parameters.Select(p => p.ParameterName));
                }

                return await _context
                    .Set<T>()
                    .FromSqlRaw(sql, parameters)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error while executing procedure: {procedureName}",
                    ex);
            }
        }
        //public async Task<List<T>> ExecuteAsync<T>(string procedureName, params SqlParameter[] parameters) where T : class
        //{
        //    try
        //    {
        //        // Build SQL
        //        _context.Database.SetCommandTimeout(300);
        //        int skip = 0;
        //        int take = 100;

        //        string sql = $"EXEC {procedureName}";

        //        if (parameters != null && parameters.Length > 0)
        //        {
        //            sql += " " + string.Join(", ",
        //                parameters.Select(x => x.ParameterName));
        //        }

        //        //return await _context
        //        //    .Set<T>()
        //        //    .FromSqlRaw(sql, parameters)
        //        //    .AsNoTracking()
        //        //    .ToListAsync();
        //        return await _context
        //            .Set<T>()
        //            .FromSqlRaw(
        //                $"EXEC {procedureName} @parameters,@Skip, @Take",
        //                new SqlParameter("@parameters", parameters),
        //                new SqlParameter("@Skip", skip),
        //                new SqlParameter("@Take", take))
        //            .AsNoTracking()
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(
        //            $"Error while executing procedure: {procedureName}",
        //            ex);
        //    }
        // }

        public async Task<DataTable> ExecuteDataTableAsync(string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new Exception("Database connection string is null or empty.");
                }

                await using var connection = new SqlConnection(connectionString);

                await connection.OpenAsync();

                await using var command = connection.CreateCommand();

                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 300;

                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                await using var reader = await command.ExecuteReaderAsync();

                var dt = new DataTable();
                dt.Load(reader);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while executing procedure: {procedureName}", ex);
            }
        }
    }
}
