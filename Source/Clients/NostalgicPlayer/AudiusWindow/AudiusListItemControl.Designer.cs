namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
{
	partial class AudiusListItemControl
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
			controlGroup = new Krypton.Toolkit.KryptonGroup();
			artistLabel = new Krypton.Toolkit.KryptonLabel();
			titleLabel = new Krypton.Toolkit.KryptonLabel();
			bigBoldFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			positionLabel = new Krypton.Toolkit.KryptonLabel();
			bigFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			itemPictureBox = new System.Windows.Forms.PictureBox();
			favoritePictureBox = new System.Windows.Forms.PictureBox();
			favoritesLabel = new Krypton.Toolkit.KryptonLabel();
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			repostsLabel = new Krypton.Toolkit.KryptonLabel();
			repostsPictureBox = new System.Windows.Forms.PictureBox();
			playsLabel = new Krypton.Toolkit.KryptonLabel();
			durationLabel = new Krypton.Toolkit.KryptonLabel();
			separatorGroup = new Krypton.Toolkit.KryptonGroup();
			addButton = new Krypton.Toolkit.KryptonButton();
			playButton = new Krypton.Toolkit.KryptonButton();
			((System.ComponentModel.ISupportInitialize)controlGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).BeginInit();
			controlGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)itemPictureBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)favoritePictureBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)repostsPictureBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)separatorGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)separatorGroup.Panel).BeginInit();
			SuspendLayout();
			// 
			// controlGroup
			// 
			controlGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			controlGroup.Location = new System.Drawing.Point(0, 0);
			controlGroup.Name = "controlGroup";
			// 
			// 
			// 
			controlGroup.Panel.Controls.Add(artistLabel);
			controlGroup.Panel.Controls.Add(titleLabel);
			controlGroup.Panel.Controls.Add(positionLabel);
			controlGroup.Panel.Controls.Add(itemPictureBox);
			controlGroup.Panel.Controls.Add(favoritePictureBox);
			controlGroup.Panel.Controls.Add(favoritesLabel);
			controlGroup.Panel.Controls.Add(repostsLabel);
			controlGroup.Panel.Controls.Add(repostsPictureBox);
			controlGroup.Panel.Controls.Add(playsLabel);
			controlGroup.Panel.Controls.Add(durationLabel);
			controlGroup.Panel.Controls.Add(separatorGroup);
			controlGroup.Panel.Controls.Add(addButton);
			controlGroup.Panel.Controls.Add(playButton);
			controlGroup.Size = new System.Drawing.Size(463, 144);
			controlGroup.StateCommon.Back.Color1 = System.Drawing.Color.White;
			controlGroup.TabIndex = 0;
			// 
			// artistLabel
			// 
			artistLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			artistLabel.AutoSize = false;
			artistLabel.Location = new System.Drawing.Point(171, 36);
			artistLabel.Name = "artistLabel";
			artistLabel.Size = new System.Drawing.Size(247, 20);
			artistLabel.TabIndex = 2;
			artistLabel.Values.Text = "";
			// 
			// titleLabel
			// 
			titleLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			titleLabel.AutoSize = false;
			titleLabel.LabelStyle = Krypton.Toolkit.LabelStyle.BoldPanel;
			titleLabel.Location = new System.Drawing.Point(171, 8);
			titleLabel.Name = "titleLabel";
			titleLabel.Palette = bigBoldFontPalette;
			titleLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			titleLabel.Size = new System.Drawing.Size(230, 19);
			titleLabel.TabIndex = 1;
			titleLabel.Values.Text = "";
			// 
			// bigBoldFontPalette
			// 
			bigBoldFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			bigBoldFontPalette.BaseFontSize = 10F;
			bigBoldFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			bigBoldFontPalette.FontStyle = System.Drawing.FontStyle.Bold;
			bigBoldFontPalette.ThemeName = "";
			bigBoldFontPalette.UseKryptonFileDialogs = true;
			// 
			// positionLabel
			// 
			positionLabel.Location = new System.Drawing.Point(4, 63);
			positionLabel.Name = "positionLabel";
			positionLabel.Palette = bigFontPalette;
			positionLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			positionLabel.Size = new System.Drawing.Size(6, 2);
			positionLabel.TabIndex = 0;
			positionLabel.Values.Text = "";
			// 
			// bigFontPalette
			// 
			bigFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			bigFontPalette.BaseFontSize = 10F;
			bigFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			bigFontPalette.ThemeName = "";
			bigFontPalette.UseKryptonFileDialogs = true;
			// 
			// itemPictureBox
			// 
			itemPictureBox.Image = Resources.IDB_UNKNOWN_ALBUM_COVER;
			itemPictureBox.Location = new System.Drawing.Point(39, 8);
			itemPictureBox.Name = "itemPictureBox";
			itemPictureBox.Size = new System.Drawing.Size(128, 128);
			itemPictureBox.TabIndex = 1;
			itemPictureBox.TabStop = false;
			// 
			// favoritePictureBox
			// 
			favoritePictureBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			favoritePictureBox.Image = Resources.IDB_FAVORITE;
			favoritePictureBox.Location = new System.Drawing.Point(261, 84);
			favoritePictureBox.Name = "favoritePictureBox";
			favoritePictureBox.Size = new System.Drawing.Size(16, 16);
			favoritePictureBox.TabIndex = 3;
			favoritePictureBox.TabStop = false;
			// 
			// favoritesLabel
			// 
			favoritesLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			favoritesLabel.Location = new System.Drawing.Point(277, 98);
			favoritesLabel.Name = "favoritesLabel";
			favoritesLabel.Palette = fontPalette;
			favoritesLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			favoritesLabel.Size = new System.Drawing.Size(6, 2);
			favoritesLabel.TabIndex = 5;
			favoritesLabel.Values.Text = "";
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// repostsLabel
			// 
			repostsLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			repostsLabel.Location = new System.Drawing.Point(187, 98);
			repostsLabel.Name = "repostsLabel";
			repostsLabel.Palette = fontPalette;
			repostsLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			repostsLabel.Size = new System.Drawing.Size(6, 2);
			repostsLabel.TabIndex = 4;
			repostsLabel.Values.Text = "";
			// 
			// repostsPictureBox
			// 
			repostsPictureBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			repostsPictureBox.Image = Resources.IDB_REPOST;
			repostsPictureBox.Location = new System.Drawing.Point(171, 84);
			repostsPictureBox.Name = "repostsPictureBox";
			repostsPictureBox.Size = new System.Drawing.Size(16, 16);
			repostsPictureBox.TabIndex = 1;
			repostsPictureBox.TabStop = false;
			// 
			// playsLabel
			// 
			playsLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			playsLabel.Location = new System.Drawing.Point(449, 98);
			playsLabel.Name = "playsLabel";
			playsLabel.Palette = fontPalette;
			playsLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			playsLabel.Size = new System.Drawing.Size(6, 2);
			playsLabel.TabIndex = 6;
			playsLabel.Values.Text = "";
			// 
			// durationLabel
			// 
			durationLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			durationLabel.Location = new System.Drawing.Point(449, 8);
			durationLabel.Name = "durationLabel";
			durationLabel.Palette = fontPalette;
			durationLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			durationLabel.Size = new System.Drawing.Size(6, 2);
			durationLabel.TabIndex = 3;
			durationLabel.Values.Text = "";
			// 
			// separatorGroup
			// 
			separatorGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			separatorGroup.Location = new System.Drawing.Point(171, 104);
			separatorGroup.Name = "separatorGroup";
			separatorGroup.Size = new System.Drawing.Size(284, 2);
			separatorGroup.TabIndex = 7;
			// 
			// addButton
			// 
			addButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			addButton.Location = new System.Drawing.Point(199, 112);
			addButton.Name = "addButton";
			addButton.Size = new System.Drawing.Size(24, 24);
			addButton.TabIndex = 9;
			addButton.Values.Image = Resources.IDB_ADD;
			addButton.Values.Text = "";
			addButton.Click += Add_Click;
			// 
			// playButton
			// 
			playButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			playButton.Location = new System.Drawing.Point(171, 112);
			playButton.Name = "playButton";
			playButton.Size = new System.Drawing.Size(24, 24);
			playButton.TabIndex = 8;
			playButton.Values.Image = Resources.IDB_PLAY;
			playButton.Values.Text = "";
			playButton.Click += Play_Click;
			// 
			// AudiusListItemControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(controlGroup);
			DoubleBuffered = true;
			Margin = new System.Windows.Forms.Padding(8);
			Name = "AudiusListItemControl";
			Size = new System.Drawing.Size(463, 144);
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).EndInit();
			controlGroup.Panel.ResumeLayout(false);
			controlGroup.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)controlGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)itemPictureBox).EndInit();
			((System.ComponentModel.ISupportInitialize)favoritePictureBox).EndInit();
			((System.ComponentModel.ISupportInitialize)repostsPictureBox).EndInit();
			((System.ComponentModel.ISupportInitialize)separatorGroup.Panel).EndInit();
			((System.ComponentModel.ISupportInitialize)separatorGroup).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Krypton.Toolkit.KryptonGroup controlGroup;
		private System.Windows.Forms.PictureBox itemPictureBox;
		private Krypton.Toolkit.KryptonLabel positionLabel;
		private Krypton.Toolkit.KryptonLabel titleLabel;
		private Krypton.Toolkit.KryptonLabel artistLabel;
		private GuiKit.Components.FontPalette bigFontPalette;
		private GuiKit.Components.FontPalette bigBoldFontPalette;
		private Krypton.Toolkit.KryptonLabel repostsLabel;
		private Krypton.Toolkit.KryptonLabel favoritesLabel;
		private System.Windows.Forms.PictureBox favoritePictureBox;
		private GuiKit.Components.FontPalette fontPalette;
		private System.Windows.Forms.PictureBox repostsPictureBox;
		private Krypton.Toolkit.KryptonLabel durationLabel;
		private Krypton.Toolkit.KryptonLabel playsLabel;
		private Krypton.Toolkit.KryptonButton playButton;
		private Krypton.Toolkit.KryptonButton addButton;
		private Krypton.Toolkit.KryptonGroup separatorGroup;
	}
}
