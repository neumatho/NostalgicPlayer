namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SplashScreen
{
	partial class SplashScreenForm
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
			logoPictureBox = new System.Windows.Forms.PictureBox();
			progressBar = new Krypton.Toolkit.KryptonProgressBar();
			((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
			SuspendLayout();
			// 
			// logoPictureBox
			// 
			logoPictureBox.Image = Resources.IDB_ABOUT_LOGO;
			logoPictureBox.Location = new System.Drawing.Point(0, 0);
			logoPictureBox.Name = "logoPictureBox";
			logoPictureBox.Size = new System.Drawing.Size(340, 130);
			logoPictureBox.TabIndex = 0;
			logoPictureBox.TabStop = false;
			// 
			// progressBar
			// 
			progressBar.Location = new System.Drawing.Point(8, 138);
			progressBar.Name = "progressBar";
			progressBar.Size = new System.Drawing.Size(324, 26);
			progressBar.StateCommon.Back.Color1 = System.Drawing.Color.Green;
			progressBar.StateDisabled.Back.ColorStyle = Krypton.Toolkit.PaletteColorStyle.OneNote;
			progressBar.StateNormal.Back.ColorStyle = Krypton.Toolkit.PaletteColorStyle.OneNote;
			progressBar.TabIndex = 1;
			progressBar.Values.Text = "";
			// 
			// SplashScreenForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(340, 172);
			ControlBox = false;
			Controls.Add(progressBar);
			Controls.Add(logoPictureBox);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SplashScreenForm";
			ShowIcon = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.PictureBox logoPictureBox;
		private Krypton.Toolkit.KryptonProgressBar progressBar;
	}
}