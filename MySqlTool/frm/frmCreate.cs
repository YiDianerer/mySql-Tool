using MySqlTool.Class;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MySqlTool
{
	public class frmCreate : frmBase
	{
		private DBHost m_host;

		private DBInfo m_info;

		private IContainer components = null;

		private Label label1;

		private TextBox txtDBName;

		private Button btnOk;

		private Button btnCancle;

		public frmCreate(DBHost host, DBInfo info)
		{
			this.InitializeComponent();
			this.m_host = host;
			this.m_info = info;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			Core.Instance.CreateDatabase(this.m_host, this.txtDBName.Text);
			this.m_info.DBName = this.txtDBName.Text;
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
			this.txtDBName = new TextBox();
			this.btnOk = new Button();
			this.btnCancle = new Button();
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Location = new Point(49, 30);
			this.label1.Name = "label1";
			this.label1.Size = new Size(65, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "数据库名称";
			this.txtDBName.Location = new Point(141, 27);
			this.txtDBName.Name = "txtDBName";
			this.txtDBName.Size = new Size(245, 21);
			this.txtDBName.TabIndex = 1;
			this.btnOk.Location = new Point(61, 107);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new Size(75, 23);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "确定";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new EventHandler(this.btnOk_Click);
			this.btnCancle.Location = new Point(283, 107);
			this.btnCancle.Name = "btnCancle";
			this.btnCancle.Size = new Size(75, 23);
			this.btnCancle.TabIndex = 3;
			this.btnCancle.Text = "取消";
			this.btnCancle.UseVisualStyleBackColor = true;
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(426, 163);
			base.Controls.Add(this.btnCancle);
			base.Controls.Add(this.btnOk);
			base.Controls.Add(this.txtDBName);
			base.Controls.Add(this.label1);
			base.Name = "frmCreate";
			this.Text = "frmCreate";
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
