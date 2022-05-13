using System.Security.Cryptography;
using System.Text;

namespace Terminal.Services;

internal class HashService
{
    internal static string GetHashSha256(params string[] text)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(text.Aggregate((acc, cur) => $"{acc}/{cur}"));
        var hashstring = new SHA256Managed();
        byte[] hash = hashstring.ComputeHash(bytes);
        string hashString = string.Empty;
        foreach (byte x in hash)
        {
            hashString += String.Format("{0:x2}", x);
        }
        return hashString;
    }
}