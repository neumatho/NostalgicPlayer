
namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display
{
	partial class OscilloscopeControl
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
			this.leftPanel = new System.Windows.Forms.Panel();
			this.leftSpeakerOscilloscopeControl = new Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display.SpeakerOscilloscopeControl();
			this.rightPanel = new System.Windows.Forms.Panel();
			this.rightSpeakerOscilloscopeControl = new Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display.SpeakerOscilloscopeControl();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.leftPanel.SuspendLayout();
			this.rightPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// leftPanel
			// 
			this.leftPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.leftPanel.Controls.Add(this.leftSpeakerOscilloscopeControl);
			this.leftPanel.Location = new System.Drawing.Point(8, 8);
			this.leftPanel.Name = "leftPanel";
			this.leftPanel.Size = new System.Drawing.Size(138, 138);
			this.leftPanel.TabIndex = 0;
			// 
			// leftSpeakerOscilloscopeControl
			// 
			this.leftSpeakerOscilloscopeControl.BackColor = System.Drawing.Color.Black;
			this.leftSpeakerOscilloscopeControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.leftSpeakerOscilloscopeControl.ForeColor = System.Drawing.Color.Green;
			this.leftSpeakerOscilloscopeControl.Location = new System.Drawing.Point(0, 0);
			this.leftSpeakerOscilloscopeControl.Name = "leftSpeakerOscilloscopeControl";
			this.leftSpeakerOscilloscopeControl.Size = new System.Drawing.Size(134, 134);
			this.leftSpeakerOscilloscopeControl.TabIndex = 0;
			// 
			// rightPanel
			// 
			this.rightPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.rightPanel.Controls.Add(this.rightSpeakerOscilloscopeControl);
			this.rightPanel.Location = new System.Drawing.Point(154, 8);
			this.rightPanel.Name = "rightPanel";
			this.rightPanel.Size = new System.Drawing.Size(138, 138);
			this.rightPanel.TabIndex = 1;
			// 
			// rightSpeakerOscilloscopeControl
			// 
			this.rightSpeakerOscilloscopeControl.BackColor = System.Drawing.Color.Black;
			this.rightSpeakerOscilloscopeControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightSpeakerOscilloscopeControl.ForeColor = System.Drawing.Color.Green;
			this.rightSpeakerOscilloscopeControl.Location = new System.Drawing.Point(0, 0);
			this.rightSpeakerOscilloscopeControl.Name = "rightSpeakerOscilloscopeControl";
			this.rightSpeakerOscilloscopeControl.Size = new System.Drawing.Size(134, 134);
			this.rightSpeakerOscilloscopeControl.TabIndex = 0;
			// 
			// OscilloscopeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.rightPanel);
			this.Controls.Add(this.leftPanel);
			this.MinimumSize = new System.Drawing.Size(300, 150);
			this.Name = "OscilloscopeControl";
			this.Size = new System.Drawing.Size(300, 150);
			this.VisibleChanged += new System.EventHandler(this.Control_VisibleChanged);
			this.Click += new System.EventHandler(this.Control_Click);
			this.Resize += new System.EventHandler(this.Control_Resize);
			this.leftPanel.ResumeLayout(false);
			this.rightPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel leftPanel;
		private System.Windows.Forms.Panel rightPanel;
		private SpeakerOscilloscopeControl leftSpeakerOscilloscopeControl;
		private SpeakerOscilloscopeControl rightSpeakerOscilloscopeControl;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
