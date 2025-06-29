namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	partial class ProfileControl
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
			infoPanel = new Polycode.NostalgicPlayer.GuiKit.Controls.ImprovedPanel();
			closeButton = new Krypton.Toolkit.KryptonButton();
			handleLabel = new Krypton.Toolkit.KryptonLabel();
			bigFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			nameLabel = new Krypton.Toolkit.KryptonLabel();
			extraBigBoldFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			profilePictureBox = new System.Windows.Forms.PictureBox();
			infoPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)profilePictureBox).BeginInit();
			SuspendLayout();
			// 
			// infoPanel
			// 
			infoPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			infoPanel.BackColor = System.Drawing.Color.Transparent;
			infoPanel.Controls.Add(closeButton);
			infoPanel.Controls.Add(handleLabel);
			infoPanel.Controls.Add(nameLabel);
			infoPanel.Controls.Add(profilePictureBox);
			infoPanel.Location = new System.Drawing.Point(0, 0);
			infoPanel.Name = "infoPanel";
			infoPanel.Size = new System.Drawing.Size(766, 224);
			infoPanel.TabIndex = 0;
			// 
			// closeButton
			// 
			closeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			closeButton.Location = new System.Drawing.Point(734, 8);
			closeButton.Name = "closeButton";
			closeButton.Size = new System.Drawing.Size(24, 24);
			closeButton.StateCommon.Back.Draw = Krypton.Toolkit.InheritBool.False;
			closeButton.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			closeButton.TabIndex = 3;
			closeButton.Values.Image = Resources.IDB_CLOSE_WHITE;
			closeButton.Values.Text = "";
			closeButton.Click += Close_Click;
			closeButton.MouseEnter += Close_MouseEnter;
			closeButton.MouseLeave += Close_MouseLeave;
			// 
			// handleLabel
			// 
			handleLabel.Location = new System.Drawing.Point(144, 155);
			handleLabel.Name = "handleLabel";
			handleLabel.Palette = bigFontPalette;
			handleLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			handleLabel.Size = new System.Drawing.Size(6, 2);
			handleLabel.StateCommon.ShortText.Color1 = System.Drawing.Color.White;
			handleLabel.TabIndex = 2;
			handleLabel.Values.Text = "";
			// 
			// bigFontPalette
			// 
			bigFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			bigFontPalette.BaseFontSize = 10F;
			bigFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			bigFontPalette.ThemeName = "";
			bigFontPalette.UseKryptonFileDialogs = true;
			// 
			// nameLabel
			// 
			nameLabel.Location = new System.Drawing.Point(144, 126);
			nameLabel.Name = "nameLabel";
			nameLabel.Palette = extraBigBoldFontPalette;
			nameLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			nameLabel.Size = new System.Drawing.Size(6, 2);
			nameLabel.StateCommon.ShortText.Color1 = System.Drawing.Color.White;
			nameLabel.TabIndex = 1;
			nameLabel.Values.Text = "";
			// 
			// extraBigBoldFontPalette
			// 
			extraBigBoldFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			extraBigBoldFontPalette.BaseFontSize = 16F;
			extraBigBoldFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			extraBigBoldFontPalette.FontStyle = System.Drawing.FontStyle.Bold;
			extraBigBoldFontPalette.ThemeName = "";
			extraBigBoldFontPalette.UseKryptonFileDialogs = true;
			// 
			// profilePictureBox
			// 
			profilePictureBox.Location = new System.Drawing.Point(8, 88);
			profilePictureBox.Name = "profilePictureBox";
			profilePictureBox.Size = new System.Drawing.Size(128, 128);
			profilePictureBox.TabIndex = 0;
			profilePictureBox.TabStop = false;
			// 
			// ProfileControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(infoPanel);
			Name = "ProfileControl";
			Size = new System.Drawing.Size(766, 368);
			infoPanel.ResumeLayout(false);
			infoPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)profilePictureBox).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Polycode.NostalgicPlayer.GuiKit.Controls.ImprovedPanel infoPanel;
		private System.Windows.Forms.PictureBox profilePictureBox;
		private GuiKit.Components.FontPalette extraBigBoldFontPalette;
		private GuiKit.Components.FontPalette bigFontPalette;
		private Krypton.Toolkit.KryptonLabel handleLabel;
		private Krypton.Toolkit.KryptonLabel nameLabel;
		private Krypton.Toolkit.KryptonButton closeButton;
	}
}
