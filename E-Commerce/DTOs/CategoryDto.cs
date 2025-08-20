using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<ProductDto>? Products { get; set; } // Include products
    }

    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
