using System;

namespace MySql.Data.MySqlClient
{
	public class ExportCompleteArg
	{
		public enum CompleteType
		{
			Completed,
			Cancelled,
			Error
		}

		public DateTime TimeStart;

		public DateTime TimeEnd;

		public Exception Error = null;

		public ExportCompleteArg.CompleteType CompletedType = ExportCompleteArg.CompleteType.Completed;

		public TimeSpan TimeUsed
		{
			get
			{
				return this.TimeEnd - this.TimeStart;
			}
		}
	}
}
