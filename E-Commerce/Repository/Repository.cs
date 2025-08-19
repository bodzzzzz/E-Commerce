using E_Commerce.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace E_Commerce.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        private IQueryable<T> IncludeProperties(IQueryable<T> query, IEnumerable<string> includePaths)
        {
            foreach (var includePath in includePaths)
            {
                query = query.Include(includePath);
            }
            return query;
        }

        public async Task<T?> GetByIdAsync(Expression<Func<T, bool>> filter, params string[] includes)
        {
            var query = IncludeProperties(_dbSet, includes);
            return await query.FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<T>> GetAllAsync(params string[] includes)
        {
            var query = IncludeProperties(_dbSet, includes);
            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
    }

