namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages
{
	partial class InfoPageControl
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
			moduleInfoDataGridView = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicDataGridView();
			((System.ComponentModel.ISupportInitialize)moduleInfoDataGridView).BeginInit();
			SuspendLayout();
			// 
			// moduleInfoDataGridView
			// 
			moduleInfoDataGridView.AllowUserToSelectRows = false;
			moduleInfoDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			moduleInfoDataGridView.Location = new System.Drawing.Point(8, 8);
			moduleInfoDataGridView.Name = "moduleInfoDataGridView";
			moduleInfoDataGridView.Size = new System.Drawing.Size(266, 142);
			moduleInfoDataGridView.TabIndex = 0;
			moduleInfoDataGridView.CellContentClick += ModuleInfoDataGridView_CellContentClick;
			// 
			// InfoPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(moduleInfoDataGridView);
			Name = "InfoPageControl";
			Size = new System.Drawing.Size(282, 158);
			((System.ComponentModel.ISupportInitialize)moduleInfoDataGridView).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private Polycode.NostalgicPlayer.Controls.Lists.NostalgicDataGridView moduleInfoDataGridView;
	}
}
