using ChatApp.Data.Models;
using ChatApp.Data.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace ChatApp.Services
{
    public class UserService
    {
        public UserModel? CurrentUser { get; set; }
        private readonly CosmosService _cosmosService;

        public UserService(CosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        public async Task RegisterUserAsync(string username, string password)
        {
            byte[] saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            string saltBase64 = Convert.ToBase64String(saltBytes);

            string passwordHash = UserService.HashPassword(password, saltBytes);

            var user = new UserModel
            {
                Username = username,
                PasswordHash = passwordHash,
                Salt = saltBase64
            };

            await _cosmosService.AddUserAsync(user);
        }

        public async Task<UserModel?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var user = await _cosmosService.GetUserByIdAsync(userId);

            if (user == null)
                Console.WriteLine($"User not found: {userId}");

            return user;
        }

        public async Task<UserModel?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("User ID cannot be null or empty", nameof(username));

            var user = await _cosmosService.GetUserByUsernameAsync(username);

            if (user == null)
                Console.WriteLine($"User not found: {username}");

            return user;
        }

        public async Task<UserModel?> LoginAsync(string username, string enteredPassword)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null) return null;

            byte[] saltBytes = Convert.FromBase64String(user.Salt);
            bool valid = VerifyPassword(enteredPassword, user.PasswordHash, saltBytes);

            if (valid) CurrentUser = user;

            return valid ? user : null;
        }


        public static string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));
        }

        private bool VerifyPassword(string enteredPassword, string storedHash, byte[] salt)
        {
            var hash = HashPassword(enteredPassword, salt);
            return hash == storedHash;
        }

    }
}

