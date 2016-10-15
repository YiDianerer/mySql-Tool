using MySqlTool.Class;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MySqlTool
{
	public class frmSetTag : frmBase
	{
		private DBInfo m_DbInfo;

		private IContainer components = null;

		private Label label1;

		private Label label2;

		private Label lbName;

		private TextBox txtTag;

		private Button btnOk;

		private Button btnCancle;

		private TextBox txtOutTag;

		private Label label3;

		public frmSetTag(DBInfo dbinfo)
		{
			this.m_DbInfo = dbinfo;
			this.InitializeComponent();
			this.lbName.Text = this.m_DbInfo.DBName;
			this.txtTag.Text = this.m_DbInfo.DBTag;
			this.txtOutTag.Text = this.m_DbInfo.OutDBTag;
		}

		private void btnCancle_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			this.m_DbInfo.DBTag = this.txtTag.Text.Trim();
			this.m_DbInfo.OutDBTag = this.txtOutTag.Text.Trim();
			base.DialogResult = DialogResult.OK;
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
			this.label1 = new Label();
			this.label2 = new Label();
			this.lbName = new Label();
			this.txtTag = new TextBox();
			this.btnOk = new Button();
			this.btnCancle = new Button();
			this.txtOutTag = new TextBox();
			this.label3 = new Label();
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Location = new Point(57, 24);
			this.label1.Name = "label1";
			this.label1.Size = new Size(65, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "数据库名称";
			this.label2.AutoSize = true;
			this.label2.Location = new Point(57, 53);
			this.label2.Name = "label2";
			this.label2.Size = new Size(53, 12);
			this.label2.TabIndex = 1;
			this.label2.Text = "导入标志";
			this.lbName.AutoSize = true;
			this.lbName.Font = new Font("宋体", 9f, FontStyle.Bold, GraphicsUnit.Point, 134);
			this.lbName.Location = new Point(140, 24);
			this.lbName.Name = "lbName";
			this.lbName.Size = new Size(47, 12);
			this.lbName.TabIndex = 2;
			this.lbName.Text = "lbName";
			this.txtTag.Location = new Point(140, 53);
			this.txtTag.Name = "txtTag";
			this.txtTag.Size = new Size(103, 21);
			this.txtTag.TabIndex = 3;
			this.btnOk.Location = new Point(47, 125);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new Size(75, 23);
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "确定";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new EventHandler(this.btnOk_Click);
			this.btnCancle.DialogResult = DialogResult.Cancel;
			this.btnCancle.Location = new Point(168, 125);
			this.btnCancle.Name = "btnCancle";
			this.btnCancle.Size = new Size(75, 23);
			this.btnCancle.TabIndex = 5;
			this.btnCancle.Text = "取消";
			this.btnCancle.UseVisualStyleBackColor = true;
			this.btnCancle.Click += new EventHandler(this.btnCancle_Click);
			this.txtOutTag.Location = new Point(140, 84);
			this.txtOutTag.Name = "txtOutTag";
			this.txtOutTag.Size = new Size(103, 21);
			this.txtOutTag.TabIndex = 7;
			this.label3.AutoSize = true;
			this.label3.Location = new Point(57, 84);
			this.label3.Name = "label3";
			this.label3.Size = new Size(53, 12);
			this.label3.TabIndex = 6;
			this.label3.Text = "导出标志";
			base.AcceptButton = this.btnOk;
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.CancelButton = this.btnCancle;
			base.ClientSize = new Size(294, 172);
			base.Controls.Add(this.txtOutTag);
			base.Controls.Add(this.label3);
			base.Controls.Add(this.btnCancle);
			base.Controls.Add(this.btnOk);
			base.Controls.Add(this.txtTag);
			base.Controls.Add(this.lbName);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label1);
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "frmSetTag";
			this.Text = "设置标志";
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
