using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using webapi_yzy.common;

namespace webapi_yzy.Controllers {

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RecipeController : Controller {

        //定义类变量，主要是spid和密钥等
        private const string SPID_PROD = "718806033635475456";              //生产环境
        private const string SPID_DEV = "718816295813906432";               //测试环境

        private const string PWD_PROD = "0a0aea4d992fff8291f7edba2930a759";
        private const string PWD_DEV = "d26eef44876cfcdabe24045d55aed673";

        /* 2023-09-14 注释正式环境的pharmacyId，不再使用统一的远程审方中心绑定的药店，而是使用各家门店自己的pharmacyId
        private const string PHARMACY_ID_PROD = "718806463757156352";
        */
        private const string PHARMACY_ID_DEV = "718816376378097664";
        

        private const string URL_PROD = "https://recipe.hospf.com/clinic/open/app";
        private const string URL_DEV = "https://dev.hospf.com/clinic/open/app";


        public ActionResult getRecipeList(object obj)
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
                string type = bodyObject["type"] == null ? "" : bodyObject["type"].ToString();

                string dbRes = DbOperator.getRecipeList(openid,type) ;
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取用户的所有处方", body, dbRes, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public ResultMessage getTokenResult(string body, string appid, string method) {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回值
            ResultMessage resultmsg = new ResultMessage();
            string timestamp = "";
            string nonce = "";
            string sign = "";
            string inputstr = "";
            try {
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
            } catch (Exception ex) {
                resultmsg = new ResultMessage { code = -1, msg = "exception:" + ex.Message };
                //执行失败记录日志
                DbOperator.saveWebapiOutputLog(appid, method, "身份验证", inputstr, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return resultmsg;

            }
        }

        //判断该药品用户是否存在有效处方
        public ActionResult existsEffectiveRecipe(object obj)
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
                string goodsId = bodyObject["goodsId"] == null ? "" : bodyObject["goodsId"].ToString();
                decimal num = bodyObject["num"] == null ? 1 : (decimal)bodyObject["num"];

                string dbRes = DbOperator.existsEffectiveRecipe(openid, goodsId,num);
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "查询当前商品该用户是否存在可用的处方", body, dbRes, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }


        /// <summary>
        /// 通过药品来获取疾病名称
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult getDisease(object obj) {
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

                //获取商品的商品代码
                string goodsId = bodyObject["goodsId"] == null ? "" : bodyObject["goodsId"].ToString(); 
                
                string dbRes = DbOperator.getSmarthosGoodsId(goodsId);
                JObject jsonResultObject = (JObject)JsonConvert.DeserializeObject(dbRes);
                string dataResult = jsonResultObject["data"].ToString();
                //如果没有获取到该商品对应的在水医方的商品码
                if (dataResult == "")
                {
                    resultmsg.code = -99;
                    resultmsg.msg = (string)jsonResultObject["result"];
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "获取疾病名称", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }
                //如果获取到了在水医方的商品ID，则可以进行后续的操作
                JArray jsonArray = JArray.Parse(dataResult);
                JObject jsonObject = (JObject)jsonArray[0];                 //只解析第一个数组中的数据，因为商品代码和医方的商品代码是对应的
                string smarthosGoodsId = (string)jsonObject["smarthosGoodsId"];

                Dictionary<string, object> paramsDict = new Dictionary<string, object>
                {
                    { "spid", SPID_PROD },
                    { "service", "smarthos.recipe.rational.drug.diagnosis.list" },
                    { "drugIdList", new List<string> { smarthosGoodsId }}
                };
                
                //请求在水医方的接口：获取药品的适应症
                HttpWebResponse result = ApiRequestUtils.SendPostRequest(URL_PROD, PWD_PROD, paramsDict);
                string icdNameJson = string.Empty;
                //处理响应
                if (result.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(result.GetResponseStream() ?? new MemoryStream(Encoding.UTF8.GetBytes("")))) {
                        string readToEnd = streamReader.ReadToEnd();
                        JObject jo = JObject.Parse(readToEnd);
                        JArray icdNameArray = new JArray();
                        foreach (JToken item in jo["list"]) {
                            string icdName = (string)item["extResponse"]["icdName"];
                            icdNameArray.Add(icdName);
                        }
                        icdNameJson = icdNameArray.ToString();
                        icdNameJson = icdNameJson.Replace("\n", "").Replace("\r", "").Replace(" ", "");
                    }
                }

                resultmsg.code = 99;
                resultmsg.msg = "查询成功";
                resultmsg.data = icdNameJson;

