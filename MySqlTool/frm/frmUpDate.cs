using MySqlBll;
using MySqlTool.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySqlTool
{
	public class frmUpDate : frmBase
	{

		private List<DBHost> m_List;

		private PACKSCHEMA m_Pack;

		private IContainer components = null;

		private Button btnOpen;

		private GroupBox groupBox1;

		private GroupBox groupBox2;

		private Label label3;

		private Label lbPDate;

		private Label lbPVersion;

		private Label label7;

		private Label label8;

		private Label lbPTag;

		private Button btnStart;

		private RichTextBox txtLog;

		private ListView lvMain;

		public frmUpDate(List<DBHost> list)
		{
			this.InitializeComponent();
			this.m_List = list;
			this.ControlInit();
		}

		private void ControlInit()
		{
			this.lvMain.Columns.Add("数据库");
			this.lvMain.Columns.Add("标志");
			this.lvMain.Columns.Add("版本");
			this.lvMain.Columns.Add("状态");
			foreach (DBHost current in this.m_List)
			{
				foreach (DBInfo current2 in current.DBInfos)
				{
					ListViewItem listViewItem = new ListViewItem(current2.DBName);
					listViewItem.Tag = current2;
					listViewItem.SubItems.Add(current2.DBTag);
					listViewItem.SubItems.Add(current2.DBVersion.ToString());
					this.lvMain.Items.Add(listViewItem);
				}
			}
			this.lbPDate.Text = "";
			this.lbPTag.Text = "";
			this.lbPVersion.Text = "";
		}

		private void frmUpDate_Load(object sender, EventArgs e)
		{
			Core.Instance.OnProgress += new Progress(this.Instance_OnProgress);
		}

		private void Instance_OnProgress(string msg)
		{
			base.Invoke(new MethodInvoker(delegate
			{
				this.txtLog.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":" + msg + "\r\n");
				this.txtLog.AppendText("*********************************************************************\r\n");
			}));
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "*.dat|*.dat";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				ResultMessage resultMessage = SerializeHelper.Deserialize(openFileDialog.FileName);
				if (resultMessage.Result)
				{
					this.m_Pack = (resultMessage.ObjResult as PACKSCHEMA);
					this.lbPDate.Text = this.m_Pack.PackDate.ToString("yyyy-MM-dd HH:mm:ss");
					this.lbPTag.Text = this.m_Pack.Tag;
					this.lbPVersion.Text = this.m_Pack.Version.ToString();
				}
			}
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			if (this.m_Pack == null)
			{
				MessageBox.Show("请选择更新包");
			}
			else
			{
				//foreach (DBHost current in this.m_List)
				//{
				//	foreach (DBInfo current2 in current.DBInfos)
				//	{
				//		if (current2.DBTag != this.m_Pack.Tag)
				//		{
				//			MessageBox.Show("有一个数据库标志不正确，不能批量更新，请检查");
				//			return;
				//		}
				//	}
				//}
				foreach (DBHost current in this.m_List)
				{
					foreach (DBInfo current2 in current.DBInfos)
					{
						if (current2.DBVersion >= this.m_Pack.Version)
						{
							if (MessageBox.Show("有一个当前数据库版本大于更新包的版本，确定是否要更新？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
							{
								goto IL_182;
							}
							return;
						}
					}
				}
				IL_182:
				if (this.m_Pack != null)
				{
					Task task = new Task(delegate
					{
						int i = 0;
						foreach (DBHost current3 in this.m_List)
						{
							foreach (DBInfo current4 in current3.DBInfos)
							{
								ResultMessage resultMessage = Core.Instance.InData(current3, current4, this.m_Pack.Schema);
								if (resultMessage.Result)
								{
									current4.DBVersion = this.m_Pack.Version;
									Data.Instance.SaveDbInfo(current3, current4);
									base.Invoke(new MethodInvoker(delegate
									{
										this.lvMain.Items[i].SubItems.Add("完成");
									}));
								}
								else
								{
									this.lvMain.Items[i].SubItems.Add("失败", Color.Red, Color.White, this.Font);
								}
								i++;
							}
						}
					});
					task.ContinueWith(delegate(Task p)
					{
						base.Invoke(new MethodInvoker(delegate
						{
							this.btnStart.Enabled = true;
						}));
					});
					this.btnStart.Enabled = false;
					task.Start();
				}
			}
		}

		private void frmUpDate_FormClosing(object sender, FormClosingEventArgs e)
		{
			Core.Instance.OnProgress -= new Progress(this.Instance_OnProgress);
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
			this.txtLog = new RichTextBox();
			this.btnStart = new Button();
			this.groupBox2 = new GroupBox();
			this.label3 = new Label();
			this.lbPDate = new Label();
			this.lbPVersion = new Label();
			this.label7 = new Label();
			this.label8 = new Label();
			this.lbPTag = new Label();
			this.groupBox1 = new GroupBox();
			this.lvMain = new ListView();
			this.btnOpen = new Button();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			base.SuspendLayout();
			this.txtLog.Location = new Point(13, 308);
			this.txtLog.Name = "txtLog";
			this.txtLog.Size = new Size(532, 170);
			this.txtLog.TabIndex = 10;
			this.txtLog.Text = "";
			this.btnStart.Location = new Point(113, 188);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new Size(75, 23);
			this.btnStart.TabIndex = 9;
			this.btnStart.Text = "开始更新";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new EventHandler(this.btnStart_Click);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.lbPDate);
			this.groupBox2.Controls.Add(this.lbPVersion);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.lbPTag);
			this.groupBox2.Location = new Point(12, 227);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new Size(532, 65);
			this.groupBox2.TabIndex = 8;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "包信息";
			this.label3.AutoSize = true;
			this.label3.Location = new Point(31, 29);
			this.label3.Name = "label3";
			this.label3.Size = new Size(41, 12);
			this.label3.TabIndex = 0;
			this.label3.Text = "包日期";
			this.lbPDate.AutoSize = true;
			this.lbPDate.Font = new Font("宋体", 9f, FontStyle.Bold, GraphicsUnit.Point, 134);
			this.lbPDate.Location = new Point(76, 30);
			this.lbPDate.Name = "lbPDate";
			this.lbPDate.Size = new Size(54, 12);
			this.lbPDate.TabIndex = 1;
			this.lbPDate.Text = "lbPName";
			this.lbPVersion.AutoSize = true;
			this.lbPVersion.Font = new Font("宋体", 9f, FontStyle.Bold, GraphicsUnit.Point, 134);
			this.lbPVersion.Location = new Point(428, 31);
			this.lbPVersion.Name = "lbPVersion";
			this.lbPVersion.Size = new Size(75, 12);
			this.lbPVersion.TabIndex = 5;
			this.lbPVersion.Text = "lbPVersion";
			this.label7.AutoSize = true;
			this.label7.Location = new Point(238, 30);
			this.label7.Name = "label7";
			this.label7.Size = new Size(41, 12);
			this.label7.TabIndex = 2;
			this.label7.Text = "包标志";
			this.label8.AutoSize = true;
			this.label8.Location = new Point(372, 30);
			this.label8.Name = "label8";
			this.label8.Size = new Size(41, 12);
			this.label8.TabIndex = 4;
			this.label8.Text = "包版本";
			this.lbPTag.AutoSize = true;
			this.lbPTag.Font = new Font("宋体", 9f, FontStyle.Bold, GraphicsUnit.Point, 134);
			this.lbPTag.Location = new Point(288, 31);
			this.lbPTag.Name = "lbPTag";
			this.lbPTag.Size = new Size(47, 12);
			this.lbPTag.TabIndex = 3;
			this.lbPTag.Text = "lbPTag";
			this.groupBox1.Controls.Add(this.lvMain);
			this.groupBox1.Location = new Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new Size(532, 170);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "数据库信息";
			this.lvMain.Dock = DockStyle.Fill;
			this.lvMain.Location = new Point(3, 17);
			this.lvMain.Name = "lvMain";
			this.lvMain.Size = new Size(526, 150);
			this.lvMain.TabIndex = 0;
			this.lvMain.UseCompatibleStateImageBehavior = false;
			this.lvMain.View = View.Details;
			this.btnOpen.Location = new Point(12, 188);
			this.btnOpen.Name = "btnOpen";
			this.btnOpen.Size = new Size(75, 23);
			this.btnOpen.TabIndex = 6;
			this.btnOpen.Text = "打开包";
			this.btnOpen.UseVisualStyleBackColor = true;
			this.btnOpen.Click += new EventHandler(this.btnOpen_Click);
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(557, 490);
			base.Controls.Add(this.txtLog);
			base.Controls.Add(this.btnStart);
			base.Controls.Add(this.groupBox2);
			base.Controls.Add(this.groupBox1);
			base.Controls.Add(this.btnOpen);
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "frmUpDate";
			this.Text = "更新数据库";
			base.FormClosing += new FormClosingEventHandler(this.frmUpDate_FormClosing);
			base.Load += new EventHandler(this.frmUpDate_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}
}
