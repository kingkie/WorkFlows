using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;
using System.IO;

namespace Yu3zx.Util
{
    public class EncryptUtil
    {
        /// <summary>
        /// MD5
        /// </summary>
        /// <param name="encypStr">需要加密计算的字符串</param>
        /// <returns>加密的字符串</returns>
        public static string Md5(string encypStr)
        {
            string retStr;
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();
            byte[] inputBye;
            byte[] outputBye;
            inputBye = System.Text.Encoding.ASCII.GetBytes(encypStr);
            outputBye = m5.ComputeHash(inputBye);
            retStr = Convert.ToBase64String(outputBye);
            return (retStr);
        }
        /// <summary>
        /// 系统自带Base64加密
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// 系统自带Base64解密
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string Base64Decode(string base64EncodedData)
        {
            byte[] bytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        
        /// <summary>
        /// Base64
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(bytes, 0, bytes.Length);
            }
            memoryStream.Position = 0L;
            byte[] array = new byte[memoryStream.Length];
            memoryStream.Read(array, 0, array.Length);
            byte[] array2 = new byte[array.Length + 4];
            System.Buffer.BlockCopy(array, 0, array2, 4, array.Length);
            System.Buffer.BlockCopy(System.BitConverter.GetBytes(bytes.Length), 0, array2, 0, 4);
            return System.Convert.ToBase64String(array2);
        }
        /// <summary>
        /// Base64
        /// </summary>
        /// <param name="compressedText"></param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] array = System.Convert.FromBase64String(compressedText);
            string @string;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                int num = System.BitConverter.ToInt32(array, 0);
                memoryStream.Write(array, 4, array.Length - 4);
                byte[] array2 = new byte[num];
                memoryStream.Position = 0L;
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(array2, 0, array2.Length);
                }
                @string = System.Text.Encoding.UTF8.GetString(array2);
            }
            return @string;
        }

        #region DES对称加密解密(密钥加密：加密和解密双方要使用同一个密钥)

        /**//// <summary>
        /// DES加密
        /// </summary>
        /// <param name="encryptString"></param>
        /// <returns></returns>
        public static string DesEncrypt(string encryptString, string key = "Yu3zx123")
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 8));
            byte[] keyIV = keyBytes;
            byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, provider.CreateEncryptor(keyBytes, keyIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }

        /**//// <summary>
        /// DES解密
        /// </summary>
        /// <param name="decryptString"></param>
        /// <returns></returns>
        public static string DesDecrypt(string decryptString, string key = "Yu3zx123")
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 8));
            byte[] keyIV = keyBytes;
            byte[] inputByteArray = Convert.FromBase64String(decryptString);
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, provider.CreateDecryptor(keyBytes, keyIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(mStream.ToArray());
        }
        #endregion
    }
}
