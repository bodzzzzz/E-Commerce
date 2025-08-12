using System.Linq.Expressions;

namespace E_Commerce.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
