using System.Security.Cryptography;
using System.Text;

namespace api.services.Handlers
{
    public class EncriptHandler
    {
        public static string Hash(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] hashBytes = SHA256.HashData(textBytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public static string Encode(string text)
        {
            return Hash(text);
        }

        public static bool Verify(string text, string hash)
        {
            return Hash(text).Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}