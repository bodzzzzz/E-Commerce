namespace E_Commerce.Entities
{
    public class Product
    {
       
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        // FK
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Navigation
        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
    }
}
