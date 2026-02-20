using System;
using System.Security.Cryptography;
using System.Text;

namespace ProjectManagementApi.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateSecureToken(int length = 32)
        {
            var randomBytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
