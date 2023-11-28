using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi_yzy.common;

namespace webapi_yzy.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ZhyfController : Controller
    {
        /// <summary>
        /// 智慧药房接收外部订单并下账
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult makeAccountForOuterOrder(object obj)
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
                    return new JsonResult(resultmsg);
                }
                //身份验证成功，开始处理数据
                resultmsg = DbZhyf.makeAccountForOuterOrder(appid, body); //做账到智慧药房

                //保存上传到智慧药房做账后的返回结果
                JObject bodyObject = JObject.Parse(body);

                //准备要操作的ID
                string operateId = bodyObject["operateId"] == null ? "" : bodyObject["operateId"].ToString();
                var resultmsgJsonArray = new { code = resultmsg.code, msg = resultmsg.msg };
                string returnJson = JsonConvert.SerializeObject(new[] {resultmsgJsonArray});
                
                string saveZhyfMakeAccountRetrunInfo = DbOperator.updateOperateRecordByOperateId(operateId, returnJson);

                JObject jsonResultObject = (JObject)JsonConvert.DeserializeObject(saveZhyfMakeAccountRetrunInfo);
                string dataFlag = jsonResultObject["flag"].ToString();
              
                if (dataFlag != "99") {
                    resultmsg.code = -99;
                    resultmsg.msg = (string)jsonResultObject["sm"];
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "智慧药房下账", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                resultmsg.code = int.Parse(dataFlag);
                resultmsg.msg = (string)jsonResultObject["sm"];
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "智慧药房下账", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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
        /// 智慧药房接收外部订单的退货申请
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<ActionResult> getApplyForOuterOrderReturned(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
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
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据
                resultmsg = DbZhyf.getApplyForOuterOrderReturned(appid, body); //接收外部订单的退货申请
                //

                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                //回填上传状态
                JObject bodyObject = JObject.Parse(body);

                string operateId = bodyObject["operateId"] == null ? "" : bodyObject["operateId"].ToString();
                var resultmsgJsonArray = new { code = resultmsg.code, msg = resultmsg.msg };
                string returnJson = JsonConvert.SerializeObject(new[] { resultmsgJsonArray });

                string saveZhyfMakeAccountRetrunInfo = DbOperator.updateOperateRecordByOperateId(operateId, returnJson);
               
                //云找药退款
                RefundUtils refundUtils = new RefundUtils();
                await refundUtils.refundApplication(resultmsg.data.ToString());

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
        /// 智慧药房接收外部订单已确认收货的状态
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult getOuterOrderReceived(object obj)
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
                    return new JsonResult(resultmsg);
                }

                //身份验证成功，开始处理数据
                resultmsg = DbZhyf.getOuterOrderReceived(body); //接收已确认收货状态
                //

                //保存上传到智慧药房做账后的返回结果
                //JObject bodyObject = JObject.Parse(body);

                //准备要操作的ID
                //string operateId = bodyObject["operateId"] == null ? "" : bodyObject["operateId"].ToString();
                var resultmsgJsonArray = new {
                    code = resultmsg.code,
                    msg = resultmsg.msg
                };
                string returnJson = JsonConvert.SerializeObject(new[] { resultmsgJsonArray });

                //string saveZhyfMakeAccountRetrunInfo = DbOperator.updateOperateRecordByOperateId(operateId, returnJson);
                string saveZhyfMakeAccountRetrunInfo = DbOperator.updateOperateRecordByReturnJson(body, returnJson);
                
                JObject jsonResultObject = (JObject)JsonConvert.DeserializeObject(saveZhyfMakeAccountRetrunInfo);
                string dataFlag = jsonResultObject["flag"].ToString();
                if (dataFlag != "99") {
                    resultmsg.code = -99;
                    resultmsg.msg = (string)jsonResultObject["sm"];
                    //日志
                    DbOperator.saveWebapiOutputLog(appid, method, "智慧药房确认收货", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return new JsonResult(resultmsg);
                }

                resultmsg.code = int.Parse(dataFlag);
                resultmsg.msg = (string)jsonResultObject["sm"];
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "智慧药房确认收货", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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
        /// 智慧药房商品当前的库存和价格
        /// </summary>
        /// <param name="obj">body的格式：'[{"goodsId":"A146626","goodsOuterId":"A146626","shopId":"11896"},{"goodsId":"A144715","goodsOuterId":"A144715","shopId":"11896"}]'</param>
        /// <returns></returns>
        public ActionResult getShopSellGoodsInfo(object obj)
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
                    return new JsonResult(resultmsg);
                }

                JObject bodyObject = JObject.Parse(body);

                string bodyRequest = (string)bodyObject["Body"];

                //身份验证成功，开始处理数据
                resultmsg = DbZhyf.getShopSellGoodsInfo(bodyRequest);
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


        ///// <summary>
        ///// 智慧药房接收外部订单已确认收货的状态(批量)
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public ActionResult getOuterOrderReceivedBatch(object obj) {
        //    string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    //返回结果
        //    ResultMessage resultmsg = new ResultMessage();
        //    //头部信息
        //    string method = "";
        //    string appid = "";
        //    string timestamp = "";
        //    string nonce = "";
        //    string sign = "";

        //    string body = "";
        //    try {
        //        //取头部信息
        //        method = HttpContext.Request.Headers["X-Service-Method"];//方法名称 
        //        appid = HttpContext.Request.Headers["X-Ca-Key"];//appid
        //        timestamp = HttpContext.Request.Headers["X-Ca-Timestamp"];//时间戳
        //        nonce = HttpContext.Request.Headers["X-Ca-Nonce"];//随机数
        //        sign = HttpContext.Request.Headers["X-Ca-Signature"]; //签名
        //        //取body
        //        body = obj.ToString();
        //        //身份验证
        //        resultmsg = GetToken.getTokenResult(appid, method, timestamp, nonce, sign, body);

        //        //身份验证失败
        //        if (resultmsg.code != 99) {
        //            return new JsonResult(resultmsg);
        //        }

        //        //身份验证成功，开始处理数据
        //        resultmsg = DbZhyf.getOuterOrderReceived(body); //接收已确认收货状态
                
        //        var resultmsgJsonArray = new {
        //            code = resultmsg.code,
        //            msg = resultmsg.msg
        //        };
        //        string returnJson = JsonConvert.SerializeObject(new[] { resultmsgJsonArray });

        //        resultmsg.code = 99;
        //        resultmsg.msg = "智慧药房接收外部订单已确认收货的状态(批量)成功";
        //        resultmsg.data = returnJson;
        //        //日志
        //        DbOperator.saveWebapiOutputLog(appid, method, "智慧药房确认收货", body, (resultmsg.data ?? "").ToString(), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        //        return new JsonResult(resultmsg);
        //    } catch (Exception ex) {
        //        resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
        //        //日志
        //        DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //        return new JsonResult(resultmsg);
        //    }
        //}


    }
}
