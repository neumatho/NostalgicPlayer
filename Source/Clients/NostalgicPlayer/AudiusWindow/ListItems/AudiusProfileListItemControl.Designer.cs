namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.ListItems
{
	partial class AudiusProfileListItemControl
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
			handleLabel = new Krypton.Toolkit.KryptonLabel();
			nameLabel = new Krypton.Toolkit.KryptonLabel();
			bigBoldFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			positionLabel = new Krypton.Toolkit.KryptonLabel();
			bigFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			itemPictureBox = new System.Windows.Forms.PictureBox();
			separatorGroup = new Krypton.Toolkit.KryptonGroup();
			showInfoButton = new Krypton.Toolkit.KryptonButton();
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			((System.ComponentModel.ISupportInitialize)controlGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).BeginInit();
			controlGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)itemPictureBox).BeginInit();
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
			controlGroup.Panel.Controls.Add(handleLabel);
			controlGroup.Panel.Controls.Add(nameLabel);
			controlGroup.Panel.Controls.Add(positionLabel);
			controlGroup.Panel.Controls.Add(itemPictureBox);
			controlGroup.Panel.Controls.Add(separatorGroup);
			controlGroup.Panel.Controls.Add(showInfoButton);
			controlGroup.Size = new System.Drawing.Size(463, 144);
			controlGroup.StateCommon.Back.Color1 = System.Drawing.Color.White;
			controlGroup.TabIndex = 0;
			// 
			// handleLabel
			// 
			handleLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			handleLabel.AutoSize = false;
			handleLabel.Location = new System.Drawing.Point(171, 36);
			handleLabel.Name = "handleLabel";
			handleLabel.Size = new System.Drawing.Size(247, 20);
			handleLabel.TabIndex = 2;
			handleLabel.Values.Text = "";
			// 
			// nameLabel
			// 
			nameLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nameLabel.AutoSize = false;
			nameLabel.LabelStyle = Krypton.Toolkit.LabelStyle.BoldPanel;
			nameLabel.Location = new System.Drawing.Point(171, 8);
			nameLabel.Name = "nameLabel";
			nameLabel.Palette = bigBoldFontPalette;
			nameLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			nameLabel.Size = new System.Drawing.Size(230, 19);
			nameLabel.TabIndex = 1;
			nameLabel.Values.Text = "";
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
			itemPictureBox.Image = Resources.IDB_UNKNOWN_PROFILE_SMALL;
			itemPictureBox.Location = new System.Drawing.Point(39, 8);
			itemPictureBox.Name = "itemPictureBox";
			itemPictureBox.Size = new System.Drawing.Size(128, 128);
			itemPictureBox.TabIndex = 1;
			itemPictureBox.TabStop = false;
			// 
			// separatorGroup
			// 
			separatorGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			separatorGroup.Location = new System.Drawing.Point(171, 104);
			separatorGroup.Name = "separatorGroup";
			separatorGroup.Size = new System.Drawing.Size(284, 2);
			separatorGroup.TabIndex = 7;
			// 
			// showInfoButton
			// 
			showInfoButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			showInfoButton.Location = new System.Drawing.Point(171, 112);
			showInfoButton.Name = "showInfoButton";
			showInfoButton.Size = new System.Drawing.Size(24, 24);
			showInfoButton.TabIndex = 8;
			showInfoButton.Values.Image = Resources.IDB_SHOW_PROFILE_INFO;
			showInfoButton.Values.Text = "";
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// AudiusProfileListItemControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(controlGroup);
			DoubleBuffered = true;
			Margin = new System.Windows.Forms.Padding(8);
			Name = "AudiusProfileListItemControl";
			Size = new System.Drawing.Size(463, 144);
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).EndInit();
			controlGroup.Panel.ResumeLayout(false);
			controlGroup.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)controlGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)itemPictureBox).EndInit();
			((System.ComponentModel.ISupportInitialize)separatorGroup.Panel).EndInit();
			((System.ComponentModel.ISupportInitialize)separatorGroup).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Krypton.Toolkit.KryptonGroup controlGroup;
		private System.Windows.Forms.PictureBox itemPictureBox;
		private Krypton.Toolkit.KryptonLabel positionLabel;
		private Krypton.Toolkit.KryptonLabel nameLabel;
		private Krypton.Toolkit.KryptonLabel handleLabel;
		private GuiKit.Components.FontPalette bigFontPalette;
		private GuiKit.Components.FontPalette bigBoldFontPalette;
		private GuiKit.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonButton showInfoButton;
		private Krypton.Toolkit.KryptonGroup separatorGroup;
	}
}
