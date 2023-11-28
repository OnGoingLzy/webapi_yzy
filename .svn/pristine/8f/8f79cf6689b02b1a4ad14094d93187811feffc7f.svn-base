using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace webapi_yzy
{
    public class Signature
    {
        /// <summary>
        /// 生成签名的算法
        /// </summary>
        /// <param name="body">数据</param>
        /// <param name="method">方法</param>
        /// <param name="appid">用户id</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <returns></returns>
        public static string createSignature(string body, string method, string appid,string appsecret,string timestamp, string nonce)
        {
            //body进行md5加密
            string content_md5 = Convert.ToBase64String(calcMD5(Encoding.UTF8.GetBytes(body)));
            //生成签名
            string stringToSign = "x-ca-key:" + appid + "&x-ca-nonce:" + nonce + "&x-ca-timestamp:" + timestamp + "&x-content-md5:" + content_md5 + "&x-service-method:" + method;
            string signature = Convert.ToBase64String(
                hmacSHA256(Encoding.UTF8.GetBytes(stringToSign)
                , Encoding.UTF8.GetBytes(appsecret)
                ));

            return signature;
        }
        /// <summary>
        /// 计算字节数组的MD5值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] calcMD5(byte[] buffer)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] md5Bytes = md5.ComputeHash(buffer);
                return md5Bytes;
            }
        }

        /// <summary>
        /// HmacSHA256算法,返回的结果始终是32位
        /// </summary>
        /// <param name="content">待加密的内容</param>
        /// <param name="key">加密的键，可以是任何数据</param>
        /// <returns></returns>
        public static byte[] hmacSHA256(byte[] content, byte[] key)
        {
            using (var hmacsha256 = new HMACSHA256(key))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(content);
                return hashmessage;
            }
        }

    }
}
