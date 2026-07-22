namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow
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
			components = new System.ComponentModel.Container();
			flowLayoutPanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicFlowLayoutPanel();
			statusLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			fontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			SuspendLayout();
			// 
			// flowLayoutPanel
			// 
			flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
			flowLayoutPanel.Name = "flowLayoutPanel";
			flowLayoutPanel.Size = new System.Drawing.Size(276, 175);
			flowLayoutPanel.TabIndex = 0;
			flowLayoutPanel.WrapContents = false;
			flowLayoutPanel.ContentScrolled += FlowLayout_Scroll;
			flowLayoutPanel.Resize += FlowLayout_Resize;
			// 
			// statusLabel
			// 
			statusLabel.Location = new System.Drawing.Point(44, 78);
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new System.Drawing.Size(0, 15);
			statusLabel.TabIndex = 1;
			statusLabel.UseFont = fontConfiguration;
			statusLabel.Visible = false;
			// 
			// fontConfiguration
			// 
			fontConfiguration.FontSize = 1;
			// 
			// AudiusListControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(statusLabel);
			Controls.Add(flowLayoutPanel);
			Name = "AudiusListControl";
			Size = new System.Drawing.Size(276, 175);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicFlowLayoutPanel flowLayoutPanel;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel statusLabel;
		private NostalgicPlayer.Controls.Components.FontConfiguration fontConfiguration;
	}
}
