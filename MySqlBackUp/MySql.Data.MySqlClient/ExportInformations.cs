using System;
using System.Collections.Generic;

namespace MySql.Data.MySqlClient
{
	public class ExportInformations
	{
		private string _encryptionKey = "1234";

		private int _saltSize = 0;

		private Dictionary<string, string> _tableCustomSql = null;

		public bool RecordDumpTime = true;

		public bool AsynchronousMode = false;

		public bool CalculateTotalRowsFromDatabase = false;

		public bool AutoCloseConnection = true;

		public bool EnableEncryption = false;

		public string FileName = "";

		public bool AddCreateDatabase = false;

		public bool ExportTableStructure = true;

		public bool ResetAutoIncrement = false;

		public bool ExportRows = true;

		public int MaxSqlLength = 1048576;

		public bool ExportStoredProcedures = true;

		public bool ExportFunctions = true;

		public bool ExportTriggers = true;

		public bool ExportViews = true;

		public bool ExportEvents = true;

		public bool ZipOutputFile = false;

		public ExportCompleteArg CompleteArg = null;

		public Dictionary<string, string> TableCustomSql
		{
			get
			{
				return this._tableCustomSql;
			}
			set
			{
				this._tableCustomSql = value;
				if (this._tableCustomSql != null)
				{
					foreach (System.Collections.Generic.KeyValuePair<string, string> current in this._tableCustomSql)
					{
						if (current.Value == null || current.Value == "")
						{
							this._tableCustomSql[current.Key] = string.Format("SELECT * FROM `{0}`;", current.Key);
						}
					}
				}
			}
		}

		public string[] TablesToBeExported
		{
			get
			{
				string[] result;
				if (this.TableCustomSql == null || this.TableCustomSql.Count == 0)
				{
					result = null;
				}
				else
				{
					string[] array = new string[this.TableCustomSql.Count];
					int num = -1;
					foreach (System.Collections.Generic.KeyValuePair<string, string> current in this.TableCustomSql)
					{
						num++;
						array[num] = current.Key;
					}
					result = array;
				}
				return result;
			}
			set
			{
				if (value != null && value.Length > 0)
				{
					for (int i = 0; i < value.Length; i++)
					{
						string text = value[i];
						this.TableCustomSql.Add(text, string.Format("SELECT * FROM `{0}`;", text));
					}
				}
				else
				{
					this.TableCustomSql = null;
				}
			}
		}

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
	}
}
