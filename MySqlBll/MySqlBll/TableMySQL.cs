using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace MySqlBll
{
	public class TableMySQL
	{
		private string m_ConnString = "";

		public TableMySQL(string connstring)
		{
			this.m_ConnString = connstring;
		}

		public DataTable Get(string sql)
		{
			DataTable result;
			try
			{
				DataTable dt = new DataTable();
				using (MySqlConnection conn = new MySqlConnection(this.m_ConnString))
				{
					conn.Open();
					new MySqlDataAdapter(sql, conn)
					{
						SelectCommand = 
						{
							CommandTimeout = 0
						}
					}.Fill(dt);
				}
				result = dt;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return result;
		}

		public int ExecuteNonQuery(string sql)
		{
			int result;
			using (MySqlConnection conn = new MySqlConnection(this.m_ConnString))
			{
				conn.Open();
				result = new MySqlCommand(sql, conn)
				{
					CommandTimeout = 6000
				}.ExecuteNonQuery();
			}
			return result;
		}

		public object ExecuteScalar(string sql)
		{
			object result;
			using (MySqlConnection conn = new MySqlConnection(this.m_ConnString))
			{
				conn.Open();
				MySqlCommand comm = new MySqlCommand(sql, conn);
				result = comm.ExecuteScalar();
			}
			return result;
		}

		public void Update(DataTable dt, string commandText)
		{
			this.Update(dt, commandText, null);
		}

		public void Update(DataTable dt, string commandText, object[,] parameters)
		{
			using (MySqlConnection conn = new MySqlConnection(this.m_ConnString))
			{
				conn.Open();
				MySqlTransaction trans = conn.BeginTransaction();
				MySqlCommand comm = new MySqlCommand(commandText, conn, trans);
				MySqlDataAdapter dap = new MySqlDataAdapter(comm);
				MySqlParameter[] parames = this.GetParameter(parameters);
				if (parames != null)
				{
					dap.SelectCommand.Parameters.AddRange(parames);
				}
				MySqlCommandBuilder cbd = new MySqlCommandBuilder(dap);
				try
				{
					dap.Update(dt);
					trans.Commit();
				}
				catch (Exception ex)
				{
					trans.Rollback();
					throw ex;
				}
				finally
				{
				}
			}
		}

		public MySqlParameter[] GetParameter(object[,] parameters)
		{
			MySqlParameter[] result;
			if (parameters == null)
			{
				result = null;
			}
			else
			{
				int parametersLength = parameters.GetLength(0);
				MySqlParameter[] parames = new MySqlParameter[parametersLength];
				for (int i = 0; i < parametersLength; i++)
				{
					object parameterValue = parameters[i, 1];
					if (parameterValue == null)
					{
						parames[i] = new MySqlParameter(parameters[i, 0].ToString(), DBNull.Value);
					}
					else if (parameterValue.GetType() == typeof(byte[]) && ((byte[])parameterValue).Length == 0)
					{
						parames[i] = new MySqlParameter(parameters[i, 0].ToString(), SqlDbType.Image);
						parames[i].Value = DBNull.Value;
					}
					else if (parameterValue.GetType() == typeof(DateTime) && (DateTime)parameterValue < new DateTime(1753, 1, 1))
					{
						parames[i] = new MySqlParameter(parameters[i, 0].ToString(), new DateTime(1753, 1, 1));
					}
					else
					{
						parames[i] = new MySqlParameter(parameters[i, 0].ToString(), parameterValue);
					}
					parames[i].ParameterName = parames[i].ParameterName.Replace("@", "?");
				}
				result = parames;
			}
			return result;
		}
	}
}
