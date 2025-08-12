using E_Commerce.Data;
using E_Commerce.IRepository;

namespace E_Commerce.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceDbContext _context;
        public ICategoryRepo Category { get; private set; }

        public IProductRepo Product { get; private set; }

        public UnitOfWork(EcommerceDbContext context, ICategoryRepo categoryRepo, IProductRepo productRepo)
        {
            _context = context;
            Category = categoryRepo;
            Product = productRepo;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
