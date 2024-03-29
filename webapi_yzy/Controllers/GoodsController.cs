﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi_yzy.Model;
using webapi_yzy.Service;
namespace webapi_yzy.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GoodsController : Controller
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

        //查询商品
        public ActionResult searchGoods(object obj)
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
                //反序列化为对象获取drugslist
                drugsListObj drugObj = JsonConvert.DeserializeObject<drugsListObj>(JsonConvert.SerializeObject(bodyObject["searchObj"]));
                string searchContent = bodyObject["searchObj"]["searchContent"] == null ? "" : bodyObject["searchObj"]["searchContent"].ToString(); //商品查询名
                string pageIndex = bodyObject["searchObj"]["pageIndex"] == null ? "" : bodyObject["searchObj"]["pageIndex"].ToString();//页码
                string pageSize = bodyObject["searchObj"]["pageSize"] == null ? "" : bodyObject["searchObj"]["pageSize"].ToString();//每页记录数
                string sortRule = bodyObject["searchObj"]["sortRule"] == null ? "" : bodyObject["searchObj"]["sortRule"].ToString(); //排序规则
                string filterRule = bodyObject["searchObj"]["filterRule"] == null ? "" : bodyObject["searchObj"]["filterRule"].ToString(); //筛选条件
                List<Drugs> drugsList = drugObj.drugsList;//list
                //查询商品内容
               
                if ((drugsList == null || drugsList.Count == 0 || drugsList.Equals("[]")) && !searchContent.Equals(""))
                {
                    searchService service = new searchService();
                    drugsList = service.GetDrugsList(searchContent);
                    if (drugsList.Count==0)
                    {
                        //每一次新的查询都保存搜索记录
                        DbOperator.saveSearchRecord(searchContent, "不存在商品", "搜索");
                    }
                    else
                    {
                        
                        DbOperator.saveSearchRecord(searchContent, drugsList[0].spdm, "搜索");
                    }
                    
                }
                string dbRes = DbOperator.searchGoods(appid, drugsList, searchContent, sortRule, filterRule, int.Parse(pageIndex), int.Parse(pageSize));        
                string drugsListJson = JsonConvert.SerializeObject(drugsList);
                
                if (pageIndex.Equals("1"))
                {
                    dbRes = dbRes.Substring(0, dbRes.Length - 1) + ",\"drugsList\":" + drugsListJson + "}";
                }
                else dbRes = dbRes.Substring(0, dbRes.Length - 1) + ",\"drugsList\":" + drugsListJson + "}";

                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "查询商品", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }

        //获取分类
        public ActionResult getGoodsCategory(object obj)
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
                
                int parentId = (int)bodyObject["parentId"];

                string dbRes = DbOperator.getGoodsCategory(parentId);
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取商品分类", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }


        //获取用户可能想搜索的内容
        public ActionResult getMaySearch(object obj)
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

                string searchContent = (string)bodyObject["searchContent"];

                string dbRes = DbOperator.getMaySearch(searchContent);
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "推荐搜索", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
            catch (Exception ex)
            {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }




        /// <summary>
        /// 获取商品当前的库存和价格
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
                //身份验证
                resultmsg = getTokenResult(trueBody, appid, method);
                //身份验证失败
                if (resultmsg.code != 99)
                {
                    return new JsonResult(resultmsg);
                }

                JObject bodyObject = JObject.Parse(trueBody);

                string bodyRequest = (string)bodyObject["Body"];

                //身份验证成功，开始处理数据
                resultmsg = DbOperator.getShopSellGoodsInfo(bodyRequest);
                //

                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理", body, JSON.Encode(resultmsg), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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
        /// 获取当前的药品中是否含有冷链药品的标识
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult getGoodsColdFlag(object obj) {
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

                //先获取门店ID
                string goodsList = bodyObject["goodsList"] == null ? "" : bodyObject["goodsList"].ToString();

                string dbRes = DbOperator.getGoodsColdFlag(goodsList);
                
                object dataObj = JSON.Decode(dbRes);
                resultmsg.data = dataObj;
                resultmsg.msg = "获取冷链标识成功";
                resultmsg.code = 99;

                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "获取冷链标识", body, JSON.Encode(resultmsg.data), resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            } catch (Exception ex) {
                resultmsg = new ResultMessage { msg = "exception:" + ex.Message, code = -99 };
                //日志
                DbOperator.saveWebapiOutputLog(appid, method, "数据处理Exception", body, "", resultmsg.code, resultmsg.msg, beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new JsonResult(resultmsg);
            }
        }
    }
}
