namespace E_Commerce.IRepository
{
    public interface IUnitOfWork 
    {
        ICategoryRepo Category { get; }
        IProductRepo Product { get; }
        ICartRepo Cart { get; }
        ICartItemRepo CartItem { get; }
        IOrderRepo Order { get; }
        Task SaveAsync();
    }
    
}

