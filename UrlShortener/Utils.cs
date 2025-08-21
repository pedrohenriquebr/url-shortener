using System.Security.Cryptography;
using System.Text;

namespace UrlShortener;

public static class Utils
{
    public static string CreateMD5(string input)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
    }
}