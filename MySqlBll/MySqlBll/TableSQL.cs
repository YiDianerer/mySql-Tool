using System;
using System.Data;
using System.Data.SqlClient;

namespace MySqlBll
{
	public class TableSQL
	{
		private string m_ConnString = "";

		public TableSQL(string connstring)
		{
			this.m_ConnString = connstring;
		}

		public DataTable Get(string sql)
		{
			DataTable dt = new DataTable();
			using (SqlConnection conn = new SqlConnection(this.m_ConnString))
			{
				conn.Open();
				SqlDataAdapter sda = new SqlDataAdapter(sql, conn);
				sda.Fill(dt);
			}
			return dt;
		}

		public int ExecuteNonQuery(string sql)
		{
			int result;
			using (SqlConnection conn = new SqlConnection(this.m_ConnString))
			{
				conn.Open();
				SqlCommand comm = new SqlCommand(sql, conn);
				result = comm.ExecuteNonQuery();
			}
			return result;
		}
	}
}
