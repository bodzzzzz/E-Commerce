using E_Commerce.DTOs;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await unitOfWork.Category.GetAllAsync();
            var categoryDtos = categories.Adapt<IEnumerable<CategoryDto>>();
            return Ok(categoryDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await unitOfWork.Category.GetByIdAsync(c => c.Id == id);
            if (category == null)
                return NotFound();
            var categoryDto = category.Adapt<CategoryDto>();
            return Ok(categoryDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await unitOfWork.Category.GetByIdAsync(c => c.Id == id);
            if (category == null)
                return NotFound();
            await unitOfWork.Category.DeleteAsync(category);
            await unitOfWork.SaveAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto createDto)
        {
            if (createDto == null)
                return BadRequest("Category cannot be null.");

            if (!ModelState.IsValid)
                return BadRequest();

            var category = createDto.Adapt<Category>();
            await unitOfWork.Category.AddAsync(category);
            await unitOfWork.SaveAsync();

            var categoryDto = category.Adapt<CategoryDto>();
            return CreatedAtAction(nameof(GetById), new { id = categoryDto.Id }, categoryDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCategoryDto updateDto)
        {
            if (updateDto == null)
                return BadRequest("Category cannot be null.");

            var existingCategory = await unitOfWork.Category.GetByIdAsync(c => c.Id == id);
            if (existingCategory == null)
                return NotFound();

            updateDto.Adapt(existingCategory);

            await unitOfWork.Category.UpdateAsync(existingCategory);
            await unitOfWork.SaveAsync();
            return NoContent();
        }
    }
}