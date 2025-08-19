using E_Commerce.Data;
using E_Commerce.IRepository;

namespace E_Commerce.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceDbContext _context;
        public ICategoryRepo Category { get; private set; }

        public IProductRepo Product { get; private set; }
        public ICartRepo Cart { get; private set; }
        public ICartItemRepo CartItem { get; private set; }
        public IOrderRepo Order { get; private set; }

        public UnitOfWork(EcommerceDbContext context, ICategoryRepo categoryRepo, IProductRepo productRepo,ICartRepo cartRepo, ICartItemRepo cartItemRepo,IOrderRepo orderRepo)
        {
            _context = context;
            Category = categoryRepo;
            Product = productRepo;
            Cart = cartRepo;
            CartItem = cartItemRepo;
            Order = orderRepo;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
