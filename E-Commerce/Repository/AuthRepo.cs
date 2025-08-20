using E_Commerce.Data;
using E_Commerce.DTOs;
using E_Commerce.Entities;
using E_Commerce.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace E_Commerce.Repository
{
    public class AuthRepo(EcommerceDbContext context ,IConfiguration configuration) : IAuthRepo
    {
        public async Task<TokenResponseDto?> LoginAsync(LoginDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return null; // User not found
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user,user.PasswordHash,request.Password)==PasswordVerificationResult.Failed )
            {

                return null;
            }
            return await CreateTokenResponse(user);
        }
        private string GenerateRefreshtoken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);

        }
        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto { AccessToken=CreateToken(user),
            RefreshToken=await GenerateAndSaveRefreshTokenAsync(user),
            };
        }
        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow || user.RefreshToken != refreshToken)
            {
                return null; // Invalid or expired refresh token
            }
            return user;
        }
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshtoken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
            {
                return null;
            }
            return await CreateTokenResponse(user);
        }

        public async Task<User?> RegisterAsync(RegisterDto request)
        {
            if(await context.Users.AnyAsync(u=>u.Username==request.Username))
                {   
                return null; // User already exists
            }
            var user = new User();
           var passwordHash = new PasswordHasher<User>();
            user.Username = request.Username;
            user.PasswordHash = passwordHash.HashPassword(user, request.Password);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var cart = new Cart
            {
                UserId = user.Id
            };
            context.Carts.Add(cart);
            await context.SaveChangesAsync();
            return user;



        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Token"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var TokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
              audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds

                );
            return new JwtSecurityTokenHandler().WriteToken(TokenDescriptor);
        }
    }
}
