﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using webapi_yzy.Model;
using Newtonsoft.Json;
using System.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using System.IO.Compression;
using webapi_yzy.Service;
using System.Collections;

namespace webapi_yzy.common
{
    public class yinhaiTool
    {
        private int keySize = 1024; // 密钥大小 单位bit
        //平台公钥 加密用
//        private string publicKey = @"<RSAKeyValue>
//<Modulus>
//oes9HYmbkO3h5PiAaYFwVZ2m/Z/ZF9Lt4jx8673UHF10oE9c9/nHwxgPfX50ew9YoGSq82NM4yrgAq6SOYRYYc3I5veXhsIR1bq04jgeSQp8sLLG7GdIFLMvuB4rS3UTV0TC7+AZhoGgj2QEbigelwzFhrXRRljOYu9VSB5NFqk=
//</Modulus>
//<Exponent>
//AQAB
//</Exponent>
//</RSAKeyValue>";

        //私钥 解密用
        //private string yyjgbm = "P53050201208"; //医药机构编码
        //private string jrxtsqm = "e03c9add2ba740afa19ff936acb059b8"; //接入系统授权码
        //private string jrxtbm = "1002100022"; //接入系统编码
        //private string jrxtsqm = "cb3c8bd23e2746ef8f79dc6cf5d0e54d"; //生产环境 接入系统授权码
        
//        private string privateKey = @"<RSAKeyValue>
//<Modulus>
//ssP3LmipWP8zYo1vaNhFFEFaMO6QYjsE0ByJcxACnbcohUo6g2QnhamyrAhMCXLNb11tCiy870J+BHiHjuCrSD7FLO22SQ2eTPlPxb+rK+AXA2ZsVACxg0BFAAASlV+1DWQM1HwfvM5joPLaFX26GrMK7YOpK/cziM86b60Bh5k=
//</Modulus>
//<Exponent>
//AQAB
//</Exponent>
//<D>
//XuyQJKSOyCM7NenEbvfoNok9Sx5irMaKF7gPhHnL6dOIQL7Zs0tLcT5bEd6WAa5kR+5kKDL5YFL+d4FI+iVyzVbRqeTY+1GVkG+Y6R7zk5YNnAlNUuDHj6mRvyTy3VfdjBNR2eiayGPKyXMr56DHxEDyXsMfjxwgjIdmplOWVME=
//</D>
//<P>
//2LpD+47NcXxLf7Og3/GSXpu+8+P1tC2WginD/cfIpaUYFK6v80DmvkM3WTkY3bZfFbdhWqrBqDQ0Qa7GeYIxtw==
//</P>
//<Q>
//0yiu3xDD6xqTYvFLqE/ipSLbwp5J81a5aqVZ+TbkMCd4L/TwMsvhBQoRz2D+4sFuGmTaCbKZa7gOspkYeJzRLw==
//</Q>
//<DP>
//m58sf67r4IiC4gDHDOc21g550EEpRibSA0cgP9O8RQ1GPFuvZjl9NrOnQ78XFuUEY+CY3HTut+w8dcmdJNUTAw==
//</DP>
//<DQ>
//bv2jbXzSfnGppwhOTFoxN7vq55FsLYwYSgZAFM7Vgro8YnjNCOZBkPSKCAdj8qzZwuXf9dj667QDhb0TL9K94Q==
//</DQ>
//<InverseQ>
//CUuTOweK6xfddX2xK9Z+y2AR7I0PK/FRboaa0+yP7BClla0woPgWwimh7xUccfCg3XIYF6gFHL5Zsolj/ZeXhw==
//</InverseQ>
//</RSAKeyValue>";//私钥

        private string yinhaiServer = "https://ynyb.yinhaiyun.com/dsmp-api/api/public/app/";//银海接口服务器
        private string wechatJrxtbm = "1002100005";
        private string aliPayJrxtbm = "2002100006";
        private string wechatJrxtsqm = "cb3c8bd23e2746ef8f79dc6cf5d0e54d";
        private string aliPayJrxtsqm = "6022fbdab5b84c7f889cfb726a85c7dd";

