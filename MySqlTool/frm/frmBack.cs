using MySql.Data.MySqlClient;
using MySqlTool.Class;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MySqlTool
{
	public class frmBack : frmBase
	{
		private MySqlBackup mb;

		private Methods md = new Methods();

		private Timer TimerExport;

		private bool TimerStopExport = true;

		private string m_FinishedMessage = "";

		private string m_constr;

		private Speed m_Speed = new Speed();

		private string CurrentTableName = "";

		private long TotalRowsInCurrentTable = 0L;

		private long TotalRowsInAllTables = 0L;

		private long CurrentRowInCurrentTable = 0L;

		private long CurrentRowInAllTable = 0L;

		private int TotalTables = 0;

		private int CurrentTableIndex = 0;

		private int PercentageComplete = 0;

		private int PercentageGetTotalRowsCompleted = 0;

		private string m_dbname = "";

		private IContainer components = null;

		private GroupBox groupBox1;

		private ProgressBar pgGetTotalRows;

		private Label lbTotalTableDB;

		private Label lbGetTotalRows;

		private Label lbCurrentTable;

		private ProgressBar pbRowsTable;

		private Label label9;

		private Label label4;

		private ProgressBar pbTable;

		private Label lbPercentage;

		private Label lbRowProgressDB;

		private Label label5;

		private Label label8;

		private ProgressBar pbPercent;

		private ProgressBar pbRowsDB;

		private Label lbRowProgressCurTable;

		private TextBox txtFile;

		private Button btnSelect;

		private Button btnBack;

		private Label label1;

		private Label labSpeed;

		private Label labTime;

		private Label label3;

		public frmBack(string constr, string _dbname)
		{
			this.InitializeComponent();
			this.m_constr = constr;
			this.m_dbname = _dbname;
			this.txtFile.Text = Config.BackPath + this.m_dbname + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".sql";
		}

		private void btnSelect_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.FileName = this.txtFile.Text;
			saveFileDialog.Filter = "*.sql|*.sql";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				this.txtFile.Text = saveFileDialog.FileName;
			}
		}

		private void btnBack_Click(object sender, EventArgs e)
		{
			this.btnBack.Enabled = false;
			this.mb = new MySqlBackup(this.m_constr);
			this.mb.ExportInfo.FileName = this.txtFile.Text;
			this.mb.ExportInfo.AsynchronousMode = true;
			this.mb.ExportInfo.CalculateTotalRowsFromDatabase = true;
			this.mb.ExportInfo.ZipOutputFile = false;
			this.TimerExport = new Timer();
			this.TimerExport.Interval = 1000;
			this.TimerExport.Tick += new EventHandler(this.TimerExport_Tick);
			this.TimerExport.Start();
			this.TimerStopExport = false;
			this.mb.ExportProgressChanged += new MySqlBackup.exportProgressChange(this.mb_ExportProgressChanged);
			this.mb.ExportCompleted += new MySqlBackup.exportComplete(this.mb_ExportCompleted);
			try
			{
				this.mb.Export();
			}
			catch
			{
				this.ShowExportCompleteMessage(this.mb.ExportInfo.CompleteArg);
			}
		}

		private void TimerExport_Tick(object sender, EventArgs e)
		{
			if (this.PercentageGetTotalRowsCompleted == 100)
			{
				this.lbGetTotalRows.Text = "100%. Total Rows Calculation Completed. Begin backup process...";
			}
			else if (this.PercentageGetTotalRowsCompleted == 0)
			{
				this.lbGetTotalRows.Text = "";
			}
			else
			{
				this.lbGetTotalRows.Text = string.Concat(new string[]
				{
					"Calculating Total Rows... ",
					this.CurrentTableName,
					"-",
					this.PercentageGetTotalRowsCompleted.ToString(),
					"% completed."
				});
			}
			this.pgGetTotalRows.Value = this.PercentageGetTotalRowsCompleted;
			this.pbPercent.Value = this.PercentageComplete;
			this.lbPercentage.Text = this.PercentageComplete + "% Completed";
			this.lbCurrentTable.Text = this.CurrentTableName;
			this.lbRowProgressCurTable.Text = this.CurrentRowInCurrentTable + " / " + this.TotalRowsInCurrentTable;
			this.lbRowProgressDB.Text = this.CurrentRowInAllTable + " / " + this.TotalRowsInAllTables;
			this.lbTotalTableDB.Text = this.CurrentTableIndex + " / " + this.TotalTables;
			this.pbTable.Maximum = this.TotalTables * 10;
			this.pbTable.Value = this.CurrentTableIndex * 10;
			this.m_Speed.Add(this.CurrentRowInAllTable);
			this.labSpeed.Text = this.m_Speed.CurrSpeed + "(行/秒)";
			if (this.TotalRowsInAllTables != 0L)
			{
				try
				{
					int num = Convert.ToInt32((this.TotalRowsInAllTables - this.CurrentRowInAllTable) / this.m_Speed.CurrSpeed);
					this.labTime.Text = num.ToString() + "(秒)";
					this.pbRowsDB.Maximum = (int)this.TotalRowsInAllTables;
					this.pbRowsDB.Value = (int)this.CurrentRowInAllTable;
					this.pbRowsTable.Maximum = (int)this.TotalRowsInCurrentTable;
					this.pbRowsTable.Value = (int)this.CurrentRowInCurrentTable;
				}
				catch
				{
				}
			}
			if (this.TimerStopExport)
			{
				this.btnBack.Enabled = true;
				this.TimerExport.Stop();
				MessageBox.Show(this.m_FinishedMessage, "完成");
			}
		}

		private void mb_ExportCompleted(object sender, ExportCompleteArg e)
		{
			this.ShowExportCompleteMessage(e);
		}

		private void ShowExportCompleteMessage(ExportCompleteArg e)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Export " + e.CompletedType.ToString() + "\r\n");
			stringBuilder.Append("Time Start: " + e.TimeStart.ToString() + "\r\n");
			stringBuilder.Append("Time End: " + e.TimeEnd.ToString() + "\r\n");
			stringBuilder.Append(string.Concat(new object[]
			{
				"Time Used: ",
				e.TimeUsed.Minutes,
				" m ",
				e.TimeUsed.Seconds,
				" s ",
				e.TimeUsed.Milliseconds,
				" ms\r\n"
			}));
			if (e.Error != null)
			{
				stringBuilder.Append("\r\nError Message:\r\n\r\n");
				stringBuilder.Append(e.Error.ToString());
			}
			this.m_FinishedMessage = stringBuilder.ToString();
			this.TimerStopExport = true;
		}

		private void mb_ExportProgressChanged(object sender, ExportProgressArg e)
		{
			this.PercentageGetTotalRowsCompleted = e.PercentageGetTotalRowsCompleted;
			this.PercentageComplete = e.PercentageCompleted;
			this.CurrentTableName = e.CurrentTableName;
			this.TotalRowsInCurrentTable = e.TotalRowsInCurrentTable;
			this.TotalRowsInAllTables = e.TotalRowsInAllTables;
			this.CurrentRowInCurrentTable = e.CurrentRowInCurrentTable;
			this.CurrentRowInAllTable = e.CurrentRowInAllTable;
			this.TotalTables = e.TotalTables;
			this.CurrentTableIndex = e.CurrentTableIndex;
			this.PercentageGetTotalRowsCompleted = e.PercentageGetTotalRowsCompleted;
			this.PercentageComplete = e.PercentageCompleted;
			this.CurrentTableName = e.CurrentTableName;
			this.TotalRowsInCurrentTable = e.TotalRowsInCurrentTable;
			this.TotalRowsInAllTables = e.TotalRowsInAllTables;
			this.CurrentRowInCurrentTable = e.CurrentRowInCurrentTable;
			this.CurrentRowInAllTable = e.CurrentRowInAllTable;
			this.TotalTables = e.TotalTables;
			this.CurrentTableIndex = e.CurrentTableIndex;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.btnBack = new Button();
			this.btnSelect = new Button();
			this.txtFile = new TextBox();
			this.groupBox1 = new GroupBox();
			this.labTime = new Label();
			this.label3 = new Label();
			this.labSpeed = new Label();
			this.label1 = new Label();
			this.pgGetTotalRows = new ProgressBar();
			this.lbTotalTableDB = new Label();
			this.lbGetTotalRows = new Label();
			this.lbCurrentTable = new Label();
			this.pbRowsTable = new ProgressBar();
			this.label9 = new Label();
			this.label4 = new Label();
			this.pbTable = new ProgressBar();
			this.lbPercentage = new Label();
			this.lbRowProgressDB = new Label();
			this.label5 = new Label();
			this.label8 = new Label();
			this.pbPercent = new ProgressBar();
			this.pbRowsDB = new ProgressBar();
			this.lbRowProgressCurTable = new Label();
			this.groupBox1.SuspendLayout();
			base.SuspendLayout();
			this.btnBack.Location = new Point(552, 14);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new Size(75, 23);
			this.btnBack.TabIndex = 76;
			this.btnBack.Text = "备份";
			this.btnBack.UseVisualStyleBackColor = true;
			this.btnBack.Click += new EventHandler(this.btnBack_Click);
			this.btnSelect.Location = new Point(471, 14);
			this.btnSelect.Name = "btnSelect";
			this.btnSelect.Size = new Size(75, 23);
			this.btnSelect.TabIndex = 75;
			this.btnSelect.Text = "选择";
			this.btnSelect.UseVisualStyleBackColor = true;
			this.btnSelect.Click += new EventHandler(this.btnSelect_Click);
			this.txtFile.Location = new Point(12, 14);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new Size(452, 21);
			this.txtFile.TabIndex = 74;
			this.groupBox1.Controls.Add(this.labTime);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.labSpeed);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.pgGetTotalRows);
			this.groupBox1.Controls.Add(this.lbTotalTableDB);
			this.groupBox1.Controls.Add(this.lbGetTotalRows);
			this.groupBox1.Controls.Add(this.lbCurrentTable);
			this.groupBox1.Controls.Add(this.pbRowsTable);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.pbTable);
			this.groupBox1.Controls.Add(this.lbPercentage);
			this.groupBox1.Controls.Add(this.lbRowProgressDB);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.pbPercent);
			this.groupBox1.Controls.Add(this.pbRowsDB);
			this.groupBox1.Controls.Add(this.lbRowProgressCurTable);
			this.groupBox1.Location = new Point(12, 45);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new Size(621, 223);
			this.groupBox1.TabIndex = 73;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "备份进度";
			this.labTime.AutoSize = true;
			this.labTime.Location = new Point(396, 78);
			this.labTime.Name = "labTime";
			this.labTime.Size = new Size(41, 12);
			this.labTime.TabIndex = 75;
			this.labTime.Text = "0 (秒)";
			this.label3.AutoSize = true;
			this.label3.Location = new Point(327, 79);
			this.label3.Name = "label3";
			this.label3.Size = new Size(59, 12);
			this.label3.TabIndex = 74;
			this.label3.Text = "剩余时间:";
			this.labSpeed.AutoSize = true;
			this.labSpeed.Location = new Point(122, 78);
			this.labSpeed.Name = "labSpeed";
			this.labSpeed.Size = new Size(59, 12);
			this.labSpeed.TabIndex = 73;
			this.labSpeed.Text = "0 (行/秒)";
			this.label1.AutoSize = true;
			this.label1.Location = new Point(53, 79);
			this.label1.Name = "label1";
			this.label1.Size = new Size(59, 12);
			this.label1.TabIndex = 72;
			this.label1.Text = "当前速度:";
			this.pgGetTotalRows.Location = new Point(47, 41);
			this.pgGetTotalRows.Name = "pgGetTotalRows";
			this.pgGetTotalRows.Size = new Size(531, 21);
			this.pgGetTotalRows.TabIndex = 70;
			this.lbTotalTableDB.AutoSize = true;
			this.lbTotalTableDB.Location = new Point(401, 174);
			this.lbTotalTableDB.Name = "lbTotalTableDB";
			this.lbTotalTableDB.Size = new Size(0, 12);
			this.lbTotalTableDB.TabIndex = 67;
			this.lbGetTotalRows.AutoSize = true;
			this.lbGetTotalRows.Location = new Point(46, 26);
			this.lbGetTotalRows.Name = "lbGetTotalRows";
			this.lbGetTotalRows.Size = new Size(0, 12);
			this.lbGetTotalRows.TabIndex = 71;
			this.lbCurrentTable.AutoSize = true;
			this.lbCurrentTable.Location = new Point(159, 155);
			this.lbCurrentTable.Name = "lbCurrentTable";
			this.lbCurrentTable.Size = new Size(0, 12);
			this.lbCurrentTable.TabIndex = 59;
			this.pbRowsTable.Location = new Point(47, 188);
			this.pbRowsTable.Name = "pbRowsTable";
			this.pbRowsTable.Size = new Size(249, 21);
			this.pbRowsTable.TabIndex = 57;
			this.label9.AutoSize = true;
			this.label9.Location = new Point(326, 174);
			this.label9.Name = "label9";
			this.label9.Size = new Size(83, 12);
			this.label9.TabIndex = 66;
			this.label9.Text = "Total Tables:";
			this.label4.AutoSize = true;
			this.label4.Location = new Point(46, 155);
			this.label4.Name = "label4";
			this.label4.Size = new Size(89, 12);
			this.label4.TabIndex = 58;
			this.label4.Text = "Current Table:";
			this.pbTable.Location = new Point(329, 188);
			this.pbTable.Name = "pbTable";
			this.pbTable.Size = new Size(249, 21);
			this.pbTable.TabIndex = 65;
			this.lbPercentage.AutoSize = true;
			this.lbPercentage.Location = new Point(51, 108);
			this.lbPercentage.Name = "lbPercentage";
			this.lbPercentage.Size = new Size(0, 12);
			this.lbPercentage.TabIndex = 69;
			this.lbRowProgressDB.AutoSize = true;
			this.lbRowProgressDB.Location = new Point(401, 108);
			this.lbRowProgressDB.Name = "lbRowProgressDB";
			this.lbRowProgressDB.Size = new Size(0, 12);
			this.lbRowProgressDB.TabIndex = 64;
			this.label5.AutoSize = true;
			this.label5.Location = new Point(44, 174);
			this.label5.Name = "label5";
			this.label5.Size = new Size(35, 12);
			this.label5.TabIndex = 60;
			this.label5.Text = "Rows:";
			this.label8.AutoSize = true;
			this.label8.Location = new Point(326, 108);
			this.label8.Name = "label8";
			this.label8.Size = new Size(71, 12);
			this.label8.TabIndex = 63;
			this.label8.Text = "Total Rows:";
			this.pbPercent.Location = new Point(47, 123);
			this.pbPercent.Name = "pbPercent";
			this.pbPercent.Size = new Size(249, 21);
			this.pbPercent.TabIndex = 68;
			this.pbRowsDB.Location = new Point(329, 123);
			this.pbRowsDB.MarqueeAnimationSpeed = 1000;
			this.pbRowsDB.Name = "pbRowsDB";
			this.pbRowsDB.Size = new Size(249, 21);
			this.pbRowsDB.TabIndex = 62;
			this.lbRowProgressCurTable.AutoSize = true;
			this.lbRowProgressCurTable.Location = new Point(120, 174);
			this.lbRowProgressCurTable.Name = "lbRowProgressCurTable";
			this.lbRowProgressCurTable.Size = new Size(0, 12);
			this.lbRowProgressCurTable.TabIndex = 61;
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(644, 272);
			base.Controls.Add(this.btnBack);
			base.Controls.Add(this.btnSelect);
			base.Controls.Add(this.txtFile);
			base.Controls.Add(this.groupBox1);
			base.Name = "frmBack";
			this.Text = "数据库备份";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
