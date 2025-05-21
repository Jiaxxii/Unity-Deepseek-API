using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Xiyu.DeepSeek.Cryptography
{
    public static class CryptoExtensions
    {
        private const int KeySaltSize = 16; // 16字节盐值
        private const int KeyIterations = 100000; // PBKDF2迭代次数
        private const int KeySize = 32; // AES-256密钥长度
        private const int KeyIvSize = 16; // AES IV长度

        // 加密方法
        public static string Encrypt(string plainText, string password)
        {
            // 生成随机盐值
            byte[] salt = GenerateSalt();

            // 派生密钥和IV
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, KeyIterations);
            (byte[] key, byte[] iv) = DeriveKeyAndIv(deriveBytes);

            // 执行AES加密
            byte[] cipherBytes;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(plainBytes, 0, plainBytes.Length);
                }

                cipherBytes = ms.ToArray();
            }

            // 组合盐值+密文
            byte[] combinedBytes = CombineBytes(salt, cipherBytes);
            return Convert.ToBase64String(combinedBytes);
        }

        // 解密方法
        public static string Decrypt(string cipherText, string password)
        {
            if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(password))
            {
                throw new CryptographicException("Cipher text and password must not be null or empty.");
            }

            try
            {
                var combinedBytes = Convert.FromBase64String(cipherText);
                // 分离盐值和密文
                var (salt, cipherBytes) = SplitBytes(combinedBytes);

                // 派生密钥和IV
                using var deriveBytes = new Rfc2898DeriveBytes(password, salt, KeyIterations);
                var (key, iv) = DeriveKeyAndIv(deriveBytes);

                // 执行AES解密
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var ms = new MemoryStream(cipherBytes);
                using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (FormatException)
            {
                throw new CryptographicException("Cipher text is not a valid Base64 string.");
            }
            catch (CryptographicException)
            {
                throw new CryptographicException("Invalid password or cipher text.");
            }
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[KeySaltSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        private static (byte[] key, byte[] iv) DeriveKeyAndIv(Rfc2898DeriveBytes deriveBytes)
        {
            var key = deriveBytes.GetBytes(KeySize);
            var iv = deriveBytes.GetBytes(KeyIvSize);
            return (key, iv);
        }

        private static byte[] CombineBytes(byte[] salt, byte[] cipherBytes)
        {
            var combinedBytes = new byte[salt.Length + cipherBytes.Length];
            Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
            Buffer.BlockCopy(cipherBytes, 0, combinedBytes, salt.Length, cipherBytes.Length);
            return combinedBytes;
        }

        private static (byte[] salt, byte[] cipherBytes) SplitBytes(byte[] combinedBytes)
        {
            var salt = new byte[KeySaltSize];
            var cipherBytes = new byte[combinedBytes.Length - KeySaltSize];
            Buffer.BlockCopy(combinedBytes, 0, salt, 0, KeySaltSize);
            Buffer.BlockCopy(combinedBytes, KeySaltSize, cipherBytes, 0, cipherBytes.Length);
            return (salt, cipherBytes);
        }
    }
}