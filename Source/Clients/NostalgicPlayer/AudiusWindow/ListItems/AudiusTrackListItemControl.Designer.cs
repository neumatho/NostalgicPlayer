namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems
{
	partial class AudiusTrackListItemControl
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
			playButton = new Krypton.Toolkit.KryptonButton();
			addButton = new Krypton.Toolkit.KryptonButton();
			durationLabel = new Krypton.Toolkit.KryptonLabel();
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			titleLabel = new Krypton.Toolkit.KryptonLabel();
			SuspendLayout();
			// 
			// playButton
			// 
			playButton.Location = new System.Drawing.Point(4, 4);
			playButton.Name = "playButton";
			playButton.Size = new System.Drawing.Size(24, 24);
			playButton.TabIndex = 0;
			playButton.Values.Image = Resources.IDB_PLAY;
			playButton.Values.Text = "";
			playButton.Click += Play_Click;
			// 
			// addButton
			// 
			addButton.Location = new System.Drawing.Point(32, 4);
			addButton.Name = "addButton";
			addButton.Size = new System.Drawing.Size(24, 24);
			addButton.TabIndex = 1;
			addButton.Values.Image = Resources.IDB_ADD;
			addButton.Values.Text = "";
			addButton.Click += Add_Click;
			// 
			// durationLabel
			// 
			durationLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			durationLabel.Location = new System.Drawing.Point(449, 8);
			durationLabel.Name = "durationLabel";
			durationLabel.LocalCustomPalette = fontPalette;
			durationLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			durationLabel.Size = new System.Drawing.Size(6, 2);
			durationLabel.TabIndex = 3;
			durationLabel.Values.Text = "";
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			// 
			// titleLabel
			// 
			titleLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			titleLabel.AutoSize = false;
			titleLabel.Location = new System.Drawing.Point(56, 8);
			titleLabel.Name = "titleLabel";
			titleLabel.LocalCustomPalette = fontPalette;
			titleLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			titleLabel.Size = new System.Drawing.Size(358, 19);
			titleLabel.TabIndex = 2;
			titleLabel.Values.Text = "";
			// 
			// AudiusTrackListItemControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.FromArgb(252, 252, 252);
			Controls.Add(playButton);
			Controls.Add(addButton);
			Controls.Add(titleLabel);
			Controls.Add(durationLabel);
			Margin = new System.Windows.Forms.Padding(0);
			Name = "AudiusTrackListItemControl";
			Size = new System.Drawing.Size(463, 32);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private Krypton.Toolkit.KryptonButton playButton;
		private Krypton.Toolkit.KryptonButton addButton;
		private GuiKit.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonLabel titleLabel;
		private Krypton.Toolkit.KryptonLabel durationLabel;
	}
}
