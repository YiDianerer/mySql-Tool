using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MySql.Data.MySqlClient
{
	public class Methods
	{
		public System.Random random = new System.Random((int)System.DateTime.Now.Ticks);

		public string GetCreateTableSql(string table, MySqlCommand cmd)
		{
			cmd.CommandText = "SHOW CREATE TABLE `" + table + "`;";
			MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
			DataTable dataTable = new DataTable();
			mySqlDataAdapter.Fill(dataTable);
			string text = "";
			object obj = dataTable.Rows[0][1];
			if (obj is string)
			{
				text = string.Concat(obj);
			}
			else if (obj is byte[])
			{
				System.Text.UTF8Encoding uTF8Encoding = new System.Text.UTF8Encoding();
				text = uTF8Encoding.GetString((byte[])obj);
			}
			return text.Replace("CREATE TABLE", "CREATE TABLE IF NOT EXISTS").Replace("\n", "\r\n") + ";";
		}

		public string GetCreateDatabaseSql(MySqlCommand cmd)
		{
			cmd.CommandText = "SELECT DATABASE();";
			string str = cmd.ExecuteScalar().ToString();
			cmd.CommandText = "SHOW CREATE DATABASE `" + str + "`;";
			MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
			DataTable dataTable = new DataTable();
			mySqlDataAdapter.Fill(dataTable);
			return dataTable.Rows[0][1].ToString().Replace("CREATE DATABASE", "CREATE DATABASE IF NOT EXISTS") + ";";
		}

		public Dictionary<string, long> GetTotalRowsInTables(string[] tableNames, ref MySqlCommand cmd)
		{
            Dictionary<string, long> dictionary = new Dictionary<string, long>();
			for (int i = 0; i < tableNames.Length; i++)
			{
				string text = tableNames[i];
				dictionary.Add(text, this.GetTotalRowsInTable(text, cmd));
			}
			return dictionary;
		}

		public long GetTotalRowsInTable(string tableName, MySqlCommand cmd)
		{
			cmd.CommandText = "SELECT COUNT(*) FROM `" + tableName + "`;";
			return (long)cmd.ExecuteScalar();
		}

		public string[] GetColumnNames(string table, MySqlCommand cmd)
		{
			cmd.CommandText = "SHOW COLUMNS FROM `" + table + "`;";
			MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
			DataTable dataTable = new DataTable();
			mySqlDataAdapter.Fill(dataTable);
			string[] array = new string[dataTable.Rows.Count];
			int num = -1;
			foreach (DataRow dataRow in dataTable.Rows)
			{
				num++;
				array[num] = dataRow[0].ToString();
			}
			return array;
		}

		public string[] GetTableNames(ref MySqlCommand cmd)
		{
			cmd.CommandText = "SHOW FULL TABLES WHERE Table_type LIKE 'BASE TABLE';";
			MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
			DataTable dataTable = new DataTable();
			mySqlDataAdapter.Fill(dataTable);
			string[] array = new string[dataTable.Rows.Count];
			int num = -1;
			foreach (DataRow dataRow in dataTable.Rows)
			{
				num++;
				array[num] = string.Concat(dataRow[0]);
			}
			return array;
		}

		public string GetServerVersion(ref MySqlCommand cmd, ref string version)
		{
			cmd.CommandText = "SHOW variables WHERE Variable_name = 'version';";
			MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
			DataTable dataTable = new DataTable();
			mySqlDataAdapter.Fill(dataTable);
			version = dataTable.Rows[0][1].ToString();
			cmd.CommandText = "SHOW variables WHERE Variable_name = 'version_comment';";
			mySqlDataAdapter = new MySqlDataAdapter(cmd);
			dataTable = new DataTable();
			mySqlDataAdapter.Fill(dataTable);
			return version + " " + dataTable.Rows[0][1].ToString();
		}

		public long GetServerMaxAllowedPacket(ref MySqlCommand cmd)
		{
			cmd.CommandText = "SHOW variables WHERE Variable_name = 'max_allowed_packet';";
			MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(cmd);
			DataTable dataTable = new DataTable();
			mySqlDataAdapter.Fill(dataTable);
			return System.Convert.ToInt64(dataTable.Rows[0][1]);
		}

		public string GetDatabaseName(string ConnectionString)
		{
			string[] array = ConnectionString.Split(new char[]
			{
				';'
			});
			string[] array2 = array;
			int i = 0;
			while (i < array2.Length)
			{
				string text = array2[i];
				string result;
				if (text.ToLower().StartsWith("database"))
				{
					string[] array3 = text.Split(new char[]
					{
						'='
					});
					result = array3[1];
				}
				else
				{
					if (!text.ToLower().StartsWith("initial catalog"))
					{
						i++;
						continue;
					}
					string[] array3 = text.Split(new char[]
					{
						'='
					});
					result = array3[1];
				}
				return result;
			}
			throw new System.Exception("Database Name is not detected in Connection String.");
		}

		public string GetBLOBSqlDataStringFromBytes(byte[] ba)
		{
			string result;
			if (ba.Length == 0)
			{
				result = "NULL";
			}
			else
			{
				char[] array = new char[ba.Length * 2 + 2];
				array[0] = '0';
				array[1] = 'x';
				int i = 0;
				int num = 2;
				while (i < ba.Length)
				{
					byte b = (byte)(ba[i] >> 4);
					array[num] = (char)((b > 9) ? (b + 55) : (b + 48));
					b = (byte)(ba[i] & 15);
					array[++num] = (char)((b > 9) ? (b + 55) : (b + 48));
					i++;
					num++;
				}
				result = new string(array);
			}
			return result;
		}

		public string EncryptWithSalt(string input, string key, int saltSize)
		{
			string text = this.RandomString(saltSize);
			return text + this.AES_Encrypt(input, key + text);
		}

		public string DecryptWithSalt(string input, string key, int saltSize)
		{
			string result;
			try
			{
				string str = input.Substring(0, saltSize);
				string input2 = input.Substring(saltSize);
				result = this.AES_Decrypt(input2, key + str);
			}
			catch
			{
				throw new System.Exception("Invalid Key.");
			}
			return result;
		}

		public string RandomString(int size)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			for (int i = 0; i < size; i++)
			{
				char value = System.Convert.ToChar(System.Convert.ToInt32(System.Math.Floor(26.0 * this.random.NextDouble() + 65.0)));
				stringBuilder.Append(value);
			}
			return this.AES_Encrypt(stringBuilder.ToString(), this.random.Next(1, 1000000).ToString()).Substring(0, size);
		}

		public int GetSaltSize(string key)
		{
			int hashCode = key.GetHashCode();
			string text = System.Convert.ToString(hashCode);
			char[] array = text.ToCharArray();
			int num = 0;
			char[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				char c = array2[i];
				if (char.IsNumber(c))
				{
					num += System.Convert.ToInt32(c.ToString());
				}
			}
			return num;
		}

		public string Sha1Hash(string input)
		{
			System.Security.Cryptography.SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new System.Security.Cryptography.SHA1CryptoServiceProvider();
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
			byte[] value = sHA1CryptoServiceProvider.ComputeHash(bytes);
			return System.BitConverter.ToString(value).Replace("-", string.Empty).ToLower();
		}

		public string Sha2Hash(string input)
		{
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
			return this.Sha2Hash(bytes);
		}

		public string Sha2Hash(byte[] ba)
		{
			System.Security.Cryptography.SHA256Managed sHA256Managed = new System.Security.Cryptography.SHA256Managed();
			byte[] value = sHA256Managed.ComputeHash(ba);
			return System.BitConverter.ToString(value).Replace("-", string.Empty).ToLower();
		}

		public string AES_Encrypt(string input, string password)
		{
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
			System.Security.Cryptography.PasswordDeriveBytes passwordDeriveBytes = new System.Security.Cryptography.PasswordDeriveBytes(password, new byte[]
			{
				73,
				118,
				97,
				110,
				32,
				77,
				101,
				100,
				118,
				101,
				100,
				101,
				118
			});
			byte[] inArray = Methods.AES_Encrypt(bytes, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
			return System.Convert.ToBase64String(inArray);
		}

		public string AES_Decrypt(string input, string password)
		{
			byte[] cipherData = System.Convert.FromBase64String(input);
			System.Security.Cryptography.PasswordDeriveBytes passwordDeriveBytes = new System.Security.Cryptography.PasswordDeriveBytes(password, new byte[]
			{
				73,
				118,
				97,
				110,
				32,
				77,
				101,
				100,
				118,
				101,
				100,
				101,
				118
			});
			byte[] bytes = Methods.AES_Decrypt(cipherData, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
			return System.Text.Encoding.UTF8.GetString(bytes);
		}

		private static byte[] AES_Encrypt(byte[] clearData, byte[] Key, byte[] IV)
		{
			System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
			System.Security.Cryptography.Rijndael rijndael = System.Security.Cryptography.Rijndael.Create();
			rijndael.Key = Key;
			rijndael.IV = IV;
			System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, rijndael.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write);
			cryptoStream.Write(clearData, 0, clearData.Length);
			cryptoStream.Close();
			return memoryStream.ToArray();
		}

		private static byte[] AES_Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
		{
			System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
			System.Security.Cryptography.Rijndael rijndael = System.Security.Cryptography.Rijndael.Create();
			rijndael.Key = Key;
			rijndael.IV = IV;
			System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, rijndael.CreateDecryptor(), System.Security.Cryptography.CryptoStreamMode.Write);
			cryptoStream.Write(cipherData, 0, cipherData.Length);
			cryptoStream.Close();
			return memoryStream.ToArray();
		}
	}
}
