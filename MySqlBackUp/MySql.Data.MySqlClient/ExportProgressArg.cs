using System;

namespace MySql.Data.MySqlClient
{
	public class ExportProgressArg : System.EventArgs
	{
		public string CurrentTableName
		{
			get;
			set;
		}

		public long TotalRowsInCurrentTable
		{
			get;
			set;
		}

		public long TotalRowsInAllTables
		{
			get;
			set;
		}

		public long CurrentRowInCurrentTable
		{
			get;
			set;
		}

		public long CurrentRowInAllTable
		{
			get;
			set;
		}

		public int TotalTables
		{
			get;
			set;
		}

		public int CurrentTableIndex
		{
			get;
			set;
		}

		public int PercentageCompleted
		{
			get;
			set;
		}

		public int PercentageGetTotalRowsCompleted
		{
			get;
			set;
		}
	}
}
