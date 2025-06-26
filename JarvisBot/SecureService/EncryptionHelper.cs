using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;

namespace JarvisBot.SecureService
{
    public class EncryptionHelper
    {
        private readonly EncryptionSettings _encryptionSettings;
        private readonly string _masterKey;
        private readonly bool _isProduction;


        public EncryptionHelper(IOptions<EncryptionSettings> options, IHostEnvironment env)
        {
            _encryptionSettings = options.Value;
            _isProduction = env.IsProduction();

            if (_isProduction)
            {
                _masterKey = Environment.GetEnvironmentVariable("MY_SECRET_KEY");
                if (string.IsNullOrEmpty(_masterKey))
                {
                    throw new Exception("MASTER_ENCRYPTION_KEY не установлен в переменных окружения!");
                }

                _encryptionSettings.EncryptionKey = DecryptWithMasterKey(_encryptionSettings.EncryptionKey);
                _encryptionSettings.EncryptionSalt = DecryptWithMasterKey(_encryptionSettings.EncryptionSalt);
            }            
        }


        public string Decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            // Извлечение IV и зашифрованных данных
            byte[] iv = new byte[16];
            byte[] encryptedBytes = new byte[cipherBytes.Length - iv.Length];

            Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
            Array.Copy(cipherBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

            // Преобразование текстовой соли в массив байтов
            byte[] saltBytes = Encoding.UTF8.GetBytes(_encryptionSettings.EncryptionSalt);

            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = Rfc2898DeriveBytes.Pbkdf2(_encryptionSettings.EncryptionKey, saltBytes, 10000, HashAlgorithmName.SHA256, 32);
                aes.Key = keyBytes;
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.Unicode.GetString(decryptedBytes);
                }
            }
        }

        public string DecryptWithMasterKey(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            // Извлечение IV и зашифрованных данных
            byte[] iv = new byte[16];
            byte[] encryptedBytes = new byte[cipherBytes.Length - iv.Length];

            Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
            Array.Copy(cipherBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

            // Преобразование текстовой соли в массив байтов
            byte[] saltBytes = Encoding.UTF8.GetBytes(_masterKey);

            using (Aes aes = Aes.Create())
            {
                byte[] keyBytes = Rfc2898DeriveBytes.Pbkdf2(_masterKey, saltBytes, 10000, HashAlgorithmName.SHA256, 32);
                aes.Key = keyBytes;
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.Unicode.GetString(decryptedBytes);
                }
            }
        }
    }
}
