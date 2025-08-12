using E_Commerce.Entities;

namespace E_Commerce.IRepository
{
    public interface IProductRepo: IRepository<Product>
    {
        Task Update(Product product);
    }
}
