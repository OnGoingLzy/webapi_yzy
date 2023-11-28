﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi_yzy.Service;

namespace webapi_yzy.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PublishMsgController : Controller
    {
        public async  Task<ActionResult> PublishNoPayMsg(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            //string tokenRes = DbOperator.GetAccessToken(appid, "");
            //JObject jsonsp = (JObject)JsonConvert.DeserializeObject(tokenRes);
            WechatPayService payService = new WechatPayService();
            string accessToken = payService.getAccessToken();
            try
            {
                PublishMsgService publishMsgService = new PublishMsgService();
                string dbRes = DbOperator.getNoPayOrder();
                JObject dbResObj = JObject.Parse(dbRes);
                int i = 0;
                if (dbRes.Contains("\"data\":null"))
                {
                    resultmsg.data = "已执行,推送数量0";

                    return new JsonResult(resultmsg);
                }
                if ((int)dbResObj["flag"]==99 )
                {
                    JArray dataArr = (JArray)dbResObj["data"];
                    foreach (JObject item in dataArr)
                    {
                        i++;
                        string createTime = (string)item["createTime"];
                        string openid = (string)item["openid"];
                        string totalPrice = (string)item["totalPrice"];
                        string result = await publishMsgService.PublishNoPayMsg(openid,  createTime, totalPrice, accessToken);
                        DbOperator.saveWebapiOutputLog(appid, method, "推送用户未付款订单消息", body, result, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }

                resultmsg.data = "已执行,推送数量" + i;

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

        public async Task<ActionResult> PublishRefundMsg(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            // tokenRes = DbOperator.GetAccessToken(appid, "");
            //JObject jsonsp = (JObject)JsonConvert.DeserializeObject(tokenRes);
            WechatPayService payService = new WechatPayService();
            string accessToken = payService.getAccessToken();
            try
            {
                PublishMsgService publishMsgService = new PublishMsgService();
                string dbRes = DbOperator.getRefundOrder("医保");
                JObject dbResObj = JObject.Parse(dbRes);
                int i = 0;
                if (dbRes.Contains("\"data\":null"))
                {
                    resultmsg.data = "已执行,推送数量0";

                    return new JsonResult(resultmsg);
                }
                if ((int)dbResObj["flag"] == 99)
                {
                    JArray dataArr = (JArray)dbResObj["data"];
                    foreach (JObject item in dataArr)
                    {
                        i++;
                        string finishedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); 
                        string openid = (string)item["openid"];
                        string msg = "您有一笔退款订单已可退还金额,请提取";
                        //string result = await publishMsgService.PublishRefundMsg(openid, finishedTime, msg, jsonsp["access_token"].ToString());
                        string result = await publishMsgService.PublishRefundMsg(openid, finishedTime, msg, accessToken);
                        DbOperator.saveWebapiOutputLog(appid, method, "推送用户退款信息", body, result, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }

                resultmsg.data = "已执行,推送数量" + i;

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


        public ActionResult PublishRecipeAuditMsg(object obj)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回结果
            ResultMessage resultmsg = new ResultMessage();
            //头部信息
            string method = "";
            string appid = "";

            string body = "";
            //string tokenRes = DbOperator.GetAccessToken(appid, "");
            //JObject jsonsp = (JObject)JsonConvert.DeserializeObject(tokenRes);
            WechatPayService payService = new WechatPayService();
            string accessToken = payService.getAccessToken();
            try
            {
                PublishMsgService publishMsgService = new PublishMsgService();
                string dbRes = DbOperator.getNoUseRecipeAudit();
                JObject dbResObj = JObject.Parse(dbRes);
                int i = 0;
                if (dbRes.Contains("\"data\":null"))
                {
                    resultmsg.data = "已执行,推送数量0";

                    return new JsonResult(resultmsg);
                }
                if ((int)dbResObj["flag"] == 99)
                {
                    JArray dataArr = (JArray)dbResObj["data"];
                    
                    foreach (JObject item in dataArr)
                    {
                        i++;
                        string status = (string)item["status"];
                        if (status.Equals("AUDIT"))
                        {
                            status = "审核通过,点击前往支付";
                            string openid = (string)item["openid"];
                            publishMsgService.publishRecipeAuditMsg(status, "您的处方审核完成", openid, accessToken);
                        }
                        else if (status.Equals("REFUSE"))
                        {
                            status = "审核不通过";
                            string openid = (string)item["openid"];
                            publishMsgService.publishRecipeAuditMsg(status, "您的处方审核完成", openid, accessToken);
                        }
                            
                        
                    }
                }
                DbOperator.saveWebapiOutputLog(appid, method, "推送用户审方结果", body, "推送数量" + i, resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                resultmsg.data = "已执行,推送数量"+i;

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

    }
}
