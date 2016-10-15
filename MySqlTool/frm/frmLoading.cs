using MySqlTool.Class;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySqlTool
{
	public class frmLoading : frmBase
	{
		private int i = 0;

		private IContainer components = null;

		private Label labText;

		private Timer timer1;

		private Label label1;

		private Label labVersion;

		private Label label3;

		public frmLoading()
		{
            InitializeComponent();
            labVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			this.i++;
			string text = "LOADING";
			for (int i = 1; i < this.i; i++)
			{
				text += ".";
			}
            labText.Text = text;
			if (this.i > 5)
			{
                i = 0;
			}
		}

		private void frmLoading_Load(object sender, EventArgs e)
		{
			Task task = new Task(delegate
			{
				Data.Instance.DataInit();
			});
			task.ContinueWith(delegate(Task p)
			{
                Invoke(new MethodInvoker(delegate
				{
                    DialogResult = DialogResult.OK;
				}));
			});
			task.Start();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
                components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            components = new Container();
            timer1 = new Timer(components);
            labText = new Label();
            label1 = new Label();
            labVersion = new Label();
            label3 = new Label();
            SuspendLayout();
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 200;
            timer1.Tick += new System.EventHandler(timer1_Tick);
            // 
            // labText
            // 
            labText.AutoSize = true;
            labText.Font = new Font("方正舒体", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            labText.Location = new System.Drawing.Point(77, 80);
            labText.Name = "labText";
            labText.Size = new System.Drawing.Size(257, 42);
            labText.TabIndex = 0;
            labText.Text = "LOADING.....";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            label1.Location = new System.Drawing.Point(29, 22);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(138, 16);
            label1.TabIndex = 1;
            label1.Text = "MySql数据库工具";
            // 
            // labVersion
            // 
            labVersion.AutoSize = true;
            labVersion.Location = new System.Drawing.Point(315, 185);
            labVersion.Name = "labVersion";
            labVersion.Size = new System.Drawing.Size(65, 12);
            labVersion.TabIndex = 2;
            labVersion.Text = "labVersion";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(315, 166);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(65, 12);
            label3.TabIndex = 3;
            label3.Text = "三生石科技";
            // 
            // frmLoading
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(224)))), ((int)(((byte)(164)))));
            ClientSize = new System.Drawing.Size(400, 200);
            Controls.Add(label3);
            Controls.Add(labVersion);
            Controls.Add(label1);
            Controls.Add(labText);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "frmLoading";
            Text = "frmLoading";
            Load += new System.EventHandler(frmLoading_Load);
            ResumeLayout(false);
            PerformLayout();

		}
	}
}
