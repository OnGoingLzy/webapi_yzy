using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace webapi_yzy.common {
    public class Md5Utils {
        public static string Encode(string arg) {
            if (arg == null) {
                arg = "";
            }

            using (var md5 = MD5.Create()) {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(arg));
                return BitConverter.ToString(result).Replace("-", "").ToLower();
            }
        }
    }
}
