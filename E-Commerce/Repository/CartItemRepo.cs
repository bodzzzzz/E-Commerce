using E_Commerce.Data;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repository
{
    public class CartItemRepo : Repository<CartItem>, ICartItemRepo
    {
        private readonly EcommerceDbContext _context;
        public CartItemRepo(EcommerceDbContext context) : base(context)
        {
            _context = context;
        }

        public Task Update(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            return _context.SaveChangesAsync();
        }
    }
}
