﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using webapi_yzy.Service;
using webapi_yzy.Model;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;

namespace webapi_yzy.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExampleController : Controller
    {
        [HttpGet]
        public string getSimple()
        {
            return "Hello World!";
        }

        [HttpPost]
        public ActionResult getExample(object obj)
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
                //身份验证
                resultmsg = getTokenResult(body, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据#################################

                resultmsg.data = "webapi_yzy返回的数据object";
                resultmsg.code = 99;
                resultmsg.msg = "GetExample成功调用";
                //

                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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

        public ActionResult getOpenid(object obj)
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
                string secret = bodyObject["secret"] == null ? "" : bodyObject["secret"].ToString();
                
                string js_code = bodyObject["js_code"] == null ? "" : bodyObject["js_code"].ToString(); 

                //获取OPENID前，先要获取token
                resultmsg = DbOperator.GetOpenid(appid, secret, js_code);

                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取openid", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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

        //测试支付
        public async Task<ActionResult> TestWechatPay(object obj)
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
                //List<string> goodsIdList = (List)bodyObject[""]
                string businessUnitId = bodyObject["businessUnitId"] == null ? "" : bodyObject["businessUnitId"].ToString();
                string shopId = bodyObject["shopId"] == null ? "" : bodyObject["shopId"].ToString();
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString();
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();
                List<purchaseGoods> goodsList = JsonConvert.DeserializeObject<List<purchaseGoods>>(JsonConvert.SerializeObject(bodyObject["goodsList"]));
                WechatPayService payService = new WechatPayService();
                //创建预付单
                //*********************************************************************************************************************
                decimal totalPrice = 0.01m;
                string prePayOrderGenerateResult = await payService.JSAPIpreOrderAsync(main_order_id, businessUnitId, totalPrice,openid);


                if (!prePayOrderGenerateResult.Contains("error"))
                {
                    /*创建微信支付订单及购物订单*/
                    resultmsg.data = JSON.Decode(prePayOrderGenerateResult);
                }
                else
                {
                    resultmsg.code = -99;
                    resultmsg.msg = "生成预支付订单失败!";
                    resultmsg.data = JSON.Decode(prePayOrderGenerateResult);
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "生成预支付订单失败", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                }
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

        //测试查询支付订单
        public async Task<ActionResult> testQueryPayOrder(object obj) {
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
                WechatPayService payService = new WechatPayService();
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString();
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();
                string out_order_id = bodyObject["out_order_id"] == null ? "" : bodyObject["out_order_id"].ToString();
                JObject getBusinessUnitIdResult = JObject.Parse(DbOperator.getOrderBusinessId(main_order_id));
                if ((int)getBusinessUnitIdResult["flag"] != 99) {
                    resultmsg.code = -99;
                    resultmsg.msg = (string)getBusinessUnitIdResult["sm"];
                    resultmsg.data = null;
                    return new JsonResult(resultmsg);
                }
                string businessUnitId = (string)getBusinessUnitIdResult["businessUnitId"];

                //*********************************************************************************************************************
                GetPayTransactionByOutTradeNumberResponse wxPayOrderInfo = await payService.GetPayTransactionByOutTradeNumber(out_order_id, businessUnitId);
                string dbRes = payService.addOrUpdateWxPayOrder(wxPayOrderInfo, main_order_id);
                //这里返回的是wxPayOrder数据
                //*********************************************************************************************************************
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;

                string strReturnObj = JsonConvert.SerializeObject(dataObj);
                JObject joReturnObj = (JObject)JsonConvert.DeserializeObject(strReturnObj);
                if ((int)joReturnObj["flag"] == 99 && joReturnObj["data"] != null) {
                    //生成做账所需的json参数的语句↓，之前都是检验是否支付完成、更新状态
                    dbRes = DbOperator.getMakeAccount(main_order_id);
                    resultmsg.msg = dbRes;
                }
                else
                {
                    resultmsg.code = -99;
                    resultmsg.msg = "用户未支付";
                    resultmsg.data = "";
                }

                return new JsonResult(resultmsg);
            } catch (Exception ex) {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = 99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        public ActionResult testExistsPrePayOrderWxPay(object obj)
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
                string main_order_id = bodyObject["main_order_id"] == null ? "" : bodyObject["main_order_id"].ToString();
                string businessUnitId = bodyObject["businessUnitId"] == null ? "" : bodyObject["businessUnitId"].ToString();

                //2023-09-20 后禹谦添加：每次用户点击支付时需要判断智慧药房当前库存和价格是否同步
                //先从数据库中获取main_order_id下的商品品种
                string dbRes = DbOperator.getExistOrderContentByMainOrderId(main_order_id);
                JObject contentJObject = JObject.Parse(dbRes);
                if ((contentJObject["flag"] ?? "").ToString() == "-99")
                {
                    resultmsg.code = -99;
                    resultmsg.msg = "该主订单不存在！";
                    resultmsg.data = null;
                    DbOperator.saveWebapiOutputLog(appid, method, "支付已存在的订单", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }
                //通过获取到的订单表中的记录来请求接口信息
                //待比较的JSON数组1
                string orderContent = (contentJObject["data"] ?? "").ToString();
                string shopSellGoodsInfoOrigin = JSON.Encode(DbOperator.getShopSellGoodsInfo(orderContent));
                //待比较的JSON数组2
                string orderContentCompare = (JObject.Parse(shopSellGoodsInfoOrigin)["data"] ?? "").ToString();

                //需要对JSON数组1和JSON数组2进行比较
                JArray orderArray = JArray.Parse(orderContent);
                JArray compareArray = JArray.Parse(orderContentCompare);

                foreach (var jToken in orderArray) {
                    var order = (JObject)jToken;

                    JObject compare = compareArray.Cast<JObject>().FirstOrDefault(x => x["shopId"].ToString() == order["shopId"].ToString() && x["goodsId"].ToString() == order["goodsId"].ToString() && x["goodsOuterId"].ToString() == order["goodsOuterId"].ToString());

                    if (compare == null) continue;

                    if (Convert.ToDouble(compare["inventory"]) < Convert.ToDouble(order["num"]) || Convert.ToDouble(compare["price"]) != Convert.ToDouble(order["unitPrice"])) {

                        resultmsg.code = -99;
                        resultmsg.msg = "后台数据已发生变化，该订单无法支付！";
                        resultmsg.data = null;
                        DbOperator.saveWebapiOutputLog(appid, method, "支付已存在的订单", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        return new JsonResult(resultmsg);
                    }
                }
                //2023-09-20 后禹谦添加 结束

                //*********************************************************************************************************************
                //获取已存在的预付单参数
                string prePayOrderResult = payService.getPrePayParam(main_order_id, businessUnitId,openid);
                if (prePayOrderResult.Equals("该预付单不存在!"))
                {
                    resultmsg.code = -99;
                    resultmsg.msg = prePayOrderResult;
                    resultmsg.data = null;
                }
                else
                {
                    object dataObj = JSON.Decode(prePayOrderResult);
                    resultmsg.msg = "获取预付单成功";
                    resultmsg.data = dataObj;
                }
                //*********************************************************************************************************************

                DbOperator.saveWebapiOutputLog(appid, method, "支付已存在的订单", body, prePayOrderResult, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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

        //测试提交退款申请
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
                string new_main_order_id = bodyObject["new_main_order_id"] == null ? "" : bodyObject["new_main_order_id"].ToString();
                string out_order_id = bodyObject["out_order_id"] == null ? "" : bodyObject["out_order_id"].ToString();
                string wxpay_order_id = bodyObject["wxpay_order_id"] == null ? "" : bodyObject["wxpay_order_id"].ToString();
                string businessUnitId = bodyObject["businessUnitId"] == null ? "" : bodyObject["businessUnitId"].ToString();
                //*********************************************************************************************************************
                //提交退货申请只需要这些参数即可， new_main_order_id参数需上级方法给出(订单退单生成好之后,再将退单的新主订单号传给该方法)
                refundObj refundObj = new refundObj();
                refundObj.refundAmount = 0.01m;
                refundObj.out_order_id = out_order_id;
                refundObj.wxpay_order_id = wxpay_order_id;
                string submitRefundApplicationResult = payService.submitRefundApplication(refundObj,businessUnitId, new_main_order_id,  openid);

                //*********************************************************************************************************************
                resultmsg.data = JSON.Decode(submitRefundApplicationResult);

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

        //测试请求退款
        public async Task<ActionResult> testRequestRefund(object obj)
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
                //*********************************************************************************************************************
                string openid = bodyObject["openid"] == null ? "" : bodyObject["openid"].ToString();
                string origin_out_order_id = bodyObject["origin_out_order_id"] == null ? "" : bodyObject["origin_out_order_id"].ToString();
                string out_order_id = bodyObject["out_order_id"] == null ? "" : bodyObject["out_order_id"].ToString();
                //注意，上级方法传递的商户号及原商户号必须严格验证是否存在且对应!
                string businessUnitId = bodyObject["businessUnitId"] == null ? "" : bodyObject["businessUnitId"].ToString();
               
                refundObj refundObj = new refundObj();
                refundObj.out_order_id = origin_out_order_id;
                refundObj.refundAmount = 0.01m;//要退款的金额
                refundObj.origin_totalPrice = 0.01m; //原订单总金额
                string requestRefundResult = await payService.requestRefund(refundObj, out_order_id, openid, businessUnitId);

                //*********************************************************************************************************************
                resultmsg.data = JSON.Decode(requestRefundResult);

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
       
        //测试请求查询退款订单
        public async Task<ActionResult> testRequestQueryRefund(object obj)
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
                //*********************************************************************************************************************
                string out_order_id = bodyObject["out_order_id"] == null ? "" : bodyObject["out_order_id"].ToString();
                string businessUnitId = bodyObject["businessUnitId"] == null ? "" : bodyObject["businessUnitId"].ToString();

                string testRequestQueryRefundResult = await payService.requestQueryRefund(out_order_id, businessUnitId);

                //*********************************************************************************************************************
                resultmsg.data = JSON.Decode(testRequestQueryRefundResult);

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

        public ActionResult wxPayNotice(object obj)
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

                ////身份验证
                //resultmsg = getTokenResult(trueBody, appid, method);
                ////身份验证失败
                //if (resultmsg.code != 99)
                //{
                //    DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //    return new JsonResult(resultmsg);
                //}

                //身份验证成功，开始处理数据#################################
                //查询商品内容
                // "Body" 字段解析为一个 JObject 对象
                JObject bodyObject = JObject.Parse(trueBody);
                WechatPayService payService = new WechatPayService();
              


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

    }
}
