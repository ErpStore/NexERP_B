using System.Linq.Expressions;

namespace V.SMART.Shared.Repository.IRepository
{
    /// <summary>
    /// Generic repository interface for common database operations.
    /// Works with any entity class (T).
    /// </summary>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Get an entity by its primary key (Id).
        /// </summary>
        Task<T> GetAsync(int id);

        /// <summary>
        /// Get a single entity with related data (includes) that matches the given condition.
        /// </summary>
        Task<T?> GetWithIncludeAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Get all entities (optionally without tracking for better performance).
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = false);

        /// <summary>
        /// Get all entities that match a condition.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get all entities with related data (includes) that match a condition.
        /// </summary>
        Task<IEnumerable<T>> GetAllWithIncludeAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Add a new entity to the database.
        /// </summary>
        Task<T> CreateAsync(T entity);

        Task CreateRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Update an existing entity in the database.
        /// </summary>
        Task<T> UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        /// <summary>
        /// Delete an entity by its primary key (Id).
        /// </summary>
        /// 
        Task DeleteAsync(T entity);

        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Check if any entity matches the given condition.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get the first entity that matches the condition (or null if none found).
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Find all entities matching a condition.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Check if a value exists for a specific property (optionally excluding one Id).
        /// Example: Check if customer name exists but skip current record.
        /// </summary>
        Task<bool> ExistsByNameAsync(string propertyName, string value, string keyPropertyName = "Id", int? excludeId = null);

        /// <summary>
        /// Get a specific property value for the first entity matching a condition.
        /// Example: Get customer name where customer id = 5.
        /// </summary>
        Task<TResult?> GetPropertyByIdAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);

        /// <summary>
        /// Get all records but only return selected fields.
        /// Example: Get only Id and Name for all customers.
        /// </summary>
        Task<IEnumerable<TResult>> GetAllProjectedAsync<TResult>(Expression<Func<T, TResult>> selector);

        /// <summary>
        /// Get filtered records and return only selected fields.
        /// Example: Get Id and Name for active customers.
        /// </summary>
        Task<IEnumerable<TResult>> FindAllProjectedAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);

        /// <summary>
        /// Get a "name" (or similar property) by Id.
        /// Example: Get ItemName from ItemId.
        /// </summary>
        Task<string> GetNameByIdAsync<TProperty>(int id, Expression<Func<T, int>> idSelector, Expression<Func<T, TProperty>> nameSelector);

        //Get Latest 
        Task<T?> GetLatestAsync<TKey>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TKey>> orderBy);

        Task DeleteRangeAsync(IEnumerable<T> entities);
        IQueryable<T> GetQueryable();
    }

}
