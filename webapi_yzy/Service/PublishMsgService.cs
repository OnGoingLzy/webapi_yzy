using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using webapi_yzy.Model;
using SKIT.FlurlHttpClient.Wechat;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using System.Threading.Tasks;

namespace webapi_yzy.Service
{
    //消息推送服务
    public class PublishMsgService
    {



        //待收款消息推送
        public async Task<string> PublishNoPayMsg(string openid, string createTime, string totalPrice, string token)
        {
            var options = new WechatApiClientOptions()
            {
                AppId = "wxe7c826a1a5e00055",
                AppSecret = "425edf0bcd1ef5d7c2a32d55832f643f",
            };
            var client = new WechatApiClient(options);
            IDictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem> data = new Dictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem>();
            var dataItem1 = new CgibinMessageSubscribeSendRequest.Types.DataItem
            {
                Value = "亲,您的订单即将过期,付款后立即安排发货"
            };
            var dataItem3 = new CgibinMessageSubscribeSendRequest.Types.DataItem
            {
                Value = createTime
            };
            var dataItem5 = new CgibinMessageSubscribeSendRequest.Types.DataItem
            {
                Value = totalPrice
            };
            data["thing1"] = dataItem1;
            data["amount2"] = dataItem5;
            data["time6"] = dataItem3;

            try
            {
                var request = new CgibinMessageSubscribeSendRequest() {
                    AccessToken = token,
                    ToUserOpenId = openid,
                    TemplateId = "JZGbfSSv0-jB5yS4GObkqD0TUrPqDdAbalAcz70xjZg",
                    MiniProgramState = "trial",
                    MiniProgramPagePath = "pages/user_order/user_order?num=test1",
                    Language = "zh_CN",
                    Data = data
                };
                var response = await client.ExecuteCgibinMessageSubscribeSendAsync(request);
                if (response.IsSuccessful())
                {
                    
                }
                else
                {
                    return "{\"errorCode\":\"" + response.ErrorCode + "\",\"errorInfo\":\"" + response.ErrorMessage + "\"}";
                }
            }
            catch(Exception e)
            {
                throw e;
            }
                return "";
        }

        public async Task<string> PublishRefundMsg(string openid, string finishedTime, string msg, string token)
        {
            var options = new WechatApiClientOptions()
            {
                AppId = "wxe7c826a1a5e00055",
                AppSecret = "425edf0bcd1ef5d7c2a32d55832f643f",
            };
            var client = new WechatApiClient(options);
            IDictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem> data = new Dictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem>();
            var dataItem1 = new CgibinMessageSubscribeSendRequest.Types.DataItem
            {
                Value = msg
            };
            var dataItem3 = new CgibinMessageSubscribeSendRequest.Types.DataItem
            {
                Value = finishedTime
            };
            
            data["thing3"] = dataItem1;
            data["time2"] = dataItem3;

            try
            {
                var request = new CgibinMessageSubscribeSendRequest()
                {
                    AccessToken = token,
                    ToUserOpenId = openid,
                    TemplateId = "uJZopLHooaB2-RHZ_mkkn59E_a9KJjCa_INAdBtf_9U",
                    MiniProgramState = "trial",
                    MiniProgramPagePath = "pages/user_order/user_order?num=test4",
                    Language = "zh_CN",
                    Data = data
                };
                var response = await client.ExecuteCgibinMessageSubscribeSendAsync(request);
                if (response.IsSuccessful())
                {

                }
                else
                {
                    return "{\"errorCode\":\"" + response.ErrorCode + "\",\"errorInfo\":\"" + response.ErrorMessage + "\"}";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return "";
        }


        public async Task<string> publishRecipeAuditMsg(string auditResult,string description,string openid,string token)
        {
            var options = new WechatApiClientOptions()
            {
                AppId = "wxe7c826a1a5e00055",
                AppSecret = "425edf0bcd1ef5d7c2a32d55832f643f",
            };
            var client = new WechatApiClient(options);
            IDictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem> data = new Dictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem>();
            var dataItem1 = new CgibinMessageSubscribeSendRequest.Types.DataItem
            {
                Value = description //审方结果
            };
            var dataItem2 = new CgibinMessageSubscribeSendRequest.Types.DataItem
            {
                Value = auditResult //审方说明
            };
           
            data["thing5"] = dataItem1;
            data["thing2"] = dataItem2;

            try
            {
                var request = new CgibinMessageSubscribeSendRequest()
                {
                    AccessToken = token,
                    ToUserOpenId = openid,
                    TemplateId = "K9QtB4z2B8YIyUScP1Bx5iROO-IOZqd3Opi7KjySbKc",
                    MiniProgramState = "trial",
                    MiniProgramPagePath = "pages/user_recipe/user_recipe",
                    Language = "zh_CN",
                    Data = data
                };
                var response = await client.ExecuteCgibinMessageSubscribeSendAsync(request);
                if (response.IsSuccessful())
                {

                }
                else
                {
                    return "{\"errorCode\":\"" + response.ErrorCode + "\",\"errorInfo\":\"" + response.ErrorMessage + "\"}";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return "";
        }

        


    }
}
