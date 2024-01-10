﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using webapi_yzy.common;
using webapi_yzy.Model;
using webapi_yzy.Service;
using System.Net.Http;
using System.Collections;

namespace webapi_yzy.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TradeController : Controller
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

        public ActionResult addGoodsToShoppingCart(object obj)
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
                string shopId = bodyObject["shopId"] == null ? "" : bodyObject["shopId"].ToString(); //
                string goodsId = bodyObject["goodsId"] == null ? "" : bodyObject["goodsId"].ToString(); //
                int num = (int)bodyObject["num"] == 0 ? 1 : (int)bodyObject["num"]; //

                string dbRes = DbOperator.addGoodsToShoppingCart(openid,shopId,goodsId,num);
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "添加商品至购物车", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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

        //提交退款申请
        public ActionResult submitRefundApplication(object obj)
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
                WechatPayService payService = new WechatPayService();
               
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString();
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString(); //要退款的主订单号
                string businessUnitId = bodyObject["businessUnitId"] == null ? "" : bodyObject["businessUnitId"].ToString();
                JObject wxPayOrderObj = JObject.Parse(DbOperator.getWxPayOrder(main_order_id,openid));
                refundObj refundObj = new refundObj();
                if ((int)wxPayOrderObj["flag"] == 99)
                {
                    refundObj.refundAmount = (decimal)wxPayOrderObj["data"]["totalPrice"];
                    refundObj.out_order_id = (string)wxPayOrderObj["data"]["out_order_id"];
                    refundObj.wxpay_order_id = (string)wxPayOrderObj["data"]["wxpay_order_id"];
                    //string new_main_order_id = 
                }
                
                
                //string submitRefundApplicationResult = payService.submitRefundApplication(refundObj, businessUnitId, new_main_order_id, openid);

                //*********************************************************************************************************************
                //resultmsg.data = JSON.Decode(submitRefundApplicationResult);

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        private string yyjgbm = "P53050201208";
        private string jrxtsqm = "e03c9add2ba740afa19ff936acb059b8";
        private string jrxtbm = "1002100022";
        public async Task<ActionResult> yinhaiTest(object obj)
        {
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            yinhaiTool yhtool = new yinhaiTool();
            string beginTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            yinhaiInputObject yinhaiInputObject = new yinhaiInputObject();
            yinhaiInputObject.trns_no = "T9152";
            string sender_trns_sn = yyjgbm + beginTime + "0001";
            yinhaiInputObject.sender_trns_sn = sender_trns_sn;
            yinhaiInputObject.trns_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            yinhaiInputObject.medins_code = yyjgbm;
            yinhaiInputObject.medins_name = "云南省医药有限公司新特药保山零售店";
            yinhaiInputObject.ency_type = "RSA";
            yinhaiInputObject.sign_type = "MD5";
            yinhaiInputObject.opter_id = "50002";
            yinhaiInputObject.opter_name = "das";
            yinhaiInputObject.opter_type = "102";
            yinhaiInputObject.acss_sys_code = "1002100022";
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;

            // 将毫秒级时间戳转换为字符串
            long timestampInMilliseconds = currentTime.ToUnixTimeMilliseconds();
            string timestampString = timestampInMilliseconds.ToString();
            string timestamp = timestampString;
            yinhaiInputObject.timestmp = timestamp;
            //rsa加密请求体数据
            string reqData = "{\"data\":{\"plaf_ord_no\": \"1022023111011382110005293\",\"dpp_ord_no\": \"PP202311101138214640\",\"chnl_ord_no\": \"\"}}"; // 请求体数据
            string encryptReqData = yhtool.Encrypt(reqData,"微信");
            //设置输入报文
            yinhaiInputObject.input = encryptReqData;         
            //生成签名
            string sign_data = yhtool.GenerateSignature(timestamp, sender_trns_sn, encryptReqData, "P53050201208","微信");
            yinhaiInputObject.sign_data = sign_data;
            string url = "https://ynyb.yinhaiyun.com/dsmp-api/api/public/app/T9152";
            HttpMethod method = HttpMethod.Post;
            //请求报文转为json
            string jsonData = JsonConvert.SerializeObject(yinhaiInputObject);
            JObject res = await yhtool.SendHttpRequestAsync(url, method, "application/json",jsonData);
            if(yhtool.VerifySignature((string)res["timestmp"], (string)res["sign_data"], (string)res["recer_trns_sn"], (string)res["output"],"微信"))
            {
                //string outputJson = yhtool.Decrypt((string)res["output"]);
            }
            string outputJson = yhtool.Decrypt((string)res["output"],"微信");
            resultmsg.code = 99;
            resultmsg.msg = "测试成功";
            resultmsg.data = "";

            return new JsonResult(resultmsg);

        }


        public async Task<ActionResult> T9101(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();

            string body = "";
            try
            {
                
                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = body;
              

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                yinhaiTool yhtool = new yinhaiTool();
                JObject bodyObject = JObject.Parse(trueBody);
                DbOperator.saveWebapiOutputLog("银海", "payResultCallBack", "接受支付结果回调", body, "", 99, "接收", beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));


                //string ord_pay_list = outputObj["data"]["ord_pay_list"] != null ? outputObj["data"]["ord_pay_list"].ToString() : "";
                //更新一下支付状态
                //string dbres = DbOperator.updateYinhaiOrderPayStatus(main_order_id, "付款", (string)outputObj["data"]["pay_stas"], ord_pay_list, "");

                //真正做账的语句↓，之前都是检验是否支付完成、更新状态
                //string dbRes = DbOperator.getMakeAccount(main_order_id);


                //Task<string> result = yhtool.sendSyncAcssTokenRequestAsync(); //测试同步acssToken方法
                resultmsg.code = 99;
                resultmsg.msg = "接受成功";
                resultmsg.data = "";
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog("银海", "payResultCallBack", "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }
        public async Task<ActionResult> T9102(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();

            string body = "";
            try
            {

                //取body
                body = obj.ToString();
                JObject reqObj = (JObject)JsonConvert.DeserializeObject(body);
                string trueBody = body;


                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                yinhaiTool yhtool = new yinhaiTool();
                JObject bodyObject = JObject.Parse(trueBody);
                DbOperator.saveWebapiOutputLog("银海", "refundResultCallBack", "接受退款结果回调", body, trueBody, 99, "接收", beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));


                //string ord_pay_list = outputObj["data"]["ord_pay_list"] != null ? outputObj["data"]["ord_pay_list"].ToString() : "";
                //更新一下支付状态
                //string dbres = DbOperator.updateYinhaiOrderPayStatus(main_order_id, "付款", (string)outputObj["data"]["pay_stas"], ord_pay_list, "");

                //真正做账的语句↓，之前都是检验是否支付完成、更新状态
                //string dbRes = DbOperator.getMakeAccount(main_order_id);


                //Task<string> result = yhtool.sendSyncAcssTokenRequestAsync(); //测试同步acssToken方法
                resultmsg.code = 99;
                resultmsg.msg = "接受成功";
                resultmsg.data = "";
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog("银海", "payResultCallBack", "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public async Task<ActionResult> requestYinhaiQueryOrder(object obj)
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
                yinhaiTool yhtool = new yinhaiTool();
                JObject bodyObject = JObject.Parse(trueBody);
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();
                string json = yhtool.generateYinhaiQueryOrderParam(main_order_id);
                string jsonData = json.Substring(0, json.Length - 2);
                string url = "https://ynyb.yinhaiyun.com/dsmp-api/api/public/app/T9152";
                //发送请求
                HttpMethod method1 = HttpMethod.Post;
                JObject res = await yhtool.SendHttpRequestAsync(url, method1, "application/json", jsonData);
                DbOperator.saveWebapiOutputLog(appid, method, "查询银海订单结果", body, (res).ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                if (yhtool.VerifySignature((string)res["timestmp"], (string)res["sign_data"], (string)res["recer_trns_sn"], (string)res["output"], json.Substring(json.Length - 2)))
                {
                    //string outputJson = yhtool.Decrypt((string)res["output"]);
                }
                //解码参数
                string outputJson = yhtool.Decrypt((string)res["output"], json.Substring(json.Length - 2));


                JObject outputObj = (JObject)JsonConvert.DeserializeObject(outputJson);
                string ord_pay_list = outputObj["data"]["ord_pay_list"] != null ? outputObj["data"]["ord_pay_list"].ToString() : "";
                JArray ord_pay_list_JARRAY = outputObj["data"]["ord_pay_list"] != null ? (JArray)outputObj["data"]["ord_pay_list"] : null;
                string insuplc_admdvs = "";
                ArrayList costBreakdownLists = new ArrayList();
                costBreakDown ybNode = new costBreakDown();
                costBreakDown fybNode = new costBreakDown();
                foreach (JObject item in ord_pay_list_JARRAY)
                {
                    
                    string mert_chnl_type = (string)item["mert_chnl_type"];
                    if (mert_chnl_type.Equals("MMP"))
                    {
                        insuplc_admdvs = (string)item["hi_outpara"]["insuplc_admdvs"];
                        ybNode.skfs = (string)item["hi_outpara"]["insuplc_admdvs"];
                        ybNode.jes = ybNode.jes + (decimal)item["pay_sumamt"];
                        
                    }   
                    else
                    {
                        fybNode.skfs = "云找药";
                        fybNode.jes = fybNode.jes + (decimal)item["pay_sumamt"];
                        
                    }
                }
                costBreakdownLists.Add(ybNode);
                costBreakdownLists.Add(fybNode);
                
                string costBreakdownJson = JSON.Encode(costBreakdownLists);
                //更新一下支付状态
                string dbres =  DbOperator.updateYinhaiOrderPayStatus(main_order_id,"付款",(string)outputObj["data"]["pay_stas"], ord_pay_list,"",insuplc_admdvs, costBreakdownJson);
                JObject dataObj = (JObject)JsonConvert.DeserializeObject(dbres);
                //真正做账的语句↓，之前都是检验是否支付完成、更新状态
                string dbRes = DbOperator.getMakeAccount(main_order_id);


                //Task<string> result = yhtool.sendSyncAcssTokenRequestAsync(); //测试同步acssToken方法
                string dataString = "{\"status\":\"" + (string)outputObj["data"]["pay_stas"] + "\",\"operateId\":\"" + (string)dataObj["operateId"] + "\"}"; 
                resultmsg.code = 99;
                resultmsg.msg = dbRes;
                resultmsg.data = JSON.Decode(dataString);
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

        
        public async Task<ActionResult> requestYinhaiQueryRefundOrder(object obj)
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
                yinhaiTool yhtool = new yinhaiTool();
                JObject bodyObject = JObject.Parse(trueBody);
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();
                string json = yhtool.generateYinhaiQueryRefundOrderParam(main_order_id);
                string jsonData = json.Substring(0, json.Length - 2);
                string url = "https://ynyb.yinhaiyun.com/dsmp-api/api/public/app/T9155";
                //发送请求
                HttpMethod method1 = HttpMethod.Post;
                JObject res = await yhtool.SendHttpRequestAsync(url, method1, "application/json", jsonData);
                DbOperator.saveWebapiOutputLog(appid, method, "查询银海退款订单结果", body, (res).ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                if (yhtool.VerifySignature((string)res["timestmp"], (string)res["sign_data"], (string)res["recer_trns_sn"], (string)res["output"], json.Substring(json.Length - 2)))
                {
                    //string outputJson = yhtool.Decrypt((string)res["output"]);
                }
                //解码参数
                string outputJson = yhtool.Decrypt((string)res["output"], json.Substring(json.Length - 2));
                JObject outputObj = (JObject)JsonConvert.DeserializeObject(outputJson);
                string ord_refd_list = outputObj["data"]["ord_refd_list"] != null ? outputObj["data"]["ord_refd_list"].ToString() : "";
                string dbres = DbOperator.updateYinhaiOrderPayStatus(main_order_id, "退款", (string)outputObj["data"]["refd_stas"], "", ord_refd_list,"","");


                resultmsg.code = 99;
                resultmsg.msg = "查询成功";
                resultmsg.data = (string)outputObj["data"]["refd_stas"];
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

        //批量查询银海支付订单结果
        public async Task<ActionResult> queryYinHaiPayOrder(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";
            string timestamp = "";
            string nonce = "";
            string sign = "";
            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                timestamp = HttpContext.Request.Headers["X-Ca-Timestamp"];//时间戳
                nonce = HttpContext.Request.Headers["X-Ca-Nonce"];//随机数
                sign = HttpContext.Request.Headers["X-Ca-Signature"]; //签名

                //取body
                body = obj.ToString();
                //身份验证
                resultmsg = GetToken.getTokenResult(appid, method, timestamp, nonce, sign, body);

                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                yinhaiTool yhtool = new yinhaiTool();
                string noPayOrderList = DbOperator.getOrderList("YinHaiNoPay");
                JObject noPayOrderListObj = JObject.Parse(noPayOrderList);
                if ((int)noPayOrderListObj["flag"] == 99 && noPayOrderListObj["data"] != null)
                {
                    JArray dataArr = (JArray)noPayOrderListObj["data"];
                    foreach (JObject item in dataArr)
                    {
                        string main_order_id = (string)item["main_order_id"];


                        string json = yhtool.generateYinhaiQueryOrderParam(main_order_id);
                        string jsonData = json.Substring(0, json.Length - 2);
                        string url = "https://ynyb.yinhaiyun.com/dsmp-api/api/public/app/T9152";
                        //发送请求
                        HttpMethod method1 = HttpMethod.Post;
                        JObject res = await yhtool.SendHttpRequestAsync(url, method1, "application/json", jsonData);
                        if (yhtool.VerifySignature((string)res["timestmp"], (string)res["sign_data"], (string)res["recer_trns_sn"], (string)res["output"], json.Substring(json.Length - 2)))
                        {
                            //string outputJson = yhtool.Decrypt((string)res["output"]);
                        }
                        //解码参数
                        string outputJson = yhtool.Decrypt((string)res["output"], json.Substring(json.Length - 2));


                        JObject outputObj = (JObject)JsonConvert.DeserializeObject(outputJson);
                        string ord_pay_list = outputObj["data"]["ord_pay_list"] != null ? outputObj["data"]["ord_pay_list"].ToString() : "";
                        //更新一下支付状态
                        string dbres = DbOperator.updateYinhaiOrderPayStatus(main_order_id, "付款", (string)outputObj["data"]["pay_stas"], ord_pay_list, "","","");


                    }
                }
                DbOperator.saveWebapiOutputLog(appid, method, "批量查询用户银海支付状态", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                resultmsg.data = "已执行";



                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        //批量查询银海退款订单结果
        public async Task<ActionResult> queryYinHaiRefundOrder(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";
            string timestamp = "";
            string nonce = "";
            string sign = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                timestamp = HttpContext.Request.Headers["X-Ca-Timestamp"];//时间戳
                nonce = HttpContext.Request.Headers["X-Ca-Nonce"];//随机数
                sign = HttpContext.Request.Headers["X-Ca-Signature"]; //签名
                //取body
                body = obj.ToString();
                //身份验证
                resultmsg = GetToken.getTokenResult(appid, method, timestamp, nonce, sign, body);

                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }


                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                yinhaiTool yhtool = new yinhaiTool();
                string NoRefundOrderList = DbOperator.getOrderList("YinHaiNoRefund");
                JObject NoRefundOrderListObj = JObject.Parse(NoRefundOrderList);
                JToken jToken = NoRefundOrderListObj["data"];
                if (jToken != null && (int)NoRefundOrderListObj["flag"] == 99 && jToken.HasValues)
                {
                
                    JArray dataArr = (JArray)NoRefundOrderListObj["data"];
                    foreach (JObject item in dataArr)
                    {
                        string main_order_id = (string)item["main_order_id"];

                        string json = yhtool.generateYinhaiQueryRefundOrderParam(main_order_id);
                        string jsonData = json.Substring(0, json.Length - 2);
                        string url = "https://ynyb.yinhaiyun.com/dsmp-api/api/public/app/T9155";
                        //发送请求
                        HttpMethod method1 = HttpMethod.Post;
                        JObject res = await yhtool.SendHttpRequestAsync(url, method1, "application/json", jsonData);
                        if (yhtool.VerifySignature((string)res["timestmp"], (string)res["sign_data"], (string)res["recer_trns_sn"], (string)res["output"], json.Substring(json.Length - 2)))
                        {
                            //string outputJson = yhtool.Decrypt((string)res["output"]);
                        }
                        //解码参数
                        string outputJson = yhtool.Decrypt((string)res["output"], json.Substring(json.Length - 2));
                        JObject outputObj = (JObject)JsonConvert.DeserializeObject(outputJson);
                        string ord_refd_list = outputObj["data"]["ord_refd_list"] != null ? outputObj["data"]["ord_refd_list"].ToString() : "";
                        string dbres = DbOperator.updateYinhaiOrderPayStatus(main_order_id, "退款", (string)outputObj["data"]["refd_stas"], "", ord_refd_list,"","");


                    }
                }
                DbOperator.saveWebapiOutputLog(appid, method, "批量查询用户银海退款状态", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                resultmsg.data = "已执行";

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }


        
        public async Task<ActionResult> updateYinHaiOrderInfo(object obj)
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
                JObject orderInfo = (JObject)(bodyObject["orderInfo"] == null ? "" : bodyObject["orderInfo"]);
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();
                string type = bodyObject["type"] == null ? "" : bodyObject["type"].ToString();
                
                string dbres = DbOperator.updateYinHaiOrderInfo(main_order_id, orderInfo, type);
                object dataObj = JSON.Decode(dbres);
                JObject dbresObj = (JObject)JsonConvert.DeserializeObject(dbres);
                resultmsg.code = (int)dbresObj["flag"];
                resultmsg.msg = (string)dbresObj["sm"];
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

        
        public async Task<ActionResult> yinhaiQueryOrder(object obj)
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
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();//查询的订单id
               
              

                //保存至银海订单表表

                //生成银海请求报文 调用工具类生成
                //*********************************************************************************************************************
                yinhaiTool yhtool = new yinhaiTool();
                
                //*********************************************************************************************************************


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



        
        public async Task<ActionResult> yinHaiPay(object obj)
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
                string shopId = bodyObject["shopId"] == null ? "" : bodyObject["shopId"].ToString();
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString();
                //string user_id = bodyObject["user_id"] == null ? "" : bodyObject["user_id"].ToString();
                string locationId = bodyObject["locationId"] == null ? "" : bodyObject["locationId"].ToString();
                string memo = bodyObject["memo"] == null ? "" : bodyObject["memo"].ToString();
                string shippingFee = bodyObject["shippingFee"] == null ? "" : bodyObject["shippingFee"].ToString();
                string type = bodyObject["type"] == null ? "" : bodyObject["type"].ToString();//快递类型
                string payType = bodyObject["payType"] == null ? "" : bodyObject["payType"].ToString();//微信医保支付/支付宝医保支付 /支付宝支付
                string goodsUserId = bodyObject["goodsUserId"] == null ? "" : bodyObject["goodsUserId"].ToString(); //选择的用药人id，除了微信支付，其他支付方式均要传该值
                List<purchaseGoods> goodsList = JsonConvert.DeserializeObject<List<purchaseGoods>>(JsonConvert.SerializeObject(bodyObject["goodsList"]));
                string main_order_id = String.Empty;
                string businessUnitId = String.Empty;

                string goodsListStr = JsonConvert.SerializeObject(goodsList);

                //先写入一份支付记录到订单表中
                string orderJson = DbOperator.saveOrder(shopId, openid, goodsListStr, locationId, memo, shippingFee, type);
                JSON.Decode(orderJson);
                JObject orderJsonObject = (JObject)JsonConvert.DeserializeObject(orderJson); 
                if ((string)orderJsonObject["flag"] == "-99")
                {
                    
                    resultmsg.code = -99;
                    resultmsg.msg = "保存订单失败";
                    resultmsg.data = JSON.Decode(orderJson);
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "保存订单失败", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }
                main_order_id = (string)orderJsonObject["data"][0]["main_order_id"];
                businessUnitId = (string)orderJsonObject["data"][0]["businessUnitId"];
                openid = (string)orderJsonObject["data"][0]["openid"];
                decimal totalPrice = Math.Round((decimal)orderJsonObject["data"][0]["totalPrice"], 2);


                //保存至银海订单表表

                //生成银海创建订单所需要的参数 调用工具类生成
                //*********************************************************************************************************************
                yinhaiTool yhtool = new yinhaiTool();
                responseYinhaiObject responseYinhaiObject = new responseYinhaiObject();
                if (payType == "支付宝医保支付" || payType == "支付宝支付")
                {
                   responseYinhaiObject = yhtool.generateYinHaiOrderParam(orderJsonObject, main_order_id, openid, payType, goodsUserId);
                }
                else if(payType == "微信医保支付")
                {
                   responseYinhaiObject = yhtool.generateYinHaiOrderParam(orderJsonObject, main_order_id, openid, payType, goodsUserId);
                }
                
                //*********************************************************************************************************************

                resultmsg.code = responseYinhaiObject.flag;
                resultmsg.msg = responseYinhaiObject.sm;
                resultmsg.data = responseYinhaiObject;

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

        public async Task<ActionResult> getYhInitParam(object obj)
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


                JObject bodyObject = JObject.Parse(trueBody);
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();
                string payType = bodyObject["payType"] == null ? "微信医保支付" : bodyObject["payType"].ToString();
                //获取accessToken
                //string accessTokenObj = DbOperator.GetAccessToken("", "");
                //JObject jsonsp = (JObject)JsonConvert.DeserializeObject(accessTokenObj);
                WechatPayService payService = new WechatPayService();
                string accessToken = payService.getAccessToken();
                yinhaiInitParam yinhaiInitParam = new yinhaiInitParam();


                yinhaiInitParam.acssToken = accessToken;
                string dbRes = DbOperator.getYhInitParam(main_order_id, payType);
                JObject dbResObj = (JObject)JsonConvert.DeserializeObject(dbRes);
                yinhaiInitParam.dppSyscode = (string)dbResObj["dppSyscode"];
                resultmsg.code = (int)dbResObj["flag"];
                resultmsg.msg = (string)dbResObj["sm"]; ;
                resultmsg.data = yinhaiInitParam;

                return new JsonResult(resultmsg);

            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public async Task<ActionResult> yinHaiRefund(object obj)
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


                JObject bodyObject = JObject.Parse(trueBody);
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();
                string type = bodyObject["type"] == null ? "" : bodyObject["type"].ToString(); //支付类型
                //生成银海创建退款订单所需要的参数 调用工具类生成
                //*********************************************************************************************************************
                yinhaiTool yhtool = new yinhaiTool();
                responseYinhaiRefundObject responseYinhaiRefundObject = yhtool.generateYinHaiRefundOrderParam(main_order_id, type);
                //*********************************************************************************************************************

                resultmsg.code = responseYinhaiRefundObject.flag;
                resultmsg.msg = responseYinhaiRefundObject.sm;
                resultmsg.data = responseYinhaiRefundObject;

                return new JsonResult(resultmsg);

            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        //支付
        public async Task<ActionResult> wechatPay(object obj) {
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
                string shopId = bodyObject["shopId"] == null ? "" : bodyObject["shopId"].ToString();
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString();
                string locationId = bodyObject["locationId"] == null ? "" : bodyObject["locationId"].ToString();
                string memo = bodyObject["memo"] == null ? "" : bodyObject["memo"].ToString();
                string shippingFee = bodyObject["shippingFee"] == null ? "" : bodyObject["shippingFee"].ToString();
                string type = bodyObject["type"] == null ? "" : bodyObject["type"].ToString();
                List<purchaseGoods> goodsList = JsonConvert.DeserializeObject<List<purchaseGoods>>(JsonConvert.SerializeObject(bodyObject["goodsList"]));
                string main_order_id = String.Empty;
                string businessUnitId = String.Empty;

                string goodsListStr = JsonConvert.SerializeObject(goodsList);

                //先写入一份支付记录到订单表中
                string orderJson = DbOperator.saveOrder(shopId, openid, goodsListStr, locationId, memo, shippingFee, type);
                JObject orderJsonObject = (JObject)JsonConvert.DeserializeObject(orderJson);
                if ((string)orderJsonObject["flag"] == "-99")
                {
                    resultmsg.code = -99;
                    resultmsg.msg = "保存订单失败";
                    resultmsg.data = JSON.Decode(orderJson);
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "保存订单失败", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }
                main_order_id = (string)orderJsonObject["data"][0]["main_order_id"];
                businessUnitId = (string)orderJsonObject["data"][0]["businessUnitId"];
                openid = (string)orderJsonObject["data"][0]["openid"];
                decimal totalPrice = Math.Round((decimal)orderJsonObject["data"][0]["totalPrice"], 2);

                WechatPayService payService = new WechatPayService();
                //创建预付单
                //*********************************************************************************************************************
                string prePayOrderGenerateResult = await payService.JSAPIpreOrderAsync(main_order_id, businessUnitId, totalPrice, openid);


                if (!prePayOrderGenerateResult.Contains("error")) {
                    /*创建微信支付订单及购物订单*/
                    resultmsg.data = JSON.Decode(prePayOrderGenerateResult);
                    DbOperator.saveWebapiOutputLog(appid, method, "生成预支付订单", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                } else {
                    resultmsg.code = -99;
                    resultmsg.msg = "生成预支付订单失败!";
                    resultmsg.data = JSON.Decode(prePayOrderGenerateResult);
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "生成预支付订单失败", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                }
                //*********************************************************************************************************************


                return new JsonResult(resultmsg);
            } catch (Exception ex) {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }
    
        //获取用户的订单信息
        public ActionResult getOrderList(object obj)
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
                

                string dbRes = DbOperator.getOrderList(openid);
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取用户的所有订单", body, dbRes, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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

        //获取处方药结算信息
        public ActionResult getGoodsSettlementInfo(object obj)
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
                string clinicId = bodyObject["clinicId"] == null ? "" : bodyObject["clinicId"].ToString();

                string dbRes = DbOperator.getGoodsSettlementInfo(openid, clinicId);
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取处方药结算信息", body, dbRes, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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

        public ActionResult getOrderNumByStatus(object obj)
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
                string status = bodyObject["status"] == null ? "" : bodyObject["status"].ToString(); //


                string dbRes = DbOperator.getOrderNumByStatus(openid,status);
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取用户订单"+status+"状态的订单数", body, dbRes, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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

        //批量查询微信支付结果
        public async Task<ActionResult> queryPayOrder(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";
            string timestamp = "";
            string nonce = "";
            string sign = "";
            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                timestamp = HttpContext.Request.Headers["X-Ca-Timestamp"];//时间戳
                nonce = HttpContext.Request.Headers["X-Ca-Nonce"];//随机数
                sign = HttpContext.Request.Headers["X-Ca-Signature"]; //签名

                //取body
                body = obj.ToString();
                //身份验证
                resultmsg = GetToken.getTokenResult(appid, method, timestamp, nonce, sign, body);

                //身份验证失败
                if (resultmsg.code != 99) {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                WechatPayService payService = new WechatPayService();
                string noPayOrderList = DbOperator.getOrderList("noPay");
                JObject noPayOrderListObj = JObject.Parse(noPayOrderList);
                if ((int)noPayOrderListObj["flag"] == 99 && noPayOrderListObj["data"] != null)
                {
                    JArray dataArr = (JArray)noPayOrderListObj["data"];
                    foreach (JObject item in dataArr)
                    {
                        string main_order_id = (string)item["main_order_id"];
                        string out_order_id = (string)item["out_order_id"];
                        JObject getBusinessUnitIdResult = JObject.Parse(DbOperator.getOrderBusinessId(main_order_id));
                        if ((int)getBusinessUnitIdResult["flag"] != 99)
                        {
                            continue;
                        }
                        string businessUnitId = (string)getBusinessUnitIdResult["businessUnitId"];
                        GetPayTransactionByOutTradeNumberResponse wxPayOrderInfo = await payService.GetPayTransactionByOutTradeNumber(out_order_id, businessUnitId);
                        string dbRes = payService.addOrUpdateWxPayOrder(wxPayOrderInfo, main_order_id);
                        
                    }
                }
                DbOperator.saveWebapiOutputLog(appid, method, "批量查询用户微信支付状态", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                resultmsg.data = "已执行";

                

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        //批量查询微信退款结果
        public async Task<ActionResult> queryRefundOrder(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";
            string timestamp = "";
            string nonce = "";
            string sign = "";

            string body = "";
            try
            {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                timestamp = HttpContext.Request.Headers["X-Ca-Timestamp"];//时间戳
                nonce = HttpContext.Request.Headers["X-Ca-Nonce"];//随机数
                sign = HttpContext.Request.Headers["X-Ca-Signature"]; //签名
                //取body
                body = obj.ToString();
                //身份验证
                resultmsg = GetToken.getTokenResult(appid, method, timestamp, nonce, sign, body);
                
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }
                

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                WechatPayService payService = new WechatPayService();
                string wxNoRefundOrderList = DbOperator.getOrderList("wxNoRefund");
                JObject wxNoRefundOrderListObj = JObject.Parse(wxNoRefundOrderList);
                JToken jToken = wxNoRefundOrderListObj["data"];
                if (jToken != null && (int)wxNoRefundOrderListObj["flag"] == 99 && jToken.HasValues)
                {
                    Console.WriteLine("123456");
                    JArray dataArr = (JArray)wxNoRefundOrderListObj["data"];
                    foreach (JObject item in dataArr)
                    {
                        string main_order_id = (string)item["main_order_id"];
                        string out_order_id = (string)item["out_order_id"];
                        JObject getBusinessUnitIdResult = JObject.Parse(DbOperator.getOrderBusinessId(main_order_id));
                        if ((int)getBusinessUnitIdResult["flag"] != 99)
                        {
                            continue;
                        }
                        string businessUnitId = (string)getBusinessUnitIdResult["businessUnitId"];
                        string testRequestQueryRefundResult = await payService.requestQueryRefund(out_order_id, businessUnitId);

                    }
                }
                DbOperator.saveWebapiOutputLog(appid, method, "批量查询用户微信退款状态", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                resultmsg.data = "已执行";

                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }



        /// 2023-08-22 后禹谦 退款业务接口
        /// <summary>
        /// 该接口开放给智慧药房接收退款消息，并且完成完整退款的业务逻辑和操作
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        //提交退款申请
        public async Task<ActionResult> refundApplication(object obj) {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string dbRes = String.Empty;
            string method = "";
            string appid = "";
            string timestamp = "";
            string nonce = "";
            string sign = "";

            string body = "";
            try {
                //取头部信息
                method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
                appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
                timestamp = HttpContext.Request.Headers["X-Ca-Timestamp"];//时间戳
                nonce = HttpContext.Request.Headers["X-Ca-Nonce"];//随机数
                sign = HttpContext.Request.Headers["X-Ca-Signature"]; //签名
                //取body
                body = obj.ToString();

                //身份验证
                resultmsg = GetToken.getTokenResult(appid, method, timestamp, nonce, sign, body);

                //身份验证失败
                if (resultmsg.code != 99) {
                    DbOperator.saveWebapiOutputLog(appid, method, "退款接口验签失败", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                JArray jsonArray = JArray.Parse(body ?? "[{\"returnedFlag\": \"-99\", \"returnedMsg\": \"异常情况\", \"main_order_id\": \"202300000000\"}]");
                JObject jsonObject = (JObject)jsonArray[0];

                string returnedFlag = jsonObject["returnedFlag"] == null ? "" : jsonObject["returnedFlag"].ToString();
                string returnedMsg = jsonObject["returnedMsg"] == null ? "" : jsonObject["returnedMsg"].ToString();
                string mainOrderId = jsonObject["main_order_id"] == null ? "" : jsonObject["main_order_id"].ToString();

                //2023-09-01 保存退款时，智慧药房返回的信息
                string dbResTemp = DbOperator.updateRefundAuditReason(mainOrderId, returnedMsg);

                if (returnedFlag != "99")
                {
                    if (returnedFlag == "-1")
                    {
                        resultmsg.code = 99;
                        resultmsg.msg = "";
                        //如果返回的是-1，即拒绝退款，则需要保存到数据库
                        DbOperator.denyRefund(mainOrderId);
                        DbOperator.saveWebapiOutputLog(appid, method, "提交退款申请", body, "获取智慧药房发起的退货订单", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        return new JsonResult(resultmsg);
                    }
                    resultmsg.code = int.Parse(returnedFlag);
                    resultmsg.msg = returnedMsg;
                    DbOperator.saveWebapiOutputLog(appid, method, "提交退款申请", body, "获取智慧药房发起的退货订单", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //获取智慧药房发起的退货订单，状态位为99，代表该笔订单可以进行继续退款
                dbResTemp = DbOperator.updateInventoryByRefund(mainOrderId);
                dbRes = DbOperator.getRefundInfo(mainOrderId);
                jsonObject = JObject.Parse(dbRes);
                jsonArray = (JArray)jsonObject["data"];
                JObject dataObject = (JObject)jsonArray[0];
                
                //身份验证成功，开始处理数据#################################
                //查询商品内容
                
                WechatPayService payService = new WechatPayService();
                string openid = dataObject["openid"] == null ? "" : dataObject["openid"].ToString();
                string new_main_order_id = dataObject["new_main_order_id"] == null ? "" : dataObject["new_main_order_id"].ToString();
                string out_order_id = dataObject["out_order_id"] == null ? "" : dataObject["out_order_id"].ToString();
                string wxpay_order_id = dataObject["wxpay_order_id"] == null ? "" : dataObject["wxpay_order_id"].ToString();
                string businessUnitId = dataObject["businessUnitId"] == null ? "" : dataObject["businessUnitId"].ToString();
                decimal totalPrice = dataObject["totalPrice"] == null ? 0 : decimal.Parse(dataObject["totalPrice"].ToString());

                //不是微信付款，则直接设置状态为退款中
                if (wxpay_order_id == "")
                {
                    //直接置为退款中
                    dbRes = DbOperator.saveRefundOperateRecord(mainOrderId, "", 99, "更新成功");

                    resultmsg.code = 99;
                    resultmsg.msg = "更新成功";
                    DbOperator.saveWebapiOutputLog(appid, method, "更新退款状态为退款中", "", "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    return new JsonResult(resultmsg);

                }


                //*********************************************************************************************************************
                //提交退货申请只需要这些参数即可， new_main_order_id参数需上级方法给出(订单退单生成好之后,再将退单的新主订单号传给该方法)
                refundObj refundObj = new refundObj();
                refundObj.refundAmount = totalPrice;
                refundObj.out_order_id = out_order_id;
                refundObj.wxpay_order_id = wxpay_order_id;
                string submitRefundApplicationResult = payService.submitRefundApplication(refundObj, businessUnitId, new_main_order_id, openid);

                //*********************************************************************************************************************

                //2023-08-23 下面是处理正式提起退款的环节
                //对应testRequestRefund方法
                jsonObject = JObject.Parse(submitRefundApplicationResult);
                int flag = (int)jsonObject["flag"];
                string sm = (string)jsonObject["sm"];
                
                if (flag != 99)
                {
                    dbRes = DbOperator.saveRefundOperateRecord(mainOrderId, "", flag, sm);
                    resultmsg.code = flag;
                    resultmsg.msg = sm;
                    DbOperator.saveWebapiOutputLog(appid, method, "发起退款申请", submitRefundApplicationResult, "发起退款申请时出现失败记录", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                //如果flag为99的话，证明取到的是正确的退款信息，此时就可以发起退款操作
                jsonArray = (JArray)jsonObject["data"];
                dataObject = (JObject)jsonArray[0];
                string commitOutOrderId = dataObject["out_order_id"] == null ? "" : dataObject["out_order_id"].ToString();
                string commitOriginOutOrderId = dataObject["origin_out_order_id"] == null ? "" : dataObject["origin_out_order_id"].ToString();

                refundObj commitRefundObj = new refundObj();
                commitRefundObj.out_order_id = commitOriginOutOrderId;
                commitRefundObj.refundAmount = totalPrice;                      //要退款的金额
                commitRefundObj.origin_totalPrice = totalPrice;                 //原订单总金额
                string requestRefundResult = await payService.requestRefund(commitRefundObj, commitOutOrderId, openid, businessUnitId);

                string operateMsg = String.Empty;
                jsonObject = JObject.Parse(requestRefundResult);
                if (int.TryParse((string)jsonObject["flag"], out flag))
                {
                    flag = (int)jsonObject["flag"];
                    sm = (string)jsonObject["sm"];
                }
                else
                {
                    flag = -99;
                    sm = (string)jsonObject["sm"];
                    operateMsg = requestRefundResult;
                }
                
                /* 查询最终退款结果的方法就不在此处调用了
                   而是开发一个统一的轮询支付结果和退款结果的方法或Winform前台程序
                string testRequestQueryRefundResult = await payService.requestQueryRefund(commitOutOrderId, businessUnitId);
                */
                
                dbRes = DbOperator.saveRefundOperateRecord(mainOrderId, operateMsg, flag, sm);

                resultmsg.code = flag;
                resultmsg.msg = sm;
                DbOperator.saveWebapiOutputLog(appid, method, "发起退款申请", submitRefundApplicationResult, "发起退款申请成功", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                
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





