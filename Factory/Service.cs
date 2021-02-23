using System.Data;
using System;
using System.Collections;
using Demo.Core.IFactory;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace Demo.Core.Factory
{
    public class Service : IService
    {
        public ArrayList GetTableResult(string Sql)
        {
            DataSet dataSet=Function.GetSql(Sql);
            ArrayList arr = Function.getJObject(dataSet);
            foreach (Dictionary<string,string> item in arr)
            {
                item["PASSWORD"] = JM.DESCryptoService.DESDecrypt(item["PASSWORD"].ToString(), "Core_H_N");
            }
            return arr;
        }
    }
}
