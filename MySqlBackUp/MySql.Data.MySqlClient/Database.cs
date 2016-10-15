using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace MySql.Data.MySqlClient
{
	public class Database
	{
		public delegate void calculateTotalRowsProgressChange(object sender, int percentageCompleted);

		public delegate void calculateTotalRowsProgressComplete(object sender, long totalRows);

		private long _totalRows = -1L;

		private string _createDatabaseSql = "";

		private string _databaseName = "";

		private string _serverVersionNo = "";

		private string _serverVersion = "";

		private double _ServerMajorVersion = 0.0;

		private string _defaultDatabaseCharSet = "";

		public event calculateTotalRowsProgressChange CalculateTotalRowsProgressChanged;

		public event calculateTotalRowsProgressComplete CalculateTotalRowsCompleted;

		public string CreateDatabaseSql
		{
			get
			{
				return this._createDatabaseSql;
			}
		}

		public string DatabaseName
		{
			get
			{
				return this._databaseName;
			}
		}

		public Dictionary<string, Table> Tables
		{
			get;
			set;
		}

		public string ServerVersion
		{
			get
			{
				return this._serverVersion;
			}
		}

		public string ServerVersionNo
		{
			get
			{
				return this._serverVersionNo;
			}
		}

		public double ServerMajorVersion
		{
			get
			{
				return this._ServerMajorVersion;
			}
		}

		public string DefaultDatabaseCharSet
		{
			get
			{
				return this._defaultDatabaseCharSet;
			}
		}

		public string[] TableNames
		{
			get
			{
				string[] array = new string[this.Tables.Count];
				int num = -1;
				foreach (System.Collections.Generic.KeyValuePair<string, Table> current in this.Tables)
				{
					num++;
					array[num] = current.Key;
				}
				return array;
			}
		}

		public Dictionary<string, string> StoredProcedure
		{
			get;
			set;
		}

		public Dictionary<string, string> StoredFunction
		{
			get;
			set;
		}

		public Dictionary<string, string> StoredTrigger
		{
			get;
			set;
		}

		public Dictionary<string, string> StoredEvents
		{
			get;
			set;
		}

		public Dictionary<string, string> StoredView
		{
			get;
			set;
		}

		public Database(ref MySqlCommand cmd)
		{
			this.StoredEvents = new Dictionary<string, string>();
			this.StoredFunction = new Dictionary<string, string>();
			this.StoredProcedure = new Dictionary<string, string>();
			this.StoredTrigger = new Dictionary<string, string>();
			this.StoredView = new Dictionary<string, string>();
			this.Tables = new Dictionary<string, Table>();
			if (cmd.Connection.State != ConnectionState.Open)
			{
				cmd.Connection.Open();
			}
			cmd.CommandText = "SELECT DATABASE();";
			this._databaseName = cmd.ExecuteScalar().ToString();
			if (!(this._databaseName == ""))
			{
				Methods methods = new Methods();
				cmd.CommandText = "SHOW CREATE DATABASE " + this.DatabaseName + ";";
				MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
				DataTable dataTable = new DataTable();
				mySqlDataAdapter.Fill(dataTable);
				this._createDatabaseSql = dataTable.Rows[0][1].ToString().Replace("CREATE DATABASE", "CREATE DATABASE IF NOT EXISTS") + ";";
				this._serverVersion = methods.GetServerVersion(ref cmd, ref this._serverVersionNo);
				this.GetTables(ref cmd);
				cmd.CommandText = "SHOW VARIABLES LIKE 'character_set_database';";
				MySqlDataAdapter mySqlDataAdapter2 = new MySqlDataAdapter(cmd);
				DataTable dataTable2 = new DataTable();
				mySqlDataAdapter2.Fill(dataTable2);
				this._defaultDatabaseCharSet = dataTable2.Rows[0][1].ToString();
				if (cmd.Connection.State != ConnectionState.Open)
				{
					cmd.Connection.Open();
				}
				string[] array = this.ServerVersionNo.Split(new char[]
				{
					'.'
				});
				string s;
				if (array.Length > 1)
				{
					s = array[0] + "." + array[1];
				}
				else
				{
					s = array[0];
				}
				double.TryParse(s, out this._ServerMajorVersion);
				this.GetRoutines(ref cmd, "PROCEDURE");
				this.GetRoutines(ref cmd, "FUNCTION");
				this.GetTriggers(ref cmd);
				this.GetViews(ref cmd);
				this.GetEvents(ref cmd);
			}
		}

		private void GetEvents(ref MySqlCommand cmd)
		{
			try
			{
				if (this.ServerMajorVersion >= 5.1)
				{
					cmd.CommandText = "SHOW EVENTS WHERE UPPER(TRIM(Db))=UPPER(TRIM('" + this.DatabaseName + "'));";
					DataTable dataTable = new DataTable();
					MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
					mySqlDataAdapter.Fill(dataTable);
					foreach (DataRow dataRow in dataTable.Rows)
					{
						cmd.CommandText = "SHOW CREATE EVENT `" + dataRow[1].ToString() + "`;";
						MySqlDataAdapter mySqlDataAdapter2 = new MySqlDataAdapter(cmd);
						DataTable dataTable2 = new DataTable();
						mySqlDataAdapter2.Fill(dataTable2);
						string text = "";
						object obj = dataTable2.Rows[0][3];
						if (obj is string)
						{
							text = string.Concat(obj);
						}
						else if (obj is byte[])
						{
							System.Text.UTF8Encoding uTF8Encoding = new System.Text.UTF8Encoding();
							text = uTF8Encoding.GetString((byte[])obj);
						}
						text = text.Replace("\r\n", "^^^^").Replace("\n", "\r\n").Replace("^^^^", "\r\n");
						this.StoredEvents.Add(dataRow[1].ToString(), text);
					}
				}
			}
			catch
			{
				this.StoredEvents = new Dictionary<string, string>();
			}
		}

		private void GetViews(ref MySqlCommand cmd)
		{
			try
			{
				if (this.ServerMajorVersion >= 5.0)
				{
					cmd.CommandText = "SHOW FULL TABLES FROM " + this.DatabaseName + " WHERE Table_type like 'VIEW';";
					MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
					DataTable dataTable = new DataTable();
					mySqlDataAdapter.Fill(dataTable);
					foreach (DataRow dataRow in dataTable.Rows)
					{
						cmd.CommandText = "SHOW CREATE VIEW `" + dataRow[0].ToString() + "`;";
						MySqlDataAdapter mySqlDataAdapter2 = new MySqlDataAdapter(cmd);
						DataTable dataTable2 = new DataTable();
						mySqlDataAdapter2.Fill(dataTable2);
						string text = "";
						object obj = dataTable2.Rows[0][1];
						if (obj is string)
						{
							text = string.Concat(obj);
						}
						else if (obj is byte[])
						{
							System.Text.UTF8Encoding uTF8Encoding = new System.Text.UTF8Encoding();
							text = uTF8Encoding.GetString((byte[])obj);
						}
						this.StoredView.Add(dataRow[0].ToString(), text.Replace("\r\n", "^^^^").Replace("\n", "\r\n").Replace("^^^^", "\r\n") + ";");
					}
				}
			}
			catch
			{
				this.StoredView = new Dictionary<string, string>();
			}
		}

		private void GetTriggers(ref MySqlCommand cmd)
		{
			try
			{
				if (this.ServerMajorVersion >= 5.0)
				{
					cmd.CommandText = "SHOW TRIGGERS;";
					DataTable dataTable = new DataTable();
					MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
					mySqlDataAdapter.Fill(dataTable);
					foreach (DataRow dataRow in dataTable.Rows)
					{
						DataTable dataTable2 = new DataTable();
						cmd.CommandText = "SHOW CREATE TRIGGER `" + dataRow[0] + "`;";
						MySqlDataAdapter mySqlDataAdapter2 = new MySqlDataAdapter(cmd);
						mySqlDataAdapter2.Fill(dataTable2);
						string text = "";
						object obj = dataTable2.Rows[0][2];
						if (obj is string)
						{
							text = string.Concat(obj);
						}
						else if (obj is byte[])
						{
							System.Text.UTF8Encoding uTF8Encoding = new System.Text.UTF8Encoding();
							text = uTF8Encoding.GetString((byte[])obj);
						}
						string value = text.Replace("\r\n", "^^^^").Replace("\n", "\r\n").Replace("^^^^", "\r\n");
						this.StoredTrigger.Add(dataRow[0].ToString(), value);
					}
				}
			}
			catch
			{
				this.StoredTrigger = new Dictionary<string, string>();
			}
		}

		private void GetRoutines(ref MySqlCommand cmd, string routine)
		{
			try
			{
				if (this.ServerMajorVersion >= 5.0)
				{
					DataTable dataTable = new DataTable();
					cmd.CommandText = string.Concat(new string[]
					{
						"SHOW ",
						routine,
						" STATUS WHERE UPPER(TRIM(Db))=UPPER(TRIM('",
						this.DatabaseName,
						"'));"
					});
					MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
					mySqlDataAdapter.Fill(dataTable);
					foreach (DataRow dataRow in dataTable.Rows)
					{
						cmd.CommandText = string.Concat(new object[]
						{
							"SHOW CREATE ",
							routine,
							" `",
							dataRow[1],
							"`;"
						});
						DataTable dataTable2 = new DataTable();
						MySqlDataAdapter mySqlDataAdapter2 = new MySqlDataAdapter(cmd);
						mySqlDataAdapter2.Fill(dataTable2);
						if (dataTable2.Rows.Count != 0)
						{
							string text = "";
							object obj = dataTable2.Rows[0][2];
							if (obj is string)
							{
								text = string.Concat(obj);
							}
							else if (obj is byte[])
							{
								System.Text.UTF8Encoding uTF8Encoding = new System.Text.UTF8Encoding();
								text = uTF8Encoding.GetString((byte[])obj);
							}
							string text2 = text.Replace("\r\n", "^^^^").Replace("\n", "\r\n").Replace("^^^^", "\r\n");
							Regex regex = new Regex("CREATE .*? PROCEDURE");
							text2 = regex.Replace(text2, "CREATE  PROCEDURE");
							if (routine == "PROCEDURE")
							{
								this.StoredProcedure.Add(dataRow[1].ToString(), text2);
							}
							else if (routine == "FUNCTION")
							{
								this.StoredFunction.Add(dataRow[1].ToString(), text2);
							}
						}
					}
				}
			}
			catch
			{
				if (routine == "PROCEDURE")
				{
					this.StoredProcedure = new Dictionary<string, string>();
				}
				else if (routine == "FUNCTION")
				{
					this.StoredFunction = new Dictionary<string, string>();
				}
			}
		}

		public long GetTotalRows(ref MySqlCommand cmd)
		{
			if (this._totalRows < 1L)
			{
				int num = 0;
				foreach (System.Collections.Generic.KeyValuePair<string, Table> current in this.Tables)
				{
					num++;
					current.Value.GetTotalRows(ref cmd);
					this._totalRows += current.Value.TotalRows;
					if (this.CalculateTotalRowsProgressChanged != null)
					{
						this.CalculateTotalRowsProgressChanged(this, num / this.Tables.Count * 100);
					}
				}
			}
			if (this.CalculateTotalRowsCompleted != null)
			{
				this.CalculateTotalRowsCompleted(this, this._totalRows);
			}
			return this._totalRows;
		}

		private void GetTables(ref MySqlCommand cmd)
		{
			DataTable dataTable = new DataTable();
			if (cmd.Connection.State != ConnectionState.Open)
			{
				cmd.Connection.Open();
			}
			cmd.CommandText = "SHOW FULL TABLES WHERE Table_type LIKE 'BASE TABLE';";
			MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
			mySqlDataAdapter.Fill(dataTable);
			cmd.Connection.Close();
			this.Tables = new Dictionary<string, Table>();
			foreach (DataRow dataRow in dataTable.Rows)
			{
				string text = dataRow[0].ToString();
				this.Tables.Add(text, new Table(text, ref cmd));
			}
		}
	}
}