        private string wechatPlatPublicKey = @"<RSAKeyValue>
  <Exponent>AQAB</Exponent>
  <Modulus>nw1fWLgym3s5jlC4t37uJquk1y8yHYHAkWmyOhuK79ue+k4Gufpg/Tk2bZXoaj9ICrP8WFu8t2w/HNFrBcsRkuPx6HNhvlsRd8CjGnAi0KoXE4elsO1rWvhE4+0FxSFKROq0QfLbFNaqdlmkZQjb+MiaRl6CIAn6ntzwlvsAHb8=</Modulus>
</RSAKeyValue>";
        private string wechatSysPrivateKey = @"<RSAKeyValue>
  <D>QNQDKbSdgEsdN+Qrc40YDIZu+CsOjzME7HyC3hjb65HM2KhYQkiLMgS9qPH+sC+w3n6LG1ofFgPcQSp2QirbX2wOy8NMIK5lHKYxh1CPmimaMyIpOD7Uh3q2nz8c7rl8X+G8OR6fs26ExfrPFcKNkQpgTc+ZCQ6PP7vDD7DWiAE=</D>
  <DP>IwXYFd18nUARvGIZ6XQIC7jVeBMBECtyc+yqQ/xuPRQYUXnnhkyZK0RFuNWjgizMFLU7omE+SbeEA1RXFoS6hQ==</DP>
  <DQ>qgJmb6ynjVJVLTKZ9comfp9+XQwLh0KR5YclzMVC+CHRfOouVoqRou2RrganPOzdl1PQ+y/sOqF+KP8MGVjGYQ==</DQ>
  <Exponent>AQAB</Exponent>
  <InverseQ>u6dCc5EJdB+K9DYf7yUJzeKF61aZEF+JXzASbF9vFcKsoAX39V1AqtiMCU/dodVKiVW+ZcsMokIAusrxe9xqWQ==</InverseQ>
  <Modulus>2Jc62rmEPYCYDWHELCuVbI+aFTh3XSZEELk0jK58gg7vbocciWgpsNxKGNgCHdaraJsPyjIKpqsLV16v+2u2o/j+lHhdYGyun0Kc3dMPtv8YIB3OJZkRkj5K5UxyUli5bGKo2JdeZSGpJS2FD+dybgAuTWx6l9xNqJXzHn3FsKs=</Modulus>
  <P>7nJwinC4iIB6dCk6wyZjq48OwfDRb/ocz9caPViDT6+4qWntLSk1Iet+x4yWoIiLLuUKyYEbrxgIzHgPLokmSw==</P>
  <Q>6Ijn+U5TmpqUlKKmyx951fv0lzbFpcrzxVd5VvskWr8I8UHo+fZPIoE+1Z4NTSJtJ4G9oGEmdzpy9UtNRmKjIQ==</Q>
</RSAKeyValue>";
        private string aliPayPlatPublicKey = @"<RSAKeyValue>
  <Exponent>AQAB</Exponent>
  <Modulus>hEJej2yAH6WUV2mSZSaE6lvr/jTkB2xYogccRbzb0qg6yTf6AfGDMe39paRsJL2k8wBxEkuZy9B15zyFIC/WXZl19NMMtt1ZIAp0Rusn8tDi9+rc0ZTLM8irMcsBBnIXbXjOkUany18jTH5EjqDXvkD7SjrHI0u+brcVWRfmM7s=</Modulus>
</RSAKeyValue>";  //支付宝平台公钥
        private string aliPaySysPrivateKey = @"<RSAKeyValue>
  <D>AZiukHopKrRx+AB0SktscTP0W/d6/o5Q/nxQrUYgZBx4Pri4Cj5uELOZtGmHh3Vm5j5gTfJhfgpIDpFijypfAFfUvkgh77DustBoRyoLukR+lyT/bGdk8sj9iN3F8vTzlHBcqj6784sZIKMF/NzzTejuTyy/URbcmSRcu1GABQE=</D>
  <DP>LitDEnWRELM8LmZt4NZRDm4pPQCPMt6xG+LIdSSGwSaQhtN8BRuETQadTogzPgS2ybrcpA6CbXwvPU4s/jDhZw==</DP>
  <DQ>r3O50S06Es6EmmTYtr4eR8LmZ352ARpl2UBXy8YwBPUYe63KgqfeTtGz19iBu5aixnGyLwBQ6GufKVdAkUZSAQ==</DQ>
  <Exponent>AQAB</Exponent>
  <InverseQ>3TmX+HpFfBh5qpWMahqrrT4G0T2oi/3gpi7dj0eIq1N9/LbvGLt9yJz8DrIbuW/kaKf/HhZvPeYMWEqVgbAiVQ==</InverseQ>
  <Modulus>pc6RtnSy3IvV/O0M4XS8gF0ic/d24upQp66E55PyTW7m7ILyMOATHxqJIiyakZ7GpttNnuilstMBj8yG6uAEWmXxh8GZYFvX8W9sEVP3VlBt0kUZ+qHEHifku5AIqm+SDcTvbqhrN2mvY0TMQQUmemBEIGZ2jkQRHAI0p/Fsn8c=</Modulus>
  <P>4nagLJ4GSifolEYOIE7SvRi6wg0SNeBinBPHqt7ohcOGKuF+cyTpE1moOs//0AWcONKUAuVlTNM8hHZ0T0hcRw==</P>
  <Q>u26wDPtsYQw7hrl1EtcK/a9OCyMPCZDlW9AzD25uYN3JwV2dsB+mwsquZqNzikIxOmw7OZR40Ttv8DP8XQjggQ==</Q>
</RSAKeyValue>";     //支付宝系统私钥

        //验证银海签名
        public bool VerifySignature(string timestamp, string signData, string transSerial, string transOutput,string platType)
        {
            try
            {
                string dataToSign = "";
                if (platType.Equals("微信"))
                {
                    dataToSign = wechatJrxtbm + wechatJrxtsqm + timestamp + transSerial + transOutput;
                }
                if (platType.Equals("阿里"))
                {
                    dataToSign = aliPayJrxtbm + aliPayJrxtsqm + timestamp + transSerial + transOutput;
                }
                

                // 使用 MD5 算法加密待签名数据
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(dataToSign);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // 将 MD5 哈希结果转换为字符串
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }

                    // 将生成的签名数据与传入的签名数据进行比对
                    return string.Equals(sb.ToString(), signData, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                // 验签出现异常，验签失败
                return false;
            }
        }

