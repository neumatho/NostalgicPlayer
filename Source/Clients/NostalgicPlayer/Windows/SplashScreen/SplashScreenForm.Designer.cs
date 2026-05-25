namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SplashScreen
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
			progressBar = new Polycode.NostalgicPlayer.Controls.Progress.NostalgicProgressBar();
			((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
			SuspendLayout();
			// 
			// logoPictureBox
			// 
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
			progressBar.TabIndex = 1;
			// 
			// SplashScreenForm
			// 
			BorderStyle = NostalgicPlayer.Controls.Types.BorderStyle.Thin;
			ClientSize = new System.Drawing.Size(338, 170);
			ControlBox = false;
			Controls.Add(progressBar);
			Controls.Add(logoPictureBox);
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
		private NostalgicPlayer.Controls.Progress.NostalgicProgressBar progressBar;
	}
}