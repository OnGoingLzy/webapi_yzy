
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace webapi_yzy
{
    public class IHttpWebRequest
    {
        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="method">方法名</param>
        /// <param name="body">传输body</param>
        /// <param name="url">调用路径</param>
        /// <returns></returns>
        public static string sendHttpWebRequest(string appid,string appsecret,string method, string body,string url)
        {
            string content_md5 = Convert.ToBase64String(calcMD5(Encoding.UTF8.GetBytes(body)));
            //时间戳
            string timestamp = getTimeStamp();
            //随机数
            string nonce = Guid.NewGuid().ToString("D");
            //生成签名
            string signature = Signature.createSignature(body, method, appid, appsecret, timestamp, nonce);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            Encoding encoding = Encoding.UTF8;
            byte[] byteArray = Encoding.UTF8.GetBytes(body);
            string responseData = String.Empty;
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = byteArray.Length;

            req.Headers.Add("X-Ca-Key", appid);
            req.Headers.Add("X-Service-Method", method);
            req.Headers.Add("X-Ca-Timestamp", timestamp);
            req.Headers.Add("X-Ca-Nonce", nonce);
            req.Headers.Add("X-Content-MD5", content_md5);
            req.Headers.Add("X-Ca-Signature", signature);

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    responseData = reader.ReadToEnd();

                    //object obj = JSON.Decode(str);//json字符转obj，保证json不被字符化
                }
                return responseData;
            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string getTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
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

    }
}
