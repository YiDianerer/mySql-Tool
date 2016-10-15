using System;

namespace MySqlBll
{
	[Serializable]
	public class PACKSCHEMA
	{
		public DateTime PackDate
		{
			get;
			set;
		}

		public string Tag
		{
			get;
			set;
		}

		public int Version
		{
			get;
			set;
		}

		public SCHEMA Schema
		{
			get;
			set;
		}
	}
}
