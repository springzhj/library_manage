using System.Security.Cryptography;

namespace PloliticalScienceSystemApi.Extension
{
    public class RSAFileHelper
    {
        public static RSA GetPrivateKey()
        {
            return GetRSA("key.pem");
        }
        public static RSA GetPublicKey()
        {
            return GetRSA("public.pem");
        }

        private static RSA GetRSA(string fileName)
        {
            string rootPath = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(rootPath, fileName);
            if (!System.IO.File.Exists(filePath))
                throw new Exception("文件不存在");
            string key = System.IO.File.ReadAllText(filePath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(key.AsSpan());
            return rsa;
        }
    }
}
