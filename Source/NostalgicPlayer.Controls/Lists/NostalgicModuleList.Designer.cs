namespace Polycode.NostalgicPlayer.Controls.Lists
{
	partial class NostalgicModuleList
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
			moduleListInternal = new NostalgicModuleListInternal();
			moduleListScrollBar = new NostalgicVScrollBar();
			controlBox.SuspendLayout();
			SuspendLayout();
			// 
			// controlBox
			// 
			controlBox.Controls.Add(moduleListInternal);
			controlBox.Controls.Add(moduleListScrollBar);
			controlBox.Dock = System.Windows.Forms.DockStyle.Fill;
			controlBox.Location = new System.Drawing.Point(0, 0);
			controlBox.Name = "controlBox";
			controlBox.Size = new System.Drawing.Size(337, 170);
			controlBox.TabIndex = 0;
			// 
			// moduleListInternal
			// 
			moduleListInternal.AllowDrop = true;
			moduleListInternal.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			moduleListInternal.BackColor = System.Drawing.Color.Transparent;
			moduleListInternal.Location = new System.Drawing.Point(0, 0);
			moduleListInternal.Name = "moduleListInternal";
			moduleListInternal.Size = new System.Drawing.Size(318, 168);
			moduleListInternal.TabIndex = 0;
			moduleListInternal.DragDrop += ListItemControl_DragDrop;
			moduleListInternal.KeyPress += ListItemControl_KeyPress;
			moduleListInternal.MouseDoubleClick += ListItemControl_MouseDoubleClick;
			// 
			// moduleListScrollBar
			// 
			moduleListScrollBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			moduleListScrollBar.Enabled = false;
			moduleListScrollBar.LargeChange = 1;
			moduleListScrollBar.Location = new System.Drawing.Point(317, 0);
			moduleListScrollBar.Maximum = 0;
			moduleListScrollBar.Name = "moduleListScrollBar";
			moduleListScrollBar.Size = new System.Drawing.Size(17, 168);
			moduleListScrollBar.TabIndex = 1;
			moduleListScrollBar.ValueChanged += ScrollBar_ValueChanged;
			// 
			// NostalgicModuleList
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(controlBox);
			Name = "NostalgicModuleList";
			Size = new System.Drawing.Size(337, 170);
			controlBox.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private NostalgicPlayer.Controls.Containers.NostalgicBox controlBox;
		private NostalgicPlayer.Controls.Lists.NostalgicVScrollBar moduleListScrollBar;
		private NostalgicPlayer.Controls.Lists.NostalgicModuleListInternal moduleListInternal;
	}
}
