
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AboutWindow
{
	partial class AboutWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutWindowForm));
			pictureBox = new System.Windows.Forms.PictureBox();
			pulseTimer = new System.Windows.Forms.Timer(components);
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
			SuspendLayout();
			// 
			// pictureBox
			// 
			pictureBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			pictureBox.Location = new System.Drawing.Point(8, 8);
			pictureBox.Margin = new System.Windows.Forms.Padding(0);
			pictureBox.Name = "pictureBox";
			pictureBox.Size = new System.Drawing.Size(342, 172);
			pictureBox.TabIndex = 0;
			pictureBox.TabStop = false;
			// 
			// pulseTimer
			// 
			pulseTimer.Interval = 50;
			pulseTimer.Tick += Pulse_Tick;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// AboutWindowForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(358, 188);
			Controls.Add(pictureBox);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "AboutWindowForm";
			Palette = fontPalette;
			PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
			ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.Timer pulseTimer;
		private Kit.Gui.Components.FontPalette fontPalette;
	}
}