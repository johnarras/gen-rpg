using System;
using System.Text;
using System.Security.Cryptography;

public class CryptoUtils
{
    public static string EncryptString(string txt)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(txt);
        SymmetricAlgorithm alg = TripleDES.Create();
        alg.Mode = CipherMode.ECB;
        ICryptoTransform trans = alg.CreateEncryptor(GetKey(), GetIV());
        return Convert.ToBase64String(trans.TransformFinalBlock(bytes, 0, bytes.Length));
    }

    public static string DecryptString(string txt)
    {
        byte[] bytes = Convert.FromBase64String(txt);
        SymmetricAlgorithm alg = TripleDES.Create();
        alg.Mode = CipherMode.ECB;
        ICryptoTransform trans = alg.CreateDecryptor(GetKey(), GetIV());
        return Encoding.UTF8.GetString(trans.TransformFinalBlock(bytes, 0, bytes.Length));
    }

    public static string GetDeviceId()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(AppUtils.DeviceUniqueIdentifier);
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        return Convert.ToBase64String(provider.ComputeHash(bytes));
    }

    static byte[] GetKey()
    {
        int keySize = 24;
        byte[] bytes = Encoding.UTF8.GetBytes(AppUtils.DeviceUniqueIdentifier);
        byte[] finalBytes = new byte[keySize];

        for (int b = 0; b < bytes.Length; b++)
        {
            finalBytes[b % finalBytes.Length] ^= bytes[b];
        }
        return finalBytes;
    }

    static byte[] GetIV()
    {
        return new byte[] { 77, 1, 12, 37, 33, 98, 49, 22 };
    }
}