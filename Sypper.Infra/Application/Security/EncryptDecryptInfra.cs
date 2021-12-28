using System.Security.Cryptography;
using System.Text;

namespace Sypper.Infra.Application.Security
{
    public class EncryptDecryptInfra
    {
        public byte[] Key { get; set; }
        public byte[] IniVetor { get; set; }
        public Aes Algorithm { get; set; }

        public EncryptDecryptInfra(byte[] key)
        {
            this.Key = key;
            this.IniVetor = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            this.Algorithm = Aes.Create();
        }

        public EncryptDecryptInfra(byte[] key, byte[] iniVetor)
        {
            this.Key = key;
            this.IniVetor = iniVetor;
            this.Algorithm = Aes.Create();
        }

        public string Encrypt(string entryText)
        {
            byte[] symEncryptedData;

            var dataToProtectAsArray = Encoding.UTF8.GetBytes(entryText);
            using (var encryptor = this.Algorithm.CreateEncryptor(this.Key, this.IniVetor))
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(dataToProtectAsArray, 0, dataToProtectAsArray.Length);
                cryptoStream.FlushFinalBlock();
                symEncryptedData = memoryStream.ToArray();
            }
            this.Algorithm.Dispose();
            return Convert.ToBase64String(symEncryptedData);
        }

        public string Decrypt(string entryText)
        {
            var symEncryptedData = Convert.FromBase64String(entryText);
            byte[] symUnencryptedData;
            using (var decryptor = this.Algorithm.CreateDecryptor(this.Key, this.IniVetor))
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(symEncryptedData, 0, symEncryptedData.Length);
                cryptoStream.FlushFinalBlock();
                symUnencryptedData = memoryStream.ToArray();
            }
            this.Algorithm.Dispose();
            return System.Text.Encoding.Default.GetString(symUnencryptedData);
        }
    }
}
