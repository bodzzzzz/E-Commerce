using E_Commerce.Data;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repository
{
    public class ProductRepo : Repository<Product>, IProductRepo
    {
       private readonly EcommerceDbContext _context;
        public ProductRepo(EcommerceDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task Update(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}
