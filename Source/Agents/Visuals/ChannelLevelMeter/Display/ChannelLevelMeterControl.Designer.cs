namespace Polycode.NostalgicPlayer.Agent.Visual.ChannelLevelMeter.Display
{
	partial class ChannelLevelMeterControl
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
			levelsPanel = new System.Windows.Forms.Panel();
			pulseTimer = new System.Windows.Forms.Timer(components);
			contextMenu = new System.Windows.Forms.ContextMenuStrip(components);
			horizontalItem = new System.Windows.Forms.ToolStripMenuItem();
			verticalItem = new System.Windows.Forms.ToolStripMenuItem();
			contextMenu.SuspendLayout();
			SuspendLayout();
			//
			// levelsPanel
			//
			levelsPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			levelsPanel.BackColor = System.Drawing.Color.White;
			levelsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			levelsPanel.ContextMenuStrip = contextMenu;
			levelsPanel.Location = new System.Drawing.Point(8, 8);
			levelsPanel.Name = "levelsPanel";
			levelsPanel.Size = new System.Drawing.Size(240, 56);
			levelsPanel.TabIndex = 0;
			levelsPanel.Resize += Control_Resize;
			//
			// pulseTimer
			//
			pulseTimer.Interval = 20;
			pulseTimer.Tick += PulseTimer_Tick;
			//
			// contextMenu
			//
			contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { horizontalItem, verticalItem });
			contextMenu.Name = "contextMenu";
			contextMenu.Size = new System.Drawing.Size(135, 48);
			contextMenu.Opening += ContextMenu_Opening;
			//
			// horizontalItem
			//
			horizontalItem.Checked = true;
			horizontalItem.CheckState = System.Windows.Forms.CheckState.Checked;
			horizontalItem.Name = "horizontalItem";
			horizontalItem.Size = new System.Drawing.Size(134, 22);
			horizontalItem.Text = "Horizontal";
			horizontalItem.Click += HorizontalItem_Click;
			//
			// verticalItem
			//
			verticalItem.Name = "verticalItem";
			verticalItem.Size = new System.Drawing.Size(134, 22);
			verticalItem.Text = "Vertical";
			verticalItem.Click += VerticalItem_Click;
			//
			// ChannelLevelMeterControl
			//
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(levelsPanel);
			MinimumSize = new System.Drawing.Size(256, 72);
			Name = "ChannelLevelMeterControl";
			Size = new System.Drawing.Size(256, 72);
			contextMenu.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel levelsPanel;
		private System.Windows.Forms.Timer pulseTimer;
		private System.Windows.Forms.ContextMenuStrip contextMenu;
		private System.Windows.Forms.ToolStripMenuItem horizontalItem;
		private System.Windows.Forms.ToolStripMenuItem verticalItem;
	}
}
