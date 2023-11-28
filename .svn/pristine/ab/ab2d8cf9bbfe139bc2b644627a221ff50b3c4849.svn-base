using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi_yzy.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class appointmentController : Controller
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

        //检查用户是否是医生
        public ActionResult checkUserPermissions(object obj)
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
                string phone = bodyObject["phone"] == null ? "0" : bodyObject["phone"].ToString();
                string dbRes = DbOperator.checkUserPermissions(openid, phone);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "校验用户的权限", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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


        //查询医院
        public ActionResult queryHospital(object obj)
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
                string phone = bodyObject["phone"] == null ? "0" : bodyObject["phone"].ToString();
                string dbRes = DbOperator.queryHospital(openid, phone);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "查询医院列表", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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

        //生成输注预约
        public ActionResult generateAppointment(object obj)
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
                string appointmentUserPhone = bodyObject["appointmentUserPhone"] == null ? "" : bodyObject["appointmentUserPhone"].ToString();
                string patientName = bodyObject["patientName"] == null ? "" : bodyObject["patientName"].ToString();
                string patientPhone = bodyObject["patientPhone"] == null ? "" : bodyObject["patientPhone"].ToString();
                string appointmentTime = bodyObject["appointmentTime"] == null ? "" : bodyObject["appointmentTime"].ToString();
                string appointmentHospitalId = bodyObject["appointmentHospitalId"] == null ? "" : bodyObject["appointmentHospitalId"].ToString();
                string departmentId = bodyObject["departmentId"] == null ? "" : bodyObject["departmentId"].ToString();
                string appointmentDoctorScheduleId = bodyObject["appointmentDoctorScheduleId"] == null ? "" : bodyObject["appointmentDoctorScheduleId"].ToString();
                string dbRes = DbOperator.generateAppointment(openid, appointmentUserPhone,patientName,patientPhone,appointmentTime,appointmentHospitalId, departmentId, appointmentDoctorScheduleId);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "生成预约信息", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                
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
        //获取预约信息
        public ActionResult getAppointment(object obj)
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
                string phone = bodyObject["phone"] == null ? "0" : bodyObject["phone"].ToString();
                string type = bodyObject["type"] == null ? "0" : bodyObject["type"].ToString();
                string dbRes = DbOperator.getAppointment(openid, phone,type);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取预约信息", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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

        //更新预约状态
        public ActionResult updateAppointmentStatus(object obj)
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
                string phone = bodyObject["phone"] == null ? "" : bodyObject["phone"].ToString();
                string type = bodyObject["type"] == null ? "" : bodyObject["type"].ToString();
                string appointmentId = bodyObject["appointmentId"] == null ? "" : bodyObject["appointmentId"].ToString();
                string origin_status = bodyObject["origin_status"] == null ? "" : bodyObject["origin_status"].ToString();
                string status = bodyObject["status"] == null ? "" : bodyObject["status"].ToString();
                string dbRes = DbOperator.updateAppointmentStatus(openid, phone, type,appointmentId,origin_status,status);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取预约信息", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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

        //获取科室信息
        public ActionResult getHospitalSchedule(object obj)
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

                string hospitalId = bodyObject["hospitalId"] == null ? "" : bodyObject["hospitalId"].ToString(); //openid
                string date = bodyObject["date"] == null ? "" : bodyObject["date"].ToString();
                string type = bodyObject["type"] == null ? "" : bodyObject["type"].ToString();
                string dbRes = DbOperator.getHospitalSchedule(hospitalId, date,type);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取预约信息", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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


        public ActionResult getDepartmentOrSubClass(object obj)
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

                string hospitalId = bodyObject["hospitalId"] == null ? "" : bodyObject["hospitalId"].ToString(); //openid
                string id = bodyObject["id"] == null ? "0" : bodyObject["id"].ToString();
                string queryType = bodyObject["queryType"] == null ? "0" : bodyObject["queryType"].ToString();
                string dbRes = DbOperator.getDepartmentOrSubClass(hospitalId, id, queryType);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取预约信息", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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


        public ActionResult getDoctor(object obj)
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

                string hospitalId = bodyObject["hospitalId"] == null ? "" : bodyObject["hospitalId"].ToString(); //openid
                string departmentSubClassId = bodyObject["departmentSubClassId"] == null ? "0" : bodyObject["departmentSubClassId"].ToString();
                string date = bodyObject["date"] == null ? "" : bodyObject["date"].ToString();
                string type = bodyObject["type"] == null ? "ALL" : bodyObject["type"].ToString();
                string dbRes = DbOperator.getDoctor(hospitalId, departmentSubClassId, date, type);


                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取预约信息", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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
