using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Utils
{
    public static class PasswordUtils
    {
        public static string GetPasswordHash(string salt, string passwordOrToken)
        {
            if (string.IsNullOrEmpty(passwordOrToken) || string.IsNullOrEmpty(salt))
            {
                return "";
            }

            string txt2 = salt + passwordOrToken;

            return PasswordHash(txt2);
        }

        public static string GetRandomBytes()
        {
            byte[] buff = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(buff);
        }

        public static string QuickHash (string txt)
        {
            MD5 algo = MD5.Create();
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(txt);
            byte[] arr2 = algo.ComputeHash(arr);
            return Convert.ToBase64String(arr2);
        }

        private static string PasswordHash(string txt)
        {
            // For now to avoid adding keygen lib...stronger hashes don't work always too.
            SHA256 algo = SHA256.Create();
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(txt);
            byte[] arr2 = algo.ComputeHash(arr);
            return Convert.ToBase64String(arr2);
        }
    }
}
