using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace cn.softname2.Support
{
    class securityAES
    {
        //AES加密 ECB模式 PKCS7补码
        public byte[] Encrypt(string encryptStr, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(encryptStr);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            //return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            return resultArray;
        }

        //AES解密 ECB模式 PKCS7补码
        public byte[] Decrypt(byte[] decriptbyets, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            //byte[] toEncryptArray = Convert.FromBase64String(decryptStr);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(decriptbyets, 0, decriptbyets.Length);
            //return UTF8Encoding.UTF8.GetString(resultArray);
            return resultArray;
        }


    }
}
