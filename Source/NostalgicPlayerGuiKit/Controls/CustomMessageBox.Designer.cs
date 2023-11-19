namespace Polycode.NostalgicPlayer.GuiKit.Controls
{
	partial class CustomMessageBox
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			messageLabel = new Krypton.Toolkit.KryptonLabel();
			fontPalette = new Components.FontPalette(components);
			pictureBox = new System.Windows.Forms.PictureBox();
			messagePanel = new System.Windows.Forms.Panel();
			buttonPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
			messagePanel.SuspendLayout();
			SuspendLayout();
			// 
			// messageLabel
			// 
			messageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			messageLabel.Location = new System.Drawing.Point(0, 0);
			messageLabel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			messageLabel.Name = "messageLabel";
			messageLabel.Palette = fontPalette;
			messageLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			messageLabel.Size = new System.Drawing.Size(40, 16);
			messageLabel.TabIndex = 0;
			messageLabel.Values.Text = "label1";
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// pictureBox
			// 
			pictureBox.Location = new System.Drawing.Point(15, 15);
			pictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			pictureBox.Name = "pictureBox";
			pictureBox.Size = new System.Drawing.Size(37, 37);
			pictureBox.TabIndex = 2;
			pictureBox.TabStop = false;
			// 
			// messagePanel
			// 
			messagePanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			messagePanel.AutoSize = true;
			messagePanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			messagePanel.Controls.Add(messageLabel);
			messagePanel.Location = new System.Drawing.Point(56, 15);
			messagePanel.Margin = new System.Windows.Forms.Padding(0);
			messagePanel.Name = "messagePanel";
			messagePanel.Size = new System.Drawing.Size(40, 16);
			messagePanel.TabIndex = 3;
			// 
			// buttonPanel
			// 
			buttonPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			buttonPanel.AutoSize = true;
			buttonPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			buttonPanel.Location = new System.Drawing.Point(273, 56);
			buttonPanel.Margin = new System.Windows.Forms.Padding(0, 12, 0, 6);
			buttonPanel.Name = "buttonPanel";
			buttonPanel.Size = new System.Drawing.Size(0, 0);
			buttonPanel.TabIndex = 4;
			// 
			// CustomMessageBox
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			AutoSize = true;
			AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			ClientSize = new System.Drawing.Size(284, 62);
			ControlBox = false;
			Controls.Add(buttonPanel);
			Controls.Add(messagePanel);
			Controls.Add(pictureBox);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MaximumSize = new System.Drawing.Size(581, 340);
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(114, 52);
			Name = "CustomMessageBox";
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Dialog";
			Resize += CustomMessageBox_Resize;
			((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
			messagePanel.ResumeLayout(false);
			messagePanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Krypton.Toolkit.KryptonLabel messageLabel;
		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.Panel messagePanel;
		private System.Windows.Forms.Panel buttonPanel;
		private Components.FontPalette fontPalette;
	}
}