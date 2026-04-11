

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow
{
	partial class ModuleInfoWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModuleInfoWindowForm));
			infoPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages.InfoPageControl();
			tabControl = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTab();
			tabInfoPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			tabCommentPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			commentPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages.CommentPageControl();
			tabPicturesPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			picturesPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages.PicturesPageControl();
			tabLyricsPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			lyricsPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages.LyricsPageControl();
			((System.ComponentModel.ISupportInitialize)tabControl).BeginInit();
			tabControl.SuspendLayout();
			tabInfoPage.SuspendLayout();
			tabCommentPage.SuspendLayout();
			tabPicturesPage.SuspendLayout();
			tabLyricsPage.SuspendLayout();
			SuspendLayout();
			// 
			// infoPageControl
			// 
			infoPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			infoPageControl.Location = new System.Drawing.Point(0, 0);
			infoPageControl.Name = "infoPageControl";
			infoPageControl.Size = new System.Drawing.Size(282, 157);
			infoPageControl.TabIndex = 0;
			// 
			// tabControl
			// 
			tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tabControl.Controls.Add(tabInfoPage);
			tabControl.Controls.Add(tabCommentPage);
			tabControl.Controls.Add(tabLyricsPage);
			tabControl.Controls.Add(tabPicturesPage);
			tabControl.Location = new System.Drawing.Point(8, 8);
			tabControl.Name = "tabControl";
			tabControl.SelectedIndex = 0;
			tabControl.Size = new System.Drawing.Size(284, 184);
			tabControl.TabIndex = 1;
			// 
			// tabInfoPage
			// 
			tabInfoPage.BackColor = System.Drawing.Color.Transparent;
			tabInfoPage.Controls.Add(infoPageControl);
			tabInfoPage.Location = new System.Drawing.Point(1, 24);
			tabInfoPage.MinimumSize = new System.Drawing.Size(50, 50);
			tabInfoPage.Name = "tabInfoPage";
			tabInfoPage.Size = new System.Drawing.Size(282, 157);
			tabInfoPage.TabIndex = 0;
			// 
			// tabCommentPage
			// 
			tabCommentPage.BackColor = System.Drawing.Color.Transparent;
			tabCommentPage.Controls.Add(commentPageControl);
			tabCommentPage.Location = new System.Drawing.Point(1, 24);
			tabCommentPage.MinimumSize = new System.Drawing.Size(50, 50);
			tabCommentPage.Name = "tabCommentPage";
			tabCommentPage.Size = new System.Drawing.Size(282, 157);
			tabCommentPage.TabIndex = 1;
			// 
			// commentPageControl
			// 
			commentPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			commentPageControl.Location = new System.Drawing.Point(0, 0);
			commentPageControl.Name = "commentPageControl";
			commentPageControl.Size = new System.Drawing.Size(282, 157);
			commentPageControl.TabIndex = 0;
			// 
			// tabPicturesPage
			// 
			tabPicturesPage.BackColor = System.Drawing.Color.Transparent;
			tabPicturesPage.Controls.Add(picturesPageControl);
			tabPicturesPage.Location = new System.Drawing.Point(1, 24);
			tabPicturesPage.MinimumSize = new System.Drawing.Size(50, 50);
			tabPicturesPage.Name = "tabPicturesPage";
			tabPicturesPage.Size = new System.Drawing.Size(282, 157);
			tabPicturesPage.TabIndex = 3;
			// 
			// picturesPageControl
			// 
			picturesPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			picturesPageControl.Location = new System.Drawing.Point(0, 0);
			picturesPageControl.Name = "picturesPageControl";
			picturesPageControl.Size = new System.Drawing.Size(282, 157);
			picturesPageControl.TabIndex = 0;
			// 
			// tabLyricsPage
			// 
			tabLyricsPage.BackColor = System.Drawing.Color.Transparent;
			tabLyricsPage.Controls.Add(lyricsPageControl);
			tabLyricsPage.Location = new System.Drawing.Point(1, 24);
			tabLyricsPage.MinimumSize = new System.Drawing.Size(50, 50);
			tabLyricsPage.Name = "tabLyricsPage";
			tabLyricsPage.Size = new System.Drawing.Size(282, 157);
			tabLyricsPage.TabIndex = 2;
			// 
			// lyricsPageControl
			// 
			lyricsPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			lyricsPageControl.Location = new System.Drawing.Point(0, 0);
			lyricsPageControl.Name = "lyricsPageControl";
			lyricsPageControl.Size = new System.Drawing.Size(282, 157);
			lyricsPageControl.TabIndex = 0;
			// 
			// ModuleInfoWindowForm
			// 
			ClientSize = new System.Drawing.Size(300, 200);
			Controls.Add(tabControl);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(316, 239);
			Name = "ModuleInfoWindowForm";
			FormClosed += ModuleInfoWindowForm_FormClosed;
			((System.ComponentModel.ISupportInitialize)tabControl).EndInit();
			tabControl.ResumeLayout(false);
			tabInfoPage.ResumeLayout(false);
			tabCommentPage.ResumeLayout(false);
			tabPicturesPage.ResumeLayout(false);
			tabLyricsPage.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private Pages.InfoPageControl infoPageControl;
		private NostalgicPlayer.Controls.Containers.NostalgicTab tabControl;
		private NostalgicPlayer.Controls.Containers.NostalgicTabPage tabInfoPage;
		private NostalgicPlayer.Controls.Containers.NostalgicTabPage tabCommentPage;
		private Pages.CommentPageControl commentPageControl;
		private NostalgicPlayer.Controls.Containers.NostalgicTabPage tabLyricsPage;
		private Pages.LyricsPageControl lyricsPageControl;
		private NostalgicPlayer.Controls.Containers.NostalgicTabPage tabPicturesPage;
		private Pages.PicturesPageControl picturesPageControl;
	}
}