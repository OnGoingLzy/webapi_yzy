using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace webapi_yzy
{
    public class DbZhyf
    {
        /// <summary>
        /// 智慧药房接收外部订单并下账
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="orderJson"></param>
        /// <returns></returns>
        public static ResultMessage makeAccountForOuterOrder(string appid,string orderJson)
        {
         
            ResultMessage resultMsg = new ResultMessage();
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@orderJson", SqlDbType.VarChar, orderJson.Length*2, orderJson));
                arr.Add(new UProcPara("@appid", SqlDbType.VarChar, 200, appid));

                DataTable dt = BaseSqlZhyf.getInstance().procedure("p_xsgl_xskd_outerOrder", arr);

                resultMsg.data = dt.Rows[0]["xsmlszh"].ToString();
                resultMsg.code = int.Parse(dt.Rows[0]["receiveOrderFlag"].ToString());
                resultMsg.msg = dt.Rows[0]["receiveOrderFlagMsg"].ToString();
            }
            catch (Exception ex)
            {
                resultMsg.data = "";
                resultMsg.code = -99;
                resultMsg.msg = "Exception异常:" + ex.Message;
            }
            return resultMsg;
        }

        /// <summary>
        /// 智慧药房接收外部订单的退货申请
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="orderJson"></param>
        /// <returns></returns>
        public static ResultMessage getApplyForOuterOrderReturned(string appid, string orderJson)
        {
            ResultMessage resultMsg = new ResultMessage();
            try
            {
                int flag = 0;
                string flagMsg = "";
                string returnedJson = "";
                SqlParameter[] prams = new SqlParameter[5];
                prams[0] = BaseSql.getInstance().createparam("orderJson", SqlDbType.VarChar, orderJson.Length * 2, ParameterDirection.Input, orderJson);
                prams[1] = BaseSql.getInstance().createparam("appid", SqlDbType.VarChar, 200, ParameterDirection.Input, appid); ;
                prams[2] = BaseSql.getInstance().createparam("flag", SqlDbType.Int, 0, ParameterDirection.Output, flag);
                prams[3] = BaseSql.getInstance().createparam("flagMsg", SqlDbType.VarChar, 200, ParameterDirection.Output, flagMsg);
                prams[4] = BaseSql.getInstance().createparam("returnedJson", SqlDbType.VarChar, 8000*2, ParameterDirection.Output, returnedJson);

                BaseSqlZhyf.getInstance().RunProcstr("p_interface_outerOrder_get", prams);
                resultMsg.data = prams[4].Value.ToString();
                resultMsg.code = int.Parse(prams[2].Value.ToString());
                resultMsg.msg = prams[3].Value.ToString();
            }
            catch (Exception ex)
            {
                resultMsg.data = "";
                resultMsg.code = -99;
                resultMsg.msg = "Exception异常:" + ex.Message;
            }
            return resultMsg;
        }

        /// <summary>
        /// 智慧药房接收外部订单已确认收货的状态
        /// </summary>
        /// <param name="orderReceivedJson"></param>
        /// <returns></returns>
        public static ResultMessage getOuterOrderReceived(string orderReceivedJson)
        {
            ResultMessage resultMsg = new ResultMessage();
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@orderReceivedJson", SqlDbType.VarChar, orderReceivedJson.Length*2, orderReceivedJson));

                DataTable dt = BaseSqlZhyf.getInstance().procedure("p_interface_outerOrder_received", arr);

                resultMsg.data = "";
                resultMsg.code = int.Parse(dt.Rows[0]["resultCode"].ToString());
                resultMsg.msg = dt.Rows[0]["resultMsg"].ToString();
            }
            catch (Exception ex)
            {
                resultMsg.data = "";
                resultMsg.code = -99;
                resultMsg.msg = "Exception异常:" + ex.Message;
            }
            return resultMsg;
        }
        /// <summary>
        /// 智慧药房商品当前的库存和价格
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

      
    }
}
