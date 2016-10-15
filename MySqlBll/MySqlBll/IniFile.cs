using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MySqlBll
{
	public class IniFile
	{
		public string path;

		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

		public IniFile(string INIPath)
		{
			this.path = INIPath;
		}

		public void IniWriteValue(string Section, string Key, string Value)
		{
			IniFile.WritePrivateProfileString(Section, Key, Value, this.path);
		}

		public string IniReadValue(string Section, string Key, string defvalue = "")
		{
			StringBuilder temp = new StringBuilder(255);
			int i = IniFile.GetPrivateProfileString(Section, Key, defvalue, temp, 255, this.path);
			string result = temp.ToString();
			return temp.ToString();
		}
	}
}
