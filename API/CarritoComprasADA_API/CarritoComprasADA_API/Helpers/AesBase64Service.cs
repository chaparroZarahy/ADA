using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace CarritoComprasADA_API.Helpers
{
    public class AesBase64Service
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public AesBase64Service(IOptions<EncryptionSettings> options)
        {
            var settings = options.Value;
            _key = Encoding.UTF8.GetBytes(settings.Key.PadRight(16).Substring(0, 16));
            _iv = Encoding.UTF8.GetBytes(settings.IV.PadRight(16).Substring(0, 16));
        }

        public string EncryptToBase64(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public string DecryptFromBase64(string base64Cipher)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(base64Cipher);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
