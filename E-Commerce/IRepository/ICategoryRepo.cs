using E_Commerce.Entities;

namespace E_Commerce.IRepository
{
    public interface ICategoryRepo : IRepository<Category>
    {
        Task Update(Category category);

    }
}
