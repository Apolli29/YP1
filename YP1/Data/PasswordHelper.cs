using System.Security.Cryptography;
using System.Text;

namespace YP1.Data
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes;

            using (SHA256 sha256 = SHA256.Create())
            {
                hashBytes = sha256.ComputeHash(inputBytes);
            }

            StringBuilder builder = new StringBuilder();

            foreach (byte currentByte in hashBytes)
            {
                builder.Append(currentByte.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
