using E_Commerce.DTOs;
using E_Commerce.Entities;
using E_Commerce.Helpers;
using E_Commerce.Hubs;
using E_Commerce.IRepository;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace E_Commerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductController> _logger;
        private readonly IHubContext<StockHub> _stockHub;

        public ProductController(
            IUnitOfWork unitOfWork, 
            IWebHostEnvironment webHostEnvironment, 
            ILogger<ProductController> logger,
            IHubContext<StockHub> stockHub)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _stockHub = stockHub;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _unitOfWork.Product.GetAllAsync("Category");
            var productsDtos = products.Adapt<IEnumerable<ProductDto>>();
            return Ok(productsDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var Product = await _unitOfWork.Product.GetByIdAsync(c => c.Id == id, "Category");
            if (Product == null)
                return NotFound();
            var ProductDto = Product.Adapt<ProductDto>();
            return Ok(ProductDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateProductDto createDto)
        {
            try 
            {
                if (createDto == null)
                    return BadRequest("Product cannot be null.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var categoryExists = await _unitOfWork.Category.GetByIdAsync(c => c.Id == createDto.CategoryId);
                if (categoryExists == null)
                    return BadRequest($"Category with ID {createDto.CategoryId} does not exist.");

                var product = createDto.Adapt<Product>();

                // Handle image upload
                if (createDto.Image != null)
                {
                    _logger.LogInformation("Processing image upload: {FileName}, Length: {Length}", 
                        createDto.Image.FileName, createDto.Image.Length);
                    
                    try
                    {
                        string imageUrl = await FileHelper.SaveImageAsync(createDto.Image, _webHostEnvironment.WebRootPath);
                        _logger.LogInformation("Image saved successfully. URL: {ImageUrl}", imageUrl);
                        product.ImageUrl = imageUrl;
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(ex, "Error saving image");
                        return BadRequest(ex.Message);
                    }
                }

                await _unitOfWork.Product.AddAsync(product);
                await _unitOfWork.SaveAsync();

                // Notify clients about the new product stock
                await _stockHub.Clients.All.SendAsync("ReceiveStockUpdate", product.Id, product.StockQuantity);

                var productDto = product.Adapt<ProductDto>();
                _logger.LogInformation("Product created successfully. ID: {ProductId}, ImageUrl: {ImageUrl}", 
                    productDto.Id, productDto.ImageUrl);
                    
                return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Product.GetByIdAsync(c => c.Id == id);
            if (product == null)
                return NotFound();

            // Delete associated image
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                FileHelper.DeleteImage(product.ImageUrl, _webHostEnvironment.WebRootPath);
            }

            await _unitOfWork.Product.DeleteAsync(product);
            await _unitOfWork.SaveAsync();

            // Notify clients about the product removal
            await _stockHub.Clients.All.SendAsync("ReceiveStockUpdate", id, 0);

            return NoContent();
        }

        [HttpPut("{id}/AddStock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddStock(int id, UpdateProductStockDto updateDto)
        {
            if (updateDto == null)
                return BadRequest("Stock cannot be null.");

            if (updateDto.StockQuantity <= 0)
                return BadRequest("Stock quantity must be greater than zero.");

            var existingProduct = await _unitOfWork.Product.GetByIdAsync(c => c.Id == id);
            if (existingProduct == null)
                return NotFound();

            existingProduct.StockQuantity += updateDto.StockQuantity;
            await _unitOfWork.Product.UpdateAsync(existingProduct);
            await _unitOfWork.SaveAsync();

            // Notify clients about the stock update
            await _stockHub.Clients.All.SendAsync("ReceiveStockUpdate", id, existingProduct.StockQuantity);

            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto updateDto)
        {
            try
            {
                if (updateDto == null)
                    return BadRequest("Product cannot be null.");

                var categoryExists = await _unitOfWork.Category.GetByIdAsync(c => c.Id == updateDto.CategoryId);
                if (categoryExists == null)
                    return BadRequest($"Category with ID {updateDto.CategoryId} does not exist.");

                var existingProduct = await _unitOfWork.Product.GetByIdAsync(c => c.Id == id);
                if (existingProduct == null)
                    return NotFound();

                var oldStockQuantity = existingProduct.StockQuantity;

                // Handle image update if new image is provided
                if (updateDto.Image != null)
                {
                    _logger.LogInformation("Processing image update: {FileName}, Length: {Length}", 
                        updateDto.Image.FileName, updateDto.Image.Length);

                    try
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                        {
                            FileHelper.DeleteImage(existingProduct.ImageUrl, _webHostEnvironment.WebRootPath);
                        }

                        // Save new image
                        string imageUrl = await FileHelper.SaveImageAsync(updateDto.Image, _webHostEnvironment.WebRootPath);
                        _logger.LogInformation("Image updated successfully. New URL: {ImageUrl}", imageUrl);
                        existingProduct.ImageUrl = imageUrl;
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(ex, "Error updating image");
                        return BadRequest(ex.Message);
                    }
                }

                // Update other properties
                updateDto.Adapt(existingProduct, typeof(UpdateProductDto), typeof(Product));
                await _unitOfWork.Product.UpdateAsync(existingProduct);
                await _unitOfWork.SaveAsync();

                // If stock quantity changed, notify clients
                if (oldStockQuantity != existingProduct.StockQuantity)
                {
                    await _stockHub.Clients.All.SendAsync("ReceiveStockUpdate", id, existingProduct.StockQuantity);
                }

                _logger.LogInformation("Product updated successfully. ID: {ProductId}, ImageUrl: {ImageUrl}", 
                    existingProduct.Id, existingProduct.ImageUrl);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                throw;
            }
        }
    }
}
