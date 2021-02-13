
namespace Polycode.NostalgicPlayer.Agent.Visual.SpinningSquares.Display
{
	partial class SpinningSquaresControl
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpinningSquaresControl));
			this.hashPanel = new System.Windows.Forms.Panel();
			this.squaresPanel = new System.Windows.Forms.Panel();
			this.pulseTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// hashPanel
			// 
			this.hashPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hashPanel.BackgroundImage")));
			this.hashPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hashPanel.Location = new System.Drawing.Point(0, 0);
			this.hashPanel.Name = "hashPanel";
			this.hashPanel.Size = new System.Drawing.Size(200, 200);
			this.hashPanel.TabIndex = 0;
			// 
			// squaresPanel
			// 
			this.squaresPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.squaresPanel.Location = new System.Drawing.Point(0, 0);
			this.squaresPanel.Name = "squaresPanel";
			this.squaresPanel.Size = new System.Drawing.Size(200, 200);
			this.squaresPanel.TabIndex = 0;
			this.squaresPanel.Visible = false;
			this.squaresPanel.Resize += new System.EventHandler(this.SquaresPanel_Resize);
			// 
			// pulseTimer
			// 
			this.pulseTimer.Interval = 20;
			this.pulseTimer.Tick += new System.EventHandler(this.PulseTimer_Tick);
			// 
			// SpinningSquaresControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.hashPanel);
			this.Controls.Add(this.squaresPanel);
			this.MinimumSize = new System.Drawing.Size(200, 200);
			this.Name = "SpinningSquaresControl";
			this.Size = new System.Drawing.Size(200, 200);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel hashPanel;
		private System.Windows.Forms.Panel squaresPanel;
		private System.Windows.Forms.Timer pulseTimer;
	}
}
