using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi_yzy.common
{
    public static class GetToken
    {

        public static ResultMessage getTokenResult(string appid, string method,string timestamp, string nonce, string sign,string body)
        {
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //返回值
            ResultMessage resultmsg = new ResultMessage();
            string inputstr = "";
            try
            {
                //身份验证
                resultmsg = Token.getTokenResult(appid, method, timestamp, nonce, sign, body);

                inputstr = "appid:" + appid + "&method:" + method + "&timestamp:" + timestamp + "&nonce:" + nonce + "&sign:" + sign;

                DbOperator.saveWebapiOutputLog(appid, method, "身份验证", inputstr, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return resultmsg;
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { code = -99, msg = "exception:" + ex.Message };
                //执行失败记录日志
                DbOperator.saveWebapiOutputLog(appid, method, "身份验证", inputstr, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return resultmsg;

            }
        }
    }
}
