namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudiusWindowForm));
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			navigator = new Krypton.Navigator.KryptonNavigator();
			navigatorTrendingPage = new Krypton.Navigator.KryptonPage();
			trendingPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages.TrendingPageControl();
			navigatorSearchPage = new Krypton.Navigator.KryptonPage();
			searchPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages.SearchPageControl();
			((System.ComponentModel.ISupportInitialize)navigator).BeginInit();
			((System.ComponentModel.ISupportInitialize)navigatorTrendingPage).BeginInit();
			navigatorTrendingPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorSearchPage).BeginInit();
			navigatorSearchPage.SuspendLayout();
			SuspendLayout();
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// navigator
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
			navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] { navigatorTrendingPage, navigatorSearchPage });
			navigator.Palette = fontPalette;
			navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			navigator.SelectedIndex = 0;
			navigator.Size = new System.Drawing.Size(768, 425);
			navigator.TabIndex = 0;
			navigator.SelectedPageChanged += Navigator_SelectedPageChanged;
			// 
			// navigatorTrendingPage
			// 
			navigatorTrendingPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorTrendingPage.Controls.Add(trendingPageControl);
			navigatorTrendingPage.Flags = 65534;
			navigatorTrendingPage.LastVisibleSet = true;
			navigatorTrendingPage.MinimumSize = new System.Drawing.Size(150, 50);
			navigatorTrendingPage.Name = "navigatorTrendingPage";
			navigatorTrendingPage.Size = new System.Drawing.Size(766, 399);
			navigatorTrendingPage.Text = "";
			navigatorTrendingPage.ToolTipTitle = "Page ToolTip";
			navigatorTrendingPage.UniqueName = "c6800b895c2748d58349df89d8a596b0";
			// 
			// trendingPageControl
			// 
			trendingPageControl.BackColor = System.Drawing.Color.Transparent;
			trendingPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			trendingPageControl.Location = new System.Drawing.Point(0, 0);
			trendingPageControl.Name = "trendingPageControl";
			trendingPageControl.Size = new System.Drawing.Size(766, 399);
			trendingPageControl.TabIndex = 0;
			// 
			// navigatorSearchPage
			// 
			navigatorSearchPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorSearchPage.Controls.Add(searchPageControl);
			navigatorSearchPage.Flags = 65534;
			navigatorSearchPage.LastVisibleSet = true;
			navigatorSearchPage.MinimumSize = new System.Drawing.Size(150, 50);
			navigatorSearchPage.Name = "navigatorSearchPage";
			navigatorSearchPage.Size = new System.Drawing.Size(766, 399);
			navigatorSearchPage.Text = "";
			navigatorSearchPage.ToolTipTitle = "Page ToolTip";
			navigatorSearchPage.UniqueName = "714a1f4654a044a1809724d72e41db75";
			// 
			// searchPageControl
			// 
			searchPageControl.BackColor = System.Drawing.Color.Transparent;
			searchPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			searchPageControl.Location = new System.Drawing.Point(0, 0);
			searchPageControl.Name = "searchPageControl";
			searchPageControl.Size = new System.Drawing.Size(766, 399);
			searchPageControl.TabIndex = 0;
			// 
			// AudiusWindowForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(784, 441);
			Controls.Add(navigator);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(600, 480);
			Name = "AudiusWindowForm";
			Palette = fontPalette;
			PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			FormClosed += AudiusForm_FormClosed;
			Shown += AudiusForm_Shown;
			((System.ComponentModel.ISupportInitialize)navigator).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorTrendingPage).EndInit();
			navigatorTrendingPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)navigatorSearchPage).EndInit();
			navigatorSearchPage.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private GuiKit.Components.FontPalette fontPalette;
		private Krypton.Navigator.KryptonNavigator navigator;
		private Krypton.Navigator.KryptonPage navigatorTrendingPage;
		private Pages.TrendingPageControl trendingPageControl;
		private Krypton.Navigator.KryptonPage navigatorSearchPage;
		private Pages.SearchPageControl searchPageControl;
	}
}