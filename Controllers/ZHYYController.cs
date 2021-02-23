using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using ServiceReference1;

namespace Demo.Core.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ZHYYController : ControllerBase
    {
        private readonly ILogger<ZHYYController> _logger;

        public IConfiguration Configuration { get; }

        //private readonly SqlServiceSoap sqlService;
        private string SqlStr=string.Empty;
        public ZHYYController(ILogger<ZHYYController> logger,IConfiguration  configuration)
        {
            this._logger = logger;
            this.Configuration = configuration;
            this.hisSql= Configuration.GetConnectionString("Default").ToString();
            //   this.sqlService = sqlService;
        }
        private string hisSql = "";
        #region 手机短信接口
        [HttpPost ]
        public IActionResult GetSmsInfo([FromBody] dynamic dynamic)
        {
            JObject j = Methods.JhDbManager.dynamicToJObject(dynamic);
            string phone = j.GetValue("phone",StringComparison.OrdinalIgnoreCase).ToString();
            string content ="您收到的验证码是："+ GetRandomStr(5);
            string key = phone + Guid.NewGuid().ToString().Replace("-","");
            Methods.MomeryCacheService.Set(key, content);
            string result = Methods.JhDbManager.getSMSinfo(content,phone);
            JObject jObject = JsonConvert.DeserializeObject<JObject>(result).GetValue("SendSmsResponse").ToObject<JObject>();
            if (jObject.GetValue("success").ToString().ToUpper()=="FALSE")
            {
                return new ObjectResult(new { success = jObject.GetValue("success").ToString(), rspcod = jObject.GetValue("rspcod").ToString(), msgGroup = jObject.GetValue("msgGroup").ToString()  });
            }
            return new ObjectResult(new { success = jObject.GetValue("success").ToString(), rspcod = jObject.GetValue("rspcod").ToString(), msgGroup = jObject.GetValue("msgGroup").ToString(),key=key});
        }
        [HttpGet]
        public IActionResult GetsmsResult(string key)
        {
            string smsresult = string.Empty;
            try
            {
                 smsresult = Methods.MomeryCacheService.Read(key).ToString();
            }
            catch(Exception ex)
            {
                return new JsonResult(new { code = 500, data = "相同的key只能请求一次",msg=ex.Message});
            }
            if (string.IsNullOrEmpty(smsresult))
            {
                return new JsonResult(new { code = 500, data = "该短信消息已过期" });
            }
            else
            {
                return new JsonResult(new { code = 200, data = "获取成功",msg=smsresult });
            }

        }
        
        private  string GetRandomStr(int num)
        {
            Random random = new Random();
            string contentarr = "1,2,3,4,5,6,7,8,9,0";
            string result = "";
            for (int i = 0; i < num; i++)
            {
                result += contentarr.Split(',')[random.Next(contentarr.Split(',').Length)];
            }
            return result;
        }
        #endregion
        [HttpGet]
        public IActionResult GetjcHeader(string idcard)
        {
            //JObject res = Methods.JhDbManager.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n检查报告的入参idcard" + idcard);
            string patientno = string.Empty;//DJLSH
            SqlServiceSoapClient client = new SqlServiceSoapClient(SqlServiceSoapClient.EndpointConfiguration.SqlServiceSoap);
            string res = client.GetResultAsync(idcard).Result.Body.GetResultResult ;
            List<JObject> jobj = Methods.JhDbManager.GetTbyString<List<JObject>>(res);
            if (jobj==null)
            {
                goto Continue;
            }
            if (jobj.Count==0)
            {
                goto Continue;
            }
            for (int i = 0; i < jobj.Count; i++)
            {
                var item = jobj.ElementAtOrDefault(i);
                if (item.ContainsKey("DJLSH"))
                {
                   
                    if (i==jobj.Count-1)
                    {
                        patientno += "'"+ item.GetValue("DJLSH", StringComparison.OrdinalIgnoreCase).ToString() + "'";
                    }
                    else
                    {
                        patientno += "'"+ item.GetValue("DJLSH", StringComparison.OrdinalIgnoreCase).ToString() + "'" + ",";
                    }
                }
                
            }
           Continue:
            if (string.IsNullOrEmpty(patientno))
            {
                string Sql = $"select p.card_no from com_patientinfo p where p.idenno='{idcard}' and p.is_valid='1'";
                patientno = "'"+Methods.JhDbManager.getSqlReturnOne(Sql,hisSql)+"'";
            }

            string sql = $"select  r.lab_apply_no \"id\", r.paritemname \"title\",r.micro_flag \"iswsw\",r.patient_id \"patientId\",\'' \"patientName\",r.report_date_time \"sendTime\",\'枝江市人民医院' \"hospitalName\" from v_lis_report_zhyy r where   (r.patient_id in({patientno}) and r.file_visit_type in ('1','3')) or (r.patient_id in ({patientno})  and r.file_visit_type in ('1','3')) and r.is_valid = '1'";
            var dt = Methods.JhDbManager.QuerySql(sql, this.Configuration.GetConnectionString("Lis").ToString());
            ArrayList arr = Methods.JhDbManager.getJObject(dt);
            if (arr == null)
            {
                return new JsonResult(new { msg = "没有找到如何检测报告的信息！", data = new { }, code = 404 });
            }
            if (arr.Count == 0  )
            {
                return new JsonResult(new { msg = "没有找到如何检测报告的信息！", data = new { }, code = 404 });
            }
            return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
        } 
        [HttpPost]
        public IActionResult GetjcDetail([FromBody] dynamic dy)
        {
            JObject res = Methods.JhDbManager.dynamicToJObject(dy);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\nLIS接口的入参" + res.ToString());
            string id = string.Empty;
            string iswsw = string.Empty;
            try
            {
                id = res.GetValue("id").ToString();
                iswsw = res.GetValue("iswsw").ToString();
            }
            catch
            {
                return new JsonResult(new { msg = "你输入的参数有误！", data = "参数错误", code = 403 });
            }
            string sql = string.Empty;
            if (iswsw == "0")
            {
                sql = $"select p.lab_item_name \"item\",p.lab_item_name \"itemName\", p.lab_item_sname \"name\", p.result  \"value\",p.result_range \"reference\", p.units \"unit\", p.status \"status\", ' '   \"remark\"  from v_report_item_zhyy p where p.lis_apply_no = '{id}'";
            }
            else if (iswsw == "1")
            {
                sql = $"select  m.lab_item_name \"item\",m.micro_name \"itemName\",m.anti_name \"name\",m.susquan  \"value\",m.ref_rang \"reference\",'' \"unit\",m.suscept \"status\",m.desc_name \"remark\"  from v_report_micro_zhyy m where m.lab_apply_no = '{id}'";
            }
            else
            {
                return new JsonResult(new { msg = "iswsw参数找不到对应值！", data = "参数错误", code = 403 });
            }
            var dt = Methods.JhDbManager.QuerySql(sql, this.Configuration.GetConnectionString("Lis").ToString());
            ArrayList arr = Methods.JhDbManager.getJObject(dt);
            if (arr==null)
            {
                return new JsonResult(new { msg = "没有找到如何检测报告详情的信息！", data = "查询结果为空", code = 404 });
            }
            if (arr.Count == 0 )
            {
                return new JsonResult(new { msg = "没有找到如何检测报告详情的信息！", data = "查询结果为空", code = 404 });
            }
            if (iswsw == "1")
            {
                IEnumerable<JObject> j = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(arr));
                string[] ids = j.Select(s => s.GetValue("itemName").ToString()).Distinct().ToArray();
                List<object> objects = new List<object>();
                foreach (string item in ids)
                {
                    objects.Add(new JsonResult(new { item = j.FirstOrDefault(u => u.GetValue("itemName").ToString() == item).GetValue("item").ToString(), itemName = item.ToString(), details = (from JObject x in j where x.GetValue("itemName").ToString() == item select x).Select(s => new { name = s.GetValue("name").ToString(), value = s.GetValue("value").ToString(), reference = s.GetValue("reference").ToString(), unit = s.GetValue("unit").ToString(), status = s.GetValue("status").ToString(), remark = s.GetValue("remark").ToString() }) }).Value);
                }
                return new JsonResult(new { msg = "查询成功！", data = objects, code = 200 });
            }
            else
            {
                return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
            }

        }


        [HttpGet]
        public IActionResult getjc(string idcard)
        {
            var jcres = (JsonResult)this.GetjcHeader(idcard);
            JObject j = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(jcres.Value));
            if (j.GetValue("code",StringComparison.OrdinalIgnoreCase).ToString()=="500"|| j.GetValue("code", StringComparison.OrdinalIgnoreCase).ToString()=="404")
            {
                return jcres;
            }

            List<Dictionary<string,object>> jlist = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(j.GetValue("data", StringComparison.OrdinalIgnoreCase).ToString());
            for (int i = 0; i < jlist.Count; i++)
            {
                Dictionary<string, object> jobject = jlist[i];
                string id = jobject.GetValueOrDefault("id").ToString();
                var result = (JsonResult)this.GetjcDetail(Methods.JhDbManager.GetDynamicByString("{\"id\":\"" + id + "\",\"iswsw\":\"0\"}"));
                JObject j2 = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(result.Value));
                if (j2.GetValue("code", StringComparison.OrdinalIgnoreCase).ToString() == "500" || j2.GetValue("code", StringComparison.OrdinalIgnoreCase).ToString() == "404")
                {
                    return jcres;
                }
                List<JObject> jlist2 = JsonConvert.DeserializeObject<List<JObject>>(j2.GetValue("data", StringComparison.OrdinalIgnoreCase).ToString());
                List<string> results = new List<string>();
                foreach (var item in jlist2)
                {
                    results.Add(Methods.JhDbManager.GetJobjVal(item, "value"));
                }
                jobject.Add("values",results);
            }
             return new JsonResult(new { msg = "查询成功！", data = jlist, code = 200 });
        }

        #region 绑定就诊卡
        /// <summary>
        /// 绑定就诊卡
        /// </summary>
        /// <param name="dynamic"></param>
        /// <returns></returns> 
        [HttpPost]
        public IActionResult bindCard([FromBody] dynamic dynamic)//直接点参数
        {
            JObject res = Methods.JhDbManager.dynamicToJObject(dynamic);
            Console.WriteLine("请求日期：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n查询患者基本信息的入参" + res.ToString());
            string idcard = string.Empty;
            try
            {
                idcard = res.GetValue("idcard").ToString();
            }
            catch
            {
                return new JsonResult(new { msg = "你输入的参数有误！", data = "参数错误", code = 403 });
            }
            string sql = $" select o.is_valid \"status\",  o.card_no \"patientId\",  o.name \"patientName\", o.card_no \"cardNo\",  o.idenno \"idCardNo\", decode(o.linkman_tel, '', o.home_tel, o.linkman_tel) \"phone\", o.home \"address\"  from zjhis.com_patientinfo o   where o.idenno ='{idcard}'     and o.CARD_NO not like 'T%'";
            var dt = new DataTable();
            try
            {
                dt = Methods.JhDbManager.QuerySql(sql, this.Configuration.GetConnectionString("Default").ToString());

                if (dt==null || dt.Rows.Count==0)
                {
                    SqlServiceSoapClient client = new SqlServiceSoapClient(SqlServiceSoapClient.EndpointConfiguration.SqlServiceSoap);
                    string result = client.GetResultAsync(idcard).Result.Body.GetResultResult;
                    List<JObject> jobj = Methods.JhDbManager.GetTbyString<List<JObject>>(result);
                    return new JsonResult(new { msg = "查询成功！", data = jobj, code = 200 });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { msg = "没有找到如何检测报告的信息！", data = ex.Message, code = 404 });
            }
            ArrayList arr = Methods.JhDbManager.getJObject(dt);
            if (arr.Count == 0 || arr == null)
            {
                return new JsonResult(new { msg = "没有找到如何检测报告的信息！", data = "查询结果为空", code = 404 });
            }
            return new JsonResult(new { msg = "查询成功！", data = arr, code = 200 });
        }
        #endregion
    }
}
