namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	partial class ModLibraryListControl
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
			moduleListView = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicDataGridView();
			statusLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			fontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			searchTimer = new System.Windows.Forms.Timer(components);
			((System.ComponentModel.ISupportInitialize)moduleListView).BeginInit();
			SuspendLayout();
			// 
			// moduleListView
			// 
			moduleListView.Dock = System.Windows.Forms.DockStyle.Fill;
			moduleListView.Location = new System.Drawing.Point(0, 0);
			moduleListView.Name = "moduleListView";
			moduleListView.Size = new System.Drawing.Size(250, 150);
			moduleListView.TabIndex = 0;
			moduleListView.VirtualMode = true;
			moduleListView.CellValueNeeded += ModuleListView_CellValueNeeded;
			moduleListView.CellToolTipTextNeeded += ModuleListView_CellToolTipTextNeeded;
			moduleListView.ColumnHeaderMouseClick += ModuleListView_ColumnHeaderMouseClick;
			moduleListView.KeyDown += ModuleListView_KeyDown;
			moduleListView.KeyPress += ModuleListView_KeyPress;
			moduleListView.MouseClick += ModuleListView_MouseClick;
			moduleListView.MouseDoubleClick += ModuleListView_DoubleClick;
			// 
			// statusLabel
			// 
			statusLabel.Location = new System.Drawing.Point(25, 67);
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new System.Drawing.Size(0, 15);
			statusLabel.TabIndex = 1;
			statusLabel.UseFont = fontConfiguration;
			// 
			// fontConfiguration
			// 
			fontConfiguration.FontSize = 1;
			// 
			// searchTimer
			// 
			searchTimer.Interval = 1000;
			searchTimer.Tick += SearchTimer_Tick;
			// 
			// ModLibraryListControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(statusLabel);
			Controls.Add(moduleListView);
			Name = "ModLibraryListControl";
			Size = new System.Drawing.Size(250, 150);
			((System.ComponentModel.ISupportInitialize)moduleListView).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private NostalgicPlayer.Controls.Lists.NostalgicDataGridView moduleListView;
		private NostalgicPlayer.Controls.Texts.NostalgicLabel statusLabel;
		private NostalgicPlayer.Controls.Components.FontConfiguration fontConfiguration;
		private System.Windows.Forms.Timer searchTimer;
	}
}
