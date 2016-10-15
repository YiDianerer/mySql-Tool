using System;
using System.Collections.Generic;

namespace MySqlBll
{
	[Serializable]
	public class TABLE
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

		public TABLE_TYPE TABLE_TYPE
		{
			get;
			set;
		}

		public ENGINE ENGINE
		{
			get;
			set;
		}

		public ulong TABLE_ROWS
		{
			get;
			set;
		}

		public ulong DATA_LENGTH
		{
			get;
			set;
		}

		public ulong MAX_DATA_LENGTH
		{
			get;
			set;
		}

		public ulong INDEX_LENGTH
		{
			get;
			set;
		}

		public DateTime CREATE_TIME
		{
			get;
			set;
		}

		public DateTime UPDATE_TIME
		{
			get;
			set;
		}

		public string TABLE_COMMENT
		{
			get;
			set;
		}

		public string ROW_FORMAT
		{
			get;
			set;
		}

		public List<COLUMN> COLUMNS
		{
			get;
			set;
		}

		public List<INDEX> INDEXS
		{
			get;
			set;
		}

		public TABLE()
		{
			this.COLUMNS = new List<COLUMN>();
			this.INDEXS = new List<INDEX>();
		}
	}
}
