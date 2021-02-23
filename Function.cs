using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using Oracle.ManagedDataAccess.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Demo.Core
{
    public class Function
    {
        public static IConfiguration configuration = new ConfigurationBuilder()
       .SetBasePath(System.IO.Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json").Build();
        private static char[] Key = CharArrayType.FromString("`1234567890-=~!@#$%^&*()_+qwertyuiop[]\\QWERTYUIOP{}|asdfghjkl;'ASDFGHJKL:zxcvbnm,./ZXCVBNM<>? \"");
        //启动和关闭连接对象
        private static void OpenAndCloseConn(OracleConnection oracleConn)
        {
            if (oracleConn.State == ConnectionState.Closed)
            {
                oracleConn.Open();
            }
            else
            {
                oracleConn.Close();
            }
        }
        /// <summary>
        /// 将dataset数据集转换成json对象
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static ArrayList getJObject(DataSet ds)
        {
            ArrayList arr = new ArrayList();
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    Dictionary<string, string> j = new Dictionary<string, string>();
                    for (int i = 0; i < item.ItemArray.Length; i++)
                    {
                        j.Add(ds.Tables[0].Columns[i].ColumnName.ToString(), item.ItemArray[i].ToString());
                    }
                    arr.Add(j);
                }
            }
            return arr;
        }
        //获取sql数据
        public static DataSet GetSql(string sql)
        {
            string connectionstring = configuration.GetConnectionString("Default").ToString();
            //创建Oracle连接对象
            OracleConnection conn = new OracleConnection(connectionstring);
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            //创建操作对象
            OracleCommand command = conn.CreateCommand();
            DataSet dataTable = new DataSet();
            command.CommandText = string.Format(sql);
            OracleDataAdapter da = new OracleDataAdapter(command);
            da.Fill(dataTable);
            command.Parameters.Clear();
            conn.Close();
            int count = dataTable.Tables[0].Rows.Count;

            return dataTable;

        }
        public static T GetJObject<T>(object obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        }
    }
}
