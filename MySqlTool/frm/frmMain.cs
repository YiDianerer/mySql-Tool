using MySqlBll;
using MySqlTool.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MySqlTool
{
	public class frmMain : frmBase
	{
		private DBInfo m_CurrInfo = null;

		private DBHost m_CurrHost = null;

		private IContainer components = null;

		private ToolStrip toolStrip1;

		private ToolStripButton tsbUpDate;

		private ToolStripButton tsbSetTag;

		private ToolStripButton btnOutData;

		private ToolStripButton tsbBack;

		private StatusStrip statusStrip1;

		private TableLayoutPanel tableLayoutPanel1;

		private Panel panel2;

		private Panel panel1;

		private TreeView tvMain;

		private TableLayoutPanel tableLayoutPanel2;

		private Panel panel4;

		private Panel panel3;

		private Label label1;

		private Label label2;

		private Label lbTag;

		private Label lbVersion;

		private TabControl tcMain;

		private TabPage tpData;

		private TabPage tpLog;

		private DataGridView dgvMain;

		private RichTextBox txtLog;

		private ToolStripButton tsbRestore;

		private ToolStripStatusLabel tsslVersion;

		private ToolStripButton tsbCreateDatabase;

		public DBInfo CurrInfo
		{
			get
			{
				return this.m_CurrInfo;
			}
			set
			{
				this.m_CurrInfo = value;
				this.ShowTag();
			}
		}

		public DBHost CurrHost
		{
			get
			{
				return this.m_CurrHost;
			}
			set
			{
				this.m_CurrHost = value;
			}
		}

		public frmMain()
		{
			this.InitializeComponent();
			this.tsslVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			this.Text = About.ProgramName;
			this.ControlInit();
		}

		private void Instance_OnProgress(string msg)
		{
		}

		private void ControlInit()
		{
			this.tvMain.Nodes.Clear();
			this.ShowTag();
			foreach (DBHost current in Data.Instance.DBHost)
			{
				TreeNode treeNode = new TreeNode(current.Host);
				treeNode.Tag = current;
				foreach (DBInfo current2 in current.DBInfos)
				{
					TreeNode treeNode2 = new TreeNode(string.Concat(new object[]
					{
						"[",
						current2.DBVersion,
						"]",
						current2.DBName
					}));
					treeNode2.Tag = current2;
					treeNode.Nodes.Add(treeNode2);
				}
				treeNode.ExpandAll();
				this.tvMain.Nodes.Add(treeNode);
			}
		}

		private void ShowTag()
		{
			if (this.CurrInfo != null)
			{
				this.lbTag.Text = this.CurrInfo.DBTag;
				this.lbVersion.Text = this.CurrInfo.DBVersion.ToString();
			}
			else
			{
				this.lbTag.Text = "";
				this.lbVersion.Text = "";
			}
		}

		private void tvMain_AfterCheck(object sender, TreeViewEventArgs e)
		{
			this.tvMain.AfterCheck -= new TreeViewEventHandler(this.tvMain_AfterCheck);
			Tree.SetCheck(e.Node);
			this.tvMain.AfterCheck += new TreeViewEventHandler(this.tvMain_AfterCheck);
		}

		private void tvMain_AfterSelect(object sender, TreeViewEventArgs e)
		{
			Tree.SetBackColor(this.tvMain, e.Node);
			if (e.Node.Parent != null)
			{
				this.CurrInfo = (DBInfo)e.Node.Tag;
				this.CurrHost = (DBHost)e.Node.Parent.Tag;
			}
			else
			{
				this.CurrHost = (DBHost)e.Node.Tag;
			}
		}

		private void tsbSetTag_Click(object sender, EventArgs e)
		{
			if (this.CurrInfo != null)
			{
				frmSetTag frmSetTag = new frmSetTag(this.CurrInfo);
				if (frmSetTag.ShowDialog() == DialogResult.OK)
				{
					Data.Instance.SaveDbInfo(this.CurrHost, this.CurrInfo);
				}
			}
			else
			{
				MessageBox.Show("请选择数据库", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}

		private void btnOutData_Click(object sender, EventArgs e)
		{
			if (this.CurrInfo != null)
			{
				ResultMessage resultMessage = Core.Instance.OutData(this.CurrHost, this.CurrInfo);
				this.AddLog(resultMessage.Message);
			}
			else
			{
				MessageBox.Show("请选择数据库", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}

		private void AddLog(string msg)
		{
			this.tcMain.SelectTab(this.tpLog);
			this.txtLog.SelectionColor = Color.Black;
			this.txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss    ") + msg + "\r\n");
			this.txtLog.ScrollToCaret();
		}

		private void tsbUpDate_Click(object sender, EventArgs e)
		{
			List<DBHost> list = new List<DBHost>();
			foreach (TreeNode treeNode in this.tvMain.Nodes)
			{
				if (treeNode.Checked)
				{
					DBHost dBHost = new DBHost();
					DBHost dBHost2 = (DBHost)treeNode.Tag;
					dBHost.Host = dBHost2.Host;
					dBHost.Password = dBHost2.Password;
					dBHost.Port = dBHost2.Port;
					dBHost.User = dBHost2.User;
					foreach (TreeNode treeNode2 in treeNode.Nodes)
					{
						if (treeNode2.Checked)
						{
							dBHost.DBInfos.Add((DBInfo)treeNode2.Tag);
						}
					}
					if (dBHost.DBInfos.Count > 0)
					{
						list.Add(dBHost);
					}
				}
			}
			if (list.Count > 0)
			{
				new frmUpDate(list).ShowDialog();
				foreach (TreeNode treeNode in this.tvMain.Nodes)
				{
					if (treeNode.Checked)
					{
						foreach (TreeNode treeNode2 in treeNode.Nodes)
						{
							if (treeNode2.Checked)
							{
								DBInfo dBInfo = (DBInfo)treeNode2.Tag;
								treeNode2.Text = string.Concat(new object[]
								{
									"[",
									dBInfo.DBVersion,
									"]",
									dBInfo.DBName
								});
							}
						}
					}
				}
			}
			else
			{
				MessageBox.Show("请选择数据库", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}

		private void tsbBack_Click(object sender, EventArgs e)
		{
			if (this.m_CurrHost != null && this.m_CurrInfo != null)
			{
				new frmBack(Data.Instance.DBConnString(this.m_CurrHost, this.m_CurrInfo.DBName), this.m_CurrInfo.DBName).ShowDialog();
			}
		}

		private void tsbRestore_Click(object sender, EventArgs e)
		{
			if (this.m_CurrHost != null && this.m_CurrInfo != null)
			{
				new frmRestore(Data.Instance.DBConnString(this.m_CurrHost, this.m_CurrInfo.DBName)).ShowDialog();
			}
		}

		private void tsbCreateDatabase_Click(object sender, EventArgs e)
		{
			if (this.m_CurrHost != null)
			{
				DBInfo dBInfo = new DBInfo();
				if (new frmCreate(this.m_CurrHost, dBInfo).ShowDialog() == DialogResult.OK)
				{
					string[] dbInfo = MySqlCore.GetDbInfo(Data.Instance.DBConnString(this.m_CurrHost, dBInfo.DBName));
					dBInfo.DBTag = dbInfo[0];
					dBInfo.DBVersion = Convert.ToInt32(dbInfo[1]);
					dBInfo.OutDBTag = dbInfo[2];
					dBInfo.OutDBVersion = Convert.ToInt32(dbInfo[3]);
					this.m_CurrHost.DBInfos.Add(dBInfo);
					this.ControlInit();
				}
			}
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
			ComponentResourceManager resources = new ComponentResourceManager(typeof(frmMain));
			this.tableLayoutPanel1 = new TableLayoutPanel();
			this.panel2 = new Panel();
			this.tableLayoutPanel2 = new TableLayoutPanel();
			this.panel4 = new Panel();
			this.tcMain = new TabControl();
			this.tpLog = new TabPage();
			this.txtLog = new RichTextBox();
			this.tpData = new TabPage();
			this.dgvMain = new DataGridView();
			this.panel3 = new Panel();
			this.lbVersion = new Label();
			this.lbTag = new Label();
			this.label2 = new Label();
			this.label1 = new Label();
			this.panel1 = new Panel();
			this.tvMain = new TreeView();
			this.statusStrip1 = new StatusStrip();
			this.tsslVersion = new ToolStripStatusLabel();
			this.toolStrip1 = new ToolStrip();
			this.tsbUpDate = new ToolStripButton();
			this.tsbSetTag = new ToolStripButton();
			this.btnOutData = new ToolStripButton();
			this.tsbCreateDatabase = new ToolStripButton();
			this.tsbBack = new ToolStripButton();
			this.tsbRestore = new ToolStripButton();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.panel4.SuspendLayout();
			this.tcMain.SuspendLayout();
			this.tpLog.SuspendLayout();
			this.tpData.SuspendLayout();
			((ISupportInitialize)this.dgvMain).BeginInit();
			this.panel3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			base.SuspendLayout();
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel1.Dock = DockStyle.Fill;
			this.tableLayoutPanel1.Location = new Point(0, 25);
			this.tableLayoutPanel1.Margin = new Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.Size = new Size(757, 358);
			this.tableLayoutPanel1.TabIndex = 2;
			this.panel2.Controls.Add(this.tableLayoutPanel2);
			this.panel2.Dock = DockStyle.Fill;
			this.panel2.Location = new Point(200, 0);
			this.panel2.Margin = new Padding(0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new Size(557, 358);
			this.panel2.TabIndex = 1;
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel2.Controls.Add(this.panel4, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.panel3, 0, 0);
			this.tableLayoutPanel2.Dock = DockStyle.Fill;
			this.tableLayoutPanel2.Location = new Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel2.Size = new Size(557, 358);
			this.tableLayoutPanel2.TabIndex = 0;
			this.panel4.Controls.Add(this.tcMain);
			this.panel4.Dock = DockStyle.Fill;
			this.panel4.Location = new Point(0, 60);
			this.panel4.Margin = new Padding(0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new Size(557, 298);
			this.panel4.TabIndex = 1;
			this.tcMain.Controls.Add(this.tpLog);
			this.tcMain.Controls.Add(this.tpData);
			this.tcMain.Dock = DockStyle.Fill;
			this.tcMain.Location = new Point(0, 0);
			this.tcMain.Name = "tcMain";
			this.tcMain.SelectedIndex = 0;
			this.tcMain.Size = new Size(557, 298);
			this.tcMain.TabIndex = 0;
			this.tpLog.Controls.Add(this.txtLog);
			this.tpLog.Location = new Point(4, 22);
			this.tpLog.Name = "tpLog";
			this.tpLog.Padding = new Padding(3);
			this.tpLog.Size = new Size(549, 272);
			this.tpLog.TabIndex = 1;
			this.tpLog.Text = "日志";
			this.tpLog.UseVisualStyleBackColor = true;
			this.txtLog.Dock = DockStyle.Fill;
			this.txtLog.Location = new Point(3, 3);
			this.txtLog.Name = "txtLog";
			this.txtLog.Size = new Size(543, 266);
			this.txtLog.TabIndex = 0;
			this.txtLog.Text = "";
			this.tpData.Controls.Add(this.dgvMain);
			this.tpData.Location = new Point(4, 22);
			this.tpData.Name = "tpData";
			this.tpData.Padding = new Padding(3);
			this.tpData.Size = new Size(549, 272);
			this.tpData.TabIndex = 0;
			this.tpData.Text = "数据";
			this.tpData.UseVisualStyleBackColor = true;
			this.dgvMain.AllowUserToAddRows = false;
			this.dgvMain.AllowUserToDeleteRows = false;
			this.dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvMain.Dock = DockStyle.Fill;
			this.dgvMain.Location = new Point(3, 3);
			this.dgvMain.Name = "dgvMain";
			this.dgvMain.ReadOnly = true;
			this.dgvMain.RowTemplate.Height = 23;
			this.dgvMain.Size = new Size(543, 266);
			this.dgvMain.TabIndex = 0;
			this.panel3.Controls.Add(this.lbVersion);
			this.panel3.Controls.Add(this.lbTag);
			this.panel3.Controls.Add(this.label2);
			this.panel3.Controls.Add(this.label1);
			this.panel3.Dock = DockStyle.Fill;
			this.panel3.Location = new Point(0, 0);
			this.panel3.Margin = new Padding(0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new Size(557, 60);
			this.panel3.TabIndex = 0;
			this.lbVersion.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.lbVersion.AutoSize = true;
			this.lbVersion.Font = new Font("微软雅黑", 12f, FontStyle.Bold, GraphicsUnit.Point, 134);
			this.lbVersion.ForeColor = Color.Red;
			this.lbVersion.Location = new Point(406, 19);
			this.lbVersion.Name = "lbVersion";
			this.lbVersion.Size = new Size(87, 22);
			this.lbVersion.TabIndex = 3;
			this.lbVersion.Text = "lbVersion";
			this.lbTag.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.lbTag.AutoSize = true;
			this.lbTag.Font = new Font("微软雅黑", 12f, FontStyle.Bold, GraphicsUnit.Point, 134);
			this.lbTag.ForeColor = Color.Red;
			this.lbTag.Location = new Point(168, 20);
			this.lbTag.Name = "lbTag";
			this.lbTag.Size = new Size(56, 22);
			this.lbTag.TabIndex = 2;
			this.lbTag.Text = "lbTag";
			this.label2.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.label2.AutoSize = true;
			this.label2.Font = new Font("宋体", 15.75f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.label2.Location = new Point(282, 19);
			this.label2.Name = "label2";
			this.label2.Size = new Size(126, 21);
			this.label2.TabIndex = 1;
			this.label2.Text = "数据库版本:";
			this.label1.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.label1.AutoSize = true;
			this.label1.Font = new Font("宋体", 15.75f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.label1.Location = new Point(53, 19);
			this.label1.Name = "label1";
			this.label1.Size = new Size(126, 21);
			this.label1.TabIndex = 0;
			this.label1.Text = "数据库标志:";
			this.panel1.Controls.Add(this.tvMain);
			this.panel1.Dock = DockStyle.Fill;
			this.panel1.Location = new Point(0, 0);
			this.panel1.Margin = new Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new Size(200, 358);
			this.panel1.TabIndex = 0;
			this.tvMain.CheckBoxes = true;
			this.tvMain.Dock = DockStyle.Fill;
			this.tvMain.Font = new Font("宋体", 11.25f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.tvMain.Location = new Point(0, 0);
			this.tvMain.Name = "tvMain";
			this.tvMain.Size = new Size(200, 358);
			this.tvMain.TabIndex = 0;
			this.tvMain.AfterCheck += new TreeViewEventHandler(this.tvMain_AfterCheck);
			this.tvMain.AfterSelect += new TreeViewEventHandler(this.tvMain_AfterSelect);
			this.statusStrip1.Items.AddRange(new ToolStripItem[]
			{
				this.tsslVersion
			});
			this.statusStrip1.Location = new Point(0, 383);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new Size(757, 22);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			this.tsslVersion.Name = "tsslVersion";
			this.tsslVersion.Size = new Size(131, 17);
			this.tsslVersion.Text = "toolStripStatusLabel1";
			this.toolStrip1.Items.AddRange(new ToolStripItem[]
			{
				this.tsbUpDate,
				this.tsbSetTag,
				this.btnOutData,
				this.tsbCreateDatabase,
				this.tsbBack,
				this.tsbRestore
			});
			this.toolStrip1.Location = new Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new Size(757, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			this.tsbUpDate.Image = (Image)resources.GetObject("tsbUpDate.Image");
			this.tsbUpDate.ImageTransparentColor = Color.Magenta;
			this.tsbUpDate.Name = "tsbUpDate";
			this.tsbUpDate.Size = new Size(52, 22);
			this.tsbUpDate.Text = "更新";
			this.tsbUpDate.Click += new EventHandler(this.tsbUpDate_Click);
			this.tsbSetTag.Image = (Image)resources.GetObject("tsbSetTag.Image");
			this.tsbSetTag.ImageTransparentColor = Color.Magenta;
			this.tsbSetTag.Name = "tsbSetTag";
			this.tsbSetTag.Size = new Size(76, 22);
			this.tsbSetTag.Text = "设置标志";
			this.tsbSetTag.Click += new EventHandler(this.tsbSetTag_Click);
			this.btnOutData.Image = (Image)resources.GetObject("btnOutData.Image");
			this.btnOutData.ImageTransparentColor = Color.Magenta;
			this.btnOutData.Name = "btnOutData";
			this.btnOutData.Size = new Size(76, 22);
			this.btnOutData.Text = "数据打包";
			this.btnOutData.Click += new EventHandler(this.btnOutData_Click);
			this.tsbCreateDatabase.Image = (Image)resources.GetObject("tsbCreateDatabase.Image");
			this.tsbCreateDatabase.ImageTransparentColor = Color.Magenta;
			this.tsbCreateDatabase.Name = "tsbCreateDatabase";
			this.tsbCreateDatabase.Size = new Size(88, 22);
			this.tsbCreateDatabase.Text = "创建数据库";
			this.tsbCreateDatabase.Click += new EventHandler(this.tsbCreateDatabase_Click);
			this.tsbBack.Image = (Image)resources.GetObject("tsbBack.Image");
			this.tsbBack.ImageTransparentColor = Color.Magenta;
			this.tsbBack.Name = "tsbBack";
			this.tsbBack.Size = new Size(52, 22);
			this.tsbBack.Text = "备份";
			this.tsbBack.Click += new EventHandler(this.tsbBack_Click);
			this.tsbRestore.Image = (Image)resources.GetObject("tsbRestore.Image");
			this.tsbRestore.ImageTransparentColor = Color.Magenta;
			this.tsbRestore.Name = "tsbRestore";
			this.tsbRestore.Size = new Size(52, 22);
			this.tsbRestore.Text = "还原";
			this.tsbRestore.ToolTipText = "还原";
			this.tsbRestore.Click += new EventHandler(this.tsbRestore_Click);
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(757, 405);
			base.Controls.Add(this.tableLayoutPanel1);
			base.Controls.Add(this.statusStrip1);
			base.Controls.Add(this.toolStrip1);
			base.Name = "frmMain";
			this.Text = "MYSQL工具";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.tcMain.ResumeLayout(false);
			this.tpLog.ResumeLayout(false);
			this.tpData.ResumeLayout(false);
			((ISupportInitialize)this.dgvMain).EndInit();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
