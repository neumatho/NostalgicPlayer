
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
			nostalgicBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
			nostalgicBox.SuspendLayout();
			SuspendLayout();
			// 
			// pictureBox
			// 
			pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			pictureBox.Location = new System.Drawing.Point(0, 0);
			pictureBox.Margin = new System.Windows.Forms.Padding(0);
			pictureBox.Name = "pictureBox";
			pictureBox.Size = new System.Drawing.Size(340, 170);
			pictureBox.TabIndex = 0;
			pictureBox.TabStop = false;
			// 
			// pulseTimer
			// 
			pulseTimer.Interval = 50;
			pulseTimer.Tick += Pulse_Tick;
			// 
			// nostalgicBox
			// 
			nostalgicBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nostalgicBox.Controls.Add(pictureBox);
			nostalgicBox.Location = new System.Drawing.Point(8, 8);
			nostalgicBox.Name = "nostalgicBox";
			nostalgicBox.Size = new System.Drawing.Size(342, 172);
			nostalgicBox.TabIndex = 0;
			// 
			// AboutWindowForm
			// 
			AllowResizing = false;
			ClientSize = new System.Drawing.Size(358, 188);
			Controls.Add(nostalgicBox);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "AboutWindowForm";
			((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
			nostalgicBox.ResumeLayout(false);
			ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.Timer pulseTimer;
		private NostalgicPlayer.Controls.Containers.NostalgicBox nostalgicBox;
	}
}