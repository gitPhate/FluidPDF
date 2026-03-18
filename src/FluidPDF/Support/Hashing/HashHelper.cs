using System;
using System.Security.Cryptography;
using System.Text;

namespace FluidPDF.Support.Hashing
{
    internal static class HashHelper
    {
        internal static string HashSHA256(string input)
        {
#if NETSTANDARD2_0
            using SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", string.Empty) + ".dll";
#else
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash) + ".dll";
#endif
        }
    }
}
