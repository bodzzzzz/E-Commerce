namespace E_Commerce.Entities
{
    public class Order
    {
      
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        // FK
        public int UserId { get; set; }
        public User? User { get; set; }

        // Navigation
        public  ICollection<OrderItem>? OrderItems { get; set; }
    }
}