                DbOperator.saveWebapiOutputLog(appid, method, "获取疾病名称", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new JsonResult(resultmsg);
            } catch (Exception ex) {
                resultmsg = new ResultMessage { msg = "Exception: " + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        /// <summary>
        /// 提交问诊开方的信息进行问诊开方
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult consultate(object obj) {
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

                //获取商品的商品代码
                string goodsId = bodyObject["goodsId"] == null ? "" : bodyObject["goodsId"].ToString();
                //获取患者的过敏史
                string allergyHistory = bodyObject["allergyHistory"] == null ? "" : bodyObject["allergyHistory"].ToString();
                //获取商品的购买数量
                string amount = bodyObject["amount"] == null ? "" : bodyObject["amount"].ToString();
                //获取患者的疾病名称
                string diseaseName = bodyObject["diseaseName"] == null ? "" : bodyObject["diseaseName"].ToString();
                //获取用户的实名认证ID
                string personId = bodyObject["personId"] == null ? "" : bodyObject["personId"].ToString();
                //获取准备购买的shopId
                string shopId = bodyObject["shopId"] == null ? "" : bodyObject["shopId"].ToString();

                string dbRes = DbOperator.getSmarthosGoodsId(goodsId);
                JObject jsonResultObject = (JObject)JsonConvert.DeserializeObject(dbRes);
                string dataResult = jsonResultObject["data"].ToString();
                //如果没有获取到该商品对应的在水医方的商品码
                if (dataResult == "") {
                    resultmsg.code = -99;
                    resultmsg.msg = (string)jsonResultObject["result"];
                    DbOperator.saveWebapiOutputLog(appid, method, "问诊开方", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }
                //如果获取到了在水医方的商品ID，则可以进行后续的操作
                JArray jsonArray = JArray.Parse(dataResult);
                //只解析第一个数组中的数据，因为商品代码和医方的商品代码是对应的
                JObject jsonObject = (JObject)jsonArray[0];       
                string smarthosGoodsId = (string)jsonObject["smarthosGoodsId"];

                Dictionary<string, object> paramsDict = new Dictionary<string, object>
                {
                    { "spid", SPID_PROD },
                    { "service", "smarthos.recipe.drug.info" },
                    { "drugId", smarthosGoodsId}
                };

                //请求在水医方的接口：获取药品使用方法
                HttpWebResponse result = ApiRequestUtils.SendPostRequest(URL_PROD, PWD_PROD, paramsDict);
                string drugName = string.Empty;
                string drugSpecification = string.Empty;
                string drugUnit = string.Empty;
                double dosage = 0.00;
                string dosageUnit = string.Empty;
                string drugAdmission = string.Empty;
                string frequencyCode = string.Empty;
                string frequencyName = string.Empty;
                
                //处理响应
                if (result.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(result.GetResponseStream() ?? new MemoryStream(Encoding.UTF8.GetBytes("")))) {
                        string readToEnd = streamReader.ReadToEnd();
                        // 解析JSON字符串
                        JObject jObject = JObject.Parse(readToEnd);
                        dosage = (double)jObject["obj"]["dosage"];
                        dosageUnit = (string)jObject["obj"]["dosageUnit"];
                        drugName = (string)jObject["obj"]["drugName"];
                        drugSpecification = (string)jObject["obj"]["drugSpecification"];
                        drugUnit = (string)jObject["obj"]["drugUnit"];
                        drugAdmission = (string)jObject["obj"]["drugAdmission"];
                        frequencyCode = (string)jObject["obj"]["frequencyCode"];
                        frequencyName = (string)jObject["obj"]["frequencyName"];
                    }
                }

                dbRes = DbOperator.consultate(goodsId, allergyHistory, amount, diseaseName, personId, smarthosGoodsId, dosage, dosageUnit, drugName, drugSpecification, drugUnit, drugAdmission, frequencyCode, frequencyName, shopId);
                jsonResultObject = (JObject)JsonConvert.DeserializeObject(dbRes);

                //处理从数据库层返回回来的-99的错误信息
                if (jsonResultObject["flag"].ToString() == "-99")
                {
                    resultmsg.code = int.Parse(jsonResultObject["flag"].ToString());
                    resultmsg.msg = jsonResultObject["result"].ToString();
                    resultmsg.data = null;
                    DbOperator.saveWebapiOutputLog(appid, method, "问诊开方", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //如果数据库返回的是99，代表可以处理后续的流程
                string jsonStrRecipe = jsonResultObject["data"].ToString();
                JArray jsonArrayRecipe = JArray.Parse(jsonStrRecipe);

                //2023-09-14 获取在水医方的问诊ID，完成问诊
                string pharmacyId = jsonResultObject["pharmacyId"].ToString();

                paramsDict = new Dictionary<string, object>
                {
                    { "service", "smarthos.recipe.upload" },
                    { "bizId", jsonArrayRecipe[0]["bizId"] != null ? (string)jsonArrayRecipe[0]["bizId"] : null },
                    { "spid", SPID_PROD },
                    { "pharmacyId", pharmacyId },
                    {
                        "patGender",
                        jsonArrayRecipe[0]["patGender"] != null ? (string)jsonArrayRecipe[0]["patGender"] : null
                    },
                    {
                        "patBirthday",
                        jsonArrayRecipe[0]["patBirthday"] != null ? (string)jsonArrayRecipe[0]["patBirthday"] : null
                    },
                    {
                        "patIdcard",
                        jsonArrayRecipe[0]["patIdcard"] != null ? (string)jsonArrayRecipe[0]["patIdcard"] : null
                    },
                    { "patName", jsonArrayRecipe[0]["patName"] != null ? (string)jsonArrayRecipe[0]["patName"] : null },
                    {
                        "patMobile",
                        jsonArrayRecipe[0]["patMobile"] != null ? (string)jsonArrayRecipe[0]["patMobile"] : null
                    },
                    {
                        "patWeight",
                        jsonArrayRecipe[0]["patWeight"] != null ? (string)jsonArrayRecipe[0]["patWeight"] : null
                    },
                    {
                        "diagnosisName",
                        jsonArrayRecipe[0]["diagnosisName"] != null ? (string)jsonArrayRecipe[0]["diagnosisName"] : null
                    },
                    {
                        "diseaseDescription",
                        jsonArrayRecipe[0]["diseaseDescription"] != null
                            ? (string)jsonArrayRecipe[0]["diseaseDescription"]
                            : null
                    },
                    {
                        "allergyHistory",
                        jsonArrayRecipe[0]["allergyHistory"] != null
                            ? (string)jsonArrayRecipe[0]["allergyHistory"]
                            : null
                    },
                    {
                        "clinicMode",
                        jsonArrayRecipe[0]["clinicMode"] != null ? (string)jsonArrayRecipe[0]["clinicMode"] : null
                    },
                    //后期引入附件地址列表时，attaUrlList这部分内容需要修改
                    {
                        "attaUrlList",
                        jsonArrayRecipe[0]["attaUrlList"] != null ? (string)jsonArrayRecipe[0]["attaUrlList"] : null
                    }, 
                    { "drugList", null }
                };

                //下面是赋值drugList的过程
                JArray drugListArray = new JArray();
                JObject drugList = new JObject
                {
                    ["drugId"] = jsonArrayRecipe[0]["smarthosGoodsId"] != null
                        ? (string)jsonArrayRecipe[0]["smarthosGoodsId"]
                        : null,
                    ["extDrugId"] =
                        jsonArrayRecipe[0]["goodsId"] != null ? (string)jsonArrayRecipe[0]["goodsId"] : null,
                    ["drugName"] = jsonArrayRecipe[0]["drugName"] != null
                        ? (string)jsonArrayRecipe[0]["drugName"]
                        : null,
                    ["drugSpecification"] = jsonArrayRecipe[0]["drugSpecification"] != null
                        ? (string)jsonArrayRecipe[0]["drugSpecification"]
                        : null,
                    ["drugUnit"] = jsonArrayRecipe[0]["drugUnit"] != null
                        ? (string)jsonArrayRecipe[0]["drugUnit"]
                        : null,
                    ["drugOrigin"] = jsonArrayRecipe[0]["drugOrigin"] != null
                        ? (string)jsonArrayRecipe[0]["drugOrigin"]
                        : null,
                    ["dosage"] = jsonArrayRecipe[0]["dosage"] != null ? (double)jsonArrayRecipe[0]["dosage"] : 0.00,
                    ["dosageUnit"] = jsonArrayRecipe[0]["dosageUnit"] != null
                        ? (string)jsonArrayRecipe[0]["dosageUnit"]
                        : null,
                    ["amount"] = jsonArrayRecipe[0]["amount"] != null ? (double)jsonArrayRecipe[0]["amount"] : 0.00,
                    ["admission"] = jsonArrayRecipe[0]["admission"] != null
                        ? (string)jsonArrayRecipe[0]["admission"]
                        : null,
                    ["frequencyCode"] = jsonArrayRecipe[0]["frequencyCode"] != null
                        ? (string)jsonArrayRecipe[0]["frequencyCode"]
                        : null,
                    ["frequencyName"] = jsonArrayRecipe[0]["frequencyName"] != null
                        ? (string)jsonArrayRecipe[0]["frequencyName"]
                        : null
                };
                drugListArray.Add(drugList);
                paramsDict["drugList"] = drugListArray;

                //请求在水医方的接口：上传问诊处方
                result = ApiRequestUtils.SendPostRequest(URL_PROD, PWD_PROD, paramsDict);
                //上传处方后返回的信息
                JObject jo = null;
                //处理响应
                if (result.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(result.GetResponseStream() ?? new MemoryStream(Encoding.UTF8.GetBytes("")))) {
                        string readToEnd = streamReader.ReadToEnd();
                        jo = JObject.Parse(readToEnd);
                        //如果请求返回的不是成功，则直接返回错误
                        if ((jo["succ"] ?? "").ToString() != "True")
                        {
                            int flagOut;
                            if (int.TryParse((jo["code"] ?? "0").ToString(), out flagOut)) {
                                resultmsg.code = flagOut;
                                resultmsg.msg = (jo["msg"] ?? "").ToString();
                                resultmsg.data = null;
                            } else {
                                resultmsg.code = -99;
                                resultmsg.msg = (jo["msg"] ?? "").ToString();
                                resultmsg.data = null;
                            }
                            
                            DbOperator.saveWebapiOutputLog(appid, method, "问诊开方", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            return new JsonResult(resultmsg);
                        }

                        dbRes = DbOperator.saveClinicId(jsonArrayRecipe[0]["bizId"].ToString(), jo["obj"].ToString());
                        jsonResultObject = (JObject)JsonConvert.DeserializeObject(dbRes);

                        //处理从数据库层返回回来的-99的错误信息
                        if (jsonResultObject["flag"].ToString() != "99") {
                            resultmsg.code = int.Parse(jsonResultObject["flag"].ToString());
                            resultmsg.msg = jsonResultObject["result"].ToString();
                            resultmsg.data = null;
                            DbOperator.saveWebapiOutputLog(appid, method, "问诊开方", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            return new JsonResult(resultmsg);
                        }
                    }
                }

                string clinicId = jo["obj"].ToString();

                    //后续的情况就是正常流程了，即状态码为99的情况
                paramsDict = new Dictionary<string, object>
                {
                    { "spid", SPID_PROD },
                    { "service", "smarthos.recipe.h5.get" },
                    { "clinicId", jo["obj"].ToString()}
                };
                //请求在水医方的接口：获取H5的问诊链接
                result = ApiRequestUtils.SendPostRequest(URL_PROD, PWD_PROD, paramsDict);
                //处理响应
                if (result.StatusCode == HttpStatusCode.OK) {
                    using (StreamReader streamReader = new StreamReader(result.GetResponseStream() ?? new MemoryStream(Encoding.UTF8.GetBytes("")))) {
                        string readToEnd = streamReader.ReadToEnd();
                        jo = JObject.Parse(readToEnd);
                        //如果请求返回的不是成功，则直接返回错误
                        if ((jo["succ"] ?? "").ToString() != "True") {
                            resultmsg.code = int.Parse((jo["code"] ?? "0").ToString());
                            resultmsg.msg = (jo["msg"] ?? "").ToString();
                            resultmsg.data = null;
                            DbOperator.saveWebapiOutputLog(appid, method, "问诊开方", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            return new JsonResult(resultmsg);
                        }

                        dbRes = DbOperator.saveClinicUrl(jsonArrayRecipe[0]["bizId"].ToString(), jo["obj"].ToString());
                        jsonResultObject = (JObject)JsonConvert.DeserializeObject(dbRes);

                        //处理从数据库层返回回来的-99的错误信息
                        if (jsonResultObject["flag"].ToString() != "99") {
                            resultmsg.code = int.Parse(jsonResultObject["flag"].ToString());
                            resultmsg.msg = jsonResultObject["result"].ToString();
                            resultmsg.data = null;
                            DbOperator.saveWebapiOutputLog(appid, method, "问诊开方", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            return new JsonResult(resultmsg);
                        }
                    }
                }
                //后续的情况就是正常流程了，即状态码为99的情况
                //目前已经获取到了问诊的URL链接
                resultmsg.code = 99;
                resultmsg.msg = "问诊URL链接获取成功";
                resultmsg.data = jo["obj"].ToString();

                DbOperator.saveWebapiOutputLog(appid, method, "问诊开方", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            } catch (Exception ex) {
                resultmsg = new ResultMessage { msg = "Exception: " + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }
    }
}
