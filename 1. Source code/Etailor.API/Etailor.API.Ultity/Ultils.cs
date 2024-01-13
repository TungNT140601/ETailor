using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Etailor.API.Ultity
{
    public static class Ultils
    {
        public static string GenGuidString()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString().Substring(0, 30);
        }
        public static String HmacSHA512(string key, String inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
        
    }
}