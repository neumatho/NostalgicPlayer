namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages
{
	partial class LyricsPageControl
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
			lyricsGroup = new Krypton.Toolkit.KryptonGroup();
			moduleInfoLyricsReadOnlyTextBox = new Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.ReadOnlyTextBox();
			((System.ComponentModel.ISupportInitialize)lyricsGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)lyricsGroup.Panel).BeginInit();
			lyricsGroup.Panel.SuspendLayout();
			SuspendLayout();
			// 
			// lyricsGroup
			// 
			lyricsGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			lyricsGroup.Location = new System.Drawing.Point(8, 8);
			lyricsGroup.Name = "lyricsGroup";
			// 
			// 
			// 
			lyricsGroup.Panel.Controls.Add(moduleInfoLyricsReadOnlyTextBox);
			lyricsGroup.Size = new System.Drawing.Size(266, 142);
			lyricsGroup.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			lyricsGroup.TabIndex = 0;
			// 
			// moduleInfoLyricsReadOnlyTextBox
			// 
			moduleInfoLyricsReadOnlyTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			moduleInfoLyricsReadOnlyTextBox.Location = new System.Drawing.Point(0, 0);
			moduleInfoLyricsReadOnlyTextBox.Name = "moduleInfoLyricsReadOnlyTextBox";
			moduleInfoLyricsReadOnlyTextBox.Size = new System.Drawing.Size(266, 142);
			moduleInfoLyricsReadOnlyTextBox.TabIndex = 0;
			// 
			// LyricsPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(lyricsGroup);
			Name = "LyricsPageControl";
			Size = new System.Drawing.Size(282, 158);
			((System.ComponentModel.ISupportInitialize)lyricsGroup.Panel).EndInit();
			lyricsGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)lyricsGroup).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private Krypton.Toolkit.KryptonGroup lyricsGroup;
		private Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.ReadOnlyTextBox moduleInfoLyricsReadOnlyTextBox;
	}
}
