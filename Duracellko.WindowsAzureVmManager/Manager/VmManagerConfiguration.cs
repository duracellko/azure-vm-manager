using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Manager
{
    public class VmManagerConfiguration
    {
        #region Fields

        private static readonly byte[] encryptionKey = new byte[] { 70, 100, 252, 118, 174, 154, 32, 143, 40, 95, 157, 165, 104, 190, 95, 129, 92, 77, 172, 87, 220, 14, 246, 121, 180, 198, 35, 196, 214, 213, 107, 62 };

        private readonly Lazy<byte[]> certificateData = new Lazy<byte[]>(LoadCertificateData);
        private readonly Lazy<SecureString> certificatePassword = new Lazy<SecureString>(LoadCertificatePassword);

        #endregion

        #region Properties

        public virtual string SubscriptionId
        {
            get
            {
                return ConfigurationManager.AppSettings["WindowsAzureSubscriptionID"];
            }
        }

        public virtual byte[] CertificateData
        {
            get
            {
                return this.certificateData.Value;
            }
        }

        public virtual SecureString CertificatePassword
        {
            get
            {
                return this.certificatePassword.Value;
            }
        }

        #endregion

        #region Private methods

        private static byte[] LoadCertificateData()
        {
            var path = ConfigurationManager.AppSettings["WindowsAzureClientCertificate"];
            return File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
        }

        private static SecureString LoadCertificatePassword()
        {
            var encryptedPassword = ConfigurationManager.AppSettings["WindowsAzureClientCertificatePassword"];
            if (string.IsNullOrEmpty(encryptedPassword))
            {
                return null;
            }

            var encryptedPasswordBytes = Convert.FromBase64String(encryptedPassword);
            var passwordBytes = DecryptBytes(encryptedPasswordBytes);
            var encoding = Encoding.Unicode;
            var password = encoding.GetChars(passwordBytes);
            
            var result = new SecureString();
            for (int i = 0; i < password.Length; i++)
            {
                result.AppendChar(password[i]);
            }
            result.MakeReadOnly();
            return result;
        }

        private static byte[] DecryptBytes(byte[] encryptedBytes)
        {
            Stream sourceStream = null;
            try
            {
                sourceStream = new MemoryStream(encryptedBytes);
                using (var aes = Aes.Create("AesManaged"))
                {
                    aes.Key = encryptionKey;
                    var iv = new byte[aes.BlockSize / 8];
                    sourceStream.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        using (var cryptoStream = new CryptoStream(sourceStream, decryptor, CryptoStreamMode.Read))
                        {
                            sourceStream = null;
                            using (var targetStream = new MemoryStream())
                            {
                                cryptoStream.CopyTo(targetStream);
                                return targetStream.ToArray();
                            }
                        }
                    }
                }
            }
            finally
            {
                if (sourceStream != null)
                {
                    sourceStream.Dispose();
                }
            }
        }

        #endregion
    }
}
