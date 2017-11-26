using Newtonsoft.Json;
using Simple.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataTransmission
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadTable();
        }

        private void LoadTable()
        {
            var sourceDB = Database.OpenNamedConnection("SQLConnectionString");
            var tables = sourceDB.tablespaceinfo.All().ToList<dynamic>();
            foreach (var item in tables)
            {
                listBox1.Items.Add(item.nameinfo);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ButtonEnable(false);
            DoSyn();
            ButtonEnable(true);
        }


        private void DoSyn()
        {
            string missCollumnErrorMessage = "";
            string misTableErrorMessage = "";
            string otherErrorMessage = "";
            bool hasKeyNotFoundError = false;
            bool hasNoTableError = false;
            bool hasOtherError = false;
            foreach (dynamic item in listBox1.SelectedItems)
            {
                try
                {
                    button1.Text = "！！！！！同步中请骚等！！！！！\r\n" + "正在同步表：" + item;
                    SynTable(item);
                }
                catch (Exception ex)
                {
                    if (typeof(System.Collections.Generic.KeyNotFoundException) == ex.GetType())
                    {
                        missCollumnErrorMessage += item + ";";
                        hasKeyNotFoundError = true;
                    }
                    else if (typeof(UnresolvableObjectException) == ex.GetBaseException().GetType() && ex.GetBaseException().Source == "Simple.Data.Ado" && ex.ToString().ToLower().Contains("not found"))
                    {
                        misTableErrorMessage += item + ";";
                        hasNoTableError = true;
                    }
                    else
                    {
                        otherErrorMessage += item + "：" + ex.Message + "\r\n";
                        hasOtherError = true;
                    }
                }
            }
            if (hasKeyNotFoundError)
                MessageBox.Show("数据字段不同，同步失败的表：" + missCollumnErrorMessage);
            if (hasNoTableError)
                MessageBox.Show("找不到同名数据表：" + misTableErrorMessage);
            if (hasOtherError)
                MessageBox.Show("其他错误：" + otherErrorMessage);
        }

        private void SynTable(string tableName)
        {
            var sourceDB = Database.OpenNamedConnection("SQLConnectionString");
            dynamic query = new SimpleQuery(sourceDB, tableName);
            dynamic data = new System.Dynamic.ExpandoObject();
            data = query.ToList<dynamic>();
            if (data.Count != 0)
            {
                new RunCode().Run(tableName, data[0].GetDynamicMemberNames(), data);
                //GenerageInsertCode.Insert(tableName, data[0].GetDynamicMemberNames(), data);
            }
        }

        public void ButtonEnable(bool enable)
        {
            if (!enable)
            {
                button1.Enabled = false;
                button1.Text = "！！！！！同步中请骚等！！！！！";
            }
            else
            {
                button1.Enabled = false;
                button1.Text = "！！！！！数据同步完成，为确保稳定，最好关闭程序后重新同步！！！！！";
            }
        }
    }
}
