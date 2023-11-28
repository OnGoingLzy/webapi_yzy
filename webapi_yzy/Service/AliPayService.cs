﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using webapi_yzy.Model;
using System.IO;

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Collections;
using Aop.Api.Util;
using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;

namespace webapi_yzy.Service
{

    //微信支付服务
   
    public class AliPayService
    {
        private static readonly ConnectionMultiplexer connection = RedisHelper.Connection;
        private static readonly IDatabase redis = connection.GetDatabase();


        string privateKey = "MIIEpAIBAAKCAQEAk/p6/W5jADYZMzBdnera0wOsJcUtdXV/A05MBA+h/O9H3M8Qo1UWWE9bNVMTGGPPEywVdpG4mjG4c4mW1HsyQh4XiP5hDCbFnqZ5P9U4LC1bKocNd6KkP4G3i0Pgx1hdVM43LCf0Wnyo/h1VhI+C/TQ9hu4FLbtkPKowYHWSAEnsnMGxl4NsZRpAi5WDFVi2VWXHjnyVzyAR6vCOu2Yc+xYhD3TMHVZx/XPeldqs4KEN+iWvESxedWU85VWFwlrNY5dujKJNXmKtRqCs0XkK1pakVAt9fa3B1bfgVM7Ub7lzkT9LiSoTUkX+PEbevWI6cWdFRmbjMwxvpKyQPkUHKwIDAQABAoIBAFNItXHvb87No884mGfpp3D97yRu3k/uaibdKVFbzwrBTUvMSovytpnLSAUyFVwzyHb857OM/j3iX/K+7GH4WGf38b0Llmk1ZSIOhc1UXsIWYOoFiqHbqt+HTVsXP+pSJG03hrvWXOOUp7QlpFzwPIPvo/VN/yInBppSGtofMaXxGMFMkZpaUendWdJ9v29biThuRafp8KA8xWYMCpPZ7g27aLEL2QRbnKo8uS/RNrEbqOQvXMiltTrKSZoDm8tkFRaQmRwe3rdZ6U+XPimORq5O58c5bgtnI3ncMvsG9G3qQe85+Gx3kOKbSskkMB6kJXSJFuc42xdbBRFwD3OGsVkCgYEA2a6gtqcMEUarN1/5cOWhWgUeh8v3U0aekPtUBJpuj+gyj0bl6FEn9gjIVT7yH9ejqYWaffsvSjxvt96vdBQJBnQJ3x+qXwJHrttzRszVueRmIXkeJ9amA2NofpTo1ktvNfLlt+0ytF0CW+pbtLTOv3ZQyEEyf2sILPPZsORrGw8CgYEArgbOlVoWT+ihFDTylbN1+ieKiVk8cYaru2y9ELgyX1TTzDgdqYfecHoq7uVb9DB8V0a93U6iKpaW8h8XdtFZVPt3DC0Nu3h3hRTeVYr0SEX4lYYyNcv5JFjJA9miWr/nqLT7xz5TRHxfnYq70Hl+/AVHltuMq67u1Q//bmIsAiUCgYEApRlnV2E3K9s3fNZT8CaZzBbZ4xD10rlbBoc4YA1pEciAoF6bfSogbuCWYOFO7ou025wzYp2ibmGMMh65YAbaGYTk/8+afljSWXKj6eztpQHv3C9qibipTSSWWntCZVXtUTOau3cx384zPddwGoyPynNILboVPMLG/qEsWJfGDE0CgYBDN0vZcedGTALTTDQaUCuUSGuA61+mNa7lAHXyHEMQgTIhIdeLIHhxFy64AQzIP2X3S9KPxQxryKhaslmcwfVwFoR/xnMUYs4/L7VEcxwxQh1mm2OPVc5QFJ7nhsrnpFG03tqUtWTyAdvofKpYNmUusFdbVEd8FGAZGUm+d9qppQKBgQCXsI4MN2wui+IPAqYVFI7K59Xz52Oui1MJ02M+Z2LBuRQ1OSOvH9sOnXSXiRe7EAThErW7SFZO90BDaMJEdXFEwv2FC+QEoYg618JvEunSX0gGluUsFLnQZkZZI5vI79t0tAHnbXE8/vEEYoVLLdLJxAOW64jFszIg/Wkw5L1Byw==";
        string alipayPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAg27TzKNFjjGc/NpDHLgO6zAwspfnaWDaFAvGZFbKMEuS1X9hVARpBCpWJuv1sjNOriRIboj256bg9XxUeXwrm/ASMyFNH1k9S505xiJ2jIFrCKWoZPWSKeAAbMIG5IfnjOO9/NDuYPWner7SH6pPsqKCCwvVTOPo2mGhZKz+sxLoKlziMPcL4sFJ2QGjUQHYqbV+FFJrjY1CnNGbkrVUd365Wd3CRYlXb7k1TJ1x9cpX9kdzDw2WZQAG4u4nkaUFwd5nX5d5z0XRh2m8VVnQXRPJS/dx+/l/GE4vYIDNIJjcmoer/SLFSXan+1OIFFH4bVyOayskqT8sdIqyA8379wIDAQAB";

