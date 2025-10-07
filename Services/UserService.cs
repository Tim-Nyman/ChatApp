using ChatApp.Data.Models;
using ChatApp.Data.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChatApp.Services
{
    public class UserService
    {
        public UserModel? CurrentUser { get; set; }
        private readonly CosmosService _cosmosService;
        private readonly IConfiguration _configuration;

        public UserService(CosmosService cosmosService, IConfiguration configuration)
        {
            _cosmosService = cosmosService;
            _configuration = configuration;
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

            if (!valid) return null;

            CurrentUser = user;

            var jwtSection = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSection["Key"]);
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return user;
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

