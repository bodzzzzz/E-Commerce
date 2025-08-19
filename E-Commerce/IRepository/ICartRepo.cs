using E_Commerce.Entities;

namespace E_Commerce.IRepository
{
    public interface ICartRepo : IRepository<Cart>
    {
        Task Update(Cart cart);

    }
}