        //支付宝解密前端提交的报文
        public string AliPayDecrypt(string response)
        {

            //1. 获取验签和解密所需要的参数
            JObject openapiResult = (JObject)JsonConvert.DeserializeObject(response);
            string signType;
            if (response.Contains("sign_type"))
            {
                signType = openapiResult["sign_type"].ToString();
            }
            else
            {
                signType = "RSA2";
            }
            string charset;
            if (response.Contains("charset"))
            {
                charset = openapiResult["charset"].ToString();
            }
            else
            {
                charset = "UTF-8";
            }
            string encryptType;
            if (response.Contains("encrypt_type"))
            {
                encryptType = openapiResult["encrypt_type"].ToString();
            }
            else
            {
                encryptType = "AES";
            }
            string sign = openapiResult["sign"].ToString();
            string content = openapiResult["encryptedData"].ToString();
            // 是否为加密报文
            bool isDataEncrypted = !content.StartsWith("{", StringComparison.Ordinal);
            bool signCheckPass = false;
            //2. 验签
            string signContent = content;
            string signVeriKey = alipayPublicKey;
            string decryptKey = "Bq8XiVSPImbyLfEn+0gG0Q==";
            // 如果是加密的报文则需要在密文的前后添加双引号
            if (isDataEncrypted)
            {
                signContent = "\"" + signContent + "\"";
            }
            try
            {
                signCheckPass = AlipaySignature.RSACheckContent(signContent, sign, signVeriKey, charset, signType, false);
            }
            catch (Exception ex)
            {
                //验签异常, 日志    
                throw new Exception("验签失败", ex);
            }
            if (!signCheckPass)
            {
                //验签不通过（异常或者报文被篡改），终止流程（不需要做解密）   
                throw new Exception("验签失败");
            }
            //3. 解密
            string plainData = null;
            if (isDataEncrypted)
            {
                try
                {
                    plainData = AlipayEncrypt.AesDencrypt(decryptKey, content, charset);
                }
                catch (Exception ex)
                {
                    //解密异常, 记录日志        
                    throw new Exception("解密异常", ex);
                }
            }
            else
            {
                plainData = content;
            }

            return plainData;
        }

        

