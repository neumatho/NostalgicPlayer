namespace Polycode.NostalgicPlayer.Controls.Containers
{
	partial class NostalgicFlowLayoutPanel
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
			flowLayoutPanelInternal = new NostalgicFlowLayoutPanelInternal();
			nostalgicVScrollBar = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicVScrollBar();
			nostalgicHScrollBar = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicHScrollBar();
			cornerPanel = new NostalgicPanel();
			nostalgicBox = new NostalgicBox();
			nostalgicBox.SuspendLayout();
			SuspendLayout();
			// 
			// flowLayoutPanelInternal
			// 
			flowLayoutPanelInternal.BackColor = System.Drawing.Color.Transparent;
			flowLayoutPanelInternal.Location = new System.Drawing.Point(0, 0);
			flowLayoutPanelInternal.Name = "flowLayoutPanelInternal";
			flowLayoutPanelInternal.Size = new System.Drawing.Size(131, 131);
			flowLayoutPanelInternal.TabIndex = 0;
			// 
			// nostalgicVScrollBar
			// 
			nostalgicVScrollBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			nostalgicVScrollBar.Location = new System.Drawing.Point(131, 0);
			nostalgicVScrollBar.Name = "nostalgicVScrollBar";
			nostalgicVScrollBar.Size = new System.Drawing.Size(17, 131);
			nostalgicVScrollBar.TabIndex = 1;
			// 
			// nostalgicHScrollBar
			// 
			nostalgicHScrollBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nostalgicHScrollBar.Location = new System.Drawing.Point(0, 131);
			nostalgicHScrollBar.Name = "nostalgicHScrollBar";
			nostalgicHScrollBar.Size = new System.Drawing.Size(131, 17);
			nostalgicHScrollBar.TabIndex = 2;
			// 
			// cornerPanel
			// 
			cornerPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			cornerPanel.Location = new System.Drawing.Point(131, 131);
			cornerPanel.Name = "cornerPanel";
			cornerPanel.Size = new System.Drawing.Size(17, 17);
			cornerPanel.TabIndex = 3;
			cornerPanel.Visible = false;
			// 
			// nostalgicBox
			// 
			nostalgicBox.Controls.Add(cornerPanel);
			nostalgicBox.Controls.Add(nostalgicHScrollBar);
			nostalgicBox.Controls.Add(nostalgicVScrollBar);
			nostalgicBox.Controls.Add(flowLayoutPanelInternal);
			nostalgicBox.Dock = System.Windows.Forms.DockStyle.Fill;
			nostalgicBox.Location = new System.Drawing.Point(0, 0);
			nostalgicBox.Name = "nostalgicBox";
			nostalgicBox.Size = new System.Drawing.Size(150, 150);
			nostalgicBox.TabIndex = 0;
			// 
			// NostalgicFlowLayoutPanel
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(nostalgicBox);
			Name = "NostalgicFlowLayoutPanel";
			nostalgicBox.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private NostalgicFlowLayoutPanelInternal flowLayoutPanelInternal;
		private Polycode.NostalgicPlayer.Controls.Lists.NostalgicVScrollBar nostalgicVScrollBar;
		private Polycode.NostalgicPlayer.Controls.Lists.NostalgicHScrollBar nostalgicHScrollBar;
		private NostalgicPanel cornerPanel;
		private NostalgicBox nostalgicBox;
	}
}
