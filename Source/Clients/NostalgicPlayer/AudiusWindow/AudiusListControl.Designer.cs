namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	partial class AudiusListControl
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
			flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
			controlGroup = new Krypton.Toolkit.KryptonGroup();
			((System.ComponentModel.ISupportInitialize)controlGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).BeginInit();
			controlGroup.Panel.SuspendLayout();
			SuspendLayout();
			// 
			// flowLayoutPanel
			// 
			flowLayoutPanel.AutoScroll = true;
			flowLayoutPanel.BackColor = System.Drawing.Color.Transparent;
			flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
			flowLayoutPanel.Name = "flowLayoutPanel";
			flowLayoutPanel.Size = new System.Drawing.Size(274, 173);
			flowLayoutPanel.TabIndex = 0;
			flowLayoutPanel.WrapContents = false;
			flowLayoutPanel.Scroll += FlowLayout_Scroll;
			flowLayoutPanel.Resize += FlowLayout_Resize;
			// 
			// controlGroup
			// 
			controlGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			controlGroup.Location = new System.Drawing.Point(0, 0);
			controlGroup.Name = "controlGroup";
			// 
			// 
			// 
			controlGroup.Panel.Controls.Add(flowLayoutPanel);
			controlGroup.Size = new System.Drawing.Size(276, 175);
			controlGroup.TabIndex = 1;
			// 
			// AudiusListControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(controlGroup);
			Name = "AudiusListControl";
			Size = new System.Drawing.Size(276, 175);
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).EndInit();
			controlGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)controlGroup).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
		private Krypton.Toolkit.KryptonGroup controlGroup;
	}
}