        //根据银海方规则生成银海签名
        //transSerial 流水号  transInput  输入报文
        public string GenerateSignature(string timestamp,string transSerial, string transInput,string yyjgbm,string platType)
        {
            try
            {

                string dataToSign = "";
                if (platType.Equals("微信"))
                {
                    dataToSign = yyjgbm + wechatJrxtbm + wechatJrxtsqm + timestamp + transSerial + transInput;
                }
                if (platType.Equals("阿里"))
                {
                    dataToSign = yyjgbm + aliPayJrxtbm + aliPayJrxtsqm + timestamp + transSerial + transInput;
                }
                // 拼接相关参数生成待签名数据
               

                // 使用 MD5 算法加密待签名数据
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(dataToSign);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // 将 MD5 哈希结果转换为字符串
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }

                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                // 处理签名生成错误
                return null;
            }
        }

        internal static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    zipStream.Write(data, 0, data.Length);

                }
                return compressedStream.ToArray();
            }
        }
     
        //content加密内容 publicKey 公钥
        public string Encrypt(string content,string platType)
        {
            try
            {
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(content);
                using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(keySize))
                {
                    if (platType.Equals("微信"))
                    {
                        rsaProvider.FromXmlString(wechatPlatPublicKey);
                    }
                    if (platType.Equals("阿里"))
                    {
                        rsaProvider.FromXmlString(aliPayPlatPublicKey);
                    }
                    //rsaProvider.FromXmlString(publicKey);
                    int maxLength = (keySize / 8) - 11; // 去除校验位后117字节有效内容

                    // 没有超过117字节则直接返回加密内容
                    if (dataToEncrypt.Length <= maxLength)
                    {
                        byte[] encryptedData = rsaProvider.Encrypt(dataToEncrypt, false);
                        return Convert.ToBase64String(encryptedData);
                    }
                    else
                    {
                        List<byte> encryptedDataList = new List<byte>();
                        int index = 0;

                        // 嵌套加密
                        while (index < dataToEncrypt.Length)
                        {
                            int remainingBytes = dataToEncrypt.Length - index;
                            int bytesToCopy = Math.Min(remainingBytes, maxLength);

                            byte[] chunk = new byte[bytesToCopy];
                            Array.Copy(dataToEncrypt, index, chunk, 0, bytesToCopy);

                            byte[] encryptedChunk = rsaProvider.Encrypt(chunk, false);
                            encryptedDataList.AddRange(encryptedChunk);

                            index += bytesToCopy;
                        }

                        return Convert.ToBase64String(encryptedDataList.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理加密错误
                return null;
            }
        }

        //content解密内容 privateKey 私钥
        public string Decrypt(string content,string platType)
        {
            try
            {
                byte[] encryptedData = Convert.FromBase64String(content);
                using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(keySize))
                {
                    if (platType.Equals("微信"))
                    {
                        rsaProvider.FromXmlString(wechatSysPrivateKey);
                    }
                    if (platType.Equals("阿里"))
                    {
                        rsaProvider.FromXmlString(aliPaySysPrivateKey);
                    }
                    int maxLength = (keySize / 8);

                    if (encryptedData.Length <= maxLength)
                    {
                        byte[] decryptedData = rsaProvider.Decrypt(encryptedData, false);
                        return Encoding.UTF8.GetString(decryptedData);
                    }
                    else
                    {
                        List<byte> decryptedDataList = new List<byte>();
                        int index = 0;

                        while (index < encryptedData.Length)
                        {
                            int remainingBytes = encryptedData.Length - index;
                            int bytesToCopy = Math.Min(remainingBytes, maxLength);

                            byte[] chunk = new byte[bytesToCopy];
                            Array.Copy(encryptedData, index, chunk, 0, bytesToCopy);

                            byte[] decryptedChunk = rsaProvider.Decrypt(chunk, false);
                            decryptedDataList.AddRange(decryptedChunk);

                            index += bytesToCopy;
                        }

                        return Encoding.UTF8.GetString(decryptedDataList.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理解密错误
                return null;
            }
        }

        //生成调用银海查询api参数
        public string generateYinhaiQueryOrderParam(string main_order_id)
        {
            string beginTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            yinhaiInputObject yinhaiInputObject = new yinhaiInputObject();
            string dbRes = DbOperator.getOrderIdAndYyjgInfoByMainOrderId(main_order_id,"付款");
            JObject dbResObj = (JObject)JsonConvert.DeserializeObject(dbRes);
            if ((int)dbResObj["flag"] == -99)
            {
                //抛出异常让父方法处理
                throw new Exception((string)dbResObj["flag"]);
            }
            yinhaiInputObject.trns_no = "T9152";
            Random rad = new Random();//实例化随机数产生器rad；
            int value = rad.Next(1000, 10000);//用rad生成大于等于1000，小于等于9999的随机数；
            string sender_trns_sn = dbResObj["medins_code"] + beginTime + value.ToString();
            yinhaiInputObject.sender_trns_sn = sender_trns_sn;
            yinhaiInputObject.trns_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            yinhaiInputObject.medins_code = (string)dbResObj["medins_code"];
            yinhaiInputObject.medins_name = (string)dbResObj["medins_name"];
            yinhaiInputObject.ency_type = "RSA";
            yinhaiInputObject.sign_type = "MD5";
            yinhaiInputObject.opter_id = "50002";
            yinhaiInputObject.opter_name = "ynsyy";
            yinhaiInputObject.opter_type = "102";
            string platType = (string)dbResObj["platType"];
            if (((string)dbResObj["platType"]).Equals("微信"))
            {
                yinhaiInputObject.acss_sys_code = wechatJrxtbm;
            }
            else
            {
                yinhaiInputObject.acss_sys_code = aliPayJrxtbm;
            }
            
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;

            // 将毫秒级时间戳转换为字符串
            long timestampInMilliseconds = currentTime.ToUnixTimeMilliseconds();
            string timestampString = timestampInMilliseconds.ToString();
            string timestamp = timestampString;
            yinhaiInputObject.timestmp = timestamp;
            //rsa加密请求体数据
            yinhaiQueryOrderObj yinhaiQueryOrderObj = new yinhaiQueryOrderObj();
            yinhaiQueryOrderObj.data = new yinhaiQueryOrderObj.Data();
            yinhaiQueryOrderObj.data.plaf_ord_no = (string)dbResObj["plaf_ord_no"];
            yinhaiQueryOrderObj.data.dpp_ord_no = (string)dbResObj["dpp_ord_no"];
            yinhaiQueryOrderObj.data.chnl_ord_no = "";
            //string reqData = "{\"data\":{\"plaf_ord_no\": \"1022023111011382110005293\",\"dpp_ord_no\": \"PP202311101138214640\",\"chnl_ord_no\": \"\"}}"; // 请求体数据
            string reqData = JSON.Encode(yinhaiQueryOrderObj);
            string encryptReqData = Encrypt(reqData,platType);
            //设置输入报文
            yinhaiInputObject.input = encryptReqData;
            //生成签名
            string sign_data = GenerateSignature(timestamp, sender_trns_sn, encryptReqData, (string)dbResObj["medins_code"],(string)dbResObj["platType"]);
            yinhaiInputObject.sign_data = sign_data;
            string url = "https://ynyb.yinhaiyun.com/dsmp-api/api/public/app/T9152";
            HttpMethod method = HttpMethod.Post;
            //请求报文转为json
            string jsonData = JsonConvert.SerializeObject(yinhaiInputObject) + (string)dbResObj["platType"];

            return jsonData;
        }

        //生成调用银海退款查询api参数
        public string generateYinhaiQueryRefundOrderParam(string main_order_id)
        {
            string beginTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            yinhaiInputObject yinhaiInputObject = new yinhaiInputObject();
            string dbRes = DbOperator.getOrderIdAndYyjgInfoByMainOrderId(main_order_id,"退款");
            JObject dbResObj = (JObject)JsonConvert.DeserializeObject(dbRes);
            if ((int)dbResObj["flag"] == -99)
            {
                //抛出异常让父方法处理
                throw new Exception((string)dbResObj["sm"]);
            }
            yinhaiInputObject.trns_no = "T9155";
            Random rad = new Random();//实例化随机数产生器rad；
            int value = rad.Next(1000, 10000);//用rad生成大于等于1000，小于等于9999的随机数；
            string sender_trns_sn = dbResObj["medins_code"] + beginTime + value.ToString();
            yinhaiInputObject.sender_trns_sn = sender_trns_sn;
            yinhaiInputObject.trns_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            yinhaiInputObject.medins_code = (string)dbResObj["medins_code"];
            yinhaiInputObject.medins_name = (string)dbResObj["medins_name"];
            yinhaiInputObject.ency_type = "RSA";
            yinhaiInputObject.sign_type = "MD5";
            yinhaiInputObject.opter_id = "50002";
            yinhaiInputObject.opter_name = "ynsyy";
            yinhaiInputObject.opter_type = "102";
            //20231212修改
            string platType = (string)dbResObj["platType"];
            if (((string)dbResObj["platType"]).Equals("微信"))
            {
                yinhaiInputObject.acss_sys_code = wechatJrxtbm;
            }
            else
            {
                yinhaiInputObject.acss_sys_code = aliPayJrxtbm;
            }
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;

            // 将毫秒级时间戳转换为字符串
            long timestampInMilliseconds = currentTime.ToUnixTimeMilliseconds();
            string timestampString = timestampInMilliseconds.ToString();
            string timestamp = timestampString;
            yinhaiInputObject.timestmp = timestamp;
            //rsa加密请求体数据
            yinhaiQueryRefundOrderObj yinhaiQueryRefundOrderObj = new yinhaiQueryRefundOrderObj();
            yinhaiQueryRefundOrderObj.data = new yinhaiQueryRefundOrderObj.Data();
            yinhaiQueryRefundOrderObj.data.plaf_ord_no = (string)dbResObj["plaf_ord_no"];
            yinhaiQueryRefundOrderObj.data.dpp_ord_no = (string)dbResObj["dpp_ord_no"];
            yinhaiQueryRefundOrderObj.data.chnl_ord_no = "";
            yinhaiQueryRefundOrderObj.data.acss_sys_refd_ord_no = (string)dbResObj["main_order_id"];
            //string reqData = "{\"data\":{\"plaf_ord_no\": \"1022023111011382110005293\",\"dpp_ord_no\": \"PP202311101138214640\",\"chnl_ord_no\": \"\"}}"; // 请求体数据
            string reqData = JSON.Encode(yinhaiQueryRefundOrderObj);
            string encryptReqData = Encrypt(reqData,platType);
            //设置输入报文
            yinhaiInputObject.input = encryptReqData;
            //生成签名
            string sign_data = GenerateSignature(timestamp, sender_trns_sn, encryptReqData, (string)dbResObj["medins_code"], (string)dbResObj["platType"]);
            yinhaiInputObject.sign_data = sign_data;
            string url = "https://ynyb.yinhaiyun.com/dsmp-api/api/public/app/T9155";
            HttpMethod method = HttpMethod.Post;
            //请求报文转为json
            string jsonData = JsonConvert.SerializeObject(yinhaiInputObject) + (string)dbResObj["platType"];

            return jsonData;
        }


        //生成退款单所需参数
        public responseYinhaiRefundObject generateYinHaiRefundOrderParam(string main_order_id,string type)
        {
            responseYinhaiRefundObject responseYinhaiRefundObject = new responseYinhaiRefundObject();
            yinhaiCreateRefundOrderObject yinhaiCreateRefundOrderObject = new yinhaiCreateRefundOrderObject();
            //调用存过获取参数
            string orderParam = DbOperator.getYinHaiRefundOrderParam(main_order_id,type);
            JObject orderParamObj = (JObject)JsonConvert.DeserializeObject(orderParam);
            if (orderParamObj["flag"] == null || (int)orderParamObj["flag"] == -99)
            {
                responseYinhaiRefundObject.flag = -99;
                responseYinhaiRefundObject.sm = (string)orderParamObj["sm"];
            }
        
            responseYinhaiRefundObject.flag = (int)orderParamObj["flag"];
            responseYinhaiRefundObject.sm = (string)orderParamObj["sm"];
            responseYinhaiRefundObject.medins_code = (string)orderParamObj["medins_code"];
            responseYinhaiRefundObject.medins_name = (string)orderParamObj["medins_name"];
            responseYinhaiRefundObject.dppSyscode = (string)orderParamObj["dppSyscode"];
            //获取accessToken
            //string accessTokenObj = DbOperator.GetAccessToken("", "");
            //JObject jsonsp = (JObject)JsonConvert.DeserializeObject(accessTokenObj);

            //yinhaiSyncAcssToken.data.acss_token = payService.getAccessToken();
            WechatPayService payService = new WechatPayService();
            string accessToken = payService.getAccessToken();
            responseYinhaiRefundObject.acssToken = accessToken;
            // yinhaiCreateRefundOrderObject.data = new yinhaiCreateRefundOrderObject.Data();
            yinhaiCreateRefundOrderObject.plaf_ord_no = (string)orderParamObj["plaf_ord_no"];
            yinhaiCreateRefundOrderObject.biz_ord_no = (string)orderParamObj["biz_ord_no"];
            //yinhaiCreateRefundOrderObject.refd_sumamt = (decimal)orderParamObj["refd_sumamt"];
            yinhaiCreateRefundOrderObject.refd_sumamt = (decimal)orderParamObj["origin_totalPrice"];
            yinhaiCreateRefundOrderObject.cash_add_refd_amt = (decimal)orderParamObj["cash_add_refd_amt"];
            yinhaiCreateRefundOrderObject.acss_sys_refd_ord_no = (string)orderParamObj["new_main_order_id"];
            //yinhaiCreateRefundOrderObject.acss_token = jsonsp["access_token"].ToString();
            yinhaiCreateRefundOrderObject.acss_token = accessToken;
            yinhaiCreateRefundOrderObject.chnl_user_id = (string)orderParamObj["chnl_user_id"];
            yinhaiCreateRefundOrderObject.return_url = "pages/user/user";
            yinhaiCreateRefundOrderObject.refd_rea = (string)orderParamObj["refd_rea"];

            responseYinhaiRefundObject.yinhaiCreateRefundOrderObject = yinhaiCreateRefundOrderObject;
            //--- 参数生成完毕，玩银海订单表插入数据
            string createResult = DbOperator.createYinHaiRefundOrder((string)orderParamObj["new_main_order_id"], -(decimal)orderParamObj["origin_totalPrice"], JsonConvert.SerializeObject(responseYinhaiRefundObject), main_order_id);
            JObject createResultObj = (JObject)JsonConvert.DeserializeObject(createResult);
            if ((int)createResultObj["flag"] == -99)
            {
                responseYinhaiRefundObject.flag = -99;
                responseYinhaiRefundObject.sm = (string)createResultObj["sm"];
            }
            //*************

            return responseYinhaiRefundObject;
        }

        //生成 创建银海付款单所需参数
        public responseYinhaiObject generateYinHaiOrderParam(JObject orderJsonObject, string main_order_id,string openid,string payType,  string goodsUserId)
        {
            //生成银海创建订单所需要的参数
            //*********************************************************************************************************************
            yinhaiCreateOrderObject yinhaiCreateOrderObject = new yinhaiCreateOrderObject();
            yinhaiCreateOrderObject_ordBizPara yinhaiCreateOrderObject_OrdBizPara = new yinhaiCreateOrderObject_ordBizPara();       
            responseYinhaiObject responseYinhaiObject = new responseYinhaiObject();
            //调用存过获取参数
            string orderParam = DbOperator.getYinHaiOrderParam(main_order_id, payType, goodsUserId);
            JObject orderParamObj = (JObject)JsonConvert.DeserializeObject(orderParam);
            if (orderParamObj["flag"] == null || (int)orderParamObj["flag"] == -99)
            {
                responseYinhaiObject.flag = -99;
                responseYinhaiObject.sm = (string)orderParamObj["sm"];
                return responseYinhaiObject;
            }
            responseYinhaiObject.flag = (int)orderParamObj["flag"];
            responseYinhaiObject.sm = (string)orderParamObj["sm"];
            responseYinhaiObject.medins_code = (string)orderParamObj["data"][0]["medins_code"];
            responseYinhaiObject.medins_name = (string)orderParamObj["data"][0]["medins_name"];
            responseYinhaiObject.dppSyscode = (string)orderParamObj["data"][0]["dppSyscode"];
            yinhaiCreateOrderObject.ord_biz_type = "5001";
            yinhaiCreateOrderObject.acss_sys_ord_no = main_order_id;
            yinhaiCreateOrderObject.cash_add_amt = Math.Round((decimal)orderParamObj["data"][0]["shippingFee"], 2);
            yinhaiCreateOrderObject.cash_add_dscr = "运费+非医保金额";
            // 创建一个对象数组并分配内存空间  
            cash_add_list[] cash_add_list = new cash_add_list[1];
            cash_add_list cash_Add_List_shippingFee = new cash_add_list();
            cash_Add_List_shippingFee.cash_add_amt = Math.Round((decimal)orderParamObj["data"][0]["shippingFee"], 2);
            cash_Add_List_shippingFee.cash_add_fee_type = "01";
            cash_Add_List_shippingFee.cash_add_fee_name = "配送费";
            // 创建一个新的 实例并将其添加到数组的最后一个位置  
            cash_add_list[cash_add_list.Length - 1] = cash_Add_List_shippingFee;
            

            yinhaiCreateOrderObject.ord_sumamt =  Math.Round((decimal)orderJsonObject["data"][0]["totalPrice"], 2);
            //yinhaiCreateOrderObject.ord_sumamt = 0.01M;
            yinhaiCreateOrderObject_OrdBizPara.mdtrt_sn = main_order_id;
            yinhaiCreateOrderObject_OrdBizPara.mdtrt_sid = main_order_id;
            yinhaiCreateOrderObject_OrdBizPara.doc_type = "6";
            yinhaiCreateOrderObject_OrdBizPara.setl_docno = main_order_id;
            string chrg_type = "2";
            if (payType.Equals("支付宝支付"))
            {
                chrg_type = "1";
            }
            yinhaiCreateOrderObject_OrdBizPara.chrg_type = chrg_type;  //1自费 2医保
            
            if (chrg_type.Equals("2"))
            {
                yinhaiCreateOrderObject_OrdBizPara.psn_name = (string)orderParamObj["data"][0]["goodsUserName"];
                yinhaiCreateOrderObject_OrdBizPara.mob = (string)orderParamObj["data"][0]["phone"];
                yinhaiCreateOrderObject_OrdBizPara.psn_cert_type = "01"; //证件类型
                yinhaiCreateOrderObject_OrdBizPara.brdy = (string)orderParamObj["data"][0]["birthday"];
                yinhaiCreateOrderObject_OrdBizPara.certno = (string)orderParamObj["data"][0]["idCard"];
                yinhaiCreateOrderObject_OrdBizPara.idcard = (string)orderParamObj["data"][0]["idCard"];
               
                yinhaiCreateOrderObject_OrdBizPara.gend = "0";
            }
            else
            {
                yinhaiCreateOrderObject_OrdBizPara.psn_name = "在线用户";
                yinhaiCreateOrderObject_OrdBizPara.mob = "";
            }
            DateTime birthday = DateTime.ParseExact((string)orderParamObj["data"][0]["birthday"], "yyyy-MM-dd", null);
            int age = DateTime.Now.Year - birthday.Year;
            if (DateTime.Now.DayOfYear < birthday.DayOfYear)
            {
                age--;
            }
            yinhaiCreateOrderObject_OrdBizPara.age = age.ToString(); //计算得出
            yinhaiCreateOrderObject_OrdBizPara.med_type = "41";
            //yinhaiCreateOrderObject_OrdBizPara.sumfee = Math.Round((decimal)orderJsonObject["data"][0]["totalPrice"], 2);
            //yinhaiCreateOrderObject_OrdBizPara.sumfee = 0.01M;
            yinhaiCreateOrderObject_OrdBizPara.mdtrt_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            JArray feeDetlListsArray = orderParamObj["data"][0]["feeDetlLists"] as JArray;

            //yinhaiCreateOrderObject_ordBizPara_feeDetlList[] yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists = new yinhaiCreateOrderObject_ordBizPara_feeDetlList[1];
            ArrayList yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists = new ArrayList();
            for (int i = 0; i < feeDetlListsArray.Count; i++)
            {
                //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i] = new yinhaiCreateOrderObject_ordBizPara_feeDetlList();
                JObject item = (JObject)feeDetlListsArray[i];
                if (item.HasValues && (string)item["healthInsuranceFlag"]=="y")  //只有医保品种才能算入订单明细，非医保药品作为额外现金
                {

                    
                    yinhaiCreateOrderObject_ordBizPara_feeDetlList node = new yinhaiCreateOrderObject_ordBizPara_feeDetlList();
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].feedetl_sn = (string)item["detail_order_id"];
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].chrg_bchno = (string)item["detail_order_id"];
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].rx_circ_flag = "0";
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].spec = (string)item["specification"];
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].cnt = (int)item["num"];
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].pric = (decimal)item["unitPrice"]; 
                    ////yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].pric = 0.01M;
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].det_item_fee_sumamt = (decimal)item["totalPrice"]; 
                    ////yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].det_item_fee_sumamt = 0.01M;
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].fee_ocur_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].hosp_appr_flag = "0";
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].medins_list_codg = (string)item["goodsId"];
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].medins_list_name = (string)item["genericName"];
                    node.feedetl_sn = (string)item["detail_order_id"];
                    node.chrg_bchno = main_order_id;
                    node.rx_circ_flag = "0";
                    node.spec = (string)item["specification"];
                    node.cnt = (int)item["num"];
                    node.pric = (decimal)item["unitPrice"];
                    //node.pric = 0.01M;
                    node.det_item_fee_sumamt = (decimal)item["totalPrice"];
                    //yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists[i].det_item_fee_sumamt = 0.01M;
                    node.fee_ocur_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    node.hosp_appr_flag = "0";
                    node.medins_list_codg = (string)item["goodsId"];
                    node.medins_list_name = (string)item["genericName"];
                    yinhaiCreateOrderObject_OrdBizPara.sumfee = yinhaiCreateOrderObject_OrdBizPara.sumfee+ (decimal)item["totalPrice"];
                    //将新对象添加到数组的末尾 
                    yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists.Add(node);
                    // 增加数组的大小以容纳新对象 
                    //Array.Resize(ref yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists, yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists.Length + 1);

                }
                else if(item.HasValues && (string)item["healthInsuranceFlag"] == "n")
                {
                     
                    Array.Resize(ref cash_add_list, cash_add_list.Length + 1);
                    

                    cash_add_list cash_Add_List_node = new cash_add_list();
                    cash_Add_List_node.cash_add_amt = (decimal)item["totalPrice"];
                    cash_Add_List_node.cash_add_fee_type = "03";
                    cash_Add_List_node.cash_add_fee_code = (string)item["goodsId"];
                    cash_Add_List_node.cash_add_fee_name = (string)item["genericName"];
                    yinhaiCreateOrderObject.cash_add_amt = yinhaiCreateOrderObject.cash_add_amt+ (decimal)item["totalPrice"];
                    //将新对象添加到数组的末尾 
                    cash_add_list[cash_add_list.Length - 1] = cash_Add_List_node;

                }

            }
            //数组列表转为数组
            yinhaiCreateOrderObject_OrdBizPara.fee_detl_list = (yinhaiCreateOrderObject_ordBizPara_feeDetlList[])yinhaiCreateOrderObject_OrdBizPara_FeeDetlLists.ToArray(typeof(yinhaiCreateOrderObject_ordBizPara_feeDetlList)); ;
            yinhaiCreateOrderObject.ord_biz_para = yinhaiCreateOrderObject_OrdBizPara;
            yinhaiCreateOrderObject.cash_add_list = cash_add_list;

            responseYinhaiObject.yinhaiCreateOrderObject = yinhaiCreateOrderObject;
            //获取accessToken
            //string accessTokenObj = DbOperator.GetAccessToken("", "");
            //JObject jsonsp = (JObject)JsonConvert.DeserializeObject(accessTokenObj);
            
            if (payType.Equals("微信医保支付"))
            {
                WechatPayService payService = new WechatPayService();
                string accessToken = payService.getAccessToken();
                responseYinhaiObject.acssToken = accessToken;
            }
            
            //*********************************************************************************************************************
            //--- 参数生成完毕，往银海订单表插入数据
            string createResult = DbOperator.createYinHaiOrder(main_order_id, yinhaiCreateOrderObject.ord_sumamt, JsonConvert.SerializeObject(responseYinhaiObject),openid,payType);
            JObject createResultObj = (JObject)JsonConvert.DeserializeObject(createResult);
            if ((int)createResultObj["flag"] == -99)
            {
                responseYinhaiObject.flag = -99;
                responseYinhaiObject.sm = (string)createResultObj["sm"];
            }
            //*************



            return responseYinhaiObject;
        }

        /// <summary>
        /// 发送微信token给银海
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="contentType"></param>
        /// <param name="reqData"></param>
        /// <returns></returns>
        //public async Task<string> sendSyncAcssTokenRequestAsync()
        //{
        //    string beginTime = DateTime.Now.ToString("yyyyMMddHHmmss");
        //    yinhaiInputObject yinhaiInputObject = new yinhaiInputObject();
        //    yinhaiInputObject.trns_no = "T8201";
        //    string sender_trns_sn = "P53050201208" + beginTime + "0001";
        //    yinhaiInputObject.sender_trns_sn = sender_trns_sn;
        //    yinhaiInputObject.trns_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    yinhaiInputObject.medins_code = "P53050201208";
        //    yinhaiInputObject.medins_name = "云南省医药有限公司新特药保山零售店";
        //    yinhaiInputObject.ency_type = "RSA";
        //    yinhaiInputObject.sign_type = "MD5";
        //    yinhaiInputObject.opter_id = "50002";
        //    yinhaiInputObject.opter_name = "ynsyy";
        //    yinhaiInputObject.opter_type = "102";
        //    yinhaiInputObject.acss_sys_code = "1002100022";
        //    DateTimeOffset currentTime = DateTimeOffset.UtcNow;

        //    // 将毫秒级时间戳转换为字符串
        //    long timestampInMilliseconds = currentTime.ToUnixTimeMilliseconds();
        //    string timestampString = timestampInMilliseconds.ToString();
        //    string timestamp = timestampString;
        //    yinhaiInputObject.timestmp = timestamp;
        //    //rsa加密请求体数据
        //    yinhaiSyncAcssToken yinhaiSyncAcssToken = new yinhaiSyncAcssToken();
        //    yinhaiSyncAcssToken.data = new yinhaiSyncAcssToken.Data();
        //    yinhaiSyncAcssToken.data.chnl_app_id = "wxe7c826a1a5e00055";
        //    yinhaiSyncAcssToken.data.mert_app_type= "11";
        //    //获取accessToken
        //    //string accessTokenObj = DbOperator.GetAccessToken("", "");
        //    //JObject jsonsp = (JObject)JsonConvert.DeserializeObject(accessTokenObj);
        //    //yinhaiSyncAcssToken.data.acss_token = jsonsp["access_token"].ToString();
        //    WechatPayService payService = new WechatPayService();
        //    yinhaiSyncAcssToken.data.acss_token = payService.getAccessToken();
        //    yinhaiSyncAcssToken.data.token_expy = 999;
        //    //string reqData = "{\"data\":{\"plaf_ord_no\": \"1022023111011382110005293\",\"dpp_ord_no\": \"PP202311101138214640\",\"chnl_ord_no\": \"\"}}"; // 请求体数据
        //    string reqData = JSON.Encode(yinhaiSyncAcssToken);
        //    string encryptReqData = Encrypt(reqData);
        //    //设置输入报文
        //    yinhaiInputObject.input = encryptReqData;
        //    //生成签名
        //    string sign_data = GenerateSignature(timestamp, sender_trns_sn, encryptReqData, "P53050201208");
        //    yinhaiInputObject.sign_data = sign_data;
        //    string url = "https://ynyb.yinhaiyun.com/dsmp-api/api/public/app/T8201";
   

        //    //请求报文转为json
        //    string jsonData = JsonConvert.SerializeObject(yinhaiInputObject);
        //    //发送请求
        //    HttpMethod method1 = HttpMethod.Post;
        //    JObject res = await SendHttpRequestAsync(url, method1, "application/json", jsonData);
        //    if (VerifySignature((string)res["timestmp"], (string)res["sign_data"], (string)res["recer_trns_sn"], (string)res["output"]))
        //    {
        //        //string outputJson = yhtool.Decrypt((string)res["output"]);
        //    }
        //    //解码参数
        //    string outputJson = Decrypt((string)res["output"]);
        //    //JObject outputObj = (JObject)JsonConvert.DeserializeObject(outputJson);

        //    return "成功";
        //}


        //发送http请求公共方法
        //参数：url 请求的地址 method 请求的方法(GET 或者post，del等等) contentType 请求内容类型 ，reqData：请求体参数的json字符串
        //返回：JObject对象
        /*调用样例：
                yinhaiTool yhtool = new yinhaiTool();
                string url = "https://example.com/api/resource";
                HttpMethod method = HttpMethod.Get; // 或者 HttpMethod.Post
                string contentType = "application/json"; // 根据需要设置
                string reqData = "{\"key\": \"value\"}"; // 请求体数据  不需要请求体数据的话直接传null
                JObject response = await yhtool.SendHttpRequestAsync(url, method, contentType,reqData);

                // 处理响应数据
                string reqStatus = response["reqStatus"].ToString();
                if (reqStatus == "成功")
                {
                    // 请求成功，处理响应数据
                }
                else
                {
                    string error = response["error"].ToString();
                    // 处理请求失败情况
                }
         */
        public async Task<JObject> SendHttpRequestAsync(string url, HttpMethod method, string contentType, string reqData)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 设置请求的方法
                    HttpRequestMessage request = new HttpRequestMessage(method, url);

                    // 设置请求头
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        request.Headers.Add("Accept", contentType);
                    }

                    // 设置请求体数据
                    if (!string.IsNullOrEmpty(reqData))
                    {
                        request.Content = new StringContent(reqData, Encoding.UTF8, contentType);
                    }

                    // 发送请求并获取响应
                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        JObject responseData = JObject.Parse(responseContent);
                        responseData["reqStatus"] = "成功";
                        return responseData;
                    }
                    else
                    {
                        JObject errorResponse = new JObject
                    {
                        { "reqStatus", "失败" },
                        { "error", "HTTP请求失败" }
                    };
                        return errorResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                // 捕获异常，返回包含错误信息的 JObject
                JObject errorResponse = new JObject
            {
                { "reqStatus", "失败" },
                { "error", ex.Message }
            };
                return errorResponse;
            }
        }




    }
}
