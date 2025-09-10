using HRManagementSystem.Application.DTOs;
using HRManagementSystem.Application.Users.DTOs;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRManagementSystem.Application.Services
{
    public class AuthService
    {
        private readonly HRDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(HRDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<UserDto?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            // Salt kaldırıldı. Sadece hash kontrolü yapılacak.
            var passwordHash = ComputeHash(password);
            if (user.PasswordHash != passwordHash) return null;

            // JWT Token üret
            var token = GenerateJwtToken(user);

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token // UserDto'ya token property eklenmeli!
            };
        }

        private string ComputeHash(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("role", user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserDto?> RegisterAsync(RegisterUserDto dto)
        {
            try
            {
                // Email kontrolü (zaten varsa hata ver)
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                    throw new Exception("Bu email zaten kullanılıyor.");

                // Salt KALDIRILDI! Sadece şifre hashleniyor.
                var passwordHash = ComputeHash(dto.Password);

                var user = new User
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    Role = "Personnel" // HER ZAMAN PERSONNEL!
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // JWT Token üret
                var token = GenerateJwtToken(user);

                return new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    Token = token // UserDto'ya token property eklenmeli!
                };
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException != null ? dbEx.InnerException.Message : dbEx.Message;
                throw new Exception($"Veri kaydedilirken hata oluştu: {innerMessage}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Beklenmeyen hata: {ex.Message}");
            }
        }
    }
}