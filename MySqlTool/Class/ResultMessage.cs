using System;

namespace MySqlTool.Class
{
	public class ResultMessage
	{
		public bool Result
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public object ObjResult
		{
			get;
			set;
		}

		public ResultMessage()
		{
			this.Result = true;
		}

		public ResultMessage(bool result)
		{
			this.Result = result;
		}

		public ResultMessage(bool result, string message)
		{
			this.Result = result;
			this.Message = message;
		}
	}
}
