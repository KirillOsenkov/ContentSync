using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GuiLabs.Common
{
    public static class Utilities
    {
        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                list.Add(value);
            }
        }

        public static string GetMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(bytes);
                return ByteArrayToHexString(hashBytes);
            }
        }

        public static string ByteArrayToHexString(byte[] bytes)
        {
            int digits = bytes.Length * 2;

            char[] c = new char[digits];
            byte b;
            for (int i = 0; i < digits / 2; i++)
            {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 87 : b + 0x30);
                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 87 : b + 0x30);
            }

            return new string(c);
        }
    }
}
