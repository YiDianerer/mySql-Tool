using System;

namespace MySql.Data.MySqlClient
{
	public class ImportProgressArg : System.EventArgs
	{
		public long CurrentByte
		{
			get;
			set;
		}

		public long TotalBytes
		{
			get;
			set;
		}

		public System.Exception Error
		{
			get;
			set;
		}

		public long CurrentLineNo
		{
			get;
			set;
		}

		public string ErrorSql
		{
			get;
			set;
		}

		public int PercentageCompleted
		{
			get;
			set;
		}
	}
}
