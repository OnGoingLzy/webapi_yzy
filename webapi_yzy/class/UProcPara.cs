using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace webapi_yzy
{
    public class UProcPara
    {
        public string name;   //参数名称
        public SqlDbType type;  //参数类型
        public int size;   //参数大小
        public object value;  //参数值


        public UProcPara(string name, SqlDbType type, int size, object value)
        {
            this.name = name;
            this.type = type;
            this.size = size;
            this.value = value;
        }
    }
}
