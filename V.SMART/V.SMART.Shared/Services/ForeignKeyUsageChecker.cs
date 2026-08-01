using V.SMART.Shared.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    public class ForeignKeyUsageChecker
    {
        private readonly ApplicationDbContext _context;

        public ForeignKeyUsageChecker(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetUsageTableAsync<TEntity>(object keyValue)where TEntity : class
        {
            try
            {
                var principalType = _context.Model.FindEntityType(typeof(TEntity));
                if (principalType == null)
                    return null;

                var foreignKeys = _context.Model.GetEntityTypes()
                    .SelectMany(e => e.GetForeignKeys())
                    .Where(fk => fk.PrincipalEntityType == principalType)
                    .ToList();

                foreach (var fk in foreignKeys)
                {
                    var dependentType = fk.DeclaringEntityType;
                    var clrType = dependentType.ClrType;
                    var tableName = dependentType.GetTableName();

                    foreach (var property in fk.Properties)
                    {
                        bool exists = await CheckRecordExists(clrType, property.Name, keyValue);

                        if (exists)
                            return ConvertToBusinessName(tableName!);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log internally (very important for ERP debugging)
                Console.WriteLine($"FK Checker Error: {ex.Message}");
                throw new Exception("Unable to verify record usage. Please contact administrator.");
            }
        }

        private async Task<bool> CheckRecordExists(Type entityType, string propertyName, object keyValue)
        {
            try
            {
                var dbSet = _context.GetType()
                    .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
                    .MakeGenericMethod(entityType)
                    .Invoke(_context, null);

                var queryable = (IQueryable)dbSet!;

                var parameter = Expression.Parameter(entityType, "e");

                // Check if CLR property exists
                var clrProperty = entityType.GetProperty(propertyName);

                Expression propertyAccess;

                if (clrProperty != null)
                {
                    // Normal property
                    propertyAccess = Expression.Property(parameter, clrProperty);
                }
                else
                {
                    // Shadow property → EF.Property<object>(e, "PropertyName")
                    var efPropertyMethod = typeof(EF)
                        .GetMethod(nameof(EF.Property))!
                        .MakeGenericMethod(typeof(object));

                    propertyAccess = Expression.Call(
                        efPropertyMethod,
                        parameter,
                        Expression.Constant(propertyName));
                }

                // Handle nullable
                Type targetType = Nullable.GetUnderlyingType(propertyAccess.Type) ?? propertyAccess.Type;
                object safeValue = Convert.ChangeType(keyValue, targetType);

                var constant = Expression.Constant(safeValue, propertyAccess.Type);

                var equal = Expression.Equal(propertyAccess, constant);
                var lambda = Expression.Lambda(equal, parameter);

                var anyMethod = typeof(Queryable).GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(entityType);

                var result = anyMethod.Invoke(null, new object[] { queryable, lambda });

                return (bool)result!;
            }
            catch
            {
                // Ignore broken mapping and continue scanning other tables
                return false;
            }
        }



        private string ConvertToBusinessName(string table)
        {
            return table switch
            {
                "PurchaseGRN" => "Purchase GRN",
                "MaterialIssNote" => "Material Issue",
                "ProductionLog" => "Production Entry",
                "RouteCard" => "Route Card",
                "StockIssue" => "Stock Issue",
                "StockAdd" => "Stock Receipt",
                "StoreInterTrans" => "Store Transfer",
                "SubConGRN" => "Subcontract GRN",
                "SubConSCN" => "Subcontract SCN",
                "ToolCribIssue" => "Tool Crib Issue",
                "ToolCribReturns" => "Tool Crib Return",
                "SCNGen" => "Store Transfer Note",
                "SCNGenSub" => "Store Transfer Note",
                _ => table
            };
        }
    }

}
