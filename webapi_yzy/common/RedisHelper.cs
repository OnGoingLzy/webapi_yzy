using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace webapi_yzy
{
    public class RedisHelper {
        //正式
        //private static readonly string redisConnectionString = "172.16.20.3:6379,password=yygs_2023";
        //测试
        private static readonly string redisConnectionString = "127.0.0.1:6379";

        private static readonly object _lock = new object();
        private static ConnectionMultiplexer _connection;

        public static ConnectionMultiplexer Connection {
            get {
                try
                {
                    if (_connection == null || !_connection.IsConnected) {
                        lock (_lock) {
                            if (_connection == null || !_connection.IsConnected) {
                                _connection = ConnectionMultiplexer.Connect(redisConnectionString);
                            }
                        }
                    }
                } catch (Exception e) {
                    Console.WriteLine(e);
                    return null;
                }
                return _connection;
            }
        }

        // String data type operations
        public static async Task<bool> StringSetAsync(string key, string value) {
            var db = Connection.GetDatabase();
            return await db.StringSetAsync(key, value);
        }

        public static async Task<string> StringGetAsync(string key) {
            var db = Connection.GetDatabase();
            return await db.StringGetAsync(key);
        }

        public static async Task<bool> StringDeleteAsync(string key) {
            var db = Connection.GetDatabase();
            return await db.KeyDeleteAsync(key);
        }

        // Hash data type operations
        public static async Task<bool> HashSetAsync(string key, string hashField, string value) {
            var db = Connection.GetDatabase();
            return await db.HashSetAsync(key, hashField, value);
        }

        public static async Task<string> HashGetAsync(string key, string hashField) {
            var db = Connection.GetDatabase();
            return await db.HashGetAsync(key, hashField);
        }

        public static async Task<bool> HashDeleteAsync(string key, string hashField) {
            var db = Connection.GetDatabase();
            return await db.HashDeleteAsync(key, hashField);
        }

        public static async Task<Dictionary<string, string>> HashGetAllAsync(string key) {
            var db = Connection.GetDatabase();
            var hashEntries = await db.HashGetAllAsync(key);
            var result = new Dictionary<string, string>();

            foreach (var entry in hashEntries) {
                result.Add(entry.Name, entry.Value);
            }

            return result;
        }
    }
}