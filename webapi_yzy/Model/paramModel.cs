﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webapi_yzy.Model
{
    public class pharmacyObj
    {
        public string code { get; set; }
        public string msg { get; set; }
        public List<pharmacy> list { get; set; }
    }

    public class pharmacy
    {
        public string pharmacyCode { get; set; }
        public string pharmacyName { get; set; }
        public string address { get; set; }
        public string distance { get; set; }

        public Position position;
    }

    //位置
    public class Position
    {
        public string longitude { get; set; }
        public string latitude { get; set; }
    }

    //商品查询结果
    public class SearchSpResultObj
    {
        public string code { get; set; }
        public string msg { get; set; }
        public string datalist { get; set; }
    }

    //查询结果
    public class BaseResultObj
    {
        public string code { get; set; }
        public string msg { get; set; }
    }
    //客户是否能够退款
    public class IsRefundObj
    {
        public string code { get; set; }
        public string msg { get; set; }
        public int isrefund { get; set; }
        public string refundInfo { get; set; }
    }
    //回置已退款状态
    public class TkxxObj
    {
        public string code { get; set; }
        public string clzt { get; set; }
        public string msg { get; set; }
    }
    //带data的结果
    public class ResultObj
    {
        public string code { get; set; }
        public string msg { get; set; }
        public string data { get; set; }
    }
    //GetMdInfo结果
    public class GetMdInfo_tencentResult
    {
        public string code { get; set; }
        public string message { get; set; }
        public object infoArray { get; set; }
        public int total { get; set; }
    }
    //InsertOrder_tencent结果
    public class InsertOrder_tencentResult
    {
        public string code { get; set; }
        public string message { get; set; }
        public string orgTradeNo { get; set; }
    }


    public class SearchSpmlResultObj
    {
        public string code { get; set; }
        public string msg { get; set; }
        public string dataList { get; set; }
    }

    //GetOrderState结果
    public class GetOrderStateResult
    {
        public string code { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        //订单状态
        //NOTAUTH 未授权
        //NOTPAY 未支付
        //PROCESSING 处理中
        //ERPFAIL 药店系统处理错误
        //PAYFAIL 支付失败
        //CANCEL 已取消（撤销订单）
        //NEEDCASH 现金待支付
        //CLOSED 订单已关闭(超时关闭)
        //SUCCESS 已完成（支付成功）
        //REFUND 订单退款
        public string medTransId { get; set; }//微信统一下单返回的订单号
        public decimal totalFee { get; set; }//总金额(单位分)
        public decimal cashFee { get; set; }//现金金额
        public decimal cashReducedFee { get; set; }//现金优惠金额
        public decimal insuranceFee { get; set; }//医保支付金额
        public string errInfo { get; set; }//异常信息
        public string insuranceDetail { get; set; }//医保返回结构体
        public decimal insuranceSelfFee { get; set; }//医保个人账户支付金额(单位分)
        public decimal insuranceFundFee { get; set; }//医保统筹基金支付金额（单位分）
        public decimal insuranceOtherFee { get; set; } //医保其它支付金额（单位分）

    }

    //2023/5/9添加
    public class drugsListObj
    {
        public String spcxm { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public List<Drugs> drugsList { get; set; }
    }
    public class apiDrugsListObj
    {
        public string result { get; set; }
        public string msg { get; set; }
        public List<Drugs> drugList { get; set; }
    }
    public class Drugs
    {
        public int no { get; set; }
        public String spdm { get; set; }
        public double similarity { get; set; }
    }

    //2023/5/12添加
    public class IdVerifyObj
    {
        public string lxdh { get; set; }
        public string realName { get; set; }
        public string idCard { get; set; }
        public string isOther { get; set; }

        public string gx { get; set; }

    }
    public class IdVerifyResultObj
    {
        public string code { get; set; }
        public string msg { get; set; }
        public string result { get; set; }
        public string sm { get; set; }
    }

    //购买的商品列表
    public class purchaseGoods
    {
        public string id { get; set; }

        public string goodsId { get; set; }

        public int num { get; set; }

        public double price { get; set; }

        public string detail_order_id { get; set; }

    }

    //退款参数对象
    public class refundObj
    {
        public string wxpay_order_id{ get;set; } //原微信支付订单号
        public string out_order_id { get; set; } //原商户支付订单号

        public string reason { get; set; } //退款原因

        public decimal refundAmount { get; set; } //退款金额 

        public decimal origin_totalPrice { get; set; } //原订单总金额


        // 以下若非指定退款某商品可不填
        public string goodsId { get; set; } //商品id

        public string goodsName { get; set; } //商品名

        public decimal goodsPrice { get; set; } //商品单价 

        public int refundNum { get; set; } //退货商品数量 只能是整型


    }

    public class costBreakDown
    {
        public string skfs { get; set; }
        public decimal jes { get; set; }
    } 


    public class yinhaiInitParam
    {
        public string dppSyscode { get; set; }
        public string acssToken { get; set; }
    }

    //银海接口调用 订单查询输入对象
    public class yinhaiQueryOrderObj
    {
        public Data data { get; set; }           // 节点data
        public class Data
        {
           public string plaf_ord_no { get; set; }//平台订单号 字符型 50 不能同时为空 2
            public string dpp_ord_no { get; set; } //支付平台订单号 字符型 50
            public string chnl_ord_no { get; set; }  //渠道订单号 字符型 50
        }
    }

    //银海接口调用 订单退款查询输入对象
    public class yinhaiQueryRefundOrderObj
    {
        public Data data { get; set; }           // 节点data
        public class Data
        {                                       
           public string plaf_ord_no            { get; set; }  // 平台订单号 字符型 50
           public string dpp_ord_no             { get; set; } // 支付平台订单号 字符型 50
           public string chnl_ord_no            { get; set; } // 渠道订单号 字符型 50
           public string acss_sys_refd_ord_no   { get; set; } //     接入系统退款单号 字符型 5
        }
    }

    //银海接口调用 同步accessToken接口所需参数
    public class yinhaiSyncAcssToken
    {
        public Data data { get; set; }           // 节点data
        public class Data
        {
            public string chnl_app_id { get; set; }  // 渠道应用 ID
            public string mert_app_type { get; set; } // 商户应用类别 
            public string acss_token { get; set; } // 
            public int token_expy { get; set; } //     位：秒访问令牌凭证有效时间
        }
    }

    //银海接口调用 


    //银海输入报文对象
    public class yinhaiInputObject
    {
        public string trns_no { get; set; }     //交易编号 字符型 8 Y
        public string sender_trns_sn { get; set; }     //发送方交易流水号 字符型 40 Y定点医药机构编号(12)+时间(14)+顺序号(4)时间格式：yyyyMMddHHmmss
        public string trns_time        { get; set; } //交易时间 日期时间型 Y
        public string acss_sys_code    { get; set; }     //接入系统编码 字符型 50 Y
        public string medins_code      { get; set; } //医药机构编号 字符型 20 Y
        public string medins_name      { get; set; } //医药机构名称 字符型 100 Y
        public string medins_brch_code { get; set; }     //医药机构院区编号 字符型 20
        public string opter_type       { get; set; } //经办人类别 字符型 3 Y 参照字典“OPTER_TYPE”
        public string opter_id         { get; set; } //经办人 ID 字符型 20 Y
        public string opter_name       { get; set; } //经办人姓名 字符型 50 Y
        public string prmt_psn_no      { get; set; } //推广人员编号 字符型 20
        public string acss_token       { get; set; } //访问令牌 字符型 50 用户登录交易出参
        public string ency_type        { get; set; } //加密类型 字符型 10 Y NONE:不加密；RSA:RSA 加密
        public string sign_type        { get; set; } //签名类型 字符型 10 Y NONE:不签名；MD5:MD5 签名
        public string sign_data        { get; set; } //签名数据 字符型 50 Y 详见“接口签名规则”
        public string timestmp         { get; set; } //时间戳 时间戳 Y 单位（毫秒），10 分钟内有效
        public string input            { get; set; } //交易输入 字符型 40000 Y 根据“参数加解密规则”加密

    }


    //银海创建付款单响应对象
    public class responseYinhaiObject
    {
        public yinhaiCreateOrderObject yinhaiCreateOrderObject { get; set; }
        public string acssToken { get; set; }

        public string medins_code { get; set; }

        public string medins_name { get; set; }

        public string dppSyscode { get; set; }

       
        public int flag { get; set; }

        public string sm { get; set; }
    }

    //银海创建退款单响应对象
    public class responseYinhaiRefundObject
    {
        public yinhaiCreateRefundOrderObject yinhaiCreateRefundOrderObject { get; set; }
        public string acssToken { get; set; }

        public string medins_code { get; set; }

        public string medins_name { get; set; }

        public string dppSyscode { get; set; }

        public int flag { get; set; }

        public string sm { get; set; }
    }
    public class yinhaiCreateRefundOrderObject
    {
  
        public string plaf_ord_no          {get;set;}// 平台订单号 字符型 50 Y
        public string dpp_ord_no           {get;set;}// 支付平台订单号 字符型 50
        public string biz_ord_no           {get;set;}//     业务订单号 字符型 50 Y
        public string acss_sys_refd_ord_no {get;set;}// 接入系统退款单号 字符型 50 调用方唯一
        public decimal refd_sumamt          {get;set;}// 退款金额 数值型 16,2 Y
        public decimal cash_add_refd_amt    {get;set;}//     额外现金退款金额 数值型 16,2
        public string refd_rea             {get;set;}// 退款原因 字符型 100
        public string acss_token           {get;set;}// 渠道访问令牌 字符型 200
        public string chnl_user_id         {get;set;}// 渠道用户 id 字符型 50 用户在申请权限的公众号/小程序的 openid/userId
        public string return_url          {get;set;}// 前端回跳地址 字符型 200 Y
    }
    public class yinhaiCreateRefundOrderObject2
    {
        public Data data { get; set; }           // 节点data
        public class Data
        {
            public string plaf_ord_no { get; set; }// 平台订单号 字符型 50 Y
            public string dpp_ord_no { get; set; }// 支付平台订单号 字符型 50
            public string biz_ord_no { get; set; }//     业务订单号 字符型 50 Y
            public string acss_sys_refd_ord_no { get; set; }// 接入系统退款单号 字符型 50 调用方唯一
            public decimal refd_sumamt { get; set; }// 退款金额 数值型 16,2 Y
            public string cash_add_refd_amt { get; set; }//     额外现金退款金额 数值型 16,2
            public string refd_rea { get; set; }// 退款原因 字符型 100
            public string acss_token { get; set; }// 渠道访问令牌 字符型 200
            public string chnl_user_id { get; set; }// 渠道用户 id 字符型 50 用户在申请权限的公众号/小程序的 openid/userId
            public string return_url { get; set; }// 前端回跳地址 字符型 200 Y
        }
    }

    //银海创建订单 input内容对象
    public class yinhaiCreateOrderObject
    {
        public string  ord_biz_type { get; set; }   //订单业务类型 字符型 6 Y Y
        public string  acss_sys_ord_no  { get; set; }   //接入系统订单号 字符型 50 Y 调用方唯一订单号
        public decimal  ord_sumamt      { get; set; }   //订单总金额 数值型 16,2 Y
        public string  ord_ttl          { get; set; }   //订单标题 字符型 20
        public string  ord_dscr         { get; set; }   //订单描述 字符型 100

        public decimal cash_add_amt { get; set; } //额外现金总金额

        public string cash_add_dscr { get; set; } //额外现金说明

        public  cash_add_list[] cash_add_list { get; set; }


        public yinhaiCreateOrderObject_ordBizPara ord_biz_para     { get; set; } //订单业务参数 JSON 对象 Y
    }

    public class cash_add_list
    {
       public string cash_add_fee_code    { get; set; }   //额外现金费用代码 字符型 30 N cash_add_fee_code
       public string cash_add_fee_name    { get; set; }   //额外现金费用名称 字符型 100 Y cash_add_fee_name
       public string cash_add_fee_type    { get; set; }   //额外现金费用类型 字符型 6 Y Y cash_add_fee_type 01配送费 02包装费 03非医保费 99其他
       public decimal cash_add_amt         { get; set; }   //额外现金金额 数值型 16,2 Y cash_add_amt  费用金额
       public string memo                 { get; set; }   //备注 字符型 100 N
    }

    public class yinhaiCreateOrderObject_ordBizPara
    {
        public string mdtrt_sid { get; set; }      //就诊流水 ID 字符型 30 Y 单次就诊唯一
        public string mdtrt_sn         { get; set; }     //就诊流水号 字符型 30 Y
        public string otp_no           { get; set; }     //门诊号 字符型 30
        public string doc_type         { get; set; }     //单据类型 字符型 3 Y Y
        public string setl_docno       { get; set; }     //结算单据号 字符型 30 Y 机构内唯一
        public string chrg_type        { get; set; }     //收费类别 字符型 3 Y 自费、医保
        public string psn_name         { get; set; }     //人员姓名 字符型 50 Y
        public string psn_cert_type    { get; set; }     //人员证件类型 字符型 6 Y Y
        public string certno           { get; set; }     //证件号码 字符型 50 Y
        public string  idcard          { get; set; }     //身份证号码 字符型 50 有身份证时必填
        public string brdy             { get; set; }         //出生日期 日期型 Y
        public string gend             { get; set; }     //性别 字符型 3 Y Y
        public string age              { get; set; }     //年龄 数值型 4,1 Y
        public string mob              { get; set; }     //手机号码 字符型 20 有则必传
        public string tel              { get; set; }     //联系电话 字符型 20
        public string med_type         { get; set; }     //医疗类别 字符型 6 Y Y 定点药店购药
        public string mdtrt_time       { get; set; }     //就诊时间 日期时间型 Y
        public string dept_code        { get; set; }     //科室编码 字符型 50 门诊不能为空
        public string dept_name        { get; set; }     //科室名称 字符型 100 门诊不能为空
        public string caty             { get; set; }        //医保科别 字符型 6 Y 为空默认从业务系统获取
        public string dr_id            { get; set; }     //医生 ID 字符型 50 门诊不能为空
        public string dr_name          { get; set; }     //医生姓名 字符型 50 门诊不能为空
        public string hi_dr_code       { get; set; }     //医保医师编码 字符型 50 为空默认从业务系统获取
        public string dise_codg        { get; set; }     //病种编码 字符型 30 医保结算慢特病需传入
        public string dise_name        { get; set; }     //病种名称 字符型 200 医保结算慢特病需传入
        public string main_cond_dscr   { get; set; }     //主要病情描述 字符型 1000

        public decimal sumfee { get; set; }

        public string dise_list        { get; set; }     //诊断信息列表 JSON 数组 门诊不能为空
        public yinhaiCreateOrderObject_ordBizPara_feeDetlList[] fee_detl_list    { get; set; }     //费用明细列表 JSON 数组 Y
    }

    //费用明细表
    public class yinhaiCreateOrderObject_ordBizPara_feeDetlList
    {
        public string feedetl_sn { get; set; }              //    费用明细流水号 字符型 30 Y 单次就诊内唯一
        public string chrg_bchno           { get; set; }     //        收费批次号 字符型 30 Y同一收费批次号病种编号必须一致
        public string rx_circ_flag         { get; set; }     //        外购处方标志 字符型 3 Y Y1：电子处方 ，0 不是电子处方，默认 0
        public string rxno                 { get; set; }     //    处方号 字符型 30外购处方时，传入外购处方的处方号；否则传入医药机构处方号
        public string medins_list_codg     { get; set; }     //    医药机构目录编码 字符型 50 Y
        public string medins_list_name     { get; set; }     //    医药机构目录名称 字符型 100 Y
        public string med_list_codg        { get; set; }     //     医疗目录编码 字符型 50 医保目录编码
        public string med_list_name        { get; set; }     //     医疗目录名称 字符型 100 医保目录名称
        public string tcmdrug_type         { get; set; }     //    中药类别 字符型 3 Y 配方颗粒、中药饮片
        public string comb_no              { get; set; }     //         组套编号 字符型 3同一组医嘱需一致，不同医嘱按顺序号数值编号
        public string prodentp_name        { get; set; }     //     生产企业名称 字符型 100 有则必传
        public string orplc                { get; set; }     //        产地 字符型 100 有则必传
        public string medins_brand         { get; set; }     //    国药准字号 字符型 100
        public string reg_fil_no           { get; set; }     //    注册备案号 字符型 100
        public string prodplac_type        { get; set; }     //     生产地类别 字符型 3 Y 材料费必传
        public string pacmatl_name         { get; set; }     //    包装材质名称 字符型 100
        public string dosform_name         { get; set; }     //    剂型名称 字符型 100 有则必传
        public string dosunt               { get; set; }     //        剂量单位 字符型 10
        public string spec                 { get; set; }     //    规格 字符型 30 Y
        public string unt_name             { get; set; }     //    单位 字符型 10
        public decimal cnt                  { get; set; }     //    数量 数值型 16,4 Y
        public decimal pric                 { get; set; }     //     单价 数值型 16,6 Y
        public decimal det_item_fee_sumamt  { get; set; }     //    明细项目费用总额 数值型 16,2 Y
        public decimal act_purc_pric        { get; set; }     //    实际网上采购价 数值型 16,6 有则必传
        public string fee_ocur_time        { get; set; }     //    费用发生时间 日期时间型 Y
        public string used_mtd             { get; set; }     //    使用方法 字符型 100 用法
        public string tcmdrug_used_way     { get; set; }     //        中药使用方式 字符型 3 Y
        public decimal sin_dos              { get; set; }     //    单次剂量 数值型 16,2
        public string sin_dos_dscr         { get; set; }     //    单次剂量描述 字符型 100
        public string used_frqu_dscr       { get; set; }     //        使用频次描述 字符型 100
        public int prd_days             { get; set; }     //    周期天数 数值型 4,2
        public string medc_way_dscr        { get; set; }     //    用药途径描述 字符型 100
        public string bilg_dept_codg       { get; set; }     //        开单科室编码 字符型 30 门诊不能为空
        public string bilg_dept_name       { get; set; }     //        开单科室名称 字符型 100 门诊不能为空
        public string bilg_dr_code         { get; set; }     //    开单医生代码 字符型 20 门诊不能为空
        public string bilg_dr_name         { get; set; }     //    开单医生姓名 字符型 50 门诊不能为空
        public string bilg_dr_hi_code      { get; set; }     //    开单医生医保编码 字符型 50
        public string acord_dept_codg      { get; set; }     //    受单科室编码 字符型 30
        public string acord_dept_name      { get; set; }     //    受单科室名称 字符型 100
        public string acord_dr_code        { get; set; }     //    受单医生编码 字符型 30
        public string acord_dr_name        { get; set; }     //    受单医生姓名 字符型 50
        public string acord_dr_hi_code     { get; set; }     //    受单医生医保编码 字符型 50
        public string hosp_appr_flag { get; set; }     //    医院审批标志 字符型 3 Y
    }


}