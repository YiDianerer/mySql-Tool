using MySql.Data.MySqlClient;
using MySqlTool.Class;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MySqlTool
{
	public class frmRestore : frmBase
	{
		private Timer TimerImport;

		private bool TimerStopImport = true;

		private MySqlBackup mb;

		private Methods md = new Methods();

		private Speed m_Speed = new Speed();

		private long CurrentByte = 0L;

		private long TotalBytes = 0L;

		private int PercentageComplete = 0;

		private string m_Connstr;

		private IContainer components = null;

		private Button btnBack;

		private Button btnSelect;

		private TextBox txtFile;

		private GroupBox groupBox2;

		private Label lbImportErr;

		private Label lbTotalBytes;

		private ProgressBar pbBytes;

		private Label labTime;

		private Label label3;

		private Label labSpeed;

		private Label label1;

		public frmRestore(string _connstr)
		{
			this.InitializeComponent();
			this.m_Connstr = _connstr;
		}

		private void btnSelect_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "*.sql;*.bak|*.sql;*.bak";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				this.txtFile.Text = openFileDialog.FileName;
			}
		}

		private void btnBack_Click(object sender, EventArgs e)
		{
			this.btnBack.Enabled = false;
			this.mb = new MySqlBackup(this.m_Connstr);
			this.mb.ImportInfo.AsynchronousMode = true;
			this.mb.ImportInfo.FileName = this.txtFile.Text;
			this.TimerStopImport = false;
			this.TimerImport = new Timer();
			this.TimerImport.Interval = 1000;
			this.TimerImport.Tick += new EventHandler(this.TimerImport_Tick);
			this.TimerImport.Start();
			this.mb.ImportProgressChanged += new MySqlBackup.importProgressChange(this.mb_ImportProgressChanged);
			this.mb.ImportCompleted += new MySqlBackup.importComplete(this.mb_ImportCompleted);
			try
			{
				this.mb.Import();
			}
			catch
			{
				this.ShowImportCompleteMessage(this.mb.ImportInfo.CompleteArg);
			}
		}

		private void TimerImport_Tick(object sender, EventArgs e)
		{
			this.lbTotalBytes.Text = "Processed bytes: " + Helper.GetStorageUnit(this.CurrentByte) + " / " + Helper.GetStorageUnit(this.TotalBytes);
			this.m_Speed.Add(this.CurrentByte);
			this.labSpeed.Text = Helper.GetStorageUnit(this.m_Speed.CurrSpeed) + "秒";
			this.pbBytes.Value = this.PercentageComplete;
			if (this.TotalBytes > 0L && this.m_Speed.CurrSpeed > 0L)
			{
				int num = Convert.ToInt32((this.TotalBytes - this.CurrentByte) / this.m_Speed.CurrSpeed);
				this.labTime.Text = num.ToString() + "(秒)";
			}
			if (this.TimerStopImport)
			{
				this.TimerImport.Stop();
				this.btnBack.Enabled = true;
			}
		}

		private void mb_ImportProgressChanged(object sender, ImportProgressArg e)
		{
			this.PercentageComplete = e.PercentageCompleted;
			this.TotalBytes = e.TotalBytes;
			this.CurrentByte = e.CurrentByte;
		}

		private void mb_ImportCompleted(object sender, ImportCompleteArg e)
		{
			this.TimerStopImport = true;
			this.ShowImportCompleteMessage(e);
		}

		private void ShowImportCompleteMessage(ImportCompleteArg e)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Import " + e.CompletedType.ToString() + "\r\n");
			stringBuilder.Append("Start Time: " + e.TimeStart.ToString() + "\r\n");
			stringBuilder.Append("End Time: " + e.TimeEnd.ToString() + "\r\n");
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
			if (e.HasErrors)
			{
				stringBuilder.Append("Errors occur during the import process.\r\n\r\nDisplaying the last exception:\r\n\r\n" + e.LastError.ToString());
			}
			MessageBox.Show(stringBuilder.ToString());
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
			this.groupBox2 = new GroupBox();
			this.labTime = new Label();
			this.label3 = new Label();
			this.labSpeed = new Label();
			this.label1 = new Label();
			this.lbImportErr = new Label();
			this.lbTotalBytes = new Label();
			this.pbBytes = new ProgressBar();
			this.btnBack = new Button();
			this.btnSelect = new Button();
			this.txtFile = new TextBox();
			this.groupBox2.SuspendLayout();
			base.SuspendLayout();
			this.groupBox2.Controls.Add(this.labTime);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.labSpeed);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.lbImportErr);
			this.groupBox2.Controls.Add(this.lbTotalBytes);
			this.groupBox2.Controls.Add(this.pbBytes);
			this.groupBox2.Location = new Point(12, 79);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new Size(621, 125);
			this.groupBox2.TabIndex = 80;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "恢复进度";
			this.labTime.AutoSize = true;
			this.labTime.Location = new Point(538, 40);
			this.labTime.Name = "labTime";
			this.labTime.Size = new Size(41, 12);
			this.labTime.TabIndex = 79;
			this.labTime.Text = "0 (秒)";
			this.label3.AutoSize = true;
			this.label3.Location = new Point(469, 40);
			this.label3.Name = "label3";
			this.label3.Size = new Size(59, 12);
			this.label3.TabIndex = 78;
			this.label3.Text = "剩余时间:";
			this.labSpeed.AutoSize = true;
			this.labSpeed.Location = new Point(345, 40);
			this.labSpeed.Name = "labSpeed";
			this.labSpeed.Size = new Size(59, 12);
			this.labSpeed.TabIndex = 77;
			this.labSpeed.Text = "0 (KB/秒)";
			this.label1.AutoSize = true;
			this.label1.Location = new Point(276, 40);
			this.label1.Name = "label1";
			this.label1.Size = new Size(59, 12);
			this.label1.TabIndex = 76;
			this.label1.Text = "当前速度:";
			this.lbImportErr.AutoSize = true;
			this.lbImportErr.Location = new Point(46, 82);
			this.lbImportErr.Name = "lbImportErr";
			this.lbImportErr.Size = new Size(0, 12);
			this.lbImportErr.TabIndex = 4;
			this.lbTotalBytes.AutoSize = true;
			this.lbTotalBytes.Location = new Point(46, 40);
			this.lbTotalBytes.Name = "lbTotalBytes";
			this.lbTotalBytes.Size = new Size(0, 12);
			this.lbTotalBytes.TabIndex = 3;
			this.pbBytes.Location = new Point(48, 73);
			this.pbBytes.Name = "pbBytes";
			this.pbBytes.Size = new Size(531, 21);
			this.pbBytes.TabIndex = 2;
			this.btnBack.Location = new Point(552, 12);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new Size(75, 23);
			this.btnBack.TabIndex = 79;
			this.btnBack.Text = "恢复";
			this.btnBack.UseVisualStyleBackColor = true;
			this.btnBack.Click += new EventHandler(this.btnBack_Click);
			this.btnSelect.Location = new Point(471, 12);
			this.btnSelect.Name = "btnSelect";
			this.btnSelect.Size = new Size(75, 23);
			this.btnSelect.TabIndex = 78;
			this.btnSelect.Text = "选择";
			this.btnSelect.UseVisualStyleBackColor = true;
			this.btnSelect.Click += new EventHandler(this.btnSelect_Click);
			this.txtFile.Location = new Point(12, 12);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new Size(452, 21);
			this.txtFile.TabIndex = 77;
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(644, 217);
			base.Controls.Add(this.groupBox2);
			base.Controls.Add(this.btnBack);
			base.Controls.Add(this.btnSelect);
			base.Controls.Add(this.txtFile);
			base.Name = "frmRestore";
			this.Text = "数据恢复";
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
