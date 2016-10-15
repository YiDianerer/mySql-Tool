using System;

namespace MySqlBll
{
	[Serializable]
	public class COLUMN
	{
		public string TABLE_SCHEMA
		{
			get;
			set;
		}

		public string TABLE_NAME
		{
			get;
			set;
		}

		public string COLUMN_NAME
		{
			get;
			set;
		}

		public ulong ORDINAL_POSITION
		{
			get;
			set;
		}

		public object COLUMN_DEFAULT
		{
			get;
			set;
		}

		public IS_NULLABLE IS_NULLABLE
		{
			get;
			set;
		}

		public DATA_TYPE DATA_TYPE
		{
			get;
			set;
		}

		public ulong CHARACTER_MAXIMUM_LENGTH
		{
			get;
			set;
		}

		public ulong CHARACTER_OCTET_LENGTH
		{
			get;
			set;
		}

		public ulong NUMERIC_PRECISION
		{
			get;
			set;
		}

		public ulong NUMERIC_SCALE
		{
			get;
			set;
		}

		public string COLUMN_TYPE
		{
			get;
			set;
		}

		public COLUMN_KEY COLUMN_KEY
		{
			get;
			set;
		}

		public string COLUMN_COMMENT
		{
			get;
			set;
		}

		public string EXTRA
		{
			get;
			set;
		}
	}
}
