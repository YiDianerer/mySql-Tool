using System;
using System.Collections.Generic;
using System.Data;

namespace MySql.Data.MySqlClient
{
	public class Table
	{
		private long _totalRows = -1L;

		public string TableName
		{
			get;
			set;
		}

		public string CreateTableSql
		{
			get;
			set;
		}

		public long TotalRows
		{
			get
			{
				return this._totalRows;
			}
		}

		public string[] ColumnNames
		{
			get
			{
				string[] array = new string[this.ColumnDataType.Count];
				int num = -1;
				foreach (System.Collections.Generic.KeyValuePair<string, string> current in this.ColumnDataType)
				{
					num++;
					array[num] = current.Key;
				}
				return array;
			}
		}

		public Dictionary<string, string> ColumnDataType
		{
			get;
			set;
		}

		public Table(string tableName, ref MySqlCommand cmd)
		{
			this._totalRows = 0L;
			Methods methods = new Methods();
			if (cmd.Connection.State == ConnectionState.Closed)
			{
				cmd.Connection.Open();
			}
			this.TableName = tableName;
			this.CreateTableSql = methods.GetCreateTableSql(tableName, cmd);
			cmd.CommandText = "SHOW COLUMNS FROM `" + tableName + "`;";
			MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
			DataTable dataTable = new DataTable();
			mySqlDataAdapter.Fill(dataTable);
			this.ColumnDataType = new Dictionary<string, string>();
			foreach (DataRow dataRow in dataTable.Rows)
			{
				this.ColumnDataType.Add(dataRow["Field"].ToString(), dataRow["Type"].ToString().ToLower());
			}
		}

		public long GetTotalRows(ref MySqlCommand cmd)
		{
			if (this._totalRows < 1L)
			{
				cmd.CommandText = "SELECT COUNT(*) FROM `" + this.TableName + "`;";
				this._totalRows = (long)cmd.ExecuteScalar();
			}
			return this._totalRows;
		}
	}
}