        //public string getAliPayAccessToken(string code)
        //{
        //    string privateKey = "MIIEpAIBAAKCAQEAk/p6/W5jADYZMzBdnera0wOsJcUtdXV/A05MBA+h/O9H3M8Qo1UWWE9bNVMTGGPPEywVdpG4mjG4c4mW1HsyQh4XiP5hDCbFnqZ5P9U4LC1bKocNd6KkP4G3i0Pgx1hdVM43LCf0Wnyo/h1VhI+C/TQ9hu4FLbtkPKowYHWSAEnsnMGxl4NsZRpAi5WDFVi2VWXHjnyVzyAR6vCOu2Yc+xYhD3TMHVZx/XPeldqs4KEN+iWvESxedWU85VWFwlrNY5dujKJNXmKtRqCs0XkK1pakVAt9fa3B1bfgVM7Ub7lzkT9LiSoTUkX+PEbevWI6cWdFRmbjMwxvpKyQPkUHKwIDAQABAoIBAFNItXHvb87No884mGfpp3D97yRu3k/uaibdKVFbzwrBTUvMSovytpnLSAUyFVwzyHb857OM/j3iX/K+7GH4WGf38b0Llmk1ZSIOhc1UXsIWYOoFiqHbqt+HTVsXP+pSJG03hrvWXOOUp7QlpFzwPIPvo/VN/yInBppSGtofMaXxGMFMkZpaUendWdJ9v29biThuRafp8KA8xWYMCpPZ7g27aLEL2QRbnKo8uS/RNrEbqOQvXMiltTrKSZoDm8tkFRaQmRwe3rdZ6U+XPimORq5O58c5bgtnI3ncMvsG9G3qQe85+Gx3kOKbSskkMB6kJXSJFuc42xdbBRFwD3OGsVkCgYEA2a6gtqcMEUarN1/5cOWhWgUeh8v3U0aekPtUBJpuj+gyj0bl6FEn9gjIVT7yH9ejqYWaffsvSjxvt96vdBQJBnQJ3x+qXwJHrttzRszVueRmIXkeJ9amA2NofpTo1ktvNfLlt+0ytF0CW+pbtLTOv3ZQyEEyf2sILPPZsORrGw8CgYEArgbOlVoWT+ihFDTylbN1+ieKiVk8cYaru2y9ELgyX1TTzDgdqYfecHoq7uVb9DB8V0a93U6iKpaW8h8XdtFZVPt3DC0Nu3h3hRTeVYr0SEX4lYYyNcv5JFjJA9miWr/nqLT7xz5TRHxfnYq70Hl+/AVHltuMq67u1Q//bmIsAiUCgYEApRlnV2E3K9s3fNZT8CaZzBbZ4xD10rlbBoc4YA1pEciAoF6bfSogbuCWYOFO7ou025wzYp2ibmGMMh65YAbaGYTk/8+afljSWXKj6eztpQHv3C9qibipTSSWWntCZVXtUTOau3cx384zPddwGoyPynNILboVPMLG/qEsWJfGDE0CgYBDN0vZcedGTALTTDQaUCuUSGuA61+mNa7lAHXyHEMQgTIhIdeLIHhxFy64AQzIP2X3S9KPxQxryKhaslmcwfVwFoR/xnMUYs4/L7VEcxwxQh1mm2OPVc5QFJ7nhsrnpFG03tqUtWTyAdvofKpYNmUusFdbVEd8FGAZGUm+d9qppQKBgQCXsI4MN2wui+IPAqYVFI7K59Xz52Oui1MJ02M+Z2LBuRQ1OSOvH9sOnXSXiRe7EAThErW7SFZO90BDaMJEdXFEwv2FC+QEoYg618JvEunSX0gGluUsFLnQZkZZI5vI79t0tAHnbXE8/vEEYoVLLdLJxAOW64jFszIg/Wkw5L1Byw==";
        //    string alipayPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAg27TzKNFjjGc/NpDHLgO6zAwspfnaWDaFAvGZFbKMEuS1X9hVARpBCpWJuv1sjNOriRIboj256bg9XxUeXwrm/ASMyFNH1k9S505xiJ2jIFrCKWoZPWSKeAAbMIG5IfnjOO9/NDuYPWner7SH6pPsqKCCwvVTOPo2mGhZKz+sxLoKlziMPcL4sFJ2QGjUQHYqbV+FFJrjY1CnNGbkrVUd365Wd3CRYlXb7k1TJ1x9cpX9kdzDw2WZQAG4u4nkaUFwd5nX5d5z0XRh2m8VVnQXRPJS/dx+/l/GE4vYIDNIJjcmoer/SLFSXan+1OIFFH4bVyOayskqT8sdIqyA8379wIDAQAB";
        //    AlipayConfig alipayConfig = new AlipayConfig();
        //    alipayConfig.ServerUrl = "https://openapi.alipay.com/gateway.do";
        //    alipayConfig.AppId = "2021004127636060";
        //    alipayConfig.PrivateKey = privateKey;
        //    alipayConfig.Format = "json";
        //    alipayConfig.AlipayPublicKey = alipayPublicKey;
        //    alipayConfig.Charset = "UTF-8";
        //    alipayConfig.SignType = "RSA2";
        //    IAopClient alipayClient = new DefaultAopClient(alipayConfig);
        //    AlipaySystemOauthTokenRequest request = new AlipaySystemOauthTokenRequest();
        //    request.Code = code;
        //    request.GrantType = "authorization_code";
        //    AlipaySystemOauthTokenResponse response = alipayClient.Execute(request);
        //    if (!response.IsError)
        //    {
        //        //resultmsg.data = response.AlipayUserId;
        //        Console.WriteLine("调用成功");
        //        return response.AlipayUserId;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}


        private static long lastTimestamp = -1;
        private static long sequence = 0;

        private static readonly object lockObject = new object();

        //日期+雪花算法生成序号
        //machineId 节点id，可传入业务单元id或者门店id，传入业务单元id一般作为主流水号以及支付订单号,
        public string SnowflakeGenerator(string machineId)
        {
            long asciiMachineId = GetAsciiValue(machineId);

            lock (lockObject)
            {
                long timestamp = GetTimestamp();

                if (timestamp == lastTimestamp)
                {
                    sequence = (sequence + 1) & 4095; 
                    if (sequence == 0)
                    {
                        timestamp = WaitForNextTimestamp(lastTimestamp);
                    }
                }
                else
                {
                    sequence = 0;
                }

                lastTimestamp = timestamp;

                long uniqueId = ((timestamp << 22) | (asciiMachineId << 12) | sequence);
                DateTime currentDate = DateTime.Now;
                string formattedDate = currentDate.ToString("yyyyMMdd");  //雪花算法前加日期
                return formattedDate + uniqueId.ToString(); 
            }
        }
        

        private static long GetAsciiValue(string input)
        {
            long total = 0;
            foreach (char c in input)
            {
                total += (int)c;
            }
            return total;
        }

        private static long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private static long WaitForNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetTimestamp();
            }
            return timestamp;
        }





    }
}
