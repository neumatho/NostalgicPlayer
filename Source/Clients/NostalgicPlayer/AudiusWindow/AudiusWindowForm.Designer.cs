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
			trendingPageControl1 = new Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages.TrendingPageControl();
			((System.ComponentModel.ISupportInitialize)navigator).BeginInit();
			((System.ComponentModel.ISupportInitialize)navigatorTrendingPage).BeginInit();
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
			navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] { navigatorTrendingPage });
			navigator.Palette = fontPalette;
			navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			navigator.SelectedIndex = 0;
			navigator.Size = new System.Drawing.Size(768, 395);
			navigator.TabIndex = 0;
			// 
			// navigatorTrendingPage
			// 
			navigatorTrendingPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorTrendingPage.Controls.Add(trendingPageControl1);
			navigatorTrendingPage.Flags = 65534;
			navigatorTrendingPage.LastVisibleSet = true;
			navigatorTrendingPage.MinimumSize = new System.Drawing.Size(150, 50);
			navigatorTrendingPage.Name = "navigatorTrendingPage";
			navigatorTrendingPage.Size = new System.Drawing.Size(766, 369);
			navigatorTrendingPage.Text = "";
			navigatorTrendingPage.ToolTipTitle = "Page ToolTip";
			navigatorTrendingPage.UniqueName = "c6800b895c2748d58349df89d8a596b0";
			// 
			// trendingPageControl1
			// 
			trendingPageControl1.BackColor = System.Drawing.Color.Transparent;
			trendingPageControl1.Location = new System.Drawing.Point(0, 0);
			trendingPageControl1.Name = "trendingPageControl1";
			trendingPageControl1.Size = new System.Drawing.Size(766, 368);
			trendingPageControl1.TabIndex = 1;
			// 
			// AudiusWindowForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(784, 411);
			Controls.Add(navigator);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(800, 450);
			Name = "AudiusWindowForm";
			Palette = fontPalette;
			PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			((System.ComponentModel.ISupportInitialize)navigator).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorTrendingPage).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private GuiKit.Components.FontPalette fontPalette;
		private Krypton.Navigator.KryptonNavigator navigator;
		private Krypton.Navigator.KryptonPage navigatorTrendingPage;
		private Pages.TrendingPageControl trendingPageControl1;
	}
}