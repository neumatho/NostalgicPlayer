namespace Polycode.NostalgicPlayer.Controls.Lists
{
	partial class NostalgicModuleListInternal
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
			scrollTimer = new System.Windows.Forms.Timer(components);
			toolTip = new System.Windows.Forms.ToolTip(components);
			boldFontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			SuspendLayout();
			// 
			// scrollTimer
			// 
			scrollTimer.Interval = 300;
			scrollTimer.Tick += ScrollTimer_Tick;
			// 
			// boldFontConfiguration
			// 
			boldFontConfiguration.FontStyle = System.Drawing.FontStyle.Bold;
			// 
			// NostalgicModuleListInternal
			// 
			AllowDrop = true;
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			DoubleBuffered = true;
			Name = "NostalgicModuleListInternal";
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Timer scrollTimer;
		private System.Windows.Forms.ToolTip toolTip;
		private Components.FontConfiguration boldFontConfiguration;
	}
}
