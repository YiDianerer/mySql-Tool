using MySqlBll;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MySqlTool.Class
{
	public class Data
	{
		private string m_HostConfigName = AppDomain.CurrentDomain.BaseDirectory + "host.xml";

		private static Data _Instance = new Data();

		private List<DBHost> m_DBHost;

		public static Data Instance
		{
			get
			{
				return Data._Instance;
			}
		}

		public List<DBHost> DBHost
		{
			get
			{
				return this.m_DBHost;
			}
		}

		public void DataInit()
		{
			this.m_DBHost = new List<DBHost>();
			if (File.Exists(this.m_HostConfigName))
			{
				ResultMessage resultMessage = SerializeHelper.XmlDeserialize<List<DBHost>>(this.m_HostConfigName);
				if (resultMessage.Result)
				{
					this.m_DBHost = (resultMessage.ObjResult as List<DBHost>);
				}
			}
			foreach (DBHost current in this.m_DBHost)
			{
				current.DBInfos = new List<DBInfo>();
				DataTable dataBase = MySqlCore.GetDataBase(this.InformationConnString(current));
				foreach (DataRow dataRow in dataBase.Rows)
				{
					DBInfo dBInfo = new DBInfo();
					dBInfo.DBName = dataRow["DataBase"].ToString();
					string[] dbInfo = MySqlCore.GetDbInfo(this.DBConnString(current, dBInfo.DBName));
					dBInfo.DBTag = dbInfo[0];
					dBInfo.DBVersion = Convert.ToInt32(dbInfo[1]);
					dBInfo.OutDBTag = dbInfo[2];
					dBInfo.OutDBVersion = Convert.ToInt32(dbInfo[3]);
					current.DBInfos.Add(dBInfo);
				}
			}
		}

		public void SaveDbInfo(DBHost host, DBInfo model)
		{
			MySqlCore.SaveDbInfo(this.DBConnString(host, model.DBName), model.DBTag, model.DBVersion, model.OutDBTag, model.OutDBVersion);
		}

		public string InformationConnString(DBHost host)
		{
			string text = "information_schema";
			string format = "Database={0};Data Source={1};Port={2};User Id={3};Password={4};CharSet=utf8;";
			return string.Format(format, new object[]
			{
				text,
				host.Host,
				host.Port,
				host.User,
				host.Password
			});
		}

		public string DBConnString(DBHost host, string dbname)
		{
			string format = "Database={0};Data Source={1};Port={2};User Id={3};Password={4};CharSet=utf8;";
			return string.Format(format, new object[]
			{
				dbname,
				host.Host,
				host.Port,
				host.User,
				host.Password
			});
		}
	}
}
