using System;
using System.Security.Cryptography;
using System.Text;

namespace ProjectManagementApi.Helpers
{
    public static class HashHelper
    {
        // Returns hex string for password storage
        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}