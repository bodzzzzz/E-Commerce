using E_Commerce.Entities;

namespace E_Commerce.IRepository
{
    public interface IOrderRepo: IRepository<Order>
    {
        Task Update(Order order);
    }
}
