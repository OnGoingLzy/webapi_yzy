using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace webapi_yzy.DAL
{
    public class DbOrder
    {
        /// <summary>
        /// 云找药接收商家已发货的信息
        /// </summary>
        /// <param name="orderReceivedJson"></param>
        /// <returns></returns>
        public static ResultMessage getOrderDelivered(string orderDeliveredJson)
        {
            ResultMessage resultMsg = new ResultMessage();
            try
            {
                ArrayList arr = new ArrayList();
                arr.Add(new UProcPara("@orderDeliveredJson", SqlDbType.VarChar, orderDeliveredJson.Length * 2, orderDeliveredJson));

                DataTable dt = BaseSql.getInstance().procedure("p_webapi_get_zhyf_orderDelivered", arr);

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

    }
}
