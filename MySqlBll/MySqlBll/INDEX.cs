using System;

namespace MySqlBll
{
	[Serializable]
	public class INDEX
	{
		public string INDEX_NAME
		{
			get;
			set;
		}

		public int NON_UNIQUE
		{
			get;
			set;
		}

		public string COLUMN_NAME
		{
			get;
			set;
		}
	}
}
