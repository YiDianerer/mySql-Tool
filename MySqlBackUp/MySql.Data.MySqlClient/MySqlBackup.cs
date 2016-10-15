using Ionic.Zip;
using Ionic.Zlib;
using MySql.Data.Types;
using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace MySql.Data.MySqlClient
{
	public class MySqlBackup : IDisposable
    {
		public delegate void importComplete(object sender, ImportCompleteArg e);

		public delegate void importProgressChange(object sender, ImportProgressArg e);

		public delegate void exportComplete(object sender, ExportCompleteArg e);

		public delegate void exportProgressChange(object sender, ExportProgressArg e);

		public const string Version = "1.5.7 beta";

		private MySqlConnection _conn = new MySqlConnection();

		private MySqlCommand _cmd = new MySqlCommand();

		private ExportInformations _exportInfo = new ExportInformations();

		private ImportInformations _importInfo = new ImportInformations();

		private Database _database = null;

		private Methods methods;

		private NumberFormatInfo nf;

		private DateTimeFormatInfo df;

		private Encoding utf8WithoutBOM;

		private TextReader textReader;

		private TextWriter textWriter;

		private bool cancelProcess = false;

		private BackgroundWorker bwExport;

		private BackgroundWorker bwImport;

		private string _delimeter = "|";

		private ImportCompleteArg importCompleteArg;

		private ImportProgressArg importProgressArg;

		private ExportCompleteArg exportCompleteArg;

		private ExportProgressArg exportProgressArg;

		public event importComplete ImportCompleted;

		public event MySqlBackup.importProgressChange ImportProgressChanged;

		public event MySqlBackup.exportComplete ExportCompleted;

		public event MySqlBackup.exportProgressChange ExportProgressChanged;

		public Database DatabaseInfo
		{
			get
			{
				return this._database;
			}
		}

		public MySqlConnection Connection
		{
			get
			{
				return this._conn;
			}
			set
			{
				this._conn = value;
				this._cmd = new MySqlCommand();
				this._cmd.Connection = this._conn;
				this._cmd.CommandTimeout = 0;
				this._database = new Database(ref this._cmd);
			}
		}

		public ExportInformations ExportInfo
		{
			get
			{
				return this._exportInfo;
			}
			set
			{
				if (value == null)
				{
					this._exportInfo = new ExportInformations();
				}
				else
				{
					this._exportInfo = value;
				}
			}
		}

		public ImportInformations ImportInfo
		{
			get
			{
				return this._importInfo;
			}
			set
			{
				if (value == null)
				{
					this._importInfo = new ImportInformations();
				}
				else
				{
					this._importInfo = value;
				}
			}
		}

		public MySqlBackup()
		{
			this.InitializeInternalComponent();
		}

		public MySqlBackup(string ConnectionString)
		{
			this.Connection = new MySqlConnection(ConnectionString);
			this.InitializeInternalComponent();
		}

		public MySqlBackup(MySqlConnection connection)
		{
			this.Connection = connection;
			this.InitializeInternalComponent();
		}

		public MySqlBackup(MySqlCommand Command)
		{
			this._conn = Command.Connection;
			this._cmd = Command;
			this._cmd.CommandTimeout = 0;
			this._database = new Database(ref this._cmd);
			this.InitializeInternalComponent();
		}

		private void InitializeInternalComponent()
		{
			this.nf = new System.Globalization.NumberFormatInfo();
			this.nf.NumberDecimalSeparator = ".";
			this.nf.NumberGroupSeparator = string.Empty;
			this.methods = new Methods();
			this.utf8WithoutBOM = System.Text.Encoding.Default;
			this.df = new DateTimeFormatInfo();
			this.df.DateSeparator = "-";
			this.df.TimeSeparator = ":";
		}

		public void Export(ExportInformations exportInfo)
		{
			this.ExportInfo = exportInfo;
			this.Export();
		}

		public void Export()
		{
			if (this._exportInfo.AsynchronousMode)
			{
				this.bwExport = new BackgroundWorker();
				this.bwExport.DoWork += new DoWorkEventHandler(this.bwExport_DoWork);
				this.bwExport.RunWorkerAsync();
			}
			else
			{
				this.ExportExecute();
			}
		}

		private void bwExport_DoWork(object sender, DoWorkEventArgs e)
		{
			this.ExportExecute();
		}

		private void ExportExecute()
		{
			try
			{
				using (this.textWriter = new System.IO.StreamWriter(this._exportInfo.FileName, false, this.utf8WithoutBOM))
				{
					this.ExportStart();
				}
			}
			catch (System.Exception ex)
			{
				this.exportCompleteArg.Error = ex;
				this.exportCompleteArg.CompletedType = ExportCompleteArg.CompleteType.Error;
				this.exportCompleteArg.TimeEnd = System.DateTime.Now;
				this._exportInfo.CompleteArg = this.exportCompleteArg;
				this.Dispose(this._exportInfo.AutoCloseConnection);
				if (!this._exportInfo.AsynchronousMode)
				{
					throw ex;
				}
			}
			this.Dispose(this._exportInfo.AutoCloseConnection);
			this.exportCompleteArg.TimeEnd = System.DateTime.Now;
			this._exportInfo.CompleteArg = this.exportCompleteArg;
			if (this.ExportProgressChanged != null)
			{
				this.ExportProgressChanged(this, this.exportProgressArg);
			}
			if (this.ExportCompleted != null)
			{
				this.ExportCompleted(this, this.exportCompleteArg);
			}
		}

		private void ExportStart()
		{
			this.cancelProcess = false;
			this.InitializeInternalComponent();
			this.exportProgressArg = new ExportProgressArg();
			this.exportCompleteArg = new ExportCompleteArg();
			this.exportCompleteArg.TimeStart = System.DateTime.Now;
			this.exportCompleteArg.Error = null;
			System.Collections.Generic.Dictionary<string, long> dictionary = new System.Collections.Generic.Dictionary<string, long>();
			if (this._conn == null)
			{
				throw new System.Exception("Connection has disposed. Set ExportSettings.AutoCloseConnection to false if you want to reuse this instance.");
			}
			if (this._conn.State != ConnectionState.Open)
			{
				this._conn.Open();
			}
			this._cmd.CommandText = "SELECT DATABASE();";
			if (string.Concat(this._cmd.ExecuteScalar()).Length == 0)
			{
				throw new System.Exception("No database is selected or initialized for exporting.");
			}
			System.Collections.Generic.Dictionary<string, string> dictionary2 = null;
			string str = "";
			try
			{
				this._cmd.CommandText = "SHOW GLOBAL VARIABLES LIKE 'max_allowed_packet';";
				MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(this._cmd);
				DataTable dataTable = new DataTable();
				mySqlDataAdapter.Fill(dataTable);
				str = string.Concat(dataTable.Rows[0]["Value"]);
				this._cmd.CommandText = "SET GLOBAL max_allowed_packet=1*1024*1024*1024;";
				this._cmd.ExecuteNonQuery();
				this._cmd.Connection.Close();
				this._cmd.Connection.Open();
			}
			catch
			{
			}
			if (this._exportInfo.TableCustomSql == null || this._exportInfo.TableCustomSql.Count == 0)
			{
				dictionary2 = new System.Collections.Generic.Dictionary<string, string>();
				string[] tableNames = this.DatabaseInfo.TableNames;
				for (int i = 0; i < tableNames.Length; i++)
				{
					string text = tableNames[i];
					dictionary2.Add(text, string.Format("SELECT * FROM `{0}`;", text));
				}
			}
			else
			{
				dictionary2 = this._exportInfo.TableCustomSql;
			}
			this.exportProgressArg.TotalTables = dictionary2.Count;
			double num = 0.0;
			this.exportProgressArg.TotalRowsInAllTables = 0L;
			foreach (System.Collections.Generic.KeyValuePair<string, string> current in dictionary2)
			{
				long value = 0L;
				if (this.ExportProgressChanged != null)
				{
					this.exportProgressArg.CurrentTableName = current.Key;
					this.ExportProgressChanged(this, this.exportProgressArg);
				}
				if (this._exportInfo.CalculateTotalRowsFromDatabase && (this._exportInfo.ExportRows || this._exportInfo.ExportTableStructure))
				{
					string commandText;
					if (current.Value.ToUpper().Contains(" WHERE "))
					{
						int startIndex = current.Value.ToUpper().IndexOf(" WHERE ", 0);
						string str2 = current.Value.Substring(startIndex);
						commandText = "SELECT COUNT(1) FROM `" + current.Key + "`" + str2;
					}
					else
					{
						commandText = "SELECT COUNT(1) FROM `" + current.Key + "`;";
					}
					this._cmd.CommandTimeout = 0;
					this._cmd.CommandText = commandText;
					value = System.Convert.ToInt64(this._cmd.ExecuteScalar());
				}
				dictionary[current.Key] = value;
				this.exportProgressArg.TotalRowsInAllTables += dictionary[current.Key];
				num += 1.0;
				if (this.ExportProgressChanged != null)
				{
					this.exportProgressArg.PercentageGetTotalRowsCompleted = (int)(num / (double)dictionary2.Count * 100.0);
					this.ExportProgressChanged(this, this.exportProgressArg);
				}
			}
			this.textWriter.WriteLine(this.Encrypt("-- MySqlBackup.NET dump 1.5.7 beta"));
			if (this._exportInfo.RecordDumpTime)
			{
				this.textWriter.WriteLine(this.Encrypt("-- Dump time: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
			}
			else
			{
				this.textWriter.WriteLine(this.Encrypt("--"));
			}
			this.textWriter.WriteLine(this.Encrypt("-- ------------------------------------------------------"));
			this.textWriter.WriteLine(this.Encrypt("-- Server version\t" + this.DatabaseInfo.ServerVersion));
			this.textWriter.WriteLine(this.Encrypt(""));
			this.textWriter.WriteLine(this.Encrypt(""));
			this.textWriter.WriteLine(this.Encrypt("/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40101 SET NAMES " + this.DatabaseInfo.DefaultDatabaseCharSet + " */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;"));
			if (this._exportInfo.AddCreateDatabase)
			{
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt("--"));
				this.textWriter.WriteLine(this.Encrypt("-- Create schema " + this.DatabaseInfo.DatabaseName));
				this.textWriter.WriteLine(this.Encrypt("--"));
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt(this.DatabaseInfo.CreateDatabaseSql));
				this.textWriter.WriteLine(this.Encrypt("USE " + this.DatabaseInfo.DatabaseName + ";"));
			}
			this.textWriter.Flush();
			foreach (System.Collections.Generic.KeyValuePair<string, string> current in dictionary2)
			{
				if (!this.ExportInfo.ExportRows && !this.ExportInfo.ExportTableStructure)
				{
					if (this.ExportProgressChanged != null)
					{
						this.exportProgressArg.TotalTables = 0;
						this.exportProgressArg.TotalRowsInCurrentTable = 0L;
						this.exportProgressArg.CurrentTableIndex = 0;
						this.exportProgressArg.CurrentRowInCurrentTable = 0L;
						this.exportProgressArg.PercentageCompleted = 100;
						this.ExportProgressChanged(this, this.exportProgressArg);
					}
					break;
				}
				if (this.cancelProcess)
				{
					this.exportCompleteArg.CompletedType = ExportCompleteArg.CompleteType.Cancelled;
					return;
				}
				this.exportProgressArg.CurrentTableName = current.Key;
				if (this.ExportProgressChanged != null)
				{
					if (dictionary.ContainsKey(current.Key))
					{
						this.exportProgressArg.TotalRowsInCurrentTable = dictionary[current.Key];
					}
					this.exportProgressArg.CurrentTableIndex++;
					this.exportProgressArg.CurrentRowInCurrentTable = 0L;
					this.exportProgressArg.PercentageCompleted = (int)((double)this.exportProgressArg.CurrentTableIndex / (double)this.exportProgressArg.TotalTables * 100.0);
					this.ExportProgressChanged(this, this.exportProgressArg);
				}
				if (this._exportInfo.ExportTableStructure)
				{
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt("--"));
					this.textWriter.WriteLine(this.Encrypt("-- Definition of table `" + this.exportProgressArg.CurrentTableName + "`"));
					this.textWriter.WriteLine(this.Encrypt("--"));
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt("DROP TABLE IF EXISTS `" + this.exportProgressArg.CurrentTableName + "`;"));
					this.textWriter.WriteLine(this.Encrypt(this.DatabaseInfo.Tables[this.exportProgressArg.CurrentTableName].CreateTableSql));
					if (this._exportInfo.ResetAutoIncrement)
					{
						this.textWriter.WriteLine(this.Encrypt("ALTER TABLE `" + this.exportProgressArg.CurrentTableName + "` AUTO_INCREMENT = 1;"));
					}
				}
				if (this._exportInfo.ExportRows)
				{
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt("--"));
					this.textWriter.WriteLine(this.Encrypt("-- Dumping data for table `" + this.exportProgressArg.CurrentTableName + "`"));
					this.textWriter.WriteLine(this.Encrypt("--"));
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt("/*!40000 ALTER TABLE `" + this.exportProgressArg.CurrentTableName + "` DISABLE KEYS */;"));
				}
				this.textWriter.Flush();
				if (this._exportInfo.ExportRows)
				{
					try
					{
						string text2 = null;
						this._cmd.CommandText = dictionary2[this.exportProgressArg.CurrentTableName];
						this._cmd.CommandTimeout = 0;
						MySqlDataReader mySqlDataReader = this._cmd.ExecuteReader();
						System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
						while (mySqlDataReader.Read())
						{
							if (this.cancelProcess)
							{
								this.exportCompleteArg.CompletedType = ExportCompleteArg.CompleteType.Cancelled;
								return;
							}
							if (this.ExportProgressChanged != null)
							{
								this.exportProgressArg.CurrentRowInCurrentTable += 1L;
								this.exportProgressArg.CurrentRowInAllTable += 1L;
								this.ExportProgressChanged(this, this.exportProgressArg);
							}
							if (text2 == null)
							{
								int fieldCount = mySqlDataReader.FieldCount;
								string[] array = new string[fieldCount];
								for (int j = 0; j < mySqlDataReader.FieldCount; j++)
								{
									array[j] = mySqlDataReader.GetName(j);
								}
								text2 = this.GetInsertStatementHeader(this.exportProgressArg.CurrentTableName, array);
							}
							string sqlValueString = this.GetSqlValueString(mySqlDataReader);
							if (stringBuilder.Length == 0)
							{
								stringBuilder.Append(text2);
								stringBuilder.Append("\r\n");
								stringBuilder.Append(sqlValueString);
							}
							else if ((long)stringBuilder.Length + (long)sqlValueString.Length < (long)this._exportInfo.MaxSqlLength)
							{
								stringBuilder.Append(",\r\n");
								stringBuilder.Append(sqlValueString);
							}
							else
							{
								stringBuilder.Append(";");
								string value2 = this.Encrypt(stringBuilder.ToString());
								this.textWriter.WriteLine(value2);
								this.textWriter.Flush();
								stringBuilder = new System.Text.StringBuilder();
								stringBuilder.Append(text2);
								stringBuilder.Append("\r\n");
								stringBuilder.Append(sqlValueString);
							}
						}
						mySqlDataReader.Close();
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(";");
						}
						this.textWriter.WriteLine(this.Encrypt(stringBuilder.ToString()));
						this.textWriter.Flush();
					}
					catch (System.Exception ex)
					{
						throw ex;
					}
				}
				if (this._exportInfo.ExportRows)
				{
					this.textWriter.WriteLine(this.Encrypt("/*!40000 ALTER TABLE `" + this.exportProgressArg.CurrentTableName + "` ENABLE KEYS */;"));
					this.textWriter.Flush();
				}
			}
			if (this._exportInfo.ExportFunctions && this.DatabaseInfo.StoredFunction.Count != 0)
			{
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt("--"));
				this.textWriter.WriteLine(this.Encrypt("-- Dumping functions"));
				this.textWriter.WriteLine(this.Encrypt("--"));
				foreach (System.Collections.Generic.KeyValuePair<string, string> current in this.DatabaseInfo.StoredFunction)
				{
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt(string.Format("DROP FUNCTION IF EXISTS `{0}`;", current.Key)));
					this.textWriter.WriteLine(this.Encrypt("DELIMITER " + this._delimeter));
					this.textWriter.WriteLine(this.Encrypt(current.Value));
					this.textWriter.WriteLine(this.Encrypt(this._delimeter));
					this.textWriter.WriteLine(this.Encrypt("DELIMITER ;"));
					this.textWriter.WriteLine(this.Encrypt(""));
				}
				this.textWriter.Flush();
			}
			if (this._exportInfo.ExportStoredProcedures && this.DatabaseInfo.StoredProcedure.Count != 0)
			{
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt("--"));
				this.textWriter.WriteLine(this.Encrypt("-- Dumping stored procedures"));
				this.textWriter.WriteLine(this.Encrypt("--"));
				foreach (System.Collections.Generic.KeyValuePair<string, string> current in this.DatabaseInfo.StoredProcedure)
				{
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt(string.Format("DROP PROCEDURE IF EXISTS `{0}`;", current.Key)));
					this.textWriter.WriteLine(this.Encrypt("DELIMITER " + this._delimeter));
					this.textWriter.WriteLine(this.Encrypt(current.Value));
					this.textWriter.WriteLine(this.Encrypt(this._delimeter));
					this.textWriter.WriteLine(this.Encrypt("DELIMITER ;"));
					this.textWriter.WriteLine(this.Encrypt(""));
				}
				this.textWriter.Flush();
			}
			if (this._exportInfo.ExportEvents && this.DatabaseInfo.StoredEvents.Count != 0)
			{
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt("--"));
				this.textWriter.WriteLine(this.Encrypt("-- Dumping events"));
				this.textWriter.WriteLine(this.Encrypt("--"));
				foreach (System.Collections.Generic.KeyValuePair<string, string> current in this.DatabaseInfo.StoredEvents)
				{
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt(string.Format("DROP EVENT IF EXISTS `{0}`;", current.Key)));
					this.textWriter.WriteLine(this.Encrypt("DELIMITER " + this._delimeter));
					this.textWriter.WriteLine(this.Encrypt(current.Value));
					this.textWriter.WriteLine(this.Encrypt(this._delimeter));
					this.textWriter.WriteLine(this.Encrypt("DELIMITER ;"));
					this.textWriter.WriteLine(this.Encrypt(""));
				}
				this.textWriter.WriteLine(this.Encrypt("SET GLOBAL event_scheduler = ON;"));
				this.textWriter.Flush();
			}
			if (this._exportInfo.ExportViews && this.DatabaseInfo.StoredView.Count != 0)
			{
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt("--"));
				this.textWriter.WriteLine(this.Encrypt("-- Dumping views"));
				this.textWriter.WriteLine(this.Encrypt("--"));
				foreach (System.Collections.Generic.KeyValuePair<string, string> current in this.DatabaseInfo.StoredView)
				{
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt(string.Format("DROP VIEW IF EXISTS `{0}`;", current.Key)));
					this.textWriter.WriteLine(this.Encrypt(current.Value));
					this.textWriter.WriteLine(this.Encrypt(""));
				}
				this.textWriter.Flush();
			}
			if (this._exportInfo.ExportTriggers && this.DatabaseInfo.StoredTrigger.Count != 0)
			{
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt(""));
				this.textWriter.WriteLine(this.Encrypt("--"));
				this.textWriter.WriteLine(this.Encrypt("-- Dumping triggers"));
				this.textWriter.WriteLine(this.Encrypt("--"));
				foreach (System.Collections.Generic.KeyValuePair<string, string> current in this.DatabaseInfo.StoredTrigger)
				{
					this.textWriter.WriteLine(this.Encrypt(""));
					this.textWriter.WriteLine(this.Encrypt(string.Format("DROP TRIGGER IF EXISTS `{0}`;", current.Key)));
					this.textWriter.WriteLine(this.Encrypt("DELIMITER " + this._delimeter));
					this.textWriter.WriteLine(this.Encrypt(current.Value));
					this.textWriter.WriteLine(this.Encrypt(this._delimeter));
					this.textWriter.WriteLine(this.Encrypt("DELIMITER ;"));
					this.textWriter.WriteLine(this.Encrypt(""));
				}
				this.textWriter.Flush();
			}
			this.textWriter.WriteLine(this.Encrypt(""));
			this.textWriter.WriteLine(this.Encrypt(""));
			this.textWriter.WriteLine(this.Encrypt("/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;"));
			this.textWriter.WriteLine(this.Encrypt("/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;"));
			this.textWriter.Flush();
			try
			{
				this._cmd.CommandText = "SET GLOBAL max_allowed_packet = " + str + ";";
				this._cmd.ExecuteNonQuery();
			}
			catch
			{
			}
			if (this.ExportInfo.ZipOutputFile)
			{
				using (ZipFile zipFile = new ZipFile())
				{
					string fileName = System.IO.Path.GetDirectoryName(this.ExportInfo.FileName) + "\\" + System.IO.Path.GetFileNameWithoutExtension(this.ExportInfo.FileName) + ".zip";
					zipFile.CompressionLevel = CompressionLevel.BestCompression;
					zipFile.AddFile(this.ExportInfo.FileName);
					zipFile.Save(fileName);
					this.ExportInfo.FileName = fileName;
				}
			}
			this.exportCompleteArg.CompletedType = ExportCompleteArg.CompleteType.Completed;
		}

		public void CancelExport()
		{
			this.cancelProcess = true;
		}

		private string GetInsertStatementHeader(string table, string[] columnNames)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append("INSERT INTO `" + table + "` (");
			for (int i = 0; i < columnNames.Length; i++)
			{
				string str = columnNames[i];
				stringBuilder.Append("`" + str + "`,");
			}
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			stringBuilder.Append(") VALUES");
			return stringBuilder.ToString();
		}

		private string GetSqlValueString(MySqlDataReader rdr)
		{
			string result;
			try
			{
				System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
				stringBuilder.Append("(");
				for (int i = 0; i < rdr.FieldCount; i++)
				{
					object obj = rdr[i];
					if (obj == null || obj is System.DBNull)
					{
						stringBuilder.Append("NULL,");
					}
					else if (obj is string)
					{
						string arg = obj.ToString();
						this.EscapeStringSequence(ref arg);
						stringBuilder.Append(string.Format("'{0}',", arg));
					}
					else if (obj is System.DateTime)
					{
						stringBuilder.Append(string.Format("'{0}',", ((System.DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss", this.df)));
					}
					else if (obj is bool)
					{
						stringBuilder.Append(System.Convert.ToInt32(obj).ToString() + ",");
					}
					else if (obj is byte[])
					{
						stringBuilder.Append(this.methods.GetBLOBSqlDataStringFromBytes((byte[])obj) + ",");
					}
					else if (obj is short)
					{
						stringBuilder.Append(((short)obj).ToString(this.nf) + ",");
					}
					else if (obj is int)
					{
						stringBuilder.Append(((int)obj).ToString(this.nf) + ",");
					}
					else if (obj is long)
					{
						stringBuilder.Append(((long)obj).ToString(this.nf) + ",");
					}
					else if (obj is ushort)
					{
						stringBuilder.Append(((ushort)obj).ToString(this.nf) + ",");
					}
					else if (obj is uint)
					{
						stringBuilder.Append(((uint)obj).ToString(this.nf) + ",");
					}
					else if (obj is ulong)
					{
						stringBuilder.Append(((ulong)obj).ToString(this.nf) + ",");
					}
					else if (obj is double)
					{
						stringBuilder.Append(((double)obj).ToString(this.nf) + ",");
					}
					else if (obj is decimal)
					{
						stringBuilder.Append(((decimal)obj).ToString(this.nf) + ",");
					}
					else if (obj is float)
					{
						stringBuilder.Append(((float)obj).ToString(this.nf) + ",");
					}
					else if (obj is byte)
					{
						stringBuilder.Append(((byte)obj).ToString(this.nf) + ",");
					}
					else if (obj is sbyte)
					{
						stringBuilder.Append(((sbyte)obj).ToString(this.nf) + ",");
					}
					else if (obj is System.TimeSpan)
					{
						stringBuilder.Append(string.Concat(new string[]
						{
							"'",
							((System.TimeSpan)obj).Hours.ToString().PadLeft(2, '0'),
							":",
							((System.TimeSpan)obj).Minutes.ToString().PadLeft(2, '0'),
							":",
							((System.TimeSpan)obj).Seconds.ToString().PadLeft(2, '0'),
							"',"
						}));
					}
					else if (obj is MySqlDateTime)
					{
						if (((MySqlDateTime)obj).IsNull)
						{
							stringBuilder.Append("NULL,");
						}
						else
						{
							string a = this.DatabaseInfo.Tables[this.exportProgressArg.CurrentTableName].ColumnDataType[rdr.GetName(i)];
							if (((MySqlDateTime)obj).IsValidDateTime)
							{
								System.DateTime value = ((MySqlDateTime)obj).Value;
								if (a == "datetime")
								{
									stringBuilder.Append("'" + value.ToString("yyyy-MM-dd HH:mm:ss", this.df) + "',");
								}
								else if (a == "date")
								{
									stringBuilder.Append("'" + value.ToString("yyyy-MM-dd", this.df) + "',");
								}
								else if (a == "time")
								{
									stringBuilder.Append("'" + value.ToString("HH:mm:ss", this.df) + "',");
								}
							}
							else if (a == "datetime")
							{
								stringBuilder.Append("'0000-00-00 00:00:00',");
							}
							else if (a == "date")
							{
								stringBuilder.Append("'0000-00-00',");
							}
							else if (a == "time")
							{
								stringBuilder.Append("'00:00:00',");
							}
						}
					}
					else
					{
						if (!(obj is System.Guid))
						{
							throw new System.Exception("Unhandled data type. Current processing data type: " + obj.GetType().ToString() + ". Please report this bug with this message to the development team.");
						}
						string a = this.DatabaseInfo.Tables[this.exportProgressArg.CurrentTableName].ColumnDataType[rdr.GetName(i)];
						if (a == "binary(16)")
						{
							stringBuilder.Append(this.methods.GetBLOBSqlDataStringFromBytes(((System.Guid)obj).ToByteArray()) + ",");
						}
						else if (a == "char(36)")
						{
							stringBuilder.Append("'" + obj + "',");
						}
					}
				}
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
				stringBuilder.Append(")");
				result = stringBuilder.ToString();
				return result;
			}
			catch (Exception)
			{
			}
			result = "";
			return result;
		}

		public void EscapeStringSequence(ref string data)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			string text = data;
			int i = 0;
			while (i < text.Length)
			{
				char c = text[i];
				char c2 = c;
				if (c2 <= '"')
				{
					switch (c2)
					{
					case '\a':
						stringBuilder.AppendFormat("\\a", new object[0]);
						break;
					case '\b':
						stringBuilder.AppendFormat("\\b", new object[0]);
						break;
					case '\t':
						stringBuilder.AppendFormat("\\t", new object[0]);
						break;
					case '\n':
						stringBuilder.AppendFormat("\\n", new object[0]);
						break;
					case '\v':
						stringBuilder.AppendFormat("\\v", new object[0]);
						break;
					case '\f':
						stringBuilder.AppendFormat("\\f", new object[0]);
						break;
					case '\r':
						stringBuilder.AppendFormat("\\r", new object[0]);
						break;
					default:
						if (c2 != '"')
						{
							goto IL_13F;
						}
						stringBuilder.AppendFormat("\\\"", new object[0]);
						break;
					}
				}
				else if (c2 != '\'')
				{
					if (c2 != '\\')
					{
						goto IL_13F;
					}
					stringBuilder.AppendFormat("\\\\", new object[0]);
				}
				else
				{
					stringBuilder.AppendFormat("\\'", new object[0]);
				}
				IL_149:
				i++;
				continue;
				IL_13F:
				stringBuilder.Append(c);
				goto IL_149;
			}
			data = stringBuilder.ToString();
		}

		public void Import(ImportInformations importInfo)
		{
			this.ImportInfo = importInfo;
			this.Import();
		}

		public void Import()
		{
			if (this._importInfo.AsynchronousMode)
			{
				this.bwImport = new BackgroundWorker();
				this.bwImport.DoWork += new DoWorkEventHandler(this.bwImport_DoWork);
				this.bwImport.RunWorkerAsync();
			}
			else
			{
				this.ImportExecute();
			}
		}

		private void bwImport_DoWork(object sender, DoWorkEventArgs e)
		{
			this.ImportExecute();
		}

		private void ImportExecute()
		{
			try
			{
				this.ImportStart();
			}
			catch (System.Exception ex)
			{
				this.Dispose(this._importInfo.AutoCloseConnection);
				this.importCompleteArg.LastError = ex;
				this.importCompleteArg.HasErrors = true;
				this.importCompleteArg.CompletedType = ImportCompleteArg.CompleteType.Error;
				this.importCompleteArg.TimeEnd = System.DateTime.Now;
				this._importInfo.CompleteArg = this.importCompleteArg;
				if (!this._importInfo.AsynchronousMode)
				{
					throw ex;
				}
			}
			if (this.ImportProgressChanged != null)
			{
				this.importProgressArg.CurrentByte = this.importProgressArg.TotalBytes;
				this.ImportProgressChanged(this, this.importProgressArg);
			}
			this.importCompleteArg.TimeEnd = System.DateTime.Now;
			this._importInfo.CompleteArg = this.importCompleteArg;
			if (this.ImportCompleted != null)
			{
				this.ImportCompleted(this, this.importCompleteArg);
			}
		}

		private void ImportStart()
		{
			bool flag = false;
			this.cancelProcess = false;
			bool flag2 = false;
			this.InitializeInternalComponent();
			bool flag3 = false;
			string text = "";
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			this.importCompleteArg = new ImportCompleteArg(ImportCompleteArg.CompleteType.Completed);
			this.importCompleteArg.TimeStart = System.DateTime.Now;
			this.importCompleteArg.Errors = new System.Collections.Generic.Dictionary<long, System.Exception>();
			this.importProgressArg = new ImportProgressArg();
			this.importProgressArg.CurrentLineNo = 0L;
			System.IO.FileInfo fileInfo = new System.IO.FileInfo(this._importInfo.FileName);
			this.importProgressArg.TotalBytes = fileInfo.Length;
			if (this._conn == null)
			{
				throw new System.Exception("Connection has disposed. Set ImportSettings.AutoCloseConnection to false if you want to reuse this instance.");
			}
			if (this._cmd.Connection.State != ConnectionState.Open)
			{
				this._cmd.Connection.Open();
			}
			string str = "";
			try
			{
				this._cmd.CommandText = "SHOW GLOBAL VARIABLES LIKE 'max_allowed_packet';";
				MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(this._cmd);
				DataTable dataTable = new DataTable();
				mySqlDataAdapter.Fill(dataTable);
				str = string.Concat(dataTable.Rows[0]["Value"]);
				this._cmd.CommandText = "SET GLOBAL max_allowed_packet=1024*1024*256;";
				this._cmd.ExecuteNonQuery();
				this._cmd.Connection.Close();
				this._cmd.Connection.Open();
			}
			catch
			{
			}
			bool flag7 = false;
			string createTargetDatabaseSql = this._importInfo.CreateTargetDatabaseSql;
			string text2 = "";
			if (System.IO.Path.GetExtension(this.ImportInfo.FileName).ToLower() == ".zip")
			{
				using (ZipFile zipFile = new ZipFile(this.ImportInfo.FileName))
				{
					string directoryName = System.IO.Path.GetDirectoryName(this.ImportInfo.FileName);
					zipFile.ExtractAll(directoryName, ExtractExistingFileAction.OverwriteSilently);
					foreach (string current in zipFile.EntryFileNames)
					{
						text2 = directoryName + "\\" + current;
					}
				}
				fileInfo = new System.IO.FileInfo(text2);
				this.importProgressArg.TotalBytes = fileInfo.Length;
			}
			else
			{
				text2 = this.ImportInfo.FileName;
			}
			if (createTargetDatabaseSql != "")
			{
				try
				{
					this._cmd.CommandText = createTargetDatabaseSql;
					this._cmd.ExecuteNonQuery();
					this._cmd.CommandText = string.Format("USE `{0}`;", this._importInfo.TargetDatabase);
					this._cmd.ExecuteNonQuery();
				}
				catch (System.Exception ex)
				{
					throw new System.Exception(ex.Message + ". Fail to create or use database.", ex);
				}
				flag7 = true;
			}
			this.textReader = new System.IO.StreamReader(text2, this.utf8WithoutBOM, true);
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			string text3 = "";
			while (text3 != null)
			{
				try
				{
					if (this.cancelProcess)
					{
						this.importCompleteArg.CompletedType = ImportCompleteArg.CompleteType.Cancelled;
						this.importCompleteArg.CurrentLineNo = this.importProgressArg.CurrentLineNo;
						this.Dispose(this._importInfo.AutoCloseConnection);
						return;
					}
					text3 = this.textReader.ReadLine();
					this.importCompleteArg.CurrentLineNo += 1L;
					if (text3 != null)
					{
						if (this.ImportProgressChanged != null)
						{
							this.importProgressArg.Error = null;
							this.importProgressArg.CurrentByte += (long)text3.Length;
							if (this.importProgressArg.CurrentByte != 0L && this.importProgressArg.TotalBytes != 0L)
							{
								this.importProgressArg.PercentageCompleted = (int)((double)this.importProgressArg.CurrentByte / (double)this.importProgressArg.TotalBytes * 100.0);
							}
							this.ImportProgressChanged(this, this.importProgressArg);
						}
						if (text3.Trim().Length != 0)
						{
							text3 = this.Decrypt(text3).TrimEnd(new char[0]);
							if (text3.Length != 0)
							{
								if (!(text3 == "\r\n"))
								{
									if (!text3.StartsWith("--"))
									{
										if (!flag4)
										{
											if (text3.StartsWith("/*!40101 SET NAMES ") || text3.StartsWith("SET NAMES "))
											{
												this._cmd.CommandText = "SHOW VARIABLES LIKE 'character_set_database';";
												MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(this._cmd);
												DataTable dataTable2 = new DataTable();
												mySqlDataAdapter.Fill(dataTable2);
												string str2 = string.Concat(dataTable2.Rows[0][1]);
												this._cmd.CommandText = "SET NAMES " + str2 + ";";
												this._cmd.ExecuteNonQuery();
												flag4 = true;
												continue;
											}
										}
										if (flag7)
										{
											if (!flag5)
											{
												if (text3.StartsWith("CREATE DATABASE"))
												{
													flag5 = true;
													continue;
												}
											}
											if (!flag6)
											{
												if (text3.StartsWith("USE "))
												{
													flag6 = true;
													continue;
												}
											}
										}
										stringBuilder.Append(text3);
										if (text3.StartsWith("DELIMITER"))
										{
											string text4 = text3.Replace("DELIMITER ", string.Empty);
											text4 = text4.Replace(" ", string.Empty);
											text = text4;
											if (text == ";")
											{
												stringBuilder = new System.Text.StringBuilder();
												flag3 = false;
											}
											else
											{
												flag3 = true;
												stringBuilder.Append("\r\n");
											}
										}
										else if (flag3)
										{
											stringBuilder.Append("\r\n");
											if (text3.Contains(text))
											{
												stringBuilder.Append(text);
												MySqlScript mySqlScript = new MySqlScript(this._conn, stringBuilder.ToString());
												mySqlScript.Execute();
												if (!flag2)
												{
													flag2 = true;
												}
												stringBuilder = new System.Text.StringBuilder();
												flag3 = false;
											}
										}
										else if (text3.EndsWith(";"))
										{
											if (text3.TrimStart(new char[0]).StartsWith("DELIMETER"))
											{
												stringBuilder = new System.Text.StringBuilder();
											}
											else
											{
												this._cmd.CommandText = stringBuilder.ToString();
												this._cmd.ExecuteNonQuery();
												if (!flag4 || !flag5 || !flag6)
												{
													if (this._cmd.CommandText.StartsWith("CREATE TABLE") || this._cmd.CommandText.StartsWith("INSERT"))
													{
														flag4 = true;
														flag5 = true;
														flag6 = true;
													}
												}
												if (!flag2)
												{
													flag2 = true;
												}
												stringBuilder = new System.Text.StringBuilder();
											}
										}
										else if (!flag && !flag2)
										{
											string text5 = text3.ToLower();
											if (!text5.StartsWith("/*!4") && !text5.StartsWith("drop") && !text5.StartsWith("create") && !text5.StartsWith("delimeter") && !text5.StartsWith("insert"))
											{
												throw new System.Exception("This is not a valid SQL Dump File. No executeable SQL query found.");
											}
											flag = true;
										}
									}
								}
							}
						}
					}
				}
				catch (System.Exception ex)
				{
					flag3 = false;
					this.importProgressArg.ErrorSql = stringBuilder.ToString();
					this.importProgressArg.Error = ex;
					this.importCompleteArg.HasErrors = true;
					this.importCompleteArg.LastError = ex;
					this.importCompleteArg.Errors.Add(this.importProgressArg.CurrentLineNo, ex);
					stringBuilder = new System.Text.StringBuilder();
					if (!this._importInfo.IgnoreSqlError)
					{
						throw ex;
					}
					if (this.ImportProgressChanged != null)
					{
						this.ImportProgressChanged(this, this.importProgressArg);
					}
				}
			}
			this.textReader.Close();
			this.importProgressArg.CurrentByte = this.importProgressArg.TotalBytes;
			this.importProgressArg.PercentageCompleted = 100;
			if (this.ImportProgressChanged != null)
			{
				this.ImportProgressChanged(this, this.importProgressArg);
			}
			if (!flag2)
			{
				throw new System.Exception("This is not a valid SQL Dump File. No executeable SQL query found.");
			}
			try
			{
				this._cmd.CommandText = "SET GLOBAL max_allowed_packet = " + str + ";";
				this._cmd.ExecuteNonQuery();
			}
			catch
			{
			}
			this.importCompleteArg.CompletedType = ImportCompleteArg.CompleteType.Completed;
		}

		public void CancelImport()
		{
			this.cancelProcess = true;
		}

		private string Encrypt(string input)
		{
			string result;
			if (this._exportInfo.EnableEncryption)
			{
				if (input == null || input.Length == 0)
				{
					result = this.methods.EncryptWithSalt("-- ||||" + this.methods.RandomString(this.methods.random.Next(100, 500)), this._exportInfo.EncryptionKey, this._exportInfo.SaltSize);
				}
				else
				{
					result = this.methods.EncryptWithSalt(input, this._exportInfo.EncryptionKey, this._exportInfo.SaltSize);
				}
			}
			else
			{
				result = input;
			}
			return result;
		}

		private string Decrypt(string input)
		{
			string result;
			if (input == null || input.Length == 0)
			{
				result = input;
			}
			else if (this._importInfo.EnableEncryption)
			{
				string text = this.methods.DecryptWithSalt(input, this._importInfo.EncryptionKey, this._importInfo.SaltSize);
				if (text == "-- ||||")
				{
					result = "";
				}
				else
				{
					result = text;
				}
			}
			else
			{
				result = input;
			}
			return result;
		}

		public void DeleteAllRows(bool resetAutoIncrement)
		{
			this.DeleteAllRows(resetAutoIncrement, null);
		}

		public void DeleteAllRows(bool resetAutoIncrement, string[] excludeTables)
		{
			if (this._conn.State != ConnectionState.Open)
			{
				this._conn.Open();
			}
			string[] tableNames = this._database.TableNames;
			string[] array = tableNames;
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				bool flag = false;
				if (excludeTables != null && excludeTables.Length > 0)
				{
					for (int j = 0; j < excludeTables.Length; j++)
					{
						string a = excludeTables[j];
						if (a == text)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					this._cmd.CommandText = "DELETE FROM `" + text + "`;";
					this._cmd.ExecuteNonQuery();
					if (resetAutoIncrement)
					{
						this._cmd.CommandText = "ALTER TABLE `" + text + "` AUTO_INCREMENT = 1;";
						this._cmd.ExecuteNonQuery();
					}
				}
			}
		}

		public void DecryptSqlDumpFile(string originalFile, string newFile, string encryptionKey)
		{
			this.methods = new Methods();
			encryptionKey = this.methods.Sha2Hash(encryptionKey);
			int saltSize = this.methods.GetSaltSize(encryptionKey);
			if (!System.IO.File.Exists(originalFile))
			{
				throw new System.Exception("Original file is not exists.");
			}
			this.textReader = new System.IO.StreamReader(originalFile, this.utf8WithoutBOM);
			if (System.IO.File.Exists(newFile))
			{
				System.IO.File.Delete(newFile);
			}
			string text = "";
			bool flag = true;
			while (text != null)
			{
				text = this.textReader.ReadLine();
				if (text != null)
				{
					text = this.methods.DecryptWithSalt(text, encryptionKey, saltSize);
					if (text.StartsWith("-- ||||"))
					{
						text = "";
					}
                    TextWriter textWriter = new System.IO.StreamWriter(newFile, !flag, this.utf8WithoutBOM);
					textWriter.WriteLine(text);
					textWriter.Close();
					flag = false;
				}
			}
			this.methods = null;
		}

		public void EncryptSqlDumpFile(string originalFile, string newFile, string encryptionKey)
		{
			this.methods = new Methods();
			encryptionKey = this.methods.Sha2Hash(encryptionKey);
			int saltSize = this.methods.GetSaltSize(encryptionKey);
			if (!System.IO.File.Exists(originalFile))
			{
				throw new System.Exception("Original file is not exists.");
			}
			this.textReader = new System.IO.StreamReader(originalFile, this.utf8WithoutBOM);
			if (System.IO.File.Exists(newFile))
			{
				System.IO.File.Delete(newFile);
			}
			string text = "";
			bool flag = true;
			while (text != null)
			{
				text = this.textReader.ReadLine();
				if (text != null)
				{
					if ((text ?? "") == "")
					{
						text = "-- ||||" + this.methods.RandomString(this.methods.random.Next(50, 300));
					}
					text = this.methods.EncryptWithSalt(text, encryptionKey, saltSize);
                    TextWriter textWriter = new System.IO.StreamWriter(newFile, !flag, this.utf8WithoutBOM);
					textWriter.WriteLine(text);
					textWriter.Close();
					flag = false;
				}
			}
			this.methods = null;
		}

		public void ExportBlobAsFile(string targetSaveFolder, string table, string colBlob, string colFileName, string colFilesize)
		{
			try
			{
				if (this._cmd.Connection.State != ConnectionState.Open)
				{
					this._cmd.Connection.Open();
				}
				string commandText = string.Concat(new string[]
				{
					"select `",
					colFileName,
					"`, `",
					colFilesize,
					"`, `",
					colBlob,
					"` from `",
					table,
					"`;"
				});
				this._cmd.CommandText = commandText;
				MySqlDataReader mySqlDataReader = this._cmd.ExecuteReader();
				if (!mySqlDataReader.HasRows)
				{
					throw new System.Exception("There are no BLOBs to save");
				}
				while (mySqlDataReader.Read())
				{
					uint uInt = mySqlDataReader.GetUInt32(mySqlDataReader.GetOrdinal(colFilesize));
					byte[] buffer = new byte[uInt];
					mySqlDataReader.GetBytes(mySqlDataReader.GetOrdinal(colBlob), 0L, buffer, 0, (int)uInt);
					if (!System.IO.Directory.Exists(targetSaveFolder))
					{
						System.IO.Directory.CreateDirectory(targetSaveFolder);
					}
					using (System.IO.FileStream fileStream = new System.IO.FileStream(targetSaveFolder + "\\" + mySqlDataReader[colFileName], System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
					{
						fileStream.Write(buffer, 0, (int)uInt);
						fileStream.Close();
					}
				}
				mySqlDataReader.Close();
			}
			catch (System.Exception ex)
			{
				this.Dispose();
				throw ex;
			}
			finally
			{
				this.Dispose();
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		public void Dispose(bool disposeConnection)
		{
			if (this.textReader != null)
			{
				this.textReader.Close();
			}
			if (this.textWriter != null)
			{
				this.textWriter.Close();
			}
			this.methods = null;
			this.textReader = null;
			this.textWriter = null;
			if (disposeConnection)
			{
				try
				{
					if (this._conn != null)
					{
						if (this._conn.State != ConnectionState.Closed)
						{
							this._conn.Close();
						}
					}
				}
				catch
				{
				}
				finally
				{
					this._conn = null;
					this._cmd = null;
				}
			}
		}
	}
}
