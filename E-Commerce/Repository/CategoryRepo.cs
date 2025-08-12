using E_Commerce.Data;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace E_Commerce.Repository
{
    public class CategoryRepo : Repository<Category>, ICategoryRepo
    {
        private readonly EcommerceDbContext _context;

        public CategoryRepo(EcommerceDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task Update(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
