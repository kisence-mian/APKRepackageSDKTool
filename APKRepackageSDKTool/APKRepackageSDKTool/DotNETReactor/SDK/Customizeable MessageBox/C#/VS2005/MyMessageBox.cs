using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MyMessageBox
{

	public class MyMessageBox : System.Windows.Forms.Form, MessageBoxInterface
	{
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label cationlabel;
		private System.Windows.Forms.Label textlabel;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Panel colorpanel1;
		private System.Windows.Forms.Panel colorpanel2;

		private System.ComponentModel.Container components = null;

		public MyMessageBox()
		{
			InitializeComponent();
		}

		public string MessageBoxCaption
		{
			get
			{
				return this.cationlabel.Text;
			}
			set
			{
				this.cationlabel.Text = value;
			}
		}

		public string MessageBoxText
		{
			get
			{
				return this.textlabel.Text;
			}
			set
			{
				this.textlabel.Text = value;
			}
		}

		public Color MessageBoxGradientBegin
		{
			get
			{
				return this.colorpanel1.BackColor;
			}
			set
			{
				this.colorpanel1.BackColor = value;
			}
		}

		public Color MessageBoxGradientEnd
		{
			get
			{
				return this.colorpanel2.BackColor;
			}
			set
			{
				this.colorpanel2.BackColor = value;
			}
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel2 = new System.Windows.Forms.Panel();
			this.cationlabel = new System.Windows.Forms.Label();
			this.textlabel = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.colorpanel1 = new System.Windows.Forms.Panel();
			this.colorpanel2 = new System.Windows.Forms.Panel();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.panel2.Controls.Add(this.cationlabel);
			this.panel2.Location = new System.Drawing.Point(8, 8);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(232, 32);
			this.panel2.TabIndex = 1;
			// 
			// cationlabel
			// 
			this.cationlabel.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.cationlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cationlabel.Location = new System.Drawing.Point(8, 8);
			this.cationlabel.Name = "cationlabel";
			this.cationlabel.Size = new System.Drawing.Size(216, 16);
			this.cationlabel.TabIndex = 0;
			this.cationlabel.Text = "Caption";
			this.cationlabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textlabel
			// 
			this.textlabel.BackColor = System.Drawing.Color.White;
			this.textlabel.Location = new System.Drawing.Point(8, 48);
			this.textlabel.Name = "textlabel";
			this.textlabel.Size = new System.Drawing.Size(232, 104);
			this.textlabel.TabIndex = 1;
			this.textlabel.Text = "Text";
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.SystemColors.Control;
			this.button1.Location = new System.Drawing.Point(160, 160);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(80, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// colorpanel1
			// 
			this.colorpanel1.Location = new System.Drawing.Point(8, 160);
			this.colorpanel1.Name = "colorpanel1";
			this.colorpanel1.Size = new System.Drawing.Size(24, 24);
			this.colorpanel1.TabIndex = 3;
			// 
			// colorpanel2
			// 
			this.colorpanel2.Location = new System.Drawing.Point(40, 160);
			this.colorpanel2.Name = "colorpanel2";
			this.colorpanel2.Size = new System.Drawing.Size(24, 24);
			this.colorpanel2.TabIndex = 4;
			// 
			// MyMessageBox
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(246, 188);
			this.Controls.Add(this.colorpanel2);
			this.Controls.Add(this.colorpanel1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textlabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Name = "MyMessageBox";
			this.Text = "MyOwnMessageBox";
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
