using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;

namespace DataTransmission
{
    public class RunCode
    {
        public void Run(string tableName, string[] nameList, dynamic list)
        {
            // 编译器 
            CodeDomProvider cdp = CodeDomProvider.CreateProvider("C#");

            // 编译器的参数 
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Core.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");
            cp.ReferencedAssemblies.Add("System.Configuration.dll");
            cp.ReferencedAssemblies.Add("System.Linq.dll");
            cp.ReferencedAssemblies.Add("System.Dynamic.dll");
            cp.ReferencedAssemblies.Add("Devart.Data.dll");
            cp.ReferencedAssemblies.Add("Devart.Data.Oracle.dll");
            cp.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
            cp.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            cp.ReferencedAssemblies.Add("Oracle.ManagedDataAccess.dll");
            cp.ReferencedAssemblies.Add("Simple.Data.Ado.dll");
            cp.ReferencedAssemblies.Add("Simple.Data.dll");
            cp.ReferencedAssemblies.Add("Simple.Data.Oracle.Devart.dll");
            cp.ReferencedAssemblies.Add("Simple.Data.Oracle.dll");
            cp.ReferencedAssemblies.Add("Simple.Data.Oracle.ManagedDataAccess.dll");
            cp.ReferencedAssemblies.Add("Simple.Data.SqlServer.dll");
            cp.ReferencedAssemblies.Add("DataTransmission.exe");
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;

            // 编译结果 
            CompilerResults cr = cdp.CompileAssemblyFromSource(cp, MakeCodeDomEnv(tableName, nameList, list));

            if (cr.Errors.HasErrors) Console.WriteLine("编译出错!");
            else
            {
                // 编译后的程序集
                Assembly ass = cr.CompiledAssembly;

                // 得到HelloWorld类中的SayHello方法 
                Type type = ass.GetType("DataTransmission.GenerageInsertCode");
                MethodInfo mi = type.GetMethod("Insert");

                mi.Invoke(null, new object[] { tableName, nameList, list });
            }
        }

        public string MakeCodeDomEnv(string tableName, string[] nameList, dynamic list)
        {
            StringBuilder sbCode = new StringBuilder();
            sbCode.AppendLine("using System;");
            sbCode.AppendLine("using System.Collections.Generic;");
            sbCode.AppendLine("using System.Data;");
            sbCode.AppendLine("using System.Linq;");
            sbCode.AppendLine("using System.Text;");
            sbCode.AppendLine("using System.Configuration;");
            sbCode.AppendLine("using System.Dynamic;");
            sbCode.AppendLine("using Devart.Data.Oracle;");
            sbCode.AppendLine("");
            sbCode.AppendLine("namespace DataTransmission");
            sbCode.AppendLine("{");
            sbCode.AppendLine("    public static class GenerageInsertCode");
            sbCode.AppendLine("    {");
            sbCode.AppendLine("        public static void Insert(string tableName, string [] list, dynamic data)");
            sbCode.AppendLine("        {");
            sbCode.AppendLine("            var db = Simple.Data.Database.OpenNamedConnection(\"OracleConnectionString\");");
            sbCode.AppendLine("            //使用SharedConnection必须使用Connection类，这里用的是Devart.Data.Oracle.OracleConnection");
            sbCode.AppendLine("            //如果是SQL，就使用SqlConnection");
            sbCode.AppendLine("            Devart.Data.Oracle.OracleConnection conn = new Devart.Data.Oracle.OracleConnection(ConfigurationManager.ConnectionStrings[\"OracleConnectionString\"].ConnectionString);");
            sbCode.AppendLine("            //使用SharedConnection，避免插入报错后连接无法关闭导致连接池满了之后超时！！！！！");
            sbCode.AppendLine("            conn.Open();");
            sbCode.AppendLine("            db.UseSharedConnection(conn);");
            sbCode.AppendLine("            //插入前先删除所有");
            sbCode.AppendLine("            db." + tableName + ".DeleteAll();");
            sbCode.AppendLine("            foreach (dynamic item in data)");
            sbCode.AppendLine("            {");
            sbCode.AppendLine("                 " + tableName + " oRow = new " + tableName + "();");
            sbCode.AppendLine("                 //！如果数据不为空才赋值，避免出现DateTime或int类型不能为空的情况");
            foreach (var item in nameList)
            {
                sbCode.AppendLine("                 if (item." + item + " != null)");
                sbCode.AppendLine("                     oRow." + item + " = item." + item + ";");
            }
            sbCode.AppendLine("                 try");
            sbCode.AppendLine("                 {");
            sbCode.AppendLine("                     db." + tableName.ToUpper() + ".Insert(oRow);");
            sbCode.AppendLine("                 }");
            sbCode.AppendLine("                 catch (Exception ex)");
            sbCode.AppendLine("                 {");
            sbCode.AppendLine("                     //如果插入发生错误后关闭连接，避免连接过多超时");
            sbCode.AppendLine("                     conn.Close();");
            sbCode.AppendLine("                     if (typeof(System.Collections.Generic.KeyNotFoundException) == ex.GetType())");
            sbCode.AppendLine("                         throw;");
            sbCode.AppendLine("                     else if (typeof(ArgumentException) == ex.GetBaseException().GetType() && ex.Source == \"Devart.Data\" && ex.Message.ToLower().Contains(\"is not contained by this parameters collection.\"))");
            sbCode.AppendLine("                     { }");
            sbCode.AppendLine("                     else");
            sbCode.AppendLine("                         throw;");
            sbCode.AppendLine("                 }");
            sbCode.AppendLine("             }");
            sbCode.AppendLine("         }");
            sbCode.AppendLine("     }");

            sbCode.AppendLine(GetModelCode(tableName, nameList));

            sbCode.AppendLine("}");

            return sbCode.ToString();
        }

