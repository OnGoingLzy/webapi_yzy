﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace webapi_yzy
{
    public class Token
    {
        private static readonly ConnectionMultiplexer connection = RedisHelper.Connection;
        private static readonly IDatabase redis = connection.GetDatabase();
        public static ResultMessage getTokenResult(string appid, string method, string timestamp, string nonce, string sign, string body)
        {
            try
            {
                //必填参数检查
                if (appid == "" || method == "" || sign == "" || timestamp == "" || nonce == ""
                    || appid == null || method == null || sign == null || timestamp == null || nonce == null)
                {
                    return new ResultMessage { msg = "缺少必填", code = -1 };
                }
                //身份验证
                string appsecret = DbOperator.getAppsecret(appid);
                if (appsecret == "")
                {
                    return new ResultMessage { msg = "身份验证失败", code = -1 };
                }
                //时间戳
                if (!checkTimestamp(timestamp))
                {
                    return new ResultMessage { msg = "时间戳无效", code = -1 };
                }
                //篡改验证（签名）
                var sign_service = Signature.createSignature(body, method, appid, appsecret, timestamp, nonce);
                if (sign != sign_service)
                {
                    return new ResultMessage { msg = "签名失败", code = -1 };
                }
                //Redis重放验证
                if (CheckSignature(sign_service)) {
                    return new ResultMessage { msg = "请求重复", code = -1 };
                }

                return new ResultMessage { msg = "验证成功", code = 99 };
            }
            catch (Exception ex)
            {
                return new ResultMessage { msg = "exception:" + ex.Message, code = -1 };
            }
        }

        /// <summary>  
        /// 获取Unix时间戳 
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        private static long convertDateTimeToInt()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }
        /// <summary>
        /// 判断当前的签名是否存在在Redis服务器中，防止重放攻击
        /// </summary>
        /// <param name="signature">判断是否重复的签名</param>
        /// <returns></returns>
        public static bool CheckSignature(string signature) {
            //5分钟的过期时间
            TimeSpan expiration = new TimeSpan(0, 5, 0);

            if (redis.KeyExists(signature)) {
                //如果已经存在这个键，那么就可能是重放攻击
                return true;
            }

            //如果不存在这个键，那么就添加它并设置过期时间
            redis.StringSet(signature, "", expiration);

            return false;
        }
        /// <summary>
        /// 判断时间戳是否在5分钟以内
        /// </summary>
        /// <param name="timestampString">时间戳字符串,单位是毫秒</param>
        /// <returns></returns>
        public static bool checkTimestamp(string timestampString)
        {
            long timestamp = long.Parse(timestampString);
            long timestamp_now = ConvertDateTimeToInt();

            //DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            //DateTimeOffset now = DateTimeOffset.Now;

            //TimeSpan range = now - offset;
            //return range.TotalMinutes <= 5;

            return (timestamp_now - timestamp) <= 5 * 60 * 1000;




        }
        /// <summary>  
        /// 获取Unix时间戳 
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        private static long ConvertDateTimeToInt()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }
    }
    }
