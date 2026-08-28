namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow
{
	partial class AudiusWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudiusWindowForm));
			tabControl = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTab();
			tabTrendingPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			trendingPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages.TrendingPageControl();
			tabSearchPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			searchPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages.SearchPageControl();
			((System.ComponentModel.ISupportInitialize)tabControl).BeginInit();
			tabControl.SuspendLayout();
			tabTrendingPage.SuspendLayout();
			tabSearchPage.SuspendLayout();
			SuspendLayout();
			// 
			// tabControl
			// 
			tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tabControl.Controls.Add(tabTrendingPage);
			tabControl.Controls.Add(tabSearchPage);
			tabControl.Location = new System.Drawing.Point(8, 8);
			tabControl.Name = "tabControl";
			tabControl.SelectedIndex = 0;
			tabControl.Size = new System.Drawing.Size(768, 425);
			tabControl.TabIndex = 0;
			tabControl.SelectedIndexChanged += Tab_SelectedIndexChanged;
			// 
			// tabTrendingPage
			// 
			tabTrendingPage.Controls.Add(trendingPageControl);
			tabTrendingPage.Location = new System.Drawing.Point(1, 24);
			tabTrendingPage.MinimumSize = new System.Drawing.Size(150, 50);
			tabTrendingPage.Name = "tabTrendingPage";
			tabTrendingPage.Size = new System.Drawing.Size(766, 398);
			tabTrendingPage.TabIndex = 0;
			// 
			// trendingPageControl
			// 
			trendingPageControl.BackColor = System.Drawing.Color.Transparent;
			trendingPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			trendingPageControl.Location = new System.Drawing.Point(0, 0);
			trendingPageControl.Name = "trendingPageControl";
			trendingPageControl.Size = new System.Drawing.Size(766, 398);
			trendingPageControl.TabIndex = 0;
			// 
			// tabSearchPage
			// 
			tabSearchPage.Controls.Add(searchPageControl);
			tabSearchPage.Location = new System.Drawing.Point(1, 24);
			tabSearchPage.MinimumSize = new System.Drawing.Size(150, 50);
			tabSearchPage.Name = "tabSearchPage";
			tabSearchPage.Size = new System.Drawing.Size(766, 398);
			tabSearchPage.TabIndex = 1;
			// 
			// searchPageControl
			// 
			searchPageControl.BackColor = System.Drawing.Color.Transparent;
			searchPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			searchPageControl.Location = new System.Drawing.Point(0, 0);
			searchPageControl.Name = "searchPageControl";
			searchPageControl.Size = new System.Drawing.Size(766, 398);
			searchPageControl.TabIndex = 0;
			// 
			// AudiusWindowForm
			// 
			ClientSize = new System.Drawing.Size(784, 441);
			Controls.Add(tabControl);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(600, 480);
			Name = "AudiusWindowForm";
			FormClosed += AudiusForm_FormClosed;
			Shown += AudiusForm_Shown;
			((System.ComponentModel.ISupportInitialize)tabControl).EndInit();
			tabControl.ResumeLayout(false);
			tabTrendingPage.ResumeLayout(false);
			tabSearchPage.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicTab tabControl;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage tabTrendingPage;
		private Pages.TrendingPageControl trendingPageControl;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage tabSearchPage;
		private Pages.SearchPageControl searchPageControl;
	}
}