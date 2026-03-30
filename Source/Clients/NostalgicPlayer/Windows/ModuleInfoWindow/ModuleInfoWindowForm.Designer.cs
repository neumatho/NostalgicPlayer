

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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModuleInfoWindowForm));
			infoPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages.InfoPageControl();
			tabControl = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTab();
			tabInfoPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			tabCommentPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			commentPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages.CommentPageControl();
			navigatorLyricsPage = new Krypton.Navigator.KryptonPage();
			lyricsPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages.LyricsPageControl();
			navigatorPicturesPage = new Krypton.Navigator.KryptonPage();
			picturesPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages.PicturesPageControl();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			((System.ComponentModel.ISupportInitialize)tabControl).BeginInit();
			tabControl.SuspendLayout();
			tabInfoPage.SuspendLayout();
			tabCommentPage.SuspendLayout();
			tabLyricsPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorPicturesPage).BeginInit();
			navigatorPicturesPage.SuspendLayout();
			SuspendLayout();
			// 
			// infoPageControl
			// 
			infoPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			infoPageControl.Location = new System.Drawing.Point(0, 0);
			infoPageControl.Name = "infoPageControl";
			infoPageControl.Size = new System.Drawing.Size(282, 158);
			infoPageControl.TabIndex = 0;
			// 
			// tabControl
			// 
			navigator.AllowPageReorder = false;
			navigator.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			navigator.Button.ButtonDisplayLogic = Krypton.Navigator.ButtonDisplayLogic.Context;
			navigator.Button.CloseButtonAction = Krypton.Navigator.CloseButtonAction.RemovePageAndDispose;
			navigator.Button.CloseButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			navigator.Button.ContextButtonAction = Krypton.Navigator.ContextButtonAction.SelectPage;
			navigator.Button.ContextButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			navigator.Button.ContextMenuMapImage = Krypton.Navigator.MapKryptonPageImage.Small;
			navigator.Button.ContextMenuMapText = Krypton.Navigator.MapKryptonPageText.TextTitle;
			navigator.Button.NextButtonAction = Krypton.Navigator.DirectionButtonAction.ModeAppropriateAction;
			navigator.Button.NextButtonDisplay = Krypton.Navigator.ButtonDisplay.Logic;
			navigator.Button.PreviousButtonAction = Krypton.Navigator.DirectionButtonAction.ModeAppropriateAction;
			navigator.Button.PreviousButtonDisplay = Krypton.Navigator.ButtonDisplay.Logic;
			navigator.ControlKryptonFormFeatures = false;
			navigator.Location = new System.Drawing.Point(8, 8);
			navigator.Name = "navigator";
			navigator.NavigatorMode = Krypton.Navigator.NavigatorMode.BarTabGroup;
			navigator.Owner = null;
			navigator.PageBackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
			navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] { navigatorInfoPage, navigatorCommentPage, navigatorLyricsPage, navigatorPicturesPage });
			navigator.Palette = fontPalette;
			navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			navigator.SelectedIndex = 0;
			navigator.Size = new System.Drawing.Size(284, 184);
			navigator.TabIndex = 1;
			// 
			// tabInfoPage
			// 
			navigatorInfoPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorInfoPage.Controls.Add(infoPageControl);
			navigatorInfoPage.Flags = 65534;
			navigatorInfoPage.LastVisibleSet = true;
			navigatorInfoPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorInfoPage.Name = "navigatorInfoPage";
			navigatorInfoPage.Size = new System.Drawing.Size(282, 158);
			navigatorInfoPage.Text = "";
			navigatorInfoPage.ToolTipTitle = "Page ToolTip";
			navigatorInfoPage.UniqueName = "c99656c653d3472bb30017074959420b";
			// 
			// tabCommentPage
			// 
			navigatorCommentPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorCommentPage.Controls.Add(commentPageControl);
			navigatorCommentPage.Flags = 65534;
			navigatorCommentPage.LastVisibleSet = true;
			navigatorCommentPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorCommentPage.Name = "navigatorCommentPage";
			navigatorCommentPage.Size = new System.Drawing.Size(282, 158);
			navigatorCommentPage.Text = "";
			navigatorCommentPage.ToolTipTitle = "Page ToolTip";
			navigatorCommentPage.UniqueName = "a6135a5732ce46a98b44f9d9f3bed53f";
			// 
			// commentPageControl
			// 
			commentPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			commentPageControl.Location = new System.Drawing.Point(0, 0);
			commentPageControl.Name = "commentPageControl";
			commentPageControl.Size = new System.Drawing.Size(282, 158);
			commentPageControl.TabIndex = 0;
			// 
			// tabLyricsPage
			// 
			navigatorLyricsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorLyricsPage.Controls.Add(lyricsPageControl);
			navigatorLyricsPage.Flags = 65534;
			navigatorLyricsPage.LastVisibleSet = true;
			navigatorLyricsPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorLyricsPage.Name = "navigatorLyricsPage";
			navigatorLyricsPage.Size = new System.Drawing.Size(282, 158);
			navigatorLyricsPage.Text = "";
			navigatorLyricsPage.ToolTipTitle = "Page ToolTip";
			navigatorLyricsPage.UniqueName = "c12067c426fe41ada484078b6b2d957f";
			// 
			// lyricsPageControl
			// 
			lyricsPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			lyricsPageControl.Location = new System.Drawing.Point(0, 0);
			lyricsPageControl.Name = "lyricsPageControl";
			lyricsPageControl.Size = new System.Drawing.Size(282, 158);
			lyricsPageControl.TabIndex = 0;
			// 
			// navigatorPicturesPage
			// 
			navigatorPicturesPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorPicturesPage.Controls.Add(picturesPageControl);
			navigatorPicturesPage.Flags = 65534;
			navigatorPicturesPage.LastVisibleSet = true;
			navigatorPicturesPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorPicturesPage.Name = "navigatorPicturesPage";
			navigatorPicturesPage.Size = new System.Drawing.Size(282, 158);
			navigatorPicturesPage.Text = "";
			navigatorPicturesPage.ToolTipTitle = "Page ToolTip";
			navigatorPicturesPage.UniqueName = "3facf91c879c4e6b8ef9b543ed9630a2";
			// 
			// picturesPageControl
			// 
			picturesPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			picturesPageControl.Location = new System.Drawing.Point(0, 0);
			picturesPageControl.Name = "picturesPageControl";
			picturesPageControl.Size = new System.Drawing.Size(282, 158);
			picturesPageControl.TabIndex = 0;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
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
			tabLyricsPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)navigatorPicturesPage).EndInit();
			navigatorPicturesPage.ResumeLayout(false);
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
		private Krypton.Navigator.KryptonPage navigatorPicturesPage;
		private Pages.PicturesPageControl picturesPageControl;
		private Kit.Gui.Components.FontPalette fontPalette;
	}
}