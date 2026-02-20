// Utils/PasswordGenerator.cs (or wherever you prefer to put helper classes)
using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.HumanResourceManagementApi.DTOs   // Adjust namespace as needed
{
    public static class PasswordGenerator
    {
        private const string CharsLower = "abcdefghijklmnopqrstuvwxyz";
        private const string CharsUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string CharsDigit = "0123456789";
        private const string CharsSpecial = "!@#$%^&*()-_+=[]{}|;:,.<>?";

        public static string GenerateRandomPassword(int length = 12)
        {
            if (length < 8) // Minimum length for security
                length = 8;

            StringBuilder password = new StringBuilder();
            Random random = new Random();

            // Ensure at least one from each required category
            password.Append(CharsLower[random.Next(CharsLower.Length)]);
            password.Append(CharsUpper[random.Next(CharsUpper.Length)]);
            password.Append(CharsDigit[random.Next(CharsDigit.Length)]);
            password.Append(CharsSpecial[random.Next(CharsSpecial.Length)]);

            // Fill the remaining length with random characters from all categories
            string allChars = CharsLower + CharsUpper + CharsDigit + CharsSpecial;
            for (int i = password.Length; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password to ensure randomness of character positions
            return Shuffle(password.ToString());
        }

        private static string Shuffle(string str)
        {
            char[] array = str.ToCharArray();
            Random random = new Random();
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                char value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
            return new string(array);
        }
    }
}
namespace ProjectManagementApi.DTOs
{
    public class PasswordGenarator
    {
    }
}
