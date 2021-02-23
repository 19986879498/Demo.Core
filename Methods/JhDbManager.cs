using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Demo.Core.Methods
{
    public class JhDbManager
    {
        private static string connString_From = "Data Source=172.22.156.43/orcl11g;User ID=jhemr;Password=jhemr;Min Pool Size=10;Max Pool Size=512;";

        private string errText = string.Empty;

        public string ErrMsg
        {
            get
            {
                return this.errText;
            }
        }
        /// <summary>
        /// dynamic转jobject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static JObject dynamicToJObject(dynamic obj)
        {
            return JsonConvert.DeserializeObject<JObject>(obj.ToString());
        }
        public string GetEmrData(string hoscode, string clincNO)
        {
            Console.WriteLine("入参：" + hoscode + "|" + clincNO);
            string xmlData = string.Empty;
            string strSql = @"select a.zs zs,
       a.XBS xbs,
       a.JWS jws,
       a.TGJC tgjc,
       a.FZJC fzjc,
       a.HOSPITAL_NO,
       a.HIS_REGISTER_PK
  from V_HIS_PATINFO a
 where a.HOSPITAL_NO = '{0}'
   and a.HIS_REGISTER_PK = '{1}'
and rownum=1";
            strSql = string.Format(strSql, hoscode, clincNO);
            using (OracleConnection connection = new OracleConnection(connString_From))
            {
                try
                {
                    connection.Open();
                    OracleCommand command = new OracleCommand();
                    command.Connection = connection;
                    command.CommandText = strSql;
                    OracleDataReader read = command.ExecuteReader();
                    while (read.Read())
                    {
                        xmlData = read[0].ToString() + "@@" + read[1].ToString()
                            + "@@" + read[2].ToString() + "@@" + read[3].ToString()
                            + "@@" + read[4].ToString() + "@@" + read[5].ToString() + "@@" + read[6].ToString();
                        Console.WriteLine("出参：" + hoscode + "|" + clincNO + xmlData);
                        //xmlData += "<ROW>" + "\n";
                        //xmlData += "<ZS>" + read[0].ToString() + "</ZS>" + "\n";
                        //xmlData += "<XBS>" + read[1].ToString() + "</XBS>" + "\n";
                        //xmlData += "<JWS>" + read[2].ToString() + "</YANGBENLXMC>" + "\n";
                        //xmlData += "<JIESHOUSJ>" + read[3].ToString() + "</JIESHOUSJ>" + "\n";
                        //xmlData += "<BAOGAOSJ>" + read[4].ToString() + "</BAOGAOSJ>" + "\n";
                        //xmlData += "<ISWSW>" + read[5].ToString() + "</ISWSW>" + "\n";
                        //xmlData += "</ROW>" + "\n";
                    }
                    Console.WriteLine(xmlData);
                    return xmlData;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    this.errText = ex.Message;
                    return "-1";
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// sql查询的方法
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public static DataTable QuerySql(string sql, string connStr = null)
        {
            connStr = connStr == null ? connString_From : connStr;
            OracleConnection conn = new OracleConnection(connStr);
            DataTable ds = new DataTable();
            try
            {
                conn.Open();
                OracleDataAdapter oda = new OracleDataAdapter(sql, conn);
                OracleCommand cmd = new OracleCommand(sql, conn);
                DataTable dt = new DataTable();
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 999;
                var reader = cmd.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                return dt;

                // ds.Load(reader);
            }
            catch(Exception ex)
            {
                return null;
            }
            finally
            {
                conn.Close();
            }
            return ds;
        }
        public static ArrayList getJObject(DataTable dt)
        {
            ArrayList arr = new ArrayList();
            if (dt == null)
            {
                return null;
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Dictionary<string, object> dirobj = new Dictionary<string, object>();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    dirobj.Add(dt.Columns[j].ColumnName, dt.Rows[i][j].ToString());

                }
                arr.Add(dirobj);
            }
            return arr;
        }

        /// <summary>
        /// 手机短信服务
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string getSMSinfo(string info,string phone)
        {
            string data = $"<request><id>1</id><phone>{phone}</phone><content>{info}</content></request>";
            string url = "http://192.168.1.110:8081/SmsServer.ashx";
            string message = GetHttpRequest(url, data);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);
            string res = JsonConvert.SerializeXmlNode(doc);
            return res;
        }
        /// <summary>
        /// 发送http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetHttpRequest(string url,string data)
        {
            byte[] bytedata = Encoding.UTF8.GetBytes(data);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "Post";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            request.ContentLength = bytedata.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(bytedata, 0, bytedata.Length);
            stream.Close();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                return ex.Message;
            } 
            


        }
        /// <summary>
        /// 调用存储过程获取方法
        /// </summary>
        /// <param name="db"></param>
        /// <param name="spName"></param>
        /// <param name="paramsters"></param>
        /// <returns></returns>
        public static DataSet SqlQuery( string spName,OracleParameter[] paramsters,string DBstring=null)
        {
            DBstring = DBstring == null ? connString_From : DBstring;
            OracleConnection connection = new OracleConnection(DBstring);
            OracleDataAdapter adapter = null;
            DataSet set = null;
            using (OracleCommand command = new OracleCommand(spName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = spName;
                command.Parameters.AddRange(paramsters);
                adapter = new OracleDataAdapter(command);

                set = new DataSet();
                adapter.Fill(set);
                adapter.SelectCommand.Parameters.Clear();
                adapter.Dispose();
                command.Parameters.Clear();
                command.Dispose();
                connection.Close();
                connection.Dispose();
                return set;
            }
        }
        /// <summary>
        /// 查询返回一个数值
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="connStr"></param>
        /// <returns></returns>
        public static string getSqlReturnOne(string sql, string connStr = null)
        {
            DataTable dt = QuerySql(sql, connStr);
            if (dt.Rows.Count==0||dt==null)
            {
                return "";
            }
            return dt.Rows[0][0].ToString(); 
        }
        /// <summary>
        /// 获取json返回得到的json数据
        /// </summary>
        /// <param name="parems"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static ObjectResult GetResult(List<OracleParameter> parems, DataSet ds, string ErrName = null)
        {
            ErrName = string.IsNullOrEmpty(ErrName) ? "ErrorMsg" : ErrName;
            int code = 0;
            try
            {
                code = Convert.ToInt32(parems.Where(i => i.ParameterName == "ReturnCode").FirstOrDefault().Value.ToString());
            }
            catch (Exception)
            {
                code = 404;
            }
            var data = getJObject(ds.Tables[0]);
            IEnumerable<JObject> j = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(data));
            string jsonstrData = string.Empty;
            if (data != null && data.Count > 0)
            {
                jsonstrData = JsonConvert.SerializeObject(data);
            }
            else
            {
                goto Error;
            }
            if (code == 1)
            {
                string Msg = parems.Where(i => i.ParameterName == ErrName).FirstOrDefault().Value.ToString();
                var res = new { msg = Msg, data = data, code = 200 };
                return new ObjectResult(res);

            }
            else
            {
                goto Error;

            }

            Error:
            var result = new { msg = parems.Where(i => i.ParameterName == ErrName).FirstOrDefault().Value.ToString(), data = "查询无数据", code = 404 };
            return new ObjectResult(result);
        }

        /// <summary>
        /// 获得入参
        /// </summary>
        /// <returns></returns>
        public static OracleParameter GetInput(string name, object value)
        {
            var input = new OracleParameter(name, value);
            input.OracleDbType = OracleDbType.Varchar2;
            input.Direction = System.Data.ParameterDirection.Input;
            return input;
        }
        /// <summary>
        /// 获得入参
        /// </summary>
        /// <returns></returns>
        public static OracleParameter GetInput(string name, OracleDbType dbType, object value)
        {
            var input = new OracleParameter(name, value);
            input.OracleDbType = dbType;
            input.Direction = System.Data.ParameterDirection.Input;
            return input;
        }
        /// <summary>
        /// 获得出参
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static OracleParameter GetOutput(string name, OracleDbType dbType, int dataLength)
        {
            var output = new OracleParameter(name, dbType, dataLength);
            output.Direction = System.Data.ParameterDirection.Output;
            return output;
        }

        public static T  GetTbyString<T>(string res)
        {
            return JsonConvert.DeserializeObject<T>(res);
        }
        public static string GetJobjVal( JObject jObject ,string key)
        {
            if (!jObject.ContainsKey(key))
            {
                return "";
            }
            return jObject.GetValue(key, StringComparison.OrdinalIgnoreCase).ToString();
        }

        public static dynamic GetDynamicByString(string dyStr)
        {
            return JsonConvert.DeserializeObject<dynamic>(dyStr);
        }
    }
}
