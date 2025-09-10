using HRManagementSystem.Application.DTOs;
using HRManagementSystem.Application.Users.DTOs;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

            // Hash hesapla (kaydolan user'ın salt'ı ile)
            var saltedPassword = password + user.Salt;
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var passwordHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            if (user.PasswordHash != passwordHash) return null;

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }


        private bool VerifyPassword(string password, string storedHash)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder builder = new StringBuilder();
            foreach (var b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }
            var hashString = builder.ToString();

            return hashString == storedHash;
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

                // Salt üret
                var saltBytes = new byte[16];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }
                var salt = Convert.ToBase64String(saltBytes);

                // Şifreyi salt ile hashle
                var saltedPassword = dto.Password + salt;
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                var passwordHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                var user = new User
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    Role = "Personnel" // HER ZAMAN PERSONNEL!
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role
                };
            }
            catch (DbUpdateException dbEx)
            {
                // Inner exception detayını al
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