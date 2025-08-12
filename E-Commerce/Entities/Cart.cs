namespace E_Commerce.Entities
{
    public class Cart
    {
      
        public int Id { get; set; }

        // FK
        public int UserId { get; set; }
        public User? User { get; set; }

        // Navigation
        public ICollection<CartItem>? Items { get; set; }
    }
}
