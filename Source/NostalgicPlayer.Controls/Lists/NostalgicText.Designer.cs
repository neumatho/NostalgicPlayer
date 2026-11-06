namespace Polycode.NostalgicPlayer.Controls.Lists
{
	partial class NostalgicText
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
			controlBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			textControl = new NostalgicTextInternal();
			textHScrollBar = new NostalgicHScrollBar();
			textVScrollBar = new NostalgicVScrollBar();
			controlBox.SuspendLayout();
			SuspendLayout();
			// 
			// controlBox
			// 
			controlBox.Controls.Add(textControl);
			controlBox.Controls.Add(textHScrollBar);
			controlBox.Controls.Add(textVScrollBar);
			controlBox.Dock = System.Windows.Forms.DockStyle.Fill;
			controlBox.Location = new System.Drawing.Point(0, 0);
			controlBox.Name = "controlBox";
			controlBox.Size = new System.Drawing.Size(339, 216);
			controlBox.TabIndex = 0;
			// 
			// textControl
			// 
			textControl.BackColor = System.Drawing.Color.White;
			textControl.Location = new System.Drawing.Point(0, 0);
			textControl.Name = "textControl";
			textControl.Size = new System.Drawing.Size(320, 197);
			textControl.TabIndex = 0;
			// 
			// textHScrollBar
			// 
			textHScrollBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			textHScrollBar.LargeChange = 1;
			textHScrollBar.Location = new System.Drawing.Point(0, 197);
			textHScrollBar.Maximum = 0;
			textHScrollBar.Name = "textHScrollBar";
			textHScrollBar.Size = new System.Drawing.Size(320, 17);
			textHScrollBar.TabIndex = 2;
			textHScrollBar.ValueChanged += TextHScrollBar_ValueChanged;
			// 
			// textVScrollBar
			// 
			textVScrollBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			textVScrollBar.LargeChange = 1;
			textVScrollBar.Location = new System.Drawing.Point(320, 0);
			textVScrollBar.Maximum = 0;
			textVScrollBar.Name = "textVScrollBar";
			textVScrollBar.Size = new System.Drawing.Size(17, 197);
			textVScrollBar.TabIndex = 1;
			textVScrollBar.ValueChanged += TextVScrollBar_ValueChanged;
			// 
			// NostalgicText
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(controlBox);
			Name = "NostalgicText";
			Size = new System.Drawing.Size(339, 216);
			controlBox.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Controls.Containers.NostalgicBox controlBox;
		private NostalgicTextInternal textControl;
		private NostalgicVScrollBar textVScrollBar;
		private NostalgicHScrollBar textHScrollBar;
	}
}
