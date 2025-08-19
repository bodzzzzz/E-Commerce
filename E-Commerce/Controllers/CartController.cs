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
    public class CartController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCartByUserId(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            var cart = await unitOfWork.Cart.GetByIdAsync(
                c => c.UserId == userId,
                "Items",
                "Items.Product"
            );
            if (cart == null)
                return NotFound();
            var cartDto = cart.Adapt<CartDto>();
            return Ok(cartDto);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            var cart = await unitOfWork.Cart.GetByIdAsync(
                c => c.UserId == userId,
                "Items",
                "Items.Product"
            );
            if (cart == null || cart.Items == null)
                return NotFound();
            if (cart.Items.Count == 0)
                return BadRequest("Cart is already empty.");
            cart.Items.Clear();
            await unitOfWork.Cart.UpdateAsync(cart);
            await unitOfWork.SaveAsync();
            return NoContent();
        }

        [HttpPut("{userId}/update/{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int userId, int cartItemId, UpdateCartItemQuantityDto updateCartItem)
        {
            if (userId <= 0 || cartItemId <= 0 || updateCartItem is null)
                return BadRequest("Invalid user ID, cart item ID, or cart item data.");

            var cart = await unitOfWork.Cart.GetByIdAsync(c => c.UserId == userId, "Items");
            if (cart == null || cart.Items == null)
                return NotFound("Cart not found.");

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem == null)
                return NotFound($"Cart item with ID {cartItemId} does not exist in the user's cart.");

            var product = await unitOfWork.Product.GetByIdAsync(p => p.Id == cartItem.ProductId);
            if (product == null)
                return NotFound($"Product with ID {cartItem.ProductId} does not exist.");

            if (updateCartItem.Quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            int quantityDiff = updateCartItem.Quantity - cartItem.Quantity;

            if (quantityDiff > 0 && quantityDiff > product.StockQuantity)
                return BadRequest($"Insufficient stock. Only {product.StockQuantity} items available.");

            cartItem.Quantity = updateCartItem.Quantity;

            await unitOfWork.Product.UpdateAsync(product);
            await unitOfWork.CartItem.UpdateAsync(cartItem);

            await unitOfWork.SaveAsync();
            return Ok(cartItem.Adapt<CartItemDto>());
        }

        [HttpPost("{userId}/add")]
        public async Task<IActionResult> AddToCart(int userId, AddCartItemDto addToCartDto)
        {
            if (userId <= 0 || addToCartDto == null)
                return BadRequest("Invalid user ID or cart item data.");

            if (addToCartDto.Quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            var product = await unitOfWork.Product.GetByIdAsync(p => p.Id == addToCartDto.ProductId);
            if (product == null)
                return NotFound($"Product with ID {addToCartDto.ProductId} does not exist.");

            var cart = await unitOfWork.Cart.GetByIdAsync(c => c.UserId == userId, "Items");
            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                await unitOfWork.Cart.AddAsync(cart);
            }

            var existingItem = cart.Items?.FirstOrDefault(i => i.ProductId == addToCartDto.ProductId);
            if (existingItem != null)
            {
                if (addToCartDto.Quantity > product.StockQuantity)
                    return BadRequest($"Cannot add {addToCartDto.Quantity} more items. Only {product.StockQuantity} items available.");

                existingItem.Quantity += addToCartDto.Quantity;
                await unitOfWork.CartItem.UpdateAsync(existingItem);
            }
            else
            {
                if (addToCartDto.Quantity > product.StockQuantity)
                    return BadRequest($"Insufficient stock. Only {product.StockQuantity} items available.");

                var newItem = new CartItem
                {
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    CartId = cart.Id
                };
                await unitOfWork.CartItem.AddAsync(newItem);
                cart.Items?.Add(newItem);
            }

            await unitOfWork.Product.UpdateAsync(product);
            await unitOfWork.SaveAsync();
            return Ok(cart.Adapt<CartDto>());
        }
        [HttpDelete("{userId}/remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int cartItemId)
        {
            if (userId <= 0 || cartItemId <= 0)
                return BadRequest("Invalid user ID or cart item ID.");
            var cart = await unitOfWork.Cart.GetByIdAsync(
                c => c.UserId == userId,
                "Items"
            );
            if (cart == null || cart.Items == null)
                return NotFound("Cart not found or empty.");
            var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem == null)
                return NotFound($"Cart item with ID {cartItemId} does not exist in the user's cart.");

            var product = await unitOfWork.Product.GetByIdAsync(p => p.Id == cartItem.ProductId);

            await unitOfWork.Product.UpdateAsync(product);
            await unitOfWork.CartItem.DeleteAsync(cartItem);
            cart.Items.Remove(cartItem);
            await unitOfWork.SaveAsync();
            return NoContent();
        }

    }
}
