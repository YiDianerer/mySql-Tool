using System;
using System.Windows.Forms;

namespace MySqlTool
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (new frmLoading().ShowDialog() == DialogResult.OK)
			{
				Application.Run(new frmMain());
			}
		}
	}
}
