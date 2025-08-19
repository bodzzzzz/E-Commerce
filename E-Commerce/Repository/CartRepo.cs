using E_Commerce.Data;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repository
{
    public class CartRepo : Repository<Cart>, ICartRepo
    {
        private readonly EcommerceDbContext _context;

        public CartRepo(EcommerceDbContext context) : base(context)
        {
            _context = context;
        }

        public Task Update(Cart cart)
        {
            _context.Carts.Update(cart);
            return _context.SaveChangesAsync();

        }
    }
}
