using E_Commerce.DTOs;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpPost("Checkout/{userId}")]
        [Authorize]
        public async Task<IActionResult> CheckoutAsync(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var cart = await unitOfWork.Cart.GetByIdAsync(c => c.UserId == userId, "Items", "Items.Product");
            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                return BadRequest("Cart is empty or does not exist.");
            }
            foreach (var item in cart.Items)
            {
                if (item.Product.StockQuantity < item.Quantity)
                {
                    return BadRequest($"Not enough stock for {item.Product.Name}. Only {item.Product.StockQuantity} left.");
                }
            }

            foreach (var item in cart.Items)
            {
                item.Product.StockQuantity -= item.Quantity;
                await unitOfWork.Product.UpdateAsync(item.Product);
            }


            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.Product.Price),
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
                }).ToList()
            };
            await unitOfWork.Order.AddAsync(order);
            await unitOfWork.SaveAsync();
            // Clear the cart after successful checkout
            cart.Items.Clear();
            await unitOfWork.Cart.UpdateAsync(cart);
            await unitOfWork.SaveAsync();
            var orderDto = order.Adapt<OrderDto>();

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, orderDto);


        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid order ID.");
            }
            var order = await unitOfWork.Order.GetByIdAsync(o => o.Id == id, "OrderItems", "OrderItems.Product");
            if (order == null)
            {
                return NotFound();
            }
            var orderDto = order.Adapt<OrderDto>();

            return Ok(orderDto);
        }
        [HttpGet("Admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await unitOfWork.Order.GetAllAsync("OrderItems", "OrderItems.Product");
            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found.");
            }
            var orderDtos = orders.Adapt<IEnumerable<OrderDto>>();
            return Ok(orderDtos);
        }
    }
}
