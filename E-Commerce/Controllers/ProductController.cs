using E_Commerce.DTOs;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private readonly IUnitOfWork unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await unitOfWork.Product.GetAllAsync("Category");
            var productsDtos = products.Adapt<IEnumerable<ProductDto>>();
            return Ok(productsDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var Product = await unitOfWork.Product.GetByIdAsync(c => c.Id == id,
                "Category");
            if (Product == null)
                return NotFound();
            var ProductDto = Product.Adapt<ProductDto>();
            return Ok(ProductDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var Product = await unitOfWork.Product.GetByIdAsync(c => c.Id == id);
            if (Product == null)
                return NotFound();
            await unitOfWork.Product.DeleteAsync(Product);
            await unitOfWork.SaveAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto createDto)
        {
            if (createDto == null)
                return BadRequest("Product cannot be null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if the category exists before creating the product
            var categoryExists = await unitOfWork.Category.GetByIdAsync(c => c.Id == createDto.CategoryId);
            if (categoryExists == null)
                return BadRequest($"Category with ID {createDto.CategoryId} does not exist.");

            var product = createDto.Adapt<Product>();
            await unitOfWork.Product.AddAsync(product);
            await unitOfWork.SaveAsync();

            var productDto = product.Adapt<ProductDto>();
            return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, productDto);
        }

        [HttpPut("{id}/AddStock")]
        public async Task<IActionResult> AddStock(int id, UpdateProductStockDto updateDto)
        {
            if (updateDto == null)
                return BadRequest("Stock cannot be null.");

            if (updateDto.StockQuantity <= 0)
                return BadRequest("Stock quantity must be greater than zero.");

            var existingProduct = await unitOfWork.Product.GetByIdAsync(c => c.Id == id);
            if (existingProduct == null)
                return NotFound();
            existingProduct.StockQuantity += updateDto.StockQuantity;


            await unitOfWork.Product.UpdateAsync(existingProduct);
            await unitOfWork.SaveAsync();
            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductDto updateDto)
        {
            if (updateDto == null)
                return BadRequest("Product cannot be null.");

            var categoryExists = await unitOfWork.Category.GetByIdAsync(c => c.Id == updateDto.CategoryId);
            if (categoryExists == null)
                return BadRequest($"Category with ID {updateDto.CategoryId} does not exist.");

            var existingProduct = await unitOfWork.Product.GetByIdAsync(c => c.Id == id);
            if (existingProduct == null)
                return NotFound();

            updateDto.Adapt(existingProduct);

            await unitOfWork.Product.UpdateAsync(existingProduct);
            await unitOfWork.SaveAsync();
            return NoContent();
        }
    }
}
