using E_Commerce.Entities;

namespace E_Commerce.IRepository
{
    public interface ICartItemRepo : IRepository<CartItem>
    {
        Task Update(CartItem cartItem);
    }
}
