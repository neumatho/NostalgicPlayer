namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	partial class ProfilePlaylistsPageControl
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
			audiusListControl = new AudiusListControl();
			SuspendLayout();
			// 
			// audiusListControl
			// 
			audiusListControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			audiusListControl.Location = new System.Drawing.Point(8, 8);
			audiusListControl.Name = "audiusListControl";
			audiusListControl.Size = new System.Drawing.Size(750, 352);
			audiusListControl.TabIndex = 0;
			// 
			// ProfilePlaylistsPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(audiusListControl);
			Name = "ProfilePlaylistsPageControl";
			Size = new System.Drawing.Size(766, 368);
			ResumeLayout(false);
		}

		#endregion

		private AudiusListControl audiusListControl;
	}
}
