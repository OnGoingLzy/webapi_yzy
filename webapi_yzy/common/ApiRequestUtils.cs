using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace webapi_yzy.common {
    public class ApiRequestUtils {

        public static HttpWebResponse SendPostRequest(string url, string pwd, Dictionary<string, object> paramsDict)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            var jsonString = JsonConvert.SerializeObject(paramsDict);
            var sign = Md5Utils.Encode(pwd + jsonString);
            request.Method = "POST";
            request.ContentType = "application/json";

            //添加请求头
            request.Headers.Add("sign", sign);

            //添加请求体
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            request.ContentLength = byteArray.Length;
            //请求超时时间为传入时间确定
            request.Timeout = 8000;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return (HttpWebResponse)request.GetResponse();
        }

        /// <summary>
        /// 用来验证在水医方的回调接口是否被篡改过
        /// </summary>
        /// <param name="pwd">在水医方密钥</param>
        /// <param name="response">在水医方回调接口请求参数</param>
        /// <returns></returns>
        public static string CheckSign(string pwd, string response)
        {
            string signCalc = Md5Utils.Encode(pwd + response);
            return signCalc;
        }
    }
}
