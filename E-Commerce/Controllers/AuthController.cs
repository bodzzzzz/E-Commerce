using E_Commerce.DTOs;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController (IAuthRepo authRepo) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto request)
        {
           var user = await authRepo.RegisterAsync(request);
            if (user is null)
            {
                return BadRequest("User already exists");
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginDto request)
        {
            var tokenResponse = await authRepo.LoginAsync(request);
            if (tokenResponse is null)
            {
                return Unauthorized("Invalid username or password");
            }
            return Ok(tokenResponse);
        }
        [Authorize]
        [HttpGet()]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("you are authenticated");
        }
        [Authorize(Roles ="Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("you are authenticated As an admin!");
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var tokenResponse = await authRepo.RefreshTokenAsync(request);
            if (tokenResponse is null || tokenResponse.AccessToken is null || tokenResponse.RefreshToken is null)
            {
                return Unauthorized("Invalid or expired refresh token");
            }
            return Ok(tokenResponse);
        }
    }
}
