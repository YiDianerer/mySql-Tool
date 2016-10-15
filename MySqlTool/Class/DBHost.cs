using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MySqlTool.Class
{
	public class DBHost
	{
		[XmlAttribute]
		public string Host
		{
			get;
			set;
		}

		[XmlAttribute]
		public string Port
		{
			get;
			set;
		}

		[XmlAttribute]
		public string User
		{
			get;
			set;
		}

		[XmlAttribute]
		public string Password
		{
			get;
			set;
		}

		public List<DBInfo> DBInfos
		{
			get;
			set;
		}

		public DBHost()
		{
			this.DBInfos = new List<DBInfo>();
		}
	}
}
