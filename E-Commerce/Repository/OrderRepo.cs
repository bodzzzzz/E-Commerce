using E_Commerce.Data;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repository
{
    public class OrderRepo : Repository<Order>,IOrderRepo
    {
        private readonly EcommerceDbContext _context;
        public OrderRepo(EcommerceDbContext context) : base(context) {
            _context = context;
        }

        public Task Update(Order order)
        {
            _context.Orders.Update(order);
            return _context.SaveChangesAsync();
        }
    }
}
