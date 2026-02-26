namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow
{
	partial class ModuleListControl
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
			moduleListItemsControl = new ModuleListItemsControl();
			moduleListScrollBar = new System.Windows.Forms.VScrollBar();
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
			controlGroup.Panel.Controls.Add(moduleListItemsControl);
			controlGroup.Panel.Controls.Add(moduleListScrollBar);
			controlGroup.Size = new System.Drawing.Size(337, 170);
			controlGroup.StateNormal.Back.Color1 = System.Drawing.Color.White;
			controlGroup.TabIndex = 0;
			// 
			// moduleListItemsControl
			// 
			moduleListItemsControl.AllowDrop = true;
			moduleListItemsControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			moduleListItemsControl.BackColor = System.Drawing.Color.Transparent;
			moduleListItemsControl.Location = new System.Drawing.Point(0, 0);
			moduleListItemsControl.Name = "moduleListItemsControl";
			moduleListItemsControl.Size = new System.Drawing.Size(318, 168);
			moduleListItemsControl.TabIndex = 0;
			moduleListItemsControl.DragDrop += ListItemControl_DragDrop;
			moduleListItemsControl.KeyPress += ListItemControl_KeyPress;
			moduleListItemsControl.MouseDoubleClick += ListItemControl_MouseDoubleClick;
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
			// ModuleListControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(controlGroup);
			Name = "ModuleListControl";
			Size = new System.Drawing.Size(337, 170);
			((System.ComponentModel.ISupportInitialize)controlGroup.Panel).EndInit();
			controlGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)controlGroup).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Krypton.Toolkit.KryptonGroup controlGroup;
		private System.Windows.Forms.VScrollBar moduleListScrollBar;
		private ModuleListItemsControl moduleListItemsControl;
	}
}
