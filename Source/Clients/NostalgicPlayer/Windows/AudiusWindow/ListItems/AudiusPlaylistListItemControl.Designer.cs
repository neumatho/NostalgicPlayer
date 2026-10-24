namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.ListItems
{
	partial class AudiusPlaylistListItemControl
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
			audiusMusicListItemControl = new AudiusMusicListItemControl();
			tracksFlowLayoutPanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicFlowLayoutPanel();
			SuspendLayout();
			// 
			// audiusMusicListItemControl
			// 
			audiusMusicListItemControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			audiusMusicListItemControl.BackColor = System.Drawing.Color.Transparent;
			audiusMusicListItemControl.Location = new System.Drawing.Point(0, 0);
			audiusMusicListItemControl.Margin = new System.Windows.Forms.Padding(8);
			audiusMusicListItemControl.Name = "audiusMusicListItemControl";
			audiusMusicListItemControl.Size = new System.Drawing.Size(463, 144);
			audiusMusicListItemControl.TabIndex = 0;
			// 
			// tracksFlowLayoutPanel
			// 
			tracksFlowLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tracksFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			tracksFlowLayoutPanel.Location = new System.Drawing.Point(0, 143);
			tracksFlowLayoutPanel.Name = "tracksFlowLayoutPanel";
			tracksFlowLayoutPanel.Size = new System.Drawing.Size(463, 114);
			tracksFlowLayoutPanel.TabIndex = 1;
			tracksFlowLayoutPanel.WrapContents = false;
			tracksFlowLayoutPanel.Resize += TrackFlowLayout_Resize;
			// 
			// AudiusPlaylistListItemControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(tracksFlowLayoutPanel);
			Controls.Add(audiusMusicListItemControl);
			DoubleBuffered = true;
			Name = "AudiusPlaylistListItemControl";
			Size = new System.Drawing.Size(463, 257);
			ResumeLayout(false);
		}

		#endregion

		private AudiusMusicListItemControl audiusMusicListItemControl;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicFlowLayoutPanel tracksFlowLayoutPanel;
	}
}
