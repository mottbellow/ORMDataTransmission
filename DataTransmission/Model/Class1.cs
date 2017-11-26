using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Dynamic;
using Devart.Data.Oracle;

namespace DataTransmission
{
    public static class GenerageInsertCode
    {
        public static void Insert(string tableName, string[] list, dynamic data)
        {
            var db = Simple.Data.Database.OpenNamedConnection("OracleConnectionString");
            //使用SharedConnection必须使用Connection类，这里用的是Devart.Data.Oracle.OracleConnection
            //如果是SQL，就使用SqlConnection
            Devart.Data.Oracle.OracleConnection conn = new Devart.Data.Oracle.OracleConnection(ConfigurationManager.ConnectionStrings["OracleConnectionString"].ConnectionString);
            //使用SharedConnection，避免插入报错后连接无法关闭导致连接池满了之后超时！！！！！
            conn.Open();
            db.UseSharedConnection(conn);
            //插入前先删除所有
            db.AJRYGX.DeleteAll();
            foreach (dynamic item in data)
            {
                AJRYGX oRow = new AJRYGX();
                //！如果数据不为空才赋值，避免出现DateTime或int类型不能为空的情况
                if (item.BelongXiaQuCode != null)
                    oRow.BelongXiaQuCode = item.BelongXiaQuCode;
                if (item.OperateUserName != null)
                    oRow.OperateUserName = item.OperateUserName;
                if (item.OperateDate != null)
                    oRow.OperateDate = item.OperateDate;
                if (item.Row_ID != null)
                    oRow.Row_ID = item.Row_ID;
                if (item.YearFlag != null)
                    oRow.YearFlag = item.YearFlag;
                if (item.RowGuid != null)
                    oRow.RowGuid = item.RowGuid;
                if (item.SLRowGuid != null)
                    oRow.SLRowGuid = item.SLRowGuid;
                if (item.UserGuid != null)
                    oRow.UserGuid = item.UserGuid;
                if (item.XMGUID != null)
                    oRow.XMGUID = item.XMGUID;
                try
                {
                    db.AJRYGX.Insert(oRow);
                }
                catch (Exception ex)
                {
                    //如果插入发生错误后关闭连接，避免连接过多超时
                    conn.Close();
                    if (typeof(System.Collections.Generic.KeyNotFoundException) == ex.GetType())
                        throw;
                    if (typeof(ArgumentException) == ex.GetBaseException().GetType() && ex.Source == "Devart.Data" && ex.Message.Contains("A parameter with name ':ri0' is not contained by this Parameters collection."))
                    { }
                    else
                        throw;
                }
            }
        }
    }

    public class AJRYGX
    {
        public dynamic BelongXiaQuCode { get; set; }
        public dynamic OperateUserName { get; set; }
        public dynamic OperateDate { get; set; }
        public dynamic Row_ID { get; set; }
        public dynamic YearFlag { get; set; }
        public dynamic RowGuid { get; set; }
        public dynamic SLRowGuid { get; set; }
        public dynamic UserGuid { get; set; }
        public dynamic XMGUID { get; set; }

    }
}
