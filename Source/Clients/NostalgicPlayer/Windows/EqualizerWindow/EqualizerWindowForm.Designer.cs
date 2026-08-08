namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.EqualizerWindow
{
	partial class EqualizerWindowForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EqualizerWindowForm));
			equalizerPanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel();
			equalizerControl = new EqualizerControl();
			equalizerPanel.SuspendLayout();
			SuspendLayout();
			// 
			// equalizerPanel
			// 
			equalizerPanel.Controls.Add(equalizerControl);
			equalizerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			equalizerPanel.Location = new System.Drawing.Point(8, 0);
			equalizerPanel.Name = "equalizerPanel";
			equalizerPanel.Size = new System.Drawing.Size(487, 218);
			equalizerPanel.TabIndex = 0;
			// 
			// equalizerControl
			// 
			equalizerControl.Location = new System.Drawing.Point(0, 0);
			equalizerControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			equalizerControl.Name = "equalizerControl";
			equalizerControl.Size = new System.Drawing.Size(487, 218);
			equalizerControl.TabIndex = 0;
			equalizerControl.EqualizerChanged += EqualizerControl_EqualizerChanged;
			// 
			// EqualizerWindowForm
			// 
			AllowResizing = false;
			ClientSize = new System.Drawing.Size(503, 226);
			Controls.Add(equalizerPanel);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "EqualizerWindowForm";
			Padding = new System.Windows.Forms.Padding(8, 0, 8, 8);
			FormClosed += EqualizerWindowForm_FormClosed;
			Shown += EqualizerWindowForm_Shown;
			equalizerPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel equalizerPanel;
		private EqualizerControl equalizerControl;
	}
}
