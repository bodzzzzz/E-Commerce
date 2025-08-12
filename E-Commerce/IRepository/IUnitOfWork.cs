namespace E_Commerce.IRepository
{
    public interface IUnitOfWork 
    {
        ICategoryRepo Category { get; }
        IProductRepo Product { get; }
        Task SaveAsync();
    }
    
}

