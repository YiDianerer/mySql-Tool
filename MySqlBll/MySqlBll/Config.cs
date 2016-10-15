using System;

namespace MySqlBll
{
	public class Config
	{
		public static Config Instance = new Config();

		private IniFile m_IniFile;

		private string m_ConnString;

		public string InformationDataBase
		{
			get;
			set;
		}

		public string SourceDataBase
		{
			get;
			set;
		}

		public string SourceDataSource
		{
			get;
			set;
		}

		public string SourceUser
		{
			get;
			set;
		}

		public string SourcePassword
		{
			get;
			set;
		}

		public int SourcePort
		{
			get;
			set;
		}

		public string TargetDataBase
		{
			get;
			set;
		}

		public string TargetDataSource
		{
			get;
			set;
		}

		public string TargetUser
		{
			get;
			set;
		}

		public string TargetPassword
		{
			get;
			set;
		}

		public int TargetPort
		{
			get;
			set;
		}

		public string SourceInformationConnString
		{
			get
			{
				return string.Format(this.m_ConnString, new object[]
				{
					this.InformationDataBase,
					this.SourceDataSource,
					this.SourcePort,
					this.SourceUser,
					this.SourcePassword
				});
			}
		}

		public string SourceConnString
		{
			get
			{
				return string.Format(this.m_ConnString, new object[]
				{
					this.SourceDataBase,
					this.SourceDataSource,
					this.SourcePort,
					this.SourceUser,
					this.SourcePassword
				});
			}
		}

		public string TargetInformationConnString
		{
			get
			{
				return string.Format(this.m_ConnString, new object[]
				{
					this.InformationDataBase,
					this.TargetDataSource,
					this.SourcePort,
					this.TargetUser,
					this.TargetPassword
				});
			}
		}

		public string TargetConnString
		{
			get
			{
				return string.Format(this.m_ConnString, new object[]
				{
					this.TargetDataBase,
					this.TargetDataSource,
					this.SourcePort,
					this.TargetUser,
					this.TargetPassword
				});
			}
		}

		public Config()
		{
			this.m_IniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
			this.InformationDataBase = "information_schema";
			this.m_ConnString = "Database={0};Data Source={1};Port={2};User Id={3};Password={4};";
			this.SourceDataBase = this.m_IniFile.IniReadValue("Source", "DataBase", "fulltest");
			this.SourceDataSource = this.m_IniFile.IniReadValue("Source", "DataSource", "localhost");
			this.SourceUser = this.m_IniFile.IniReadValue("Source", "User", "root");
			this.SourcePassword = this.m_IniFile.IniReadValue("Source", "Password", "111111");
			this.SourcePort = Convert.ToInt32(this.m_IniFile.IniReadValue("Source", "Port", "3306"));
			this.TargetDataBase = this.m_IniFile.IniReadValue("Target", "DataBase", "xkxch");
			this.TargetDataSource = this.m_IniFile.IniReadValue("Target", "DataSource", "192.168.0.2");
			this.TargetUser = this.m_IniFile.IniReadValue("Target", "User", "root");
			this.TargetPassword = this.m_IniFile.IniReadValue("Target", "Password", "111111");
			this.TargetPort = Convert.ToInt32(this.m_IniFile.IniReadValue("Target", "Port", "3306"));
		}

		public string GetTargetConnString(string dbname)
		{
			return string.Format(this.m_ConnString, new object[]
			{
				dbname,
				this.TargetDataSource,
				this.SourcePort,
				this.TargetUser,
				this.TargetPassword
			});
		}

		~Config()
		{
			this.m_IniFile.IniWriteValue("Source", "DataBase", this.SourceDataBase);
			this.m_IniFile.IniWriteValue("Source", "Source", this.SourceDataSource);
			this.m_IniFile.IniWriteValue("Source", "User", this.SourceUser);
			this.m_IniFile.IniWriteValue("Source", "Password", this.SourcePassword);
			this.m_IniFile.IniWriteValue("Source", "Port", this.SourcePort.ToString());
			this.m_IniFile.IniWriteValue("Target", "DataBase", this.TargetDataBase);
			this.m_IniFile.IniWriteValue("Target", "Source", this.TargetDataSource);
			this.m_IniFile.IniWriteValue("Target", "User", this.TargetUser);
			this.m_IniFile.IniWriteValue("Target", "Password", this.TargetPassword);
			this.m_IniFile.IniWriteValue("Target", "Port", this.TargetPort.ToString());
		}
	}
}
