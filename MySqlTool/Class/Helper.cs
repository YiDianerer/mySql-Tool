using System;

namespace MySqlTool.Class
{
	public class Helper
	{
		public static string GetStorageUnit(long value)
		{
			string result;
			if (value < 1024L)
			{
				result = value + "b";
			}
			else if (value < 1048576L)
			{
				result = ((double)value / 1024.0).ToString("F2") + "Kb";
			}
			else if (value < 1073741824L)
			{
				result = ((double)value / 1048576.0).ToString("F2") + "Mb";
			}
			else
			{
				result = ((double)value / 1073741824.0).ToString("F2") + "Gb";
			}
			return result;
		}
	}
}
