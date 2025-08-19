namespace E_Commerce.Entities
{
    public class User
    {
      
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public  ICollection<Order>? Orders { get; set; }
        public  Cart? Cart { get; set; }
    }
}
