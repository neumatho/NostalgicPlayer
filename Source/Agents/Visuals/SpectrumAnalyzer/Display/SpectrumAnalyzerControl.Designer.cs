
namespace Polycode.NostalgicPlayer.Agent.Visual.SpectrumAnalyzer.Display
{
	partial class SpectrumAnalyzerControl
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
			// SpectrumAnalyzerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Black;
			this.DoubleBuffered = true;
			this.MinimumSize = new System.Drawing.Size(300, 150);
			this.Name = "SpectrumAnalyzerControl";
			this.Size = new System.Drawing.Size(300, 150);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Control_Paint);
			this.ResumeLayout(false);

		}

		#endregion
	}
}
