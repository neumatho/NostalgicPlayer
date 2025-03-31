
namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display
{
	partial class OscilloscopeControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			toolTip = new System.Windows.Forms.ToolTip(components);
			oscilloscopesPanel = new System.Windows.Forms.Panel();
			hashPanel = new System.Windows.Forms.Panel();
			SuspendLayout();
			// 
			// oscilloscopesPanel
			// 
			oscilloscopesPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			oscilloscopesPanel.Location = new System.Drawing.Point(8, 8);
			oscilloscopesPanel.Name = "oscilloscopesPanel";
			oscilloscopesPanel.Size = new System.Drawing.Size(284, 128);
			oscilloscopesPanel.TabIndex = 0;
			oscilloscopesPanel.Visible = false;
			// 
			// hashPanel
			// 
			hashPanel.BackgroundImage = Resources.IDB_HASH;
			hashPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			hashPanel.Location = new System.Drawing.Point(0, 0);
			hashPanel.Name = "hashPanel";
			hashPanel.Size = new System.Drawing.Size(300, 144);
			hashPanel.TabIndex = 0;
			// 
			// OscilloscopeControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(hashPanel);
			Controls.Add(oscilloscopesPanel);
			MinimumSize = new System.Drawing.Size(300, 144);
			Name = "OscilloscopeControl";
			Size = new System.Drawing.Size(300, 144);
			Click += Control_Click;
			Resize += Control_Resize;
			ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Panel oscilloscopesPanel;
		private System.Windows.Forms.Panel hashPanel;
	}
}
