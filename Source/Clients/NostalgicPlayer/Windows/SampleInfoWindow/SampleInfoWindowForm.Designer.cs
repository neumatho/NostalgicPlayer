
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow
{
	partial class SampleInfoWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SampleInfoWindowForm));
			navigator = new Krypton.Navigator.KryptonNavigator();
			navigatorInstrumentPage = new Krypton.Navigator.KryptonPage();
			instrumentPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow.Pages.InstrumentPageControl();
			navigatorSamplePage = new Krypton.Navigator.KryptonPage();
			samplePageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow.Pages.SamplePageControl();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			((System.ComponentModel.ISupportInitialize)navigator).BeginInit();
			((System.ComponentModel.ISupportInitialize)navigatorInstrumentPage).BeginInit();
			navigatorInstrumentPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorSamplePage).BeginInit();
			navigatorSamplePage.SuspendLayout();
			SuspendLayout();
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
			navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] { navigatorInstrumentPage, navigatorSamplePage });
			navigator.Palette = fontPalette;
			navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			navigator.SelectedIndex = 0;
			navigator.Size = new System.Drawing.Size(432, 172);
			navigator.TabIndex = 0;
			navigator.SelectedPageChanged += Navigator_SelectedPageChanged;
			// 
			// navigatorInstrumentPage
			// 
			navigatorInstrumentPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorInstrumentPage.Controls.Add(instrumentPageControl);
			navigatorInstrumentPage.Flags = 65534;
			navigatorInstrumentPage.LastVisibleSet = true;
			navigatorInstrumentPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorInstrumentPage.Name = "navigatorInstrumentPage";
			navigatorInstrumentPage.Size = new System.Drawing.Size(430, 146);
			navigatorInstrumentPage.Text = "";
			navigatorInstrumentPage.ToolTipTitle = "Page ToolTip";
			navigatorInstrumentPage.UniqueName = "5d888e6082d44d78aac10a8a0c09a21e";
			// 
			// instrumentPageControl
			// 
			instrumentPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			instrumentPageControl.Location = new System.Drawing.Point(0, 0);
			instrumentPageControl.Name = "instrumentPageControl";
			instrumentPageControl.Size = new System.Drawing.Size(430, 146);
			instrumentPageControl.TabIndex = 0;
			// 
			// navigatorSamplePage
			// 
			navigatorSamplePage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorSamplePage.Controls.Add(samplePageControl);
			navigatorSamplePage.Flags = 65534;
			navigatorSamplePage.LastVisibleSet = true;
			navigatorSamplePage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorSamplePage.Name = "navigatorSamplePage";
			navigatorSamplePage.Size = new System.Drawing.Size(430, 146);
			navigatorSamplePage.Text = "";
			navigatorSamplePage.ToolTipTitle = "Page ToolTip";
			navigatorSamplePage.UniqueName = "6f932f4ccf4b4441a28afbd3a888c881";
			// 
			// samplePageControl
			// 
			samplePageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			samplePageControl.Location = new System.Drawing.Point(0, 0);
			samplePageControl.Name = "samplePageControl";
			samplePageControl.Size = new System.Drawing.Size(430, 146);
			samplePageControl.TabIndex = 0;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// SampleInfoWindowForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(448, 188);
			Controls.Add(navigator);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(464, 227);
			Name = "SampleInfoWindowForm";
			FormClosed += SampleInfoWindowForm_FormClosed;
			((System.ComponentModel.ISupportInitialize)navigator).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorInstrumentPage).EndInit();
			navigatorInstrumentPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)navigatorSamplePage).EndInit();
			navigatorSamplePage.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private Krypton.Navigator.KryptonNavigator navigator;
		private Krypton.Navigator.KryptonPage navigatorInstrumentPage;
		private Krypton.Navigator.KryptonPage navigatorSamplePage;
		private Pages.InstrumentPageControl instrumentPageControl;
		private Pages.SamplePageControl samplePageControl;
		private Kit.Gui.Components.FontPalette fontPalette;
	}
}