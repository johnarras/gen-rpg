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
        public static string GetPasswordHash(string userSalt, string password)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(userSalt))
            {
                return "";
            }

            string txt2 = userSalt + password;
            SHA256 sha = SHA256.Create();
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(txt2);
            byte[] arr2 = sha.ComputeHash(arr);
            return Convert.ToBase64String(arr2);
        }

        public static string GeneratePasswordSalt()
        {
            byte[] buff = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(buff);
        }



    }
}
