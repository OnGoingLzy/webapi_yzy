﻿using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Util;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using webapi_yzy.Model;
using webapi_yzy.Service;
using System.IO;
namespace webapi_yzy.Controllers
{

    

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : Controller


    {
        public ResultMessage getTokenResult(string body, string appid, string method)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回值
            ResultMessage resultmsg = new ResultMessage();
            string timestamp = "";
            string nonce = "";
            string sign = "";
            string inputstr = "";
            try
            {
                //获取头部信息
                timestamp = HttpContext.Request.Headers["X-Ca-Timestamp"];//时间戳
                nonce = HttpContext.Request.Headers["X-Ca-Nonce"];//随机数
                sign = HttpContext.Request.Headers["X-Ca-Signature"]; //签名

                //身份验证
                resultmsg = Token.getTokenResult(appid, method, timestamp, nonce, sign, body);

                if (resultmsg.code != 99)//验证失败记录日志
                {
                    inputstr = "appid:" + appid + "&method:" + method + "&timestamp:" + timestamp + "&nonce:" + nonce + "&sign:" + sign;

                    DbOperator.saveWebapiOutputLog(appid, method, "身份验证", inputstr, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                return resultmsg;
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { code = -1, msg = "exception:" + ex.Message };
                //执行失败记录日志
                DbOperator.saveWebapiOutputLog(appid, method, "身份验证", inputstr, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return resultmsg;

            }
        }

        //检查用户是否存在，不存在则新建用户
        public ActionResult checkUserExist(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString(); //openid
                string sharerPhone = bodyObject["sharerPhone"] == null ? "0" : bodyObject["sharerPhone"].ToString();
                string dbRes = DbOperator.CheckUserExist(openid,sharerPhone);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "校验用户是否存在", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public ActionResult getAliPayUserId(object obj)
        {

            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string code = bodyObject["code"] == null ? "" : bodyObject["code"].ToString(); //
                string phoneMsg = bodyObject["getPhoneResponse"] == null ? "" : bodyObject["getPhoneResponse"].ToString(); //
                string sharerPhone = bodyObject["sharerPhone"] == null ? "0" : bodyObject["sharerPhone"].ToString();
                AliPayService aliPayService = new AliPayService();

                string phoneMsgJson = aliPayService.AliPayDecrypt(phoneMsg);
                JObject phoneMsgObj = (JObject)JsonConvert.DeserializeObject(phoneMsgJson);
                //string privateKey = "MIIEpAIBAAKCAQEAk/p6/W5jADYZMzBdnera0wOsJcUtdXV/A05MBA+h/O9H3M8Qo1UWWE9bNVMTGGPPEywVdpG4mjG4c4mW1HsyQh4XiP5hDCbFnqZ5P9U4LC1bKocNd6KkP4G3i0Pgx1hdVM43LCf0Wnyo/h1VhI+C/TQ9hu4FLbtkPKowYHWSAEnsnMGxl4NsZRpAi5WDFVi2VWXHjnyVzyAR6vCOu2Yc+xYhD3TMHVZx/XPeldqs4KEN+iWvESxedWU85VWFwlrNY5dujKJNXmKtRqCs0XkK1pakVAt9fa3B1bfgVM7Ub7lzkT9LiSoTUkX+PEbevWI6cWdFRmbjMwxvpKyQPkUHKwIDAQABAoIBAFNItXHvb87No884mGfpp3D97yRu3k/uaibdKVFbzwrBTUvMSovytpnLSAUyFVwzyHb857OM/j3iX/K+7GH4WGf38b0Llmk1ZSIOhc1UXsIWYOoFiqHbqt+HTVsXP+pSJG03hrvWXOOUp7QlpFzwPIPvo/VN/yInBppSGtofMaXxGMFMkZpaUendWdJ9v29biThuRafp8KA8xWYMCpPZ7g27aLEL2QRbnKo8uS/RNrEbqOQvXMiltTrKSZoDm8tkFRaQmRwe3rdZ6U+XPimORq5O58c5bgtnI3ncMvsG9G3qQe85+Gx3kOKbSskkMB6kJXSJFuc42xdbBRFwD3OGsVkCgYEA2a6gtqcMEUarN1/5cOWhWgUeh8v3U0aekPtUBJpuj+gyj0bl6FEn9gjIVT7yH9ejqYWaffsvSjxvt96vdBQJBnQJ3x+qXwJHrttzRszVueRmIXkeJ9amA2NofpTo1ktvNfLlt+0ytF0CW+pbtLTOv3ZQyEEyf2sILPPZsORrGw8CgYEArgbOlVoWT+ihFDTylbN1+ieKiVk8cYaru2y9ELgyX1TTzDgdqYfecHoq7uVb9DB8V0a93U6iKpaW8h8XdtFZVPt3DC0Nu3h3hRTeVYr0SEX4lYYyNcv5JFjJA9miWr/nqLT7xz5TRHxfnYq70Hl+/AVHltuMq67u1Q//bmIsAiUCgYEApRlnV2E3K9s3fNZT8CaZzBbZ4xD10rlbBoc4YA1pEciAoF6bfSogbuCWYOFO7ou025wzYp2ibmGMMh65YAbaGYTk/8+afljSWXKj6eztpQHv3C9qibipTSSWWntCZVXtUTOau3cx384zPddwGoyPynNILboVPMLG/qEsWJfGDE0CgYBDN0vZcedGTALTTDQaUCuUSGuA61+mNa7lAHXyHEMQgTIhIdeLIHhxFy64AQzIP2X3S9KPxQxryKhaslmcwfVwFoR/xnMUYs4/L7VEcxwxQh1mm2OPVc5QFJ7nhsrnpFG03tqUtWTyAdvofKpYNmUusFdbVEd8FGAZGUm+d9qppQKBgQCXsI4MN2wui+IPAqYVFI7K59Xz52Oui1MJ02M+Z2LBuRQ1OSOvH9sOnXSXiRe7EAThErW7SFZO90BDaMJEdXFEwv2FC+QEoYg618JvEunSX0gGluUsFLnQZkZZI5vI79t0tAHnbXE8/vEEYoVLLdLJxAOW64jFszIg/Wkw5L1Byw==";
                //string alipayPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAg27TzKNFjjGc/NpDHLgO6zAwspfnaWDaFAvGZFbKMEuS1X9hVARpBCpWJuv1sjNOriRIboj256bg9XxUeXwrm/ASMyFNH1k9S505xiJ2jIFrCKWoZPWSKeAAbMIG5IfnjOO9/NDuYPWner7SH6pPsqKCCwvVTOPo2mGhZKz+sxLoKlziMPcL4sFJ2QGjUQHYqbV+FFJrjY1CnNGbkrVUd365Wd3CRYlXb7k1TJ1x9cpX9kdzDw2WZQAG4u4nkaUFwd5nX5d5z0XRh2m8VVnQXRPJS/dx+/l/GE4vYIDNIJjcmoer/SLFSXan+1OIFFH4bVyOayskqT8sdIqyA8379wIDAQAB";

                string privateKey = "MIIEpAIBAAKCAQEAhz/j/Eau60OWYvA0eZuIC3P4KOYrsTpAncUaw4is+PsWn3cuEJ95vyvO4fd/yUhMIZOX92a/C/vc54gwtPFZvrDhbuGL7Iz4SvDvNqZi1+kuPLL54AJEl1PALyCG9zuN2MB6sAiTu/tketPWGoZrHziRKsv0J306dZjtEkhTswuvqbjRU8DxlQA45JPAjmUDDynDoKm19WpTUxasKiJlPKAGWEVVCGwQvFPdX5YJ/36P7XmapR1Q40t2/BhyTcrs7YSZep066kkGx/HlXXREBVIlZFNkLlQ1O4ZpKF9PCIEGqWg7xa+S9ABvV0QDmOA0RlBEuMCsn+8KvYGLuuQKxwIDAQABAoIBAG48PO5aDifjJqftcFfN1MzA8Psk32fMOu5cwLuuoo0s2fuPgBQ8Z1bRtgWnXJgKPUMAsVQMgpqZ5iswbcKNFMDAGn3th8Z9/8HVLqQ0aUvh2rXOiqtyLcXJlVLUCC1qdOq9t0HTO9ZzyCn0jwV/mcAQswFWyuGEyCkhQ9kq6k17gVJDUj7YyWfwKreHGkLZa/qEthO5gFVfcKWBHpaS5XiMUhOYlTvCa4pGWv0k3npJ647nbAs42n9EvY+O4We5V9jMub4a9jbrsONGt3lMN+g+VdRPxPFG+PcQuJ2iBXMhS8XOp7cEPjL3T2ZflaNOlqbW9jCp6aYxxrfNK5pKu0ECgYEAwgbrW+o8ohQH/gRzB9g9LgATs8unRsslZ5Mgwis05NTXi2bMsuqfA1puZ9gOScmoTKi62Z7H6giQ9QaorvvP5SWgG5FBk33Od60GId2y1oAVYR3NLjMFfYHcSlid06jLY8A3jYvHjFRHDMz964k1uDlylgoUh6ftLcqBiZBbAlUCgYEAsnLkmEDcfh9Y1eNNJlFhAPCj9RtKMDcdSUbzBNXvPvEIplhdVK+coalHAriRjHReWAG/9354MAbcuP7FODBhjh2tROyEiu18Uta3CD5eAUN94NOXOmLFGkUZ4nir9UWuqeSngodzSswYnI9+NJKd+eV2mUpCC4KXjzmucRaHjKsCgYEAvK+n8KHxRxk90HyZNRbIIFT41A+H5COog3okEE+eR67o0mstRA1AiC1IUvpcostWPP6Vfw6XkXO1LGPGZS0DNu+JLWXsJLfisqVz4jlMMqcqo2As6fG6NBy2kyJskaqD6MTEAYXekAxhcARmJskvkbFkSp8Q6f+XesGFibb1mmUCgYANI0pTL0tMedBXhvGSJaGnqQ/ZJtYycIxWbcPMkleX4bUKUi0k4/z7JfRVbfNBTPv5LL/OX+BHYOn1MMSiW2WDSgoMrgEgHVXu32IAWF8hZq+o0ssqgEUUpwu90baPSz8NwQjYIlLJqfcR57qZ8kVxNYSiyoXMRSQVZmDCcK8wGwKBgQCueUYPEjeOV8Tx/zebSx7EqFif8vkCK4cCW4PNbAf7jy+LddPsIa+0avge/EoBcuTRqp+3a+cCalJqLqxIQMOp+2HatL3N0jIDudCcb8cGV47L4A+Lj3tvxTFzIshVQFdsIcpPztuDG50CeolBPnr0C+/0F6rt5vogAjiJ/QTDtw==";
                //string alipayPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAhz/j/Eau60OWYvA0eZuIC3P4KOYrsTpAncUaw4is+PsWn3cuEJ95vyvO4fd/yUhMIZOX92a/C/vc54gwtPFZvrDhbuGL7Iz4SvDvNqZi1+kuPLL54AJEl1PALyCG9zuN2MB6sAiTu/tketPWGoZrHziRKsv0J306dZjtEkhTswuvqbjRU8DxlQA45JPAjmUDDynDoKm19WpTUxasKiJlPKAGWEVVCGwQvFPdX5YJ/36P7XmapR1Q40t2/BhyTcrs7YSZep066kkGx/HlXXREBVIlZFNkLlQ1O4ZpKF9PCIEGqWg7xa+S9ABvV0QDmOA0RlBEuMCsn+8KvYGLuuQKxwIDAQAB";

                //AlipayConfig alipayConfig = new AlipayConfig();
                //alipayConfig.ServerUrl = "https://openapi.alipay.com/gateway.do";
                //alipayConfig.AppId = "2021004127636060";
                //alipayConfig.PrivateKey = privateKey;
                //alipayConfig.Format = "json";

                //string alipayPublicCertPath = "C:/key/alipayCertPublicKey_RSA2.crt";

                //alipayConfig.AlipayPublicKey  = aliPayService.getPemPublicKeyFromCert(alipayPublicCertPath);

                ////alipayConfig.AlipayPublicKey = alipayPublicKey;
                //alipayConfig.Charset = "UTF-8";
                //alipayConfig.SignType = "RSA2";
                //IAopClient alipayClient = new DefaultAopClient(alipayConfig);
                //设置证书相关参数
                CertParams certParams = new CertParams
                {
                    AlipayPublicCertPath = "C:/key/alipayCertPublicKey_RSA2.crt",
                    AppCertPath = "C:/key/appCertPublicKey_2021004127636060.crt",
                    RootCertPath = "C:/key/alipayRootCert.crt"
                };
                IAopClient alipayClient = new DefaultAopClient("https://openapi.alipay.com/gateway.do", "2021004127636060", privateKey, "json", "1.0", "RSA2", "utf-8", false, certParams);
                AlipaySystemOauthTokenRequest request = new AlipaySystemOauthTokenRequest();
                request.Code = code;
                request.GrantType = "authorization_code";
                AlipaySystemOauthTokenResponse response = alipayClient.CertificateExecute(request);
                if (!response.IsError)
                {
                    resultmsg.code = 99;
                    //resultmsg.data = response.AlipayUserId;
                    Console.WriteLine("调用成功");
                    string dbRes = DbOperator.CheckAliPayUserExist(response.UserId, sharerPhone,(string)phoneMsgObj["mobile"]);
                    object dataObj = JSON.Decode(dbRes);
                    resultmsg.data = dataObj;
                }
                else
                {
                    resultmsg.code = -99;
                    Console.WriteLine("调用失败");
                }
                

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public ActionResult getUserPhoneNumber(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString(); //
                string secret = bodyObject["secret"] == null ? "" : bodyObject["secret"].ToString(); //
                string code = bodyObject["code"] == null ? "" : bodyObject["code"].ToString(); //
                //获取手机号前，先要获取token
                //string dbRes = DbOperator.GetAccessToken(appid, secret);
                //JObject jsonsp = (JObject)JsonConvert.DeserializeObject(dbRes);
                WechatPayService payService = new WechatPayService();
                string accessToken = payService.getAccessToken();


                //用token和code获取手机号
                string getPhoneResult = DbOperator.GetPhoneNumber(code, accessToken);
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "微信接口获取手机号结果", body, getPhoneResult, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                JObject jobj = JObject.Parse(getPhoneResult);
                string phone = (string)jobj["data"]["phone_info"]["phoneNumber"];
                string dbRes2 = DbOperator.bindUserPhone(openid,phone);


                object dataObj = JSON.Decode(dbRes2);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "处理用户手机号", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public static string CheckUserExist(string openid, string sharerPhone) {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@sharerPhone", SqlDbType.VarChar, 50, sharerPhone));

                DataTable dt = BaseSql.getInstance().procedure("p_user_checkUserExist", arr);
                if (dt != null && dt.Rows.Count > 0) {
                    for (int i = 0; i < dt.Rows.Count; i++) {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0) {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                } else {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        //获取用药人(商品使用者)
        public ActionResult getGoodsUser(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string phoneNumber = bodyObject["phone"] == null ? "" : bodyObject["phone"].ToString(); //
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString(); //

                string dbRes = DbOperator.getGoodsUser(openid,phoneNumber);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取用药人", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }


        public ActionResult delGoodsUser(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string id = bodyObject["id"] == null ? "" : bodyObject["id"].ToString(); //
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString(); //

                string dbRes = DbOperator.delGoodsUser( id, openid);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "删除用药人", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public ActionResult addGoodsUser(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string phone = bodyObject["goodsUserInfo"]["phone"] == null ? "" : bodyObject["goodsUserInfo"]["phone"].ToString(); //
                string openid = bodyObject["goodsUserInfo"]["openid"] == null ? "" : bodyObject["goodsUserInfo"]["openid"].ToString(); //
                string sex = bodyObject["goodsUserInfo"]["sex"] == null ? "" : bodyObject["goodsUserInfo"]["sex"].ToString(); //
                string idCard = bodyObject["goodsUserInfo"]["idCard"] == null ? "" : bodyObject["goodsUserInfo"]["idCard"].ToString(); //
                string relationship = bodyObject["goodsUserInfo"]["relationship"] == null ? "" : bodyObject["goodsUserInfo"]["relationship"].ToString(); //
                string birthday = bodyObject["goodsUserInfo"]["birthday"] == null ? "" : bodyObject["goodsUserInfo"]["birthday"].ToString(); //
                string goodsUserName = bodyObject["goodsUserInfo"]["goodsUserName"] == null ? "" : bodyObject["goodsUserInfo"]["goodsUserName"].ToString(); //


                IdVerifyService service = new IdVerifyService();
                string verifyResult = service.PostALiIdVerifyService(idCard, goodsUserName, "");
                string existsResult  = DbOperator.checkGoodsUserExists(openid, phone, sex, idCard, relationship, birthday, goodsUserName);
                if (existsResult.Contains("\"flag\":-99"))
                {
                    resultmsg.code = -99;
                    resultmsg.msg = "用药人已存在";
                    resultmsg.data = "验证结束!";
                    DbOperator.saveWebapiOutputLog(appid, method, "添加用药人", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }
                //string verifyResult = service.PostALiIdVerifyServiceByThreeElement(idCard, goodsUserName, "", phone);
                if (verifyResult.Equals("验证成功!"))
                {
                    string dbRes = DbOperator.addGoodsUser(openid, phone, sex, idCard, relationship, birthday, goodsUserName);
                    object dataObj = JSON.Decode(dbRes);
                    resultmsg.data = dataObj;
                    resultmsg.code = 99;
                    DbOperator.saveWebapiOutputLog(appid, method, "添加用药人", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    resultmsg.code = -99;
                    resultmsg.msg = "错误的身份证信息";
                    resultmsg.data = "验证不通过!";
                    DbOperator.saveWebapiOutputLog(appid, method, "添加用药人", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }


                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }
        
        public ActionResult getShareCode(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string phone = bodyObject["phone"] == null ? "" : bodyObject["phone"].ToString(); //

                //string spres = DbOperator.GetAccessToken(appid, "");
                //JObject jsonsp = (JObject)JsonConvert.DeserializeObject(spres);
                WechatPayService payService = new WechatPayService();
                string accessToken = payService.getAccessToken();

                //用token和code分享码
                string spres_QRCode = DbOperator.getShareQRCode(phone, accessToken);

                JObject jsonsp_QRCode = (JObject)JsonConvert.DeserializeObject(spres_QRCode);

                resultmsg.msg = jsonsp_QRCode["sm"].ToString();
                resultmsg.data = jsonsp_QRCode["data"].ToString();
                DbOperator.saveWebapiOutputLog(appid, method, "获取个人推广码", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public ActionResult getAddressList(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询收货地址内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString(); //

                string dbRes = DbOperator.getAddressList(openid);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;

                DbOperator.saveWebapiOutputLog(appid, method, "获取用户地址信息", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }
        //新增地址
        public ActionResult addAddress(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //新增地址内容 
                // "Body" 字段解析为一个 JObject 对象 
                JObject bodyObject = JObject.Parse(trueBody);
                string name = bodyObject["pathObj"]["name"] == null ? "" : bodyObject["pathObj"]["name"].ToString(); //
                string phone = bodyObject["pathObj"]["tel"] == null ? "" : bodyObject["pathObj"]["tel"].ToString(); //
                string city = bodyObject["pathObj"]["city"] == null ? "" : bodyObject["pathObj"]["city"].ToString(); //
                string openid = bodyObject["pathObj"]["openid"] == null ? "" : bodyObject["pathObj"]["openid"].ToString(); //
                string isDefault = bodyObject["pathObj"]["isDefault"] == null ? "" : bodyObject["pathObj"]["isDefault"].ToString(); //
                string detailAddress = bodyObject["pathObj"]["details"] == null ? "" : bodyObject["pathObj"]["details"].ToString(); //        
                decimal longitude = bodyObject["pathObj"]["longitude"] == null ? 0 : (decimal)bodyObject["pathObj"]["longitude"];
                decimal latitude = bodyObject["pathObj"]["latitude"] == null ? 0 : (decimal)bodyObject["pathObj"]["latitude"];
                string dbRes = DbOperator.addAddress(name, phone, city, openid, int.Parse(isDefault), detailAddress,longitude,latitude);
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                resultmsg.code = 99;


                DbOperator.saveWebapiOutputLog(appid, method, "添加用户地址", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }
        //新增地址
        public ActionResult updateAddress(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //新增地址内容 
                // "Body" 字段解析为一个 JObject 对象 
                JObject bodyObject = JObject.Parse(trueBody);
                string id = bodyObject["pathObj"]["id"] == null ? "" : bodyObject["pathObj"]["id"].ToString(); //
                string name = bodyObject["pathObj"]["name"] == null ? "" : bodyObject["pathObj"]["name"].ToString(); //
                string phone = bodyObject["pathObj"]["tel"] == null ? "" : bodyObject["pathObj"]["tel"].ToString(); //
                string city = bodyObject["pathObj"]["city"] == null ? "" : bodyObject["pathObj"]["city"].ToString(); //
                string userId = bodyObject["pathObj"]["userId"] == null ? "" : bodyObject["pathObj"]["userId"].ToString(); //
                string isDefault = bodyObject["pathObj"]["isDefault"] == null ? "" : bodyObject["pathObj"]["isDefault"].ToString(); //
                string detailAddress = bodyObject["pathObj"]["details"] == null ? "" : bodyObject["pathObj"]["details"].ToString(); //        
                decimal longitude = bodyObject["pathObj"]["longitude"] == null ? 0 : (decimal)bodyObject["pathObj"]["longitude"];
                decimal latitude = bodyObject["pathObj"]["latitude"] == null ? 0 : (decimal)bodyObject["pathObj"]["latitude"];
                string dbRes = DbOperator.updateAddress(int.Parse(id), name, phone, city, userId, int.Parse(isDefault), detailAddress, longitude, latitude);
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                resultmsg.code = 99;

                DbOperator.saveWebapiOutputLog(appid, method, "更新用户地址", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }
        //获取收货地址
        public ActionResult deleteAddress(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询收货地址内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string userId = bodyObject["userId"] == null ? "" : bodyObject["userId"].ToString(); //
                string id = bodyObject["id"] == null ? "" : bodyObject["id"].ToString(); //
                string dbRes = DbOperator.deleteAddress(userId, int.Parse(id));


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                DbOperator.saveWebapiOutputLog(appid, method, "删除用户地址", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        /// <summary>
        /// 获取用户购物车中的商品信息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult getUserCartGoods(object obj) {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */

                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99) {
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                //获取用户对应的openid，然后去数据库中获取该用户的购物车信息
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString();

                string dbRes = DbOperator.getUserCartGoods(openid);
                JObject jsonResultObject = (JObject)JsonConvert.DeserializeObject(dbRes);
                string dataResult = jsonResultObject["data"].ToString();
                //如果没有获取到该商品对应的在水医方的商品码
                if (dataResult == "") {
                    resultmsg.code = -99;
                    resultmsg.msg = (string)jsonResultObject["result"];
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "获取用户购物车内容", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //如果获取到了购物车的内容，则将其返回给前端进行处理
                resultmsg.code = (int)jsonResultObject["flag"];
                resultmsg.msg = (string)jsonResultObject["result"];
                resultmsg.data = dataResult;

                DbOperator.saveWebapiOutputLog(appid, method, "获取用户购物车内容", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            } catch (Exception ex) {
                resultmsg = new ResultMessage { msg = "Exception: " + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        /// <summary>
        /// 用户删除购物车中的药品信息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult delUserCartGoods(object obj) {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */

                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99) {
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                //获取用户对应的openid，然后去数据库中获取该用户的购物车信息
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString();
                string cartId = bodyObject["cartId"] == null ? "" : bodyObject["cartId"].ToString();

                string dbRes = DbOperator.delUserCartGoods(openid, cartId);
                JObject jsonResultObject = (JObject)JsonConvert.DeserializeObject(dbRes);
                string flagResult = jsonResultObject["flag"].ToString();
                //如果没有获取到该商品对应的在水医方的商品码
                if (flagResult != "99") {
                    resultmsg.code = -99;
                    resultmsg.msg = (string)jsonResultObject["result"];
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "删除购物车内容", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //如果获取到了购物车的内容，则将其返回给前端进行处理
                resultmsg.code = (int)jsonResultObject["flag"];
                resultmsg.msg = (string)jsonResultObject["result"];

                DbOperator.saveWebapiOutputLog(appid, method, "删除购物车内容", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            } catch (Exception ex) {
                resultmsg = new ResultMessage { msg = "Exception: " + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public ActionResult searchHistory(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */


                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                //先获取门店信息
                string lxdh = bodyObject["lxdh"] == null ? "" : bodyObject["lxdh"].ToString(); //商品查询名
                string pageIndex = bodyObject["pageIndex"] == null ? "" : bodyObject["pageIndex"].ToString();//页码
                string pageSize = bodyObject["pageSize"] == null ? "" : bodyObject["pageSize"].ToString();//每页记录数

                string spres = DbOperator.SearchHistory(appid, lxdh, int.Parse(pageIndex), int.Parse(pageSize));




                resultmsg.data = JSON.Decode(spres);
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取预订药门店", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }




        }


        /// <summary>
        /// 用户点击按钮后上传货品确认收货的状态 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult uploadConfirmReceived(object obj) {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */

                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99) {
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                //获取用户对应的openid，然后去数据库中获取该用户的购物车信息
                string tradeNo = bodyObject["tradeNo"] == null ? "" : bodyObject["tradeNo"].ToString();
                string confirmType = bodyObject["confirmType"] == null ? "" : bodyObject["confirmType"].ToString();

                string dbRes = DbOperator.uploadConfirmReceived(tradeNo, confirmType);
                JObject jsonResultObject = (JObject)JsonConvert.DeserializeObject(dbRes);
                string flagResult = jsonResultObject["flag"].ToString();

                if (flagResult != "99") {
                    resultmsg.code = -99;
                    resultmsg.msg = (string)jsonResultObject["result"];
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "用户确认收货", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }
                
                var uploadConfirmReceivedInfoObj = new
                {
                    operateId = (int)jsonResultObject["operateId"],
                    receiveConfirmTime = (string)jsonResultObject["receiveConfirmTime"],
                };
                string uploadConfirmReceivedInfoStr = JsonConvert.SerializeObject(uploadConfirmReceivedInfoObj);
                //如果获取到了存储过程返回回来的内容，则将其返回给前端进行处理
                resultmsg.code = (int)jsonResultObject["flag"];
                resultmsg.msg = (string)jsonResultObject["result"];
                resultmsg.data = uploadConfirmReceivedInfoStr;
                
                DbOperator.saveWebapiOutputLog(appid, method, "用户确认收货", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            } catch (Exception ex) {
                resultmsg = new ResultMessage { msg = "Exception: " + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }


        /// <summary>
        /// 用户申请退款
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult applyReturned(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */

                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string tradeNo = bodyObject["orign_main_order_id"] == null ? "" : bodyObject["orign_main_order_id"].ToString();
                string reasonOfReturn = bodyObject["reasonOfReturn"] == null ? "" : bodyObject["reasonOfReturn"].ToString();

                resultmsg = DbOperator.applyReturned(tradeNo, reasonOfReturn);
                
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "Exception: " + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        /// <summary>
        /// 用户申请退款传到门店所属的JSON
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult getJsonZhyfOrderReturned(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */

                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);

                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();

                resultmsg = DbOperator.getJsonZhyfOrderReturned(main_order_id);

                DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "Exception: " + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        /// <summary>
        /// 用户获取退货原因
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult getRefundReason(object obj) {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            try {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = reqObj["Body"].ToString();
                /*
                 obj中的Body才是真实的body
                 */

                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99) {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);
                //openid
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString(); 

                string dbRes = DbOperator.getRefundReason(openid);

                resultmsg = new ResultMessage { msg = "获取退款原因成功", code = 99, data = dbRes};

                return new JsonResult(resultmsg);
            } catch (Exception ex) {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

    }
}
