namespace Polycode.NostalgicPlayer.Agent.Visual.LevelMeter.Display
{
	partial class LevelMeterControl
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
			levelsPanel = new System.Windows.Forms.Panel();
			pulseTimer = new System.Windows.Forms.Timer(components);
			SuspendLayout();
			// 
			// levelsPanel
			// 
			levelsPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			levelsPanel.BackColor = System.Drawing.Color.White;
			levelsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			levelsPanel.Location = new System.Drawing.Point(8, 8);
			levelsPanel.Name = "levelsPanel";
			levelsPanel.Size = new System.Drawing.Size(240, 56);
			levelsPanel.TabIndex = 0;
			levelsPanel.Resize += Control_Resize;
			// 
			// pulseTimer
			// 
			pulseTimer.Interval = 20;
			pulseTimer.Tick += PulseTimer_Tick;
			// 
			// LevelMeterControl
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(levelsPanel);
			MinimumSize = new System.Drawing.Size(256, 72);
			Name = "LevelMeterControl";
			Size = new System.Drawing.Size(256, 72);
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel levelsPanel;
		private System.Windows.Forms.Timer pulseTimer;
	}
}
