namespace Polycode.NostalgicPlayer.Agent.Visual.Piano.Display
{
	partial class PianoControl
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
			this.pulseTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// pulseTimer
			// 
			this.pulseTimer.Interval = 20;
			this.pulseTimer.Tick += new System.EventHandler(this.PulseTimer_Tick);
			// 
			// PianoControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.DoubleBuffered = true;
			this.MinimumSize = new System.Drawing.Size(1050, 139);
			this.Name = "PianoControl";
			this.Size = new System.Drawing.Size(1050, 139);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Control_Paint);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer pulseTimer;
	}
}
