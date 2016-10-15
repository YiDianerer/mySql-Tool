using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MySqlTool
{
	public class frmBase : Form
	{
		private IContainer components = null;

		public frmBase()
		{
			this.InitializeComponent();
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(frmBase));
			base.SuspendLayout();
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(284, 262);
			base.Icon = (Icon)resources.GetObject("$this.Icon");
			base.Name = "frmBase";
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "frmBase";
			base.ResumeLayout(false);
		}
	}
}
