using HRManagementSystem.Application.Users.DTOs;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Application.Services
{
    public class UserService
    {
        private readonly HRDbContext _context;

        public UserService(HRDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Basit bir karma işlemi (BCrypt daha güvenlidir)
            var passwordHash = ComputeSha256Hash(createUserDto.Password);

            var user = new User
            {
                FullName = createUserDto.FullName,
                Email = createUserDto.Email,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }

            user.FullName = updateUserDto.FullName;
            user.Email = updateUserDto.Email;

            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Basit karma fonksiyonu (BCrypt veya Argon2 daha iyidir)
        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Hex format
                }
                return builder.ToString();
            }
        }
    }
}
