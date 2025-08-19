using System.Linq.Expressions;

namespace E_Commerce.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Expression<Func<T, bool>> filter, params string[] includes);
        Task<IEnumerable<T>> GetAllAsync(params string[] includes);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
