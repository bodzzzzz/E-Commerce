using E_Commerce.DTOs;
using E_Commerce.Entities;

namespace E_Commerce.IRepository
{
    public interface IAuthRepo
    {
        Task<User?> RegisterAsync(RegisterDto request);
        Task<TokenResponseDto?> LoginAsync(LoginDto request);
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}
