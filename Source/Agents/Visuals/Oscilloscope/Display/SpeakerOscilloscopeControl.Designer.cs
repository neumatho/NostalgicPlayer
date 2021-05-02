
namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display
{
	partial class SpeakerOscilloscopeControl
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
			this.SuspendLayout();
			// 
			// SpeakerOscilloscopeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Black;
			this.DoubleBuffered = true;
			this.ForeColor = System.Drawing.Color.Green;
			this.Name = "SpeakerOscilloscopeControl";
			this.Click += new System.EventHandler(this.Control_Click);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Control_Paint);
			this.ResumeLayout(false);

		}

		#endregion
	}
}
