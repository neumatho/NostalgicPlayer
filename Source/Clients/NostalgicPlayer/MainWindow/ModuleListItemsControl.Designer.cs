namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	partial class ModuleListItemsControl
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
			SuspendLayout();
			// 
			// scrollTimer
			// 
			scrollTimer.Interval = 300;
			scrollTimer.Tick += ScrollTimer_Tick;
			// 
			// ModuleListItemsControl
			// 
			AllowDrop = true;
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			DoubleBuffered = true;
			Name = "ModuleListItemsControl";
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Timer scrollTimer;
	}
}
