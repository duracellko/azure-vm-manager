using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordUtil
{
    public class Program
    {
        private static readonly byte[] encryptionKey = new byte[] { 70, 100, 252, 118, 174, 154, 32, 143, 40, 95, 157, 165, 104, 190, 95, 129, 92, 77, 172, 87, 220, 14, 246, 121, 180, 198, 35, 196, 214, 213, 107, 62 };

        [STAThread]
        public static void Main(string[] args)
        {
            Console.WriteLine(Properties.Resources.Header);
            Console.WriteLine();
            
            Console.WriteLine(Properties.Resources.EnterPassword);
            var password = Console.ReadLine();

            var encryptedPassword = string.IsNullOrEmpty(password) ? string.Empty : EncryptString(password);
            Console.WriteLine(Properties.Resources.EncryptedPassword);
            Console.WriteLine(encryptedPassword);

            if (!string.IsNullOrEmpty(encryptedPassword))
            {
                System.Windows.Forms.Clipboard.SetText(encryptedPassword);
                Console.WriteLine();
                Console.WriteLine(Properties.Resources.CopiedToClipboard);
            }
        }

        private static string EncryptString(string value)
        {
            var encoding = Encoding.Unicode;
            var valueBytes = encoding.GetBytes(value);
            var encryptedBytes = EncryptBytes(valueBytes);
            return Convert.ToBase64String(encryptedBytes);
        }

        private static byte[] EncryptBytes(byte[] buffer)
        {
            Stream stream = null;
            try
            {
                var memoryStream = new MemoryStream();
                stream = memoryStream;
                using (var aes = Aes.Create())
                {
                    aes.Key = encryptionKey;
                    var iv = aes.IV;
                    stream.Write(iv, 0, iv.Length);

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        using (var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                        {
                            stream = null;
                            cryptoStream.Write(buffer, 0, buffer.Length);
                            cryptoStream.FlushFinalBlock();
                            return memoryStream.ToArray();
                        }
                    }
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }
    }
}
