using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace V.SMART.Shared.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        protected readonly ILoggingService _logs;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext db, ILoggingService logs)
        {
            _db = db;
            _dbSet = _db.Set<T>();
            _logs = logs;
        }

        // ---- GET SINGLE ----
        public virtual async Task<T> GetAsync(int id) => await _dbSet.FindAsync(id);

        // Get single with includes (expression-based, type-safe)
        public virtual async Task<T?> GetWithIncludeAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync(predicate);
        }


        // ---- GET MULTIPLE ----
        public virtual async Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = false)
        {
            IQueryable<T> query = _dbSet;
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
               => await _dbSet.Where(predicate).ToListAsync();

        // Get multiple with includes (merged version of GetAllWithIncludeAsync)
        public virtual async Task<IEnumerable<T>> GetAllWithIncludeAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
                query = query.Include(include);

            return await query.Where(predicate).ToListAsync();
        }


        // ---- CREATE / UPDATE / DELETE ----
        /// <summary>
        /// Adds a new entity to the context's change tracker asynchronously.
        /// </summary>
        public virtual async Task<T> CreateAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger or Console.WriteLine for simple examples)
                Console.WriteLine($"Error creating entity in repository: {ex.Message}");
                throw; // Re-throw the exception for the service layer to handle
            }
        }

        public async Task CreateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                await _dbSet.AddRangeAsync(entities);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating entities in repository: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Marks an existing entity as modified for update.
        /// </summary>
        public virtual Task<T> UpdateAsync(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                return Task.FromResult(entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating entity in repository: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                _dbSet.UpdateRange(entities);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating entity in repository: {ex.Message}");
                throw;
            }

        }



        /// <summary>
        /// Marks a tracked entity for deletion. This is the preferred method for deletion.
        /// </summary>
        public virtual Task DeleteAsync(T entity)
        {
            try
            {
                _dbSet.Remove(entity);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting entity by instance in repository: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fetches an entity by ID and marks it for deletion.
        /// </summary>
        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    return false;
                }

                _dbSet.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting entity by ID in repository: {ex.Message}");
                throw;
            }
        }

        // ---- CHECKS / HELPERS ----
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> ExistsByNameAsync(
            string propertyName,
            string value,
            string keyPropertyName = "Id",
            int? excludeId = null)
        {
            try
            {
                var parameter = Expression.Parameter(typeof(T), "x");

                var property = Expression.Property(parameter, propertyName);
                var constant = Expression.Constant(value);
                var equals = Expression.Equal(property, constant);

                Expression finalExpression = equals;

                if (excludeId.HasValue)
                {
                    var keyProperty = Expression.Property(parameter, keyPropertyName);
                    var excludeConstant = Expression.Constant(excludeId.Value);
                    var notEqual = Expression.NotEqual(keyProperty, excludeConstant);
                    finalExpression = Expression.AndAlso(equals, notEqual);
                }

                var lambda = Expression.Lambda<Func<T, bool>>(finalExpression, parameter);

                return await _dbSet.AnyAsync(lambda);
            }
            catch (ArgumentException argEx)
            {
                await _logs.LogDeveloperError(argEx,
                    $"Invalid property name '{propertyName}' or key '{keyPropertyName}' in ExistsByNameAsync for type {typeof(T).Name}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error in ExistsByNameAsync for type {typeof(T).Name} with property '{propertyName}' and value '{value}'");
                return false;
            }
        }

        // ---- PROJECTIONS ----
        public virtual async Task<TResult?> GetPropertyByIdAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        {
            return await _dbSet
                .Where(predicate)
                .Select(selector)
                .FirstOrDefaultAsync();
        }


        /// <summary>
        /// Retrieves all records of the specified entity type and projects only the specified fields.
        /// This method does not apply any filtering (i.e., no WHERE clause).
        /// Example use cases:
        /// - Get only ItemId and ItemCode from Items
        /// </summary>
        /// <typeparam name="TResult">The type to project the results to (e.g., a DTO).</typeparam>
        /// <param name="selector">Projection selector defining which fields to retrieve.</param>
        /// <returns>List of projected records.</returns>
        public virtual async Task<IEnumerable<TResult>> GetAllProjectedAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _db.Set<T>()
                .AsNoTracking()
                .Select(selector)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves records of the specified entity type that satisfy the given condition (WHERE clause),
        /// and projects only the specified fields.
        /// Example use cases:
        /// - Get ItemId and ItemCode where CategoryCode == 2
        /// - Get Customer details where IsActive == true
        /// </summary>
        /// <typeparam name="TResult">The type to project the results to (e.g., a DTO).</typeparam>
        /// <param name="predicate">Filtering condition (WHERE clause).</param>
        /// <param name="selector">Projection selector defining which fields to retrieve.</param>
        /// <returns>List of filtered and projected records.</returns>
        public virtual async Task<IEnumerable<TResult>> FindAllProjectedAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector)
        {
            return await _db.Set<T>()
                .AsNoTracking()
                .Where(predicate)
                .Select(selector)
                .ToListAsync();
        }


        //Getting Name by passing Id Vender/ item/ Customer or etc.
        public async Task<string> GetNameByIdAsync<TProperty>(int id, Expression<Func<T, int>> idSelector, Expression<Func<T, TProperty>> nameSelector)
        {
            var result = await _db.Set<T>()
                .AsNoTracking()
                .Where(BuildEqualsExpression(idSelector, id))
                .Select(nameSelector)
                .FirstOrDefaultAsync();

            return result?.ToString() ?? "Unknown";
        }
        private static Expression<Func<T, bool>> BuildEqualsExpression(Expression<Func<T, int>> selector, int value)
        {
            var parameter = selector.Parameters[0];
            var body = Expression.Equal(selector.Body, Expression.Constant(value));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
        ////=======================================================

        // Getting Latest Passing By Id And Order By Id
        //Example:
        //var lastQuote = await _unitOfWork.MfgQuotes.GetLatestAsync(q => q.CustId == customer.CustId, q => q.QuoteId);
        //var lastQuote = await _unitOfWork.MfgQuotes.GetLatestAsync(q => q.CustId == customer.CustId, q => q.CreatedOn);

        public virtual async Task<T?> GetLatestAsync<TKey>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TKey>> orderBy)
        {
            return await _dbSet
                .Where(predicate)
                .OrderByDescending(orderBy)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).AsNoTracking().ToListAsync();
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }


        public IQueryable<T> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}