        /// <summary>
        /// 获取model类的代码，设置每种类型为dynamic。
        /// 免去后面的转换麻烦，推荐
        /// </summary>
        /// <param name="modelname">表名，代表数据类名称</param>
        /// <param name="nameList">数据字段列表</param>
        /// <returns>代码string</returns>
        public string GetModelCode(string modelname, string[] nameList)
        {
            string PROCESS_METHOD_CODE = @"  
    public class " + modelname + @"
    {{
{0} 
    }}";
            StringBuilder sbCode = new StringBuilder();
            foreach (var item in nameList)
            {
                sbCode.AppendLine("             public dynamic " + item + " { get; set; }");
            }
            return string.Format(PROCESS_METHOD_CODE, sbCode.ToString());
        }

        /// <summary>
        /// 获取model类的代码，通过已经从sql库中取出的数据，判断每个字段的类型
        /// 但是由于情况多种多样，暂时保留
        /// </summary>
        /// <param name="modelname">表名，代表数据类名称</param>
        /// <param name="nameList">数据字段列表</param>
        /// <param name="list">数据list</param>
        /// <returns>代码string</returns>
        public string GetModelCode(string modelname, string[] nameList, dynamic list)
        {
            string PROCESS_METHOD_CODE = @"  
    public class " + modelname + @"
    {{
{0} 
    }}";
            StringBuilder sbCode = new StringBuilder();
            Dictionary<string, string> dataTypeList = new Dictionary<string, string>();
            foreach (var item in nameList)
            {
                dataTypeList.Add(item, "");
            }
            foreach (dynamic field in nameList)
            {
                foreach (dynamic oRow in list)
                {
                    if (dataTypeList[field] != "")
                        break;
                    foreach (var item in oRow)
                    {
                        if (dataTypeList[field] != "")
                            break;
                        if (dataTypeList[field] == "" && item.Key == field && item.Value != null)
                            dataTypeList[field] = getType(item.Value);
                        else
                            continue;
                    }
                }
            }
            foreach (var item in nameList)
            {
                if (dataTypeList[item] == "")
                    dataTypeList[item] = "string";
            }
            foreach (var item in dataTypeList)
            {
                sbCode.AppendLine("             public " + item.Value + " " + item.Key + " { get; set; }");
            }
            return string.Format(PROCESS_METHOD_CODE, sbCode.ToString());
        }

        /// <summary>
        /// 通过传入的动态类型数据，获取对应的数据类型
        /// </summary>
        /// <param name="data">数据值</param>
        /// <returns></returns>
        private string getType(dynamic data)
        {
            if (data == null)
                return "string";
            if (data.GetType() == typeof(Int16))
                return "dynamic";
            if (data.GetType() == typeof(Int32))
                return "dynamic";
            if (data.GetType() == typeof(Int64))
                return "dynamic";
            if (data.GetType() == typeof(Single))
                return "dynamic";
            if (data.GetType() == typeof(Int32))
                return "dynamic";
            if (data.GetType() == typeof(String))
                return "string";
            if (data.GetType() == typeof(Decimal))
                return "dynamic";
            if (data.GetType() == typeof(Double))
                return "dynamic";
            if (data.GetType() == typeof(float))
                return "dynamic";
            if (data.GetType() == typeof(Byte))
                return "byte";
            if (data.GetType() == typeof(Byte[]))
                return "byte []";
            if (data.GetType() == typeof(DateTime))
                return "dynamic";
            if (data.GetType() == typeof(bool))
                return "bool";
            if (data.GetType() == typeof(Char))
                return "char";
            if (data.GetType() == typeof(DateTime))
                return "dynamic";
            else
                return "string";
        }
    }
}
