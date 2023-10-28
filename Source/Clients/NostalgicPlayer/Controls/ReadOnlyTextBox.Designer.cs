namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Controls
{
	partial class ReadOnlyTextBox
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
			controlGroup = new Krypton.Toolkit.KryptonGroup();
			textControl = new TextControl();
			textHScrollBar = new System.Windows.Forms.HScrollBar();
			textVScrollBar = new System.Windows.Forms.VScrollBar();
			((System.ComponentModel.ISupportInitialize)controlGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).BeginInit();
			controlGroup.Panel.SuspendLayout();
			SuspendLayout();
			// 
			// controlGroup
			// 
			controlGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			controlGroup.Location = new System.Drawing.Point(0, 0);
			controlGroup.Name = "controlGroup";
			// 
			// 
			// 
			controlGroup.Panel.Controls.Add(textControl);
			controlGroup.Panel.Controls.Add(textHScrollBar);
			controlGroup.Panel.Controls.Add(textVScrollBar);
			controlGroup.Size = new System.Drawing.Size(339, 216);
			controlGroup.StateCommon.Back.Color1 = System.Drawing.SystemColors.Control;
			controlGroup.TabIndex = 0;
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
			// ReadOnlyTextBox
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(controlGroup);
			Name = "ReadOnlyTextBox";
			Size = new System.Drawing.Size(339, 216);
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).EndInit();
			controlGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)controlGroup).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Krypton.Toolkit.KryptonGroup controlGroup;
		private TextControl textControl;
		private System.Windows.Forms.VScrollBar textVScrollBar;
		private System.Windows.Forms.HScrollBar textHScrollBar;
	}
}
