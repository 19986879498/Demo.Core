using Demo.Core.IFactory;
using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Core.Factory
{
    public class JhEmrService : IJhEmrService
    {
        private string connString_From = "Data Source=172.22.156.43/orcl11g;User ID=jhemr;Password=jhemr;Min Pool Size=10;Max Pool Size=512;";

        private string errText = string.Empty;

        public string ErrMsg
        {
            get
            {
                return this.errText;
            }
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
    }
}
