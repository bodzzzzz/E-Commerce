﻿namespace E_Commerce.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        // Navigation
        public ICollection<Product>? Products { get; set; }
    }
}
