using MySqlBll;
using System;

namespace MySqlTool.Class
{
    public class Core
    {
        private string m_Path = AppDomain.CurrentDomain.BaseDirectory;

        private static Core _Instance = new Core();

        public event Progress OnProgress;

        public static Core Instance
        {
            get
            {
                return Core._Instance;
            }
        }

        public ResultMessage OutData(DBHost host, DBInfo model)
        {
            ResultMessage resultMessage = new ResultMessage();
            if (model.OutDBTag == null || model.OutDBTag != "")
            {
                PACKSCHEMA pACKSCHEMA = new PACKSCHEMA();
                string text = this.m_Path + string.Format("{0}{1:d5}_{2}.dat", model.OutDBTag, model.OutDBVersion + 1, DateTime.Now.ToString("MMddHH"));
                SCHEMA sCHEMA = MySqlCore.GetSCHEMA(Data.Instance.InformationConnString(host), model.DBName);
                pACKSCHEMA.PackDate = DateTime.Now;
                pACKSCHEMA.Schema = sCHEMA;
                pACKSCHEMA.Tag = model.OutDBTag;
                pACKSCHEMA.Version = model.OutDBVersion + 1;
                resultMessage = SerializeHelper.Serialize(text, pACKSCHEMA);
                if (resultMessage.Result)
                {
                    model.OutDBVersion++;
                    Data.Instance.SaveDbInfo(host, model);
                    resultMessage.Message = "导出成功\r\n" + text;
                }
            }
            else
            {
                resultMessage.Result = false;
                resultMessage.Message = "导出标志不正确";
            }
            return resultMessage;
        }

        public ResultMessage InData(DBHost host, DBInfo tmodel, SCHEMA source)
        {
            ResultMessage resultMessage = new ResultMessage();
            this.SetProgress("获取目标数据结构");
            SCHEMA sCHEMA = MySqlCore.GetSCHEMA(Data.Instance.InformationConnString(host), tmodel.DBName);
            MySqlScript mySqlScript = new MySqlScript(source, sCHEMA);
            TableMySQL tableMySQL = new TableMySQL(Data.Instance.DBConnString(host, tmodel.DBName));
            this.SetProgress("生成需要更新的脚本，并更新");
            try
            {
                foreach (string current in mySqlScript.MakeScript())
                {
                    this.SetProgress(current);
                    tableMySQL.ExecuteNonQuery(current);
                }
                this.SetProgress("更新成功");
                resultMessage.Result = true;
                resultMessage.Message = "更新成功";
            } 
            catch (Exception ex)
            {
                this.SetProgress("失败" + ex.Message);
                resultMessage.Result = false;
                resultMessage.Message = ex.Message;
            }
            return resultMessage;
        }

        public ResultMessage CreateDatabase(DBHost host, string dbname)
        {
            ResultMessage result = new ResultMessage(true);
            TableMySQL tableMySQL = new TableMySQL(Data.Instance.InformationConnString(host));
            string sql = "CREATE SCHEMA `" + dbname + "` ; ";
            tableMySQL.ExecuteNonQuery(sql);
            return result;
        }

        private void SetProgress(string msg)
        {
            if (this.OnProgress != null)
            {
                this.OnProgress(msg);
            }
        }
    }
}
