using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Squad.Service.Utilities
{
    public class Cipher
    {

        private readonly ILogger<Cipher> logger;
        private KeyValue key;

        public Cipher()
        {

            /*var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            logger = loggerFactory.CreateLogger<Cipher>();*/
            this.key = new KeyValue
            {
                PassPhrase = Environment.GetEnvironmentVariable("PASS_PHRASE"),
                SaltValue = Environment.GetEnvironmentVariable("SALT_VALUE"),
                InitVector = Environment.GetEnvironmentVariable("INIT_VECTOR"),
                PasswordIterations = 2,
                Blocksize = 32
            };
        }
        public static string message { get; set; }



        private struct KeyValue
        {
            public string PassPhrase { get; set; }
            public string SaltValue { get; set; }
            public string InitVector { get; set; }
            public int PasswordIterations { get; set; }
            public int Blocksize { get; set; }
        }

        public static string EncryptRequest(string clearText)
        {

            var key = new Cipher().key;
            string cipherText = "";
            try
            {
                var plainText = clearText;
                var saltValueBytes = Encoding.ASCII.GetBytes(key.SaltValue);
                var password = new Rfc2898DeriveBytes(key.PassPhrase, saltValueBytes, key.PasswordIterations);
                var keyBytes = password.GetBytes(key.Blocksize);
                var symmetricKey = new RijndaelManaged();
                var initVectorBytes = Encoding.ASCII.GetBytes(key.InitVector);
                var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                var memoryStream = new MemoryStream();
                var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                var cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                cipherText = Convert.ToBase64String(cipherTextBytes);
                Console.WriteLine(cipherText);
                Console.WriteLine("\n end");
                return cipherText;
            }
            catch (Exception ex)
            {
                var message = "";
                if (ex.Message.Contains("Padding is invalid"))
                {
                    message = "Invalid Keys";
                    return message;
                }
                if (ex.Message.Contains("The input is not a valid Base-64 string "))
                {
                    message = ex.Message;
                    return message;
                }
            }
            return cipherText;
        }



        public static string DecryptResponse(string clearText)
        {
            try
            {
                var key = new Cipher().key;
                string cipherText = "";
                var plainText = clearText;
                var saltValueBytes = Encoding.ASCII.GetBytes(key.SaltValue);
                var password = new Rfc2898DeriveBytes(key.PassPhrase, saltValueBytes, key.PasswordIterations);
                var keyBytes = password.GetBytes(key.Blocksize);
                var symmetricKey = new RijndaelManaged();

                var initVectorBytes = Encoding.ASCII.GetBytes(key.InitVector);
                var encryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                ICryptoTransform decryptor = encryptor;
                byte[] buffer = Convert.FromBase64String(plainText);
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    using (CryptoStream cs = new CryptoStream((Stream)ms, decryptor,
                    CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cs))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "";
                if (ex.Message.Contains("Padding is invalid"))
                {
                    message = "Invalid Keys";
                    return message;
                }
                if (ex.Message.Contains("The input is not a valid Base-64 string "))
                {
                    message = ex.Message;
                    return message;
                }
            }
            return "";
        }


    }
}
