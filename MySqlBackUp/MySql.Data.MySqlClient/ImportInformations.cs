using System;

namespace MySql.Data.MySqlClient
{
	public class ImportInformations
	{
		public enum CharSet
		{
			armscii8,
			ascii,
			big5,
			binary,
			cp1250,
			cp1251,
			cp1256,
			cp1257,
			cp850,
			cp852,
			cp866,
			cp932,
			dec8,
			eucjpms,
			euckr,
			gb2312,
			gbk,
			geostd8,
			greek,
			hebrew,
			hp8,
			keybcs2,
			koi8r,
			koi8u,
			latin1,
			latin2,
			latin5,
			latin7,
			macce,
			macroman,
			sjis,
			swe7,
			tis620,
			ucs2,
			ujis,
			utf8
		}

		private string _encryptionKey = "1234";

		private int _saltSize = 0;

		private string _database = "";

		private string _databaseCharSet = "";

		public bool AsynchronousMode = false;

		public bool AutoCloseConnection = true;

		public bool EnableEncryption = false;

		public string FileName = "";

		public bool IgnoreSqlError = false;

		public ImportCompleteArg CompleteArg = null;

		public string EncryptionKey
		{
			get
			{
				return this._encryptionKey;
			}
			set
			{
				Methods methods = new Methods();
				this._encryptionKey = methods.Sha2Hash(value);
				this._saltSize = methods.GetSaltSize(this._encryptionKey);
			}
		}

		public int SaltSize
		{
			get
			{
				return this._saltSize;
			}
		}

		public string CreateTargetDatabaseSql
		{
			get
			{
				string result;
				if (this._database != null && this._database != "" && this._databaseCharSet != null && this._databaseCharSet != "")
				{
					result = string.Format("CREATE DATABASE IF NOT EXISTS `{0}` DEFAULT CHARACTER SET {1};", this._database, this._databaseCharSet);
				}
				else if (this._database != null & this._database != "")
				{
					result = string.Format("CREATE DATABASE IF NOT EXISTS `{0}`;", this._database);
				}
				else
				{
					result = "";
				}
				return result;
			}
		}

		public string TargetDatabase
		{
			get
			{
				return this._database;
			}
		}

		public void SetTargetDatabase(string databaseName, string defaultCharSet)
		{
			this._database = databaseName;
			this._databaseCharSet = defaultCharSet;
		}

		public void SetTargetDatabase(string databaseName)
		{
			this._database = databaseName;
			this._databaseCharSet = "";
		}

		public void SetTargetDatabase(string databaseName, ImportInformations.CharSet charSet)
		{
			this.SetTargetDatabase(databaseName, charSet.ToString());
		}
	}
}
