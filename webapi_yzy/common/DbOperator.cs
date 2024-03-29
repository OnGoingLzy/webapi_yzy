﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using webapi_yzy.Model;

namespace webapi_yzy
{
    public class DbOperator
    {
        /// <summary>
        /// 获取Appsecret
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        public static string getAppsecret(string appid)
        {
            string sqlStr = "SELECT appsecret FROM t_webapi_appid WITH(NOLOCK) WHERE appid = '" + appid + "'";
            DataTable dt = BaseSql.getInstance().getDataTable(sqlStr);
            if (dt == null || dt.Rows.Count == 0)
            {
                return "";
            }
            else
            {
                return dt.Rows[0]["appsecret"].ToString();
            }
        }
        /// <summary>
        /// 保存外部调用的日志
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="method">方法名</param>
        /// <param name="czlx">操作类型</param>
        /// <param name="inputData">输入数据</param>
        /// <param name="outputData">输出数据</param>
        /// <param name="flag">返回值</param>
        /// <param name="flag_sm">返回值说明</param>
        /// <param name="kssj">操作的开始时间</param>
        /// <param name="jssj">操作的结束时间</param>
        public static void saveWebapiOutputLog(string appid, string method, string operateType, string inputData, string outputData,
            int resultCode, string resultMsg, string beginTime, string endTime)
        {
            try
            {
                if (inputData == null)
                {
                    inputData = "";
                }
                ArrayList arrayList = new ArrayList();
                arrayList.Add(new UProcPara("@operationType", SqlDbType.NVarChar, 30, operateType));
                arrayList.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 200, method));
                arrayList.Add(new UProcPara("@inputdata", SqlDbType.VarChar, inputData.Length * 2, inputData));
                arrayList.Add(new UProcPara("@outputdata", SqlDbType.VarChar, outputData.Length * 2, outputData));
                arrayList.Add(new UProcPara("@flag", SqlDbType.Int, 0, resultCode));
                arrayList.Add(new UProcPara("@flagMsg", SqlDbType.NVarChar, 200, resultMsg));
                arrayList.Add(new UProcPara("@beginTime", SqlDbType.VarChar, 30, beginTime));
                arrayList.Add(new UProcPara("@endTime", SqlDbType.VarChar, 30, endTime));
                arrayList.Add(new UProcPara("@operatorId", SqlDbType.VarChar, 50, appid));
                arrayList.Add(new UProcPara("@notes", SqlDbType.VarChar, 100, ""));
                DataTable dt = BaseSql.getInstance().procedure("p_operation_log_insert", arrayList);
            }
            catch (Exception ex)
            {
                
            }
        }

        /// <summary>
        /// 获取商品当前的库存和价格
        /// </summary>
        /// <param name="jsonShopGoods"></param>
        /// <returns></returns>
        public static ResultMessage getShopSellGoodsInfo(string jsonShopGoods)
        {
            ResultMessage resultMsg = new ResultMessage();
            try
            {
                SqlParameter[] prams = new SqlParameter[1];
                prams[0] = BaseSql.getInstance().createparam("jsonShopGoods", SqlDbType.VarChar, jsonShopGoods.Length * 2, ParameterDirection.Input, jsonShopGoods);

                DataSet ds = null;
                BaseSqlZhyf.getInstance().RunProcdsstr("p_interface_yzy_check_shopSellGoodsInfo", ref ds, prams);

                string strResult = "";
                for (int rowindex = 0; rowindex < ds.Tables[0].Rows.Count; rowindex++)
                {
                    strResult += ds.Tables[0].Rows[rowindex][0].ToString();
                }
                resultMsg.data = JSON.Decode(strResult);
                resultMsg.code = int.Parse(ds.Tables[1].Rows[0]["resultCode"].ToString());
                resultMsg.msg = ds.Tables[1].Rows[0]["resultMsg"].ToString();
            }
            catch (Exception ex)
            {
                resultMsg.code = -99;
                resultMsg.msg = "Exception异常，" + ex.Message;
            }
            return resultMsg;
        }


        //龙宗杨↓************************************************************************************
        public static string addGoodsToShoppingCart(string openid, string shopId, string goodsId, int num)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@shopId", SqlDbType.VarChar, 50, shopId));
                arr.Add(new UProcPara("@goodsId", SqlDbType.VarChar, 50, goodsId));
                arr.Add(new UProcPara("@num", SqlDbType.Decimal, 0, num));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "api/trade/addGoodsToShoppingCart"));


                DataTable dt = BaseSql.getInstance().procedure("p_trade_addGoodsToShoppingCart", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        /// <summary>
        /// 调用微信接口获取openid
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="secret"></param>
        /// <param name="sj_code"></param>
        public static ResultMessage GetOpenid(string appid, string secret, string js_code)
        {
            appid = "wxe7c826a1a5e00055";
            string res = "";
            ResultMessage resultmsg = new ResultMessage();
            //调用微信小程序原本的接口，获取token
            String getTokenUrl = String.Format("https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type={3}", appid, "425edf0bcd1ef5d7c2a32d55832f643f", js_code, "authorization_code");//url传递参数
            string beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getTokenUrl);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "GET";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        String str = reader.ReadToEnd();

                        // Console.WriteLine("access_token:" + tokenObj["data"]["access_token"].ToString());
                        object obj = JSON.Decode(str);
                        resultmsg.data = obj;
                        DbOperator.saveWebapiOutputLog(appid, "GetOpenid", "获取openid", "", str, 99, "", beginTime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }
            }
            resultmsg.code = 99;

            resultmsg.msg = "获取openid结果";
            return resultmsg;
        }

        public static string searchGoods(string appid, List<Drugs> drugsList, string searchContent, string sortRule, string filterRule, int pageIndex = 1, int pageSize = 20)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@appid", SqlDbType.VarChar, 20, appid));
                arr.Add(new UProcPara("@searchContent", SqlDbType.VarChar, 100, searchContent));
                arr.Add(new UProcPara("@pageIndex", SqlDbType.Int, 0, pageIndex));
                arr.Add(new UProcPara("@pageSize", SqlDbType.Int, 0, pageSize));
                //list对象转换成json
                string goodsIdListJson = JsonConvert.SerializeObject(drugsList);
                arr.Add(new UProcPara("@goodsIdListJson", SqlDbType.VarChar, goodsIdListJson.Length * 2, goodsIdListJson));



                DataTable dt = BaseSql.getInstance().procedure("p_goods_queryGoods", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        //保存搜索记录
        public static string saveSearchRecord(string searchContent, string goodsId,string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@searchContent", SqlDbType.VarChar, 100, searchContent));
                arr.Add(new UProcPara("@goodsId", SqlDbType.VarChar, 100, goodsId));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 20, type));
                DataTable dt = BaseSql.getInstance().procedure("p_goods_saveSearchRecord", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        public static string searchShops(string appid, string goods_json, float longitude, float latitude)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@appid", SqlDbType.VarChar, 20, appid));
                arr.Add(new UProcPara("@goods_json", SqlDbType.VarChar, goods_json.Length * 2, goods_json));
                arr.Add(new UProcPara("@longitude", SqlDbType.Decimal, 16, longitude));
                arr.Add(new UProcPara("@latitude", SqlDbType.Decimal, 16, latitude));

                DataTable dt = BaseSql.getInstance().procedure("p_shop_queryShop", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        public static string GetAccessToken(string appid, string secret)
        {
            appid = "wxe7c826a1a5e00055";
            string res = "";
            //调用微信小程序原本的接口，获取token
            String getTokenUrl = String.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type={0}&appid={1}&secret={2}", "client_credential", appid, "425edf0bcd1ef5d7c2a32d55832f643f");//url传递参数
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getTokenUrl);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "GET";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        String strToken = reader.ReadToEnd();
                        JObject tokenObj = JsonConvert.DeserializeObject<JObject>(strToken);
                        // Console.WriteLine("access_token:" + tokenObj["data"]["access_token"].ToString());

                        res = JsonConvert.SerializeObject(tokenObj);
                    }
                }
            }


            return res;
        }

        public static string CheckUserExist(string openid, string sharerPhone)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@sharerPhone", SqlDbType.VarChar, 50, sharerPhone));

                DataTable dt = BaseSql.getInstance().procedure("p_user_checkUserExist", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string bindUserPhone(string openid, string phone)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 50, phone));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "api/user/getUserPhoneNumber"));
                DataTable dt = BaseSql.getInstance().procedure("p_user_bindUserPhone", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

       

        public static string GetPhoneNumber(string code, string token)
        {
            string res = "";

            string body = "{\"code\":\"" + code + "\"}";


            String url = String.Format("https://api.weixin.qq.com/wxa/business/getuserphonenumber?access_token={0}", token);//url传递参数
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            Encoding encoding = Encoding.UTF8;
            byte[] byteArray = Encoding.UTF8.GetBytes(body);
            string responseData = String.Empty;
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = byteArray.Length;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    responseData = reader.ReadToEnd();
                    JObject phoneObj = JsonConvert.DeserializeObject<JObject>(responseData);
                    res = JsonConvert.SerializeObject(phoneObj);
                }

            }
            res = "{\"sm\":\"获取用户手机号成功\",\"result\":\"99\",\"data\":" + res + "}";

            return res;
        }

        public static string getGoodsUser(string openid, string phone)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 50, phone));

                DataTable dt = BaseSql.getInstance().procedure("p_user_getGoodsUser", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        

        public static string addGoodsUser(string openid, string phone, string sex, string idCard, string relationship, string birthday, string goodsUserName)
        {
            string res = "";
            try
            {
                birthday = idCard.Substring(6, 8);
                ArrayList arr = new ArrayList();
                if (sex.Equals("0")) sex = "男";
                if (sex.Equals("1")) sex = "女";
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 11, phone));
                arr.Add(new UProcPara("@sex", SqlDbType.VarChar, 5, sex));
                arr.Add(new UProcPara("@idCard", SqlDbType.VarChar, 50, idCard));
                arr.Add(new UProcPara("@relationship", SqlDbType.VarChar, 10, relationship));
                arr.Add(new UProcPara("@birthday", SqlDbType.VarChar, 20, birthday));
                arr.Add(new UProcPara("@goodsUserName", SqlDbType.VarChar, 20, goodsUserName));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "api/user/addGoodsUser"));

                DataTable dt = BaseSql.getInstance().procedure("p_user_addGoodsUser", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        

        public static string checkGoodsUserExists(string openid, string phone, string sex, string idCard, string relationship, string birthday, string goodsUserName)
        {
            string res = "";
            try
            {
                birthday = idCard.Substring(6, 8);
                ArrayList arr = new ArrayList();
                if (sex.Equals("0")) sex = "男";
                if (sex.Equals("1")) sex = "女";
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 11, phone));
                arr.Add(new UProcPara("@sex", SqlDbType.VarChar, 5, sex));
                arr.Add(new UProcPara("@idCard", SqlDbType.VarChar, 50, idCard));
                arr.Add(new UProcPara("@relationship", SqlDbType.VarChar, 10, relationship));
                arr.Add(new UProcPara("@birthday", SqlDbType.VarChar, 20, birthday));
                arr.Add(new UProcPara("@goodsUserName", SqlDbType.VarChar, 20, goodsUserName));

                DataTable dt = BaseSql.getInstance().procedure("p_user_checkGoodsUserExists", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        public static string delGoodsUser(string id, string openid)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@id", SqlDbType.Int, 0, id));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "api/user/delGoodsUser"));
                DataTable dt = BaseSql.getInstance().procedure("p_user_delGoodsUser", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        

        public static string getGoodsCategory(int parentId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@parentId", SqlDbType.Int, 0, parentId));

                DataTable dt = BaseSql.getInstance().procedure("p_goods_getGoodsCategory", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        //根据用户输入内容获取用户可能要查询的药品列表

        public static string getMaySearch(string searchContent)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@searchContent", SqlDbType.VarChar, 50, searchContent));

                DataTable dt = BaseSql.getInstance().procedure("p_goods_getMaySearch", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getMerchantInfo(string businessUnitId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@businessUnitId", SqlDbType.VarChar, 50, businessUnitId));

                DataTable dt = BaseSql.getInstance().procedure("p_merchant_getMerchantInfo", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        public static string submitRefundApplication(refundObj refundObj, string new_main_order_id, string new_out_order_id, string openid)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@new_main_order_id", SqlDbType.VarChar, 50, new_main_order_id));
                arr.Add(new UProcPara("@new_out_order_id", SqlDbType.VarChar, 50, new_out_order_id));
                arr.Add(new UProcPara("@origin_out_order_id", SqlDbType.VarChar, 50, refundObj.out_order_id));
                arr.Add(new UProcPara("@origin_wxpay_order_id", SqlDbType.VarChar, 50, refundObj.wxpay_order_id));
                arr.Add(new UProcPara("@totalPrice", SqlDbType.Decimal, 0, -refundObj.refundAmount));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@refund_status", SqlDbType.VarChar, 50, "待审核"));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "/WechatPayService/submitRefundApplication"));
                DataTable dt = BaseSql.getInstance().procedure("p_wxPayOrder_submitRefundApplication", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string updateWxPayRefundOrder(string out_order_id, string wxpay_refund_order_id, string wxpay_order_id, string trade_status, DateTimeOffset finishedTime)
        {
            string res = "";
            string refund_status = "退款中";
            if (trade_status.Equals("SUCCESS"))
            {
                refund_status = "已退款";
            }
            else if (trade_status.Equals("CLOSED") || trade_status.Equals("ABNORMAL"))
            {
                refund_status = "退款失败";
            }
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@out_order_id", SqlDbType.VarChar, 50, out_order_id));
                arr.Add(new UProcPara("@wxpay_refund_order_id", SqlDbType.VarChar, 100, wxpay_refund_order_id));
                arr.Add(new UProcPara("@wxpay_order_id", SqlDbType.VarChar, 50, wxpay_order_id));
                arr.Add(new UProcPara("@trade_status", SqlDbType.VarChar, 50, trade_status));
                arr.Add(new UProcPara("@refund_status", SqlDbType.VarChar, 50, refund_status));
                arr.Add(new UProcPara("@finishedTime", SqlDbType.DateTimeOffset, 7, finishedTime));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "/WechatPayService/updateWxPayRefundOrder"));
                DataTable dt = BaseSql.getInstance().procedure("p_wxPayOrder_updateWxPayRefundOrder", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        public static string getPrePayId(string main_order_id, string openid)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 50, main_order_id));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));

                DataTable dt = BaseSql.getInstance().procedure("p_wxPayOrder_getPrePayId", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string createWxPrePayOrder(string prepay_id, GetPayTransactionByOutTradeNumberResponse wxOrderInfo, string main_order_id, string openid)
        {
            string res = "";
            try
            {
                string out_order_id = wxOrderInfo.OutTradeNumber;
                decimal totalPrice = (decimal)wxOrderInfo.Amount.Total / 100;
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@prepay_id", SqlDbType.VarChar, 50, prepay_id));
                arr.Add(new UProcPara("@out_order_id", SqlDbType.VarChar, 50, out_order_id));
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 50, main_order_id));
                arr.Add(new UProcPara("@totalPrice", SqlDbType.Decimal, 0, totalPrice));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "/WechatPayService/JSAPIpreOrderAsync"));

                DataTable dt = BaseSql.getInstance().procedure("p_wxPayOrder_createWxPrePayOrder", arr);

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string addOrUpdateWxPayOrder(GetPayTransactionByOutTradeNumberResponse wxOrderInfo, string main_order_id)
        {
            string res = "";
            try
            {
                string out_order_id = wxOrderInfo.OutTradeNumber;
                decimal totalPrice = (decimal)wxOrderInfo.Amount.Total / 100;
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@out_order_id", SqlDbType.VarChar, 50, out_order_id));
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 50, main_order_id));
                arr.Add(new UProcPara("@totalPrice", SqlDbType.Decimal, 0, totalPrice));
                arr.Add(new UProcPara("@finishedTime", SqlDbType.DateTimeOffset, 50, wxOrderInfo.SuccessTime));
                arr.Add(new UProcPara("@trade_type", SqlDbType.VarChar, 50, wxOrderInfo.TradeType));
                arr.Add(new UProcPara("@trade_status", SqlDbType.VarChar, 50, wxOrderInfo.TradeState));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, wxOrderInfo.Payer.OpenId));
                arr.Add(new UProcPara("@wxpay_order_id", SqlDbType.VarChar, 50, wxOrderInfo.TransactionId));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "/WechatPayService/addOrUpdateWxPayOrder"));
                DataTable dt = BaseSql.getInstance().procedure("p_wxPayOrder_createWxPayOrder", arr);

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        public static string getShareQRCode(string lxdh, string token)
        {

            string res = "";

            string body = "{\"page\":\"pages/index/index\",\"scene\":\"lxdh=" + lxdh + "\",\"check_path\":true,\"env_version\":\"release\"}";

            String url = String.Format("https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token={0}", token);//url传递参数
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            Encoding encoding = Encoding.UTF8;
            byte[] byteArray = Encoding.UTF8.GetBytes(body);
            string responseData = String.Empty;
            req.Method = "POST";
            req.ContentType = "application/json;charset=UTF-8";
            req.ContentLength = byteArray.Length;


            Stream writer = req.GetRequestStream();
            writer.Write(byteArray, 0, byteArray.Length);
            writer.Close();


            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)req.GetResponse();
            System.IO.Stream stream;
            stream = response.GetResponseStream();
            List<byte> bytes = new List<byte>();
            int temp = stream.ReadByte();

            while (temp != -1)
            {
                bytes.Add((byte)temp);
                temp = stream.ReadByte();
            }
            byte[] result = bytes.ToArray();
            string base64 = Convert.ToBase64String(result);


            res = "{\"sm\":\"生成推广码成功\",\"result\":\"99\",\"data\":\"" + base64 + "\"}";

            return res;

        }

        public static string getAddressList(string openid)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));

                DataTable dt = BaseSql.getInstance().procedure("p_user_getUserAddress", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        //name, phone, city,userId,isDefault,detailAddress
        public static string addAddress(string name, string phone, string city, string openid, int isDefault, string detailAddress,decimal longitude,decimal latitude)
        {
            string res = "";
            try
            {

                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@name", SqlDbType.VarChar, 50, name));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 11, phone));
                arr.Add(new UProcPara("@city", SqlDbType.VarChar, 50, city));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@isDefault", SqlDbType.Int, 10, isDefault));
                arr.Add(new UProcPara("@detailAddress", SqlDbType.VarChar, 50, detailAddress));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "api/user/addAddress"));
                arr.Add(new UProcPara("@longitude", SqlDbType.Decimal, 8, longitude));
                arr.Add(new UProcPara("@latitude", SqlDbType.Decimal, 8, latitude));
                DataTable dt = BaseSql.getInstance().procedure("p_user_addAddress", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        public static string updateAddress(int id, string name, string phone, string city, string userId, int isDefault, string detailAddress, decimal longitude, decimal latitude)
        {
            string res = "";
            try
            {

                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@id", SqlDbType.Int, 5, id));
                arr.Add(new UProcPara("@name", SqlDbType.VarChar, 50, name));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 11, phone));
                arr.Add(new UProcPara("@city", SqlDbType.VarChar, 50, city));
                arr.Add(new UProcPara("@userId", SqlDbType.VarChar, 50, userId));
                arr.Add(new UProcPara("@isDefault", SqlDbType.Int, 10, isDefault));
                arr.Add(new UProcPara("@detailAddress", SqlDbType.VarChar, 50, detailAddress));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "api/user/updateAddress"));
                arr.Add(new UProcPara("@longitude", SqlDbType.Decimal, 8, longitude));
                arr.Add(new UProcPara("@latitude", SqlDbType.Decimal, 8, latitude));
                DataTable dt = BaseSql.getInstance().procedure("p_user_updateAddress", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        public static string deleteAddress(string userId, int id)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@userId", SqlDbType.VarChar, 50, userId));
                arr.Add(new UProcPara("@id", SqlDbType.VarChar, 50, id));
                arr.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 50, "api/user/deleteAddress"));
                DataTable dt = BaseSql.getInstance().procedure("p_user_deleteAddress", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getNoPayOrder()
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, "publishMsg"));
                DataTable dt = BaseSql.getInstance().procedure("p_order_getNoPayOrder", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getNoPayOrderByType(string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, type));
                DataTable dt = BaseSql.getInstance().procedure("p_order_getNoPayOrder", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getRefundOrder(string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, type));
                DataTable dt = BaseSql.getInstance().procedure("p_order_getRefundOrder", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getNoUseRecipeAudit()
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, "publishMsg"));
                DataTable dt = BaseSql.getInstance().procedure("p_recipe_getNoUseRecipeAudit", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getNoUseRecipeAuditByType(string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, type));
                DataTable dt = BaseSql.getInstance().procedure("p_recipe_getNoUseRecipeAudit", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        public static string getOrderBusinessId(string main_order_id)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 50, main_order_id));
                DataTable dt = BaseSql.getInstance().procedure("p_order_getOrderBusinessId", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }



        public static string getOrderList(string openid)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                DataTable dt = BaseSql.getInstance().procedure("p_order_getOrderList3", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        //获取处方药结算所需信息
        public static string getGoodsSettlementInfo(string openid,string clinicId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@clinicId", SqlDbType.VarChar, 50, clinicId));
                DataTable dt = BaseSql.getInstance().procedure("p_trade_getGoodsSettlementInfo", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        //获取处方列表
        public static string getRecipeList(string openid,string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, type));
                DataTable dt = BaseSql.getInstance().procedure("p_recipe_getRecipeList", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        public static string getOrderNumByStatus(string openid,string status)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@status", SqlDbType.VarChar, 50, status));
                DataTable dt = BaseSql.getInstance().procedure("p_order_getOrderNumByStatus", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        public static string getWxPayOrder(string main_order_id, string openid)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 50, main_order_id));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));

                DataTable dt = BaseSql.getInstance().procedure("p_wxPayOrder_getWxPayOrder", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string existsEffectiveRecipe(string openid,string goodsId, decimal num)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@goodsId", SqlDbType.VarChar, 50, goodsId));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@num", SqlDbType.Decimal, 0, num));

                DataTable dt = BaseSql.getInstance().procedure("p_recipe_existsEffectiveRecipe", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getShopCertificate(string shopId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@shopId", SqlDbType.VarChar, 50, shopId));
                DataTable dt = BaseSql.getInstance().procedure("p_shop_getShopCertificate", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        public static string getShopBaseInfo(string shopId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@shopId", SqlDbType.VarChar, 50, shopId));
                DataTable dt = BaseSql.getInstance().procedure("p_shop_getShopBaseInfo", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        //输注-查询医院
        public static string queryHospital(string openid, string phone)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 50, phone));
                DataTable dt = BaseSql.getInstance().procedure("p_appointment_queryHospital", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        //检测用户权限
        public static string checkUserPermissions(string openid, string phone)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 50, phone));
                DataTable dt = BaseSql.getInstance().procedure("p_appointment_checkUserPermissions", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getAppointment(string openid, string phone,string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 50, phone));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, type));
                DataTable dt = BaseSql.getInstance().procedure("p_appointment_getAppointment", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string generateAppointment(string openid, string appointmentUserPhone, string patientName, string patientPhone, string appointmentTime, string appointmentHospitalId, string departmentId, string appointmentDoctorScheduleId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@appointmentUserPhone", SqlDbType.VarChar, 50, appointmentUserPhone));
                arr.Add(new UProcPara("@patientName", SqlDbType.VarChar, 50, patientName));
                arr.Add(new UProcPara("@patientPhone", SqlDbType.VarChar, 50, patientPhone));
                arr.Add(new UProcPara("@appointmentTime", SqlDbType.VarChar, 50, appointmentTime));
                arr.Add(new UProcPara("@appointmentHospitalId", SqlDbType.VarChar, 50, appointmentHospitalId));
                arr.Add(new UProcPara("@departmentId", SqlDbType.VarChar, 50, departmentId));
                arr.Add(new UProcPara("@appointmentDoctorScheduleId", SqlDbType.VarChar, 50, appointmentDoctorScheduleId));
                DataTable dt = BaseSql.getInstance().procedure("p_appointment_generateAppointment", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        public static string updateAppointmentStatus(string openid, string phone, string type, string appointmentId, string origin_status, string status)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 50, openid));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 50, phone));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, type));
                arr.Add(new UProcPara("@appointmentId", SqlDbType.VarChar, 50, appointmentId));
                arr.Add(new UProcPara("@origin_status", SqlDbType.VarChar, 50, origin_status));
                arr.Add(new UProcPara("@status", SqlDbType.VarChar, 50, status));
                DataTable dt = BaseSql.getInstance().procedure("p_appointment_updateAppointmentStatus", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getHospitalSchedule(string hospitalId, string date, string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@hospitalId", SqlDbType.VarChar, 50, hospitalId));
                arr.Add(new UProcPara("@date", SqlDbType.VarChar, 50, date));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, type));

                DataTable dt = BaseSql.getInstance().procedure("p_appointment_getHospitalSchedule", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getDepartmentOrSubClass(string hospitalId, string id, string queryType)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@hospitalId", SqlDbType.VarChar, 50, hospitalId));
                arr.Add(new UProcPara("@id", SqlDbType.VarChar, 50, id));
                arr.Add(new UProcPara("@queryType", SqlDbType.VarChar, 50, queryType));

                DataTable dt = BaseSql.getInstance().procedure("p_appointment_getDepartmentOrSubClass", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string getDoctor(string hospitalId, string departmentSubClassId, string date, string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@hospitalId", SqlDbType.VarChar, 50, hospitalId));
                arr.Add(new UProcPara("@departmentSubClassId", SqlDbType.VarChar, 50, departmentSubClassId));
                arr.Add(new UProcPara("@date", SqlDbType.VarChar, 50, date));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 50, type));

                DataTable dt = BaseSql.getInstance().procedure("p_appointment_getDoctor", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        //更新银海订单状态
        public static string updateYinhaiOrderPayStatus(string main_order_id,string type,string status,string ord_pay_list,string ord_refd_list,string insuplc_admdvs,string costBreakdownJson)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 30, main_order_id));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 30, type));
                arr.Add(new UProcPara("@status", SqlDbType.VarChar, 30, status));
                arr.Add(new UProcPara("@ord_pay_list", SqlDbType.VarChar, ord_pay_list.Length*2, ord_pay_list));
                arr.Add(new UProcPara("@ord_refd_list", SqlDbType.VarChar, ord_refd_list.Length * 2, ord_refd_list));
                arr.Add(new UProcPara("@insuplc_admdvs", SqlDbType.VarChar, insuplc_admdvs.Length * 2, insuplc_admdvs));
                arr.Add(new UProcPara("@costBreakdownJson", SqlDbType.VarChar, costBreakdownJson.Length * 2, costBreakdownJson));

                DataTable dt = BaseSql.getInstance().procedure("p_yinhai_updateYinhaiOrderStatus", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        //api-根据主订单号获取订单信息和医药机构信息
        public static string getOrderIdAndYyjgInfoByMainOrderId(string main_order_id,string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 30, main_order_id));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 30, type));


                DataTable dt = BaseSql.getInstance().procedure("p_yinhai_getOrderIdAndYyjgInfoByMainOrderId", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        //获取yh插件初始化所需参数
        public static string getYhInitParam(string main_order_id,string payType)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 30, main_order_id));
                arr.Add(new UProcPara("@payType", SqlDbType.VarChar, 30, payType));


                DataTable dt = BaseSql.getInstance().procedure("p_yinhai_getOrderIdAndYyjgInfoByMainOrderId", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        //获取创建退款银海单所需参数
        public static string getYinHaiRefundOrderParam(string main_order_id,string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 30, main_order_id));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 30, type));


                DataTable dt = BaseSql.getInstance().procedure("p_yinhai_getYinHaiRefundOrderParam", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        public static string CheckAliPayUserExist(string user_id, string sharerPhone,string phone)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@user_id", SqlDbType.VarChar, 50, user_id));
                arr.Add(new UProcPara("@sharerPhone", SqlDbType.VarChar, 50, sharerPhone));
                arr.Add(new UProcPara("@phone", SqlDbType.VarChar, 50, phone));

                DataTable dt = BaseSql.getInstance().procedure("p_user_checkAliPayUserExist", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
        //获取创建银海单所需参数
        public static string getYinHaiOrderParam(string main_order_id,string type,string goodsUserId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@goodsUserId", SqlDbType.VarChar, 30, goodsUserId));
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 30, main_order_id));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 30, type));


                DataTable dt = BaseSql.getInstance().procedure("p_yinhai_getYinHaiOrderParam", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        //更新银海单基础信息
        public static string updateYinHaiOrderInfo(string main_order_id, JObject orderInfo,string type)
        {
            string res = "";
            try
            {
                if (type == "付款")
                {
                    ArrayList arr = new ArrayList();
                    arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 30, main_order_id));
                    arr.Add(new UProcPara("@plaf_ord_no", SqlDbType.VarChar, 50, (string)orderInfo["plaf_ord_no"]));
                    arr.Add(new UProcPara("@ord_sumamt", SqlDbType.VarChar, 50, (string)orderInfo["ord_sumamt"]));
                    arr.Add(new UProcPara("@dpp_ord_no", SqlDbType.VarChar, 50, (string)orderInfo["dpp_ord_no"]));
                    arr.Add(new UProcPara("@biz_ord_no", SqlDbType.VarChar, 50, (string)orderInfo["biz_ord_no"]));
                    arr.Add(new UProcPara("@begntime", SqlDbType.VarChar, 30, (string)orderInfo["begntime"]));
                    arr.Add(new UProcPara("@mert_app_type", SqlDbType.VarChar, 30, (string)orderInfo["mert_app_type"]));
                    arr.Add(new UProcPara("@cash_url", SqlDbType.VarChar, 300, (string)orderInfo["cash_url"]));
                    arr.Add(new UProcPara("@chrg_type", SqlDbType.VarChar, 300, (string)orderInfo["chrg_type"]));
                    arr.Add(new UProcPara("@type", SqlDbType.VarChar, 30, type));
                    arr.Add(new UProcPara("@app_chnl_list", SqlDbType.VarChar, JsonConvert.SerializeObject(orderInfo["app_chnl_list"]).Length*2, JsonConvert.SerializeObject(orderInfo["app_chnl_list"])));


                    DataTable dt = BaseSql.getInstance().procedure("p_yinhai_updateYinHaiOrderInfo", arr);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            res += dt.Rows[i][0].ToString();
                        }
                        if (res.Length > 0)
                        {
                            res = res.Substring(1, res.Length - 1);
                            res = res.Substring(0, res.Length - 1);
                        }
                    }
                    else
                    {
                        res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                    }
                }
                else if (type == "退款")
                {
                    ArrayList arr = new ArrayList();
                    arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 30, main_order_id));
                    arr.Add(new UProcPara("@cash_url", SqlDbType.VarChar, 300, (string)orderInfo["cash_url"]));
                    arr.Add(new UProcPara("@refd_token", SqlDbType.VarChar, 3000, (string)orderInfo["refd_token"]));
                    arr.Add(new UProcPara("@type", SqlDbType.VarChar, 30, type));


                    DataTable dt = BaseSql.getInstance().procedure("p_yinhai_updateYinHaiRefundOrderInfo", arr);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            res += dt.Rows[i][0].ToString();
                        }
                        if (res.Length > 0)
                        {
                            res = res.Substring(1, res.Length - 1);
                            res = res.Substring(0, res.Length - 1);
                        }
                    }
                    else
                    {
                        res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                    }
                }
                
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        //插入银海单
        public static string createYinHaiRefundOrder(string main_order_id, decimal totalPrice, string orderParam,string origin_main_order_id)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 30, main_order_id));
                arr.Add(new UProcPara("@totalPrice", SqlDbType.Decimal, 8, totalPrice));
                arr.Add(new UProcPara("@orderParam", SqlDbType.VarChar, orderParam.Length * 2, orderParam));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 30, "退款"));
                arr.Add(new UProcPara("@origin_main_order_id", SqlDbType.VarChar, 30, origin_main_order_id));
                DataTable dt = BaseSql.getInstance().procedure("p_yinhai_createYinHaiOrder", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        //插入银海单
        public static string createYinHaiOrder(string main_order_id,decimal totalPrice,string orderParam,string openid,string payType)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@payType", SqlDbType.VarChar, 60, payType));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 60, openid));
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 30, main_order_id));
                arr.Add(new UProcPara("@totalPrice", SqlDbType.Decimal, 8, totalPrice));
                arr.Add(new UProcPara("@orderParam", SqlDbType.VarChar, orderParam.Length*2, orderParam));
                DataTable dt = BaseSql.getInstance().procedure("p_yinhai_createYinHaiOrder", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        //云找药1.0存过

        public static string SearchMd(string mdlx, string shi, string qu, string md_json)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();

                arr.Add(new UProcPara("@mdlx", SqlDbType.VarChar, 100, mdlx));
                arr.Add(new UProcPara("@shi", SqlDbType.VarChar, 100, shi));
                arr.Add(new UProcPara("@qu", SqlDbType.VarChar, 100, qu));
                arr.Add(new UProcPara("@md_json", SqlDbType.VarChar, md_json.Length * 2, md_json));


                DataTable dt = BaseSql.getInstance().procedure("p_webapi_applet_get_mdwz", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        public static string InsertXqqd(string appid, string name, string lxdh, string openid, string spxx_json, string shdz, string mddm, string tjr, string count, string note)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@appid", SqlDbType.VarChar, 20, appid));
                arr.Add(new UProcPara("@name", SqlDbType.VarChar, 50, name));

                arr.Add(new UProcPara("@lxdh", SqlDbType.VarChar, 20, lxdh));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 100, openid));
                arr.Add(new UProcPara("@spxx_json", SqlDbType.VarChar, spxx_json.Length * 2, spxx_json));

                arr.Add(new UProcPara("@shdz", SqlDbType.VarChar, 100, shdz));
                arr.Add(new UProcPara("@mddm", SqlDbType.VarChar, 10, mddm));
                arr.Add(new UProcPara("@tjr", SqlDbType.VarChar, 50, tjr));
                arr.Add(new UProcPara("@count", SqlDbType.VarChar, 10, count));
                arr.Add(new UProcPara("@bz", SqlDbType.VarChar, 200, note));
                DataTable dt = BaseSql.getInstance().procedure("p_webapi_applet_ddxx_add", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        public static string SearchHistory(string appid, string lxdh, int pageIndex = 1, int pageSize = 20)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@appid", SqlDbType.VarChar, 20, appid));
                arr.Add(new UProcPara("@lxdh", SqlDbType.VarChar, 100, lxdh));
                arr.Add(new UProcPara("@pageIndex", SqlDbType.Int, 0, pageIndex));
                arr.Add(new UProcPara("@pageSize", SqlDbType.Int, 0, pageSize));

                DataTable dt = BaseSql.getInstance().procedure("p_webapi_applet_get_history", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }



        //后禹谦↓************************************************************************************
        /// <summary>
        /// 保存在水医方推送的日志log
        /// </summary>
        public static void saveWebapiOutputLogPush(string appid, string method, string operateType, string inputData, string outputData,
            int resultCode, string resultMsg, string beginTime, string endTime)
        {
            try
            {
                ArrayList arrayList = new ArrayList();
                arrayList.Add(new UProcPara("@operationType", SqlDbType.NVarChar, 30, operateType));
                arrayList.Add(new UProcPara("@operationPath", SqlDbType.VarChar, 200, method));
                arrayList.Add(new UProcPara("@inputdata", SqlDbType.VarChar, inputData.Length * 2, inputData));
                arrayList.Add(new UProcPara("@outputdata", SqlDbType.VarChar, outputData.Length * 2, outputData));
                arrayList.Add(new UProcPara("@flag", SqlDbType.Int, 0, resultCode));
                arrayList.Add(new UProcPara("@flagMsg", SqlDbType.NVarChar, 200, resultMsg));
                arrayList.Add(new UProcPara("@beginTime", SqlDbType.VarChar, 30, beginTime));
                arrayList.Add(new UProcPara("@endTime", SqlDbType.VarChar, 30, endTime));
                arrayList.Add(new UProcPara("@operatorId", SqlDbType.VarChar, 50, appid));
                arrayList.Add(new UProcPara("@notes", SqlDbType.VarChar, 100, ""));
                DataTable dt = BaseSql.getInstance().procedure("p_operation_log_insert", arrayList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// 通过商品代码获取在水医方对应的商品ID
        /// </summary>
        /// <param name="goodsId">商品代码</param>
        /// <returns></returns>
        public static string getSmarthosGoodsId(string goodsId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@goodsId", SqlDbType.VarChar, 50, goodsId));

                DataTable dt = BaseSql.getInstance().procedure("p_goods_getDiseaseByGoodsId", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }

        /// <summary>
        /// 处方问诊上传的功能
        /// </summary>
        /// <param name="goodsId">商品代码</param>
        /// <param name="allergyHistory">过敏史</param>
        /// <param name="amount">数量</param>
        /// <param name="diseaseName">疾病名称</param>
        /// <param name="personId">实名认证ID</param>
        /// <param name="smarthosGoodsId">在水医方的商品ID</param>
        /// <param name="dosage">单次剂量</param>
        /// <param name="dosageUnit">剂量单位</param>
        /// <param name="drugName">药品名称</param>
        /// <param name="drugSpecification">药品规格</param>
        /// <param name="drugUnit">药品单位</param>
        /// <param name="drugAdmission">药品用法</param>
        /// <param name="frequencyCode">使用频次代码</param>
        /// <param name="frequencyName">使用频次名称</param>
        /// <param name="shopId">门店ID</param>
        /// <returns></returns>
        public static string consultate(string goodsId, string allergyHistory, string amount, string diseaseName, string personId, string smarthosGoodsId, double dosage, string dosageUnit, string drugName, string drugSpecification, string drugUnit, string drugAdmission, string frequencyCode, string frequencyName, string shopId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@goodsId", SqlDbType.VarChar, 20, goodsId));
                arr.Add(new UProcPara("@allergyHistory", SqlDbType.VarChar, 20, allergyHistory));
                arr.Add(new UProcPara("@amount", SqlDbType.VarChar, 10, amount));
                arr.Add(new UProcPara("@diseaseName", SqlDbType.VarChar, 50, diseaseName));
                arr.Add(new UProcPara("@personId", SqlDbType.VarChar, 10, personId));
                arr.Add(new UProcPara("@smarthosGoodsId", SqlDbType.VarChar, 200, smarthosGoodsId));
                arr.Add(new UProcPara("@dosage", SqlDbType.Decimal, -1, dosage));
                arr.Add(new UProcPara("@dosageUnit", SqlDbType.VarChar, 50, dosageUnit));
                arr.Add(new UProcPara("@drugName", SqlDbType.VarChar, 200, drugName));
                arr.Add(new UProcPara("@drugSpecification", SqlDbType.VarChar, 100, drugSpecification));
                arr.Add(new UProcPara("@drugUnit", SqlDbType.VarChar, 50, drugUnit));
                arr.Add(new UProcPara("@drugAdmission", SqlDbType.VarChar, 200, drugAdmission));
                arr.Add(new UProcPara("@frequencyCode", SqlDbType.VarChar, 100, frequencyCode));
                arr.Add(new UProcPara("@frequencyName", SqlDbType.VarChar, 100, frequencyName));
                arr.Add(new UProcPara("@shopId", SqlDbType.VarChar, 255, shopId));

                DataTable dt = BaseSql.getInstance().procedure("p_recipe_upload_concat", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 调用在水医方的接口，通过上传处方内容，获取问诊ID，这个过程是将问诊ID重新存入数据库中的过程
        /// </summary>
        /// <param name="bizId">每次发起问诊的唯一值</param>
        /// <param name="clinicId">返回的问诊ID</param>
        /// <returns></returns>
        public static string saveClinicId(string bizId, string clinicId)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@bizId", SqlDbType.VarChar, 12, bizId));
                arr.Add(new UProcPara("@clinicId", SqlDbType.VarChar, 200, clinicId));

                DataTable dt = BaseSql.getInstance().procedure("p_recipe_feedback_clinicId", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }


        /// <summary>
        /// 调用在水医方的接口，通过上传问诊ID，获取问诊URL. 这个过程是将问诊URL重新存入数据库中的过程
        /// </summary>
        /// <param name="bizId">每次发起问诊的唯一值</param>
        /// <param name="clinicUrl">返回的问诊ID</param>
        /// <returns></returns>
        public static string saveClinicUrl(string bizId, string clinicUrl)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@bizId", SqlDbType.VarChar, 12, bizId));
                arr.Add(new UProcPara("@clinicUrl", SqlDbType.NVarChar, clinicUrl.Length * 2, clinicUrl));

                DataTable dt = BaseSql.getInstance().procedure("p_recipe_feedback_clinicUrl", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 获取问诊状态推送储存到数据库中
        /// </summary>
        /// <param name="clinicId">问诊 ID（唯一性）</param>
        /// <param name="status">问诊状态</param>
        /// <param name="statusMeaning">状态含义</param>
        /// <param name="cancelReason">取消原因</param>
        /// <returns></returns>
        public static string saveClinicStatus(string clinicId, string status, string statusMeaning, string cancelReason)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@clinicId", SqlDbType.VarChar, 255, clinicId));
                arr.Add(new UProcPara("@status", SqlDbType.VarChar, 50, status));
                arr.Add(new UProcPara("@statusMeaning", SqlDbType.NVarChar, statusMeaning.Length * 2, statusMeaning));
                arr.Add(new UProcPara("@cancelReason", SqlDbType.NVarChar, cancelReason.Length * 2, cancelReason));

                DataTable dt = BaseSql.getInstance().procedure("p_recipe_save_clinicStatus", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 将在水医方回调的处方开方结果和审方结果储存到数据库中
        /// </summary>
        /// <param name="body">完整的在水医方请求的body</param>
        /// <returns></returns>
        public static string saveClinicRecipe(string body)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@jsonStr", SqlDbType.NVarChar, body.Length * 2, body));

                DataTable dt = BaseSql.getInstance().procedure("p_recipe_save_clinic", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 通过获取到的openid，去数据库中获取该用户所对应的所有购物车信息
        /// </summary>
        /// <param name="openid">用户的openid</param>
        /// <returns></returns>
        public static string getUserCartGoods(string openid)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 200, openid));

                DataTable dt = BaseSql.getInstance().procedure("p_user_get_shoppingCart", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 前台提交订单保存到数据库中
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="openid"></param>
        /// <param name="goodsListStr"></param>
        /// <param name="locationId"></param>
        /// <param name="memo"></param>
        /// <param name="shippingFee"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string saveOrder(string shopId, string openid, string goodsListStr, string locationId, string memo, string shippingFee, string type)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@shopId", SqlDbType.VarChar, 255, shopId));
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 200, openid));
                arr.Add(new UProcPara("@goodsList", SqlDbType.NVarChar, goodsListStr.Length * 2, goodsListStr));
                arr.Add(new UProcPara("@locationId", SqlDbType.VarChar, 10, locationId));
                arr.Add(new UProcPara("@memo", SqlDbType.NVarChar, memo.Length * 2, memo));
                arr.Add(new UProcPara("@shippingFee", SqlDbType.VarChar, 50, shippingFee));
                arr.Add(new UProcPara("@type", SqlDbType.VarChar, 100, type));

                DataTable dt = BaseSql.getInstance().procedure("p_order_save_order", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 通过shopId来获取该门店的配送费
        /// </summary>
        /// <param name="shopId">门店ID</param>
        /// <param name="addressId">地址ID</param>
        /// <returns></returns>
        public static string getShopShippingFee(string shopId, string addressId)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@shopId", SqlDbType.VarChar, 255, shopId));
                arr.Add(new UProcPara("@addressId", SqlDbType.VarChar, 255, addressId));

                DataTable dt = BaseSql.getInstance().procedure("p_shop_get_shippingFee_new", arr);
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

        /// <summary>
        /// 删除用户的购物车信息
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="cartId"></param>
        /// <returns></returns>
        public static string delUserCartGoods(string openid, string cartId)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@openid", SqlDbType.VarChar, 255, openid));
                arr.Add(new UProcPara("@cartId", SqlDbType.Int, -1, cartId));

                DataTable dt = BaseSql.getInstance().procedure("p_user_del_userCartGoods", arr);
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

        public static string getMakeAccount(string mainOrderId)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@tradeNo", SqlDbType.VarChar, 50, mainOrderId));

                DataTable dt = BaseSql.getInstance().procedure("p_webapi_get_zhyf_makeAccount_json", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 通过调用存储过程，将从智慧药房返回回来的
        /// </summary>
        /// <param name="operateId"></param>
        /// <param name="returnJson"></param>
        /// <returns></returns>
        public static string updateOperateRecordByOperateId(string operateId, string returnJson)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@operateId", SqlDbType.BigInt, -1, operateId));
                arr.Add(new UProcPara("@returnJson", SqlDbType.VarChar, returnJson.Length * 2, returnJson));

                DataTable dt = BaseSql.getInstance().procedure("p_webapi_save_zhyf_retrunInfo", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 用户点击按钮后上传货品确认收货的状态
        /// </summary>
        /// <param name="tradeNo"></param>
        /// <param name="confirmType"></param>
        /// <returns></returns>
        public static string uploadConfirmReceived(string tradeNo, string confirmType)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@tradeNo", SqlDbType.VarChar, 255, tradeNo));
                arr.Add(new UProcPara("@confirmType", SqlDbType.VarChar, 255, confirmType));

                DataTable dt = BaseSql.getInstance().procedure("p_order_save_ConfirmReceive", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }
        /// <summary>
        /// 用户申请退款
        /// </summary>
        /// <param name="orign_main_order_id"></param>
        /// <returns></returns>
        public static ResultMessage applyReturned(string orign_main_order_id,string reasonOfReturn)
        {
            ResultMessage resultMsg = new ResultMessage();
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@orign_main_order_id", SqlDbType.VarChar, 50, orign_main_order_id));
                arr.Add(new UProcPara("@reasonOfReturn", SqlDbType.VarChar, 100, reasonOfReturn));

                DataTable dt = BaseSql.getInstance().procedure("p_order_commit_returned", arr);
                string dataMsg = dt.Rows[0]["data"].ToString();
                if (dataMsg.Length > 0)
                {
                    dataMsg = dataMsg.Substring(1, dataMsg.Length - 1);
                    dataMsg = dataMsg.Substring(0, dataMsg.Length - 1);
                }
                resultMsg.data = dataMsg;
                resultMsg.code = int.Parse(dt.Rows[0]["flag"].ToString());
                resultMsg.msg = dt.Rows[0]["result"].ToString();
            }
            catch (Exception ex)
            {
                resultMsg.data = "";
                resultMsg.code = -1;
                resultMsg.msg = "exception异常，" + ex.Message;
            }
            return resultMsg;
        }

        /// <summary>
        /// 用户申请退款所需的参数
        /// </summary>
        /// <param name="main_order_id"></param>
        /// <returns></returns>
        public static ResultMessage getJsonZhyfOrderReturned(string main_order_id)
        {
            ResultMessage resultMsg = new ResultMessage();
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 50, main_order_id));

                DataTable dt = BaseSql.getInstance().procedure("p_webapi_get_zhyf_orderReturned_json", arr);
                string dataMsg = dt.Rows[0]["data"].ToString();
                if (dataMsg.Length > 0)
                {
                    dataMsg = dataMsg.Substring(1, dataMsg.Length - 1);
                    dataMsg = dataMsg.Substring(0, dataMsg.Length - 1);
                }
                resultMsg.data = dataMsg;
                resultMsg.code = int.Parse(dt.Rows[0]["flag"].ToString());
                resultMsg.msg = dt.Rows[0]["result"].ToString();
            }
            catch (Exception ex)
            {
                resultMsg.data = "";
                resultMsg.code = -1;
                resultMsg.msg = "exception异常，" + ex.Message;
            }
            return resultMsg;
        }


        /// <summary>
        /// 获取通过主单号获取待退单的单号等信息
        /// </summary>
        /// <param name="mainOrderId"></param>
        /// <returns></returns>
        public static string getRefundInfo(string mainOrderId)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 255, mainOrderId));

                DataTable dt = BaseSql.getInstance().procedure("p_wxPayOrder_getRefundInfo", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 退款时填写t_order_operateRecord表
        /// </summary>
        /// <param name="mainOrderId"></param>
        /// <param name="operateMsg"></param>
        /// <param name="flag"></param>
        /// <param name="sm"></param>
        /// <returns></returns>
        public static string saveRefundOperateRecord(string mainOrderId, string operateMsg, int flag, string sm)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 255, mainOrderId));
                arr.Add(new UProcPara("@operateMsg", SqlDbType.NVarChar, 255, operateMsg));
                arr.Add(new UProcPara("@operateFlag", SqlDbType.Int, -1, flag));
                arr.Add(new UProcPara("@flagMsg", SqlDbType.NVarChar, 255, sm));

                DataTable dt = BaseSql.getInstance().procedure("p_webapi_save_refundOperateRecord", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 保存拒绝退款的订单
        /// </summary>
        /// <param name="mainOrderId"></param>
        public static void denyRefund(string mainOrderId)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 255, mainOrderId));
                
                DataTable dt = BaseSql.getInstance().procedure("p_save_order_denyRefund", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }
            

        }

        /// <summary>
        /// 通过调用存储过程，将从智慧药房返回回来的
        /// </summary>
        /// <param name="operateId"></param>
        /// <param name="returnJson"></param>
        /// <returns></returns>
        public static string updateOperateRecordByReturnJson(string orderJson, string returnJson)
        {
            string res = "";
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@orderJson", SqlDbType.VarChar, orderJson.Length*2, orderJson));
                arr.Add(new UProcPara("@returnJson", SqlDbType.VarChar, returnJson.Length * 2, returnJson));

                DataTable dt = BaseSql.getInstance().procedure("p_webapi_save_zhyf_returnJson", arr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res += dt.Rows[i][0].ToString();
                    }
                    if (res.Length > 0)
                    {
                        res = res.Substring(1, res.Length - 1);
                        res = res.Substring(0, res.Length - 1);
                    }
                }
                else
                {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }

                res = JSON.Encode(res);
            }
            catch (Exception ex)
            {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 用户获取退货原因
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static string getRefundReason(string openid)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                DataTable dt = BaseSql.getInstance().procedure("p_order_getRefundReason", arr);
                if (dt != null && dt.Rows.Count > 0) {
                    for (int i = 0; i < dt.Rows.Count; i++) {
                        res += dt.Rows[i][0].ToString();
                    }
                } else {
                    res = "{\"sm\":\"没有返回数据\",\"result\":\"-99\"}";
                }

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 通过退单主单号来还回主单号所对应的品种的库存
        /// </summary>
        /// <param name="mainOrderId"></param>
        /// <returns></returns>
        public static string updateInventoryByRefund(string mainOrderId) {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 200, mainOrderId));

                DataTable dt = BaseSql.getInstance().procedure("p_order_updateInventoryByRefund", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 保存退款时，智慧药房返回的信息
        /// </summary>
        /// <param name="mainOrderId"></param>
        /// <param name="returnedMsg"></param>
        /// <returns></returns>
        public static string updateRefundAuditReason(string mainOrderId, string returnedMsg)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, 200, mainOrderId));
                arr.Add(new UProcPara("@returnedMsg", SqlDbType.VarChar, 1000, returnedMsg));

                DataTable dt = BaseSql.getInstance().procedure("p_order_updateRefundAuditReason", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 获取当前的药品中是否含有冷链药品的标识
        /// </summary>
        /// <param name="goodsList"></param>
        /// <returns></returns>
        public static string getGoodsColdFlag(string goodsList)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@goodsList", SqlDbType.VarChar, goodsList.Length * 2, goodsList));

                DataTable dt = BaseSql.getInstance().procedure("p_goods_getGoodsColdFlag", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 将智慧药房返回的价格信息重新同步回云找药系统中
        /// </summary>
        /// <param name="realTimePriceArray"></param>
        /// <returns></returns>
        public static string updatePriceAndInventoryToYzy(string realTimePriceArray)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@realTimePriceArray", SqlDbType.VarChar, realTimePriceArray.Length * 2, realTimePriceArray));

                DataTable dt = BaseSql.getInstance().procedure("p_shop_updatePriceAndInventory", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;

        }

        /// <summary>
        /// 2023-09-20 后禹谦添加：每次用户点击支付时需要判断智慧药房当前库存和价格是否同步
        /// 先从数据库中获取main_order_id下的商品品种
        /// </summary>
        /// <param name="mainOrderId"></param>
        /// <returns></returns>
        public static string getExistOrderContentByMainOrderId(string mainOrderId)
        {
            string res = "";
            try {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@main_order_id", SqlDbType.VarChar, mainOrderId.Length * 2, mainOrderId));

                DataTable dt = BaseSql.getInstance().procedure("p_goods_getExistOrderContentByMainOrderId", arr);
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

                res = JSON.Encode(res);
            } catch (Exception ex) {
                res = "{\"sm\":\"" + "异常:" + ex.Message + "\",\"result\":\"-99\"}";
            }

            return res;
        }
    }
}

