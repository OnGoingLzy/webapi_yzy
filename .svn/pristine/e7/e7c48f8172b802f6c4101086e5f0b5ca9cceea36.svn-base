using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using webapi_yzy.Model;

namespace webapi_yzy.Service
{
    //搜索服务
    public class searchService
    {
        //获取药品信息
        public List<Drugs> GetDrugsList(string spcxm)
        {
            using (var client = new HttpClient())
            {
                // 发送请求...
                var res = client.GetAsync("http://172.16.123.41:7539/api/select?searchName=" + spcxm);
                //var res = client.GetAsync("http://localhost:8080/api/select?searchName=" + spcxm);
                var result = res.Result;
                if (result.IsSuccessStatusCode)
                {
                    var content = res.Result.Content.ReadAsStringAsync().Result;
                    List<Drugs> result2 = JsonConvert.DeserializeObject<apiDrugsListObj>(content).drugList;
                    return result2;
                }
                else
                {
                    return null;
                }

            }


        }
    }
}
