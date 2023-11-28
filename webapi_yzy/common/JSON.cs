using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;

namespace webapi_yzy
{
    public class JSON
    {
        public static string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

        //序列化对象为JSON格式
        public static string Encode(object o)
        {
            string json_str = "";

            if(o == null || o.ToString() == "null") return null;

            if (o != null && (o.GetType() == typeof(String) || o.GetType() == typeof(string)))
            {
                return o.ToString();
            }
            IsoDateTimeConverter dt = new IsoDateTimeConverter();
            dt.DateTimeFormat = DateTimeFormat;
            json_str= JsonConvert.SerializeObject(o, dt);
            return json_str;
        }

        //序列化字符串为JSON格式，以在前台显示
        public static string Encode_str(string sm)
        {
            if (sm == null || sm.ToString() == "null") return null;

            if (sm != null && (sm.GetType() == typeof(String) || sm.GetType() == typeof(string)))
            {
                sm = "[{ id: \"0\", name: \"" + sm + "\"}]";
            }

            return sm;
        }

        //序列化字符串为JSON格式，以在前台显示
        public static string Encode_restr(string re ,string sm)
        {
            if (sm == null || sm.ToString() == "null") return null;

            if (sm != null && (sm.GetType() == typeof(String) || sm.GetType() == typeof(string)))
            {
                sm = "[{ re: \"" + re + "\", sm: \"" + sm + "\"}]"; 
            }

            return sm;
        }

        //序列化字符串为JSON格式，以在前台显示
        public static string Encode_str(int resession)
        {
            String re_str = "";

            if (resession == null || resession.ToString() == "null") return null;

            if (resession != null && (resession.GetType() == typeof(String) || resession.GetType() == typeof(string)))
            {
                re_str = "[{ id: \"" + resession + "\", name: \"登录已超时\"}]";
            }

            return re_str;
        }


        //反序列化对象为JSON格式并存入对象
        public static object Decode(string json)
        {
            if (String.IsNullOrEmpty(json)) return "";
            object o = JsonConvert.DeserializeObject(json);
            if (o.GetType() == typeof(String) || o.GetType() == typeof(string))
            {
                o = JsonConvert.DeserializeObject(o.ToString());
            }
            object v = toObject(o);
            return v;
        }

        public static object Decode(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        private static object toObject(object o)
        {
            if (o == null) return null;

            if (o.GetType() == typeof(string))
            {
                //判断是否符合2010-09-02T10:00:00的格式
                string s = o.ToString();
                if (s.Length == 19 && s[10] == 'T' && s[4] == '-' && s[13] == ':')
                {
                    o = System.Convert.ToDateTime(o);
                }
            }
            else if (o is JObject)
            {
                JObject jo = o as JObject;

                Hashtable h = new Hashtable();

                foreach (KeyValuePair<string, JToken> entry in jo)
                {
                    h[entry.Key] = toObject(entry.Value);
                }

                o = h;
            }
            else if (o is IList)
            {

                ArrayList list = new ArrayList();
                list.AddRange((o as IList));
                int i = 0, l = list.Count;
                for (; i < l; i++)
                {
                    list[i] = toObject(list[i]);
                }
                o = list;

            }
            else if (typeof(JValue) == o.GetType())
            {
                JValue v = (JValue)o;
                o = toObject(v.Value);
            }
            else
            {
            }
            return o;
        }

        internal static object Decode(Task<string> task)
        {
            throw new NotImplementedException();
        }
    }
}
