﻿using Dumpify;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LibraryManageSystemApi.Extension
{
    public class MD5Helper
    {
        /// <summary>
        /// 生成PasswordSalt
        /// </summary>
        /// <returns>返回string</returns>
        public static string GenerateSalt()
        {
            byte[] buf = new byte[16];
#pragma warning disable SYSLIB0023 // 类型或成员已过时
            (new RNGCryptoServiceProvider()).GetBytes(buf);
#pragma warning restore SYSLIB0023 // 类型或成员已过时
            return Convert.ToBase64String(buf);
        }

        public const string allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        public static string FilterString( string inputString, string allowedCharacters = allowedCharacters)
        {
            // 使用正则表达式匹配字符串中包含在指定字符集中的字符
            string pattern = $"[^{Regex.Escape(allowedCharacters)}]";
            string filteredString = Regex.Replace(inputString, pattern, "");
            return filteredString;
        }


        /// <summary>
        /// 加密密码
        /// </summary>
        /// <param name="pass">密码</param>
        /// <param name="passwordFormat">加密类型</param>
        /// <param name="salt">PasswordSalt</param>
        /// <returns>加密后的密码</returns>
        public static string SHA2Encode(string pass, string salt, int passwordFormat = 1)
        {
            try
            {
                if (passwordFormat == 0) // MembershipPasswordFormat.Clear
                    return pass;
                byte[] bIn = Encoding.Unicode.GetBytes(pass);
                byte[] bSalt = Encoding.Unicode.GetBytes(salt);
                byte[] bAll = new byte[bSalt.Length + bIn.Length];
                byte[]? bRet = null;

                Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
                Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);

#pragma warning disable SYSLIB0021 // 类型或成员已过时
                var s = SHA512Managed.Create();
#pragma warning restore SYSLIB0021 // 类型或成员已过时
                bRet = s.ComputeHash(bAll);

                return ConvertEx.ToUrlBase64String(bRet);
            }
            catch (Exception ex)
            {
                ex.Dump();
                throw new Exception(ex.Message);
            }
           
        }
    }
    public class ConvertEx
    {
        static readonly char[] padding = { '=' };
        public static string ToUrlBase64String(byte[] inArray)
        {
            var str = Convert.ToBase64String(inArray);
            str = str.TrimEnd(padding).Replace('+', '-').Replace('/', '_');

            return str;
        }

        public static byte[] FromUrlBase64String(string s)
        {
            string incoming = s.Replace('_', '/').Replace('-', '+');
            switch (s.Length % 4)
            {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }
            byte[] bytes = Convert.FromBase64String(incoming);

            return bytes;
        }
    }
}
