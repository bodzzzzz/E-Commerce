using E_Commerce.DTOs;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _imageDirectory;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ILogger<CategoryController> logger)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;

            var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _imageDirectory = Path.Combine(webRootPath, "images", "categories");
            
            // Ensure directory exists
            if (!Directory.Exists(_imageDirectory))
            {
                Directory.CreateDirectory(_imageDirectory);
            }

            // Configure Mapster mapping
            TypeAdapterConfig<Category, CategoryDto>
                .NewConfig()
                .Map(dest => dest.Products, src => src.Products != null ?
                    src.Products.Select(p => p.Adapt<ProductDto>()).ToList() : null);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _unitOfWork.Category.GetAllAsync("Products");
                var categoryDtos = categories.Adapt<IEnumerable<CategoryDto>>();
                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _unitOfWork.Category.GetByIdAsync(c => c.Id == id, "Products");
                if (category == null)
                    return NotFound();
                
                var categoryDto = category.Adapt<CategoryDto>();
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {Id}", id);
                throw;
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateCategoryDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category = createDto.Adapt<Category>();

                if (createDto.Image != null)
                {
                    category.ImageUrl = await SaveImageAsync(createDto.Image);
                }

                await _unitOfWork.Category.AddAsync(category);
                await _unitOfWork.SaveAsync();

                var categoryDto = category.Adapt<CategoryDto>();
                return CreatedAtAction(nameof(GetById), new { id = categoryDto.Id }, categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingCategory = await _unitOfWork.Category.GetByIdAsync(c => c.Id == id);
                if (existingCategory == null)
                    return NotFound();

                // Update the category data first
                existingCategory.Name = updateDto.Name;

                // Handle image update if new image is provided
                if (updateDto.Image != null)
                {
                    try
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                        {
                            DeleteImage(existingCategory.ImageUrl);
                        }

                        // Save new image and update ImageUrl
                        existingCategory.ImageUrl = await SaveImageAsync(updateDto.Image);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling image update for category {Id}", id);
                        throw;
                    }
                }

                await _unitOfWork.Category.UpdateAsync(existingCategory);
                await _unitOfWork.SaveAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {Id}", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _unitOfWork.Category.GetByIdAsync(c => c.Id == id);
                if (category == null)
                    return NotFound();

                // Delete associated image if exists
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    DeleteImage(category.ImageUrl);
                }

                await _unitOfWork.Category.DeleteAsync(category);
                await _unitOfWork.SaveAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {Id}", id);
                throw;
            }
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            // Ensure directory exists
            Directory.CreateDirectory(_imageDirectory);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
            var filePath = Path.Combine(_imageDirectory, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // Return relative URL
            return $"/images/categories/{uniqueFileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_imageDirectory, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}