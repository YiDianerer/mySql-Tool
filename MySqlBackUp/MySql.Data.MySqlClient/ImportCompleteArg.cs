using System;
using System.Collections.Generic;

namespace MySql.Data.MySqlClient
{
	public class ImportCompleteArg
	{
		public enum CompleteType
		{
			Completed,
			Cancelled,
			Error
		}

		public System.DateTime TimeStart;

		public System.DateTime TimeEnd;

		public bool HasErrors = false;

		public System.Exception LastError = null;

		public ImportCompleteArg.CompleteType CompletedType = ImportCompleteArg.CompleteType.Completed;

		public System.Collections.Generic.Dictionary<long, System.Exception> Errors = new System.Collections.Generic.Dictionary<long, System.Exception>();

		public long CurrentLineNo
		{
			get;
			set;
		}

		public System.TimeSpan TimeUsed
		{
			get
			{
				return this.TimeEnd - this.TimeStart;
			}
		}

		public ImportCompleteArg(ImportCompleteArg.CompleteType completeType)
		{
			this.CompletedType = completeType;
		}
	}
}
