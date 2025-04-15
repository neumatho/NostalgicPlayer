
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow
{
	partial class SettingsWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsWindowForm));
			navigator = new Krypton.Navigator.KryptonNavigator();
			navigatorOptionsPage = new Krypton.Navigator.KryptonPage();
			optionsPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.OptionsPageControl();
			navigatorModulesPage = new Krypton.Navigator.KryptonPage();
			modulesPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.ModulesPageControl();
			navigatorPathsPage = new Krypton.Navigator.KryptonPage();
			pathsPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.PathsPageControl();
			navigatorMixerPage = new Krypton.Navigator.KryptonPage();
			mixerPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.MixerPageControl();
			navigatorAgentsPage = new Krypton.Navigator.KryptonPage();
			agentsPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.AgentsPageControl();
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			applyButton = new Krypton.Toolkit.KryptonButton();
			bigFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			cancelButton = new Krypton.Toolkit.KryptonButton();
			okButton = new Krypton.Toolkit.KryptonButton();
			((System.ComponentModel.ISupportInitialize)navigator).BeginInit();
			((System.ComponentModel.ISupportInitialize)navigatorOptionsPage).BeginInit();
			navigatorOptionsPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorModulesPage).BeginInit();
			navigatorModulesPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorPathsPage).BeginInit();
			navigatorPathsPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorMixerPage).BeginInit();
			navigatorMixerPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorAgentsPage).BeginInit();
			navigatorAgentsPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
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
			navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] { navigatorOptionsPage, navigatorModulesPage, navigatorPathsPage, navigatorMixerPage, navigatorAgentsPage });
			navigator.LocalCustomPalette = fontPalette;
			navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			controlResource.SetResourceKey(navigator, null);
			navigator.SelectedIndex = 0;
			navigator.Size = new System.Drawing.Size(610, 383);
			navigator.TabIndex = 0;
			// 
			// navigatorOptionsPage
			// 
			navigatorOptionsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorOptionsPage.Controls.Add(optionsPageControl);
			navigatorOptionsPage.Flags = 65534;
			navigatorOptionsPage.LastVisibleSet = true;
			navigatorOptionsPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorOptionsPage.Name = "navigatorOptionsPage";
			controlResource.SetResourceKey(navigatorOptionsPage, null);
			navigatorOptionsPage.Size = new System.Drawing.Size(608, 357);
			navigatorOptionsPage.Text = "";
			navigatorOptionsPage.ToolTipTitle = "Page ToolTip";
			navigatorOptionsPage.UniqueName = "d6822b271a6149e5ae2f577bfe34d8d6";
			// 
			// optionsPageControl
			// 
			optionsPageControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			optionsPageControl.BackColor = System.Drawing.Color.Transparent;
			optionsPageControl.Location = new System.Drawing.Point(0, 0);
			optionsPageControl.Name = "optionsPageControl";
			controlResource.SetResourceKey(optionsPageControl, null);
			optionsPageControl.Size = new System.Drawing.Size(608, 357);
			optionsPageControl.TabIndex = 0;
			// 
			// navigatorModulesPage
			// 
			navigatorModulesPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorModulesPage.Controls.Add(modulesPageControl);
			navigatorModulesPage.Flags = 65534;
			navigatorModulesPage.LastVisibleSet = true;
			navigatorModulesPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorModulesPage.Name = "navigatorModulesPage";
			controlResource.SetResourceKey(navigatorModulesPage, null);
			navigatorModulesPage.Size = new System.Drawing.Size(608, 357);
			navigatorModulesPage.Text = "";
			navigatorModulesPage.ToolTipTitle = "Page ToolTip";
			navigatorModulesPage.UniqueName = "2140f293f54e46a4914afbe354fd002c";
			// 
			// modulesPageControl
			// 
			modulesPageControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			modulesPageControl.BackColor = System.Drawing.Color.Transparent;
			modulesPageControl.Location = new System.Drawing.Point(0, 0);
			modulesPageControl.Name = "modulesPageControl";
			controlResource.SetResourceKey(modulesPageControl, null);
			modulesPageControl.Size = new System.Drawing.Size(608, 356);
			modulesPageControl.TabIndex = 0;
			// 
			// navigatorPathsPage
			// 
			navigatorPathsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorPathsPage.Controls.Add(pathsPageControl);
			navigatorPathsPage.Flags = 65534;
			navigatorPathsPage.LastVisibleSet = true;
			navigatorPathsPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorPathsPage.Name = "navigatorPathsPage";
			controlResource.SetResourceKey(navigatorPathsPage, null);
			navigatorPathsPage.Size = new System.Drawing.Size(608, 348);
			navigatorPathsPage.Text = "";
			navigatorPathsPage.ToolTipTitle = "Page ToolTip";
			navigatorPathsPage.UniqueName = "690d92cb4fb74babaad9b326d3b4be6d";
			// 
			// pathsPageControl
			// 
			pathsPageControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pathsPageControl.BackColor = System.Drawing.Color.Transparent;
			pathsPageControl.Location = new System.Drawing.Point(0, 0);
			pathsPageControl.Name = "pathsPageControl";
			controlResource.SetResourceKey(pathsPageControl, null);
			pathsPageControl.Size = new System.Drawing.Size(608, 348);
			pathsPageControl.TabIndex = 0;
			// 
			// navigatorMixerPage
			// 
			navigatorMixerPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorMixerPage.Controls.Add(mixerPageControl);
			navigatorMixerPage.Flags = 65534;
			navigatorMixerPage.LastVisibleSet = true;
			navigatorMixerPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorMixerPage.Name = "navigatorMixerPage";
			controlResource.SetResourceKey(navigatorMixerPage, null);
			navigatorMixerPage.Size = new System.Drawing.Size(606, 348);
			navigatorMixerPage.Text = "";
			navigatorMixerPage.ToolTipTitle = "Page ToolTip";
			navigatorMixerPage.UniqueName = "8f2b8ed82e5e492fba41a69c3687c8ed";
			// 
			// mixerPageControl
			// 
			mixerPageControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			mixerPageControl.BackColor = System.Drawing.Color.Transparent;
			mixerPageControl.Location = new System.Drawing.Point(0, 0);
			mixerPageControl.Name = "mixerPageControl";
			controlResource.SetResourceKey(mixerPageControl, null);
			mixerPageControl.Size = new System.Drawing.Size(608, 348);
			mixerPageControl.TabIndex = 0;
			// 
			// navigatorAgentsPage
			// 
			navigatorAgentsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorAgentsPage.Controls.Add(agentsPageControl);
			navigatorAgentsPage.Flags = 65534;
			navigatorAgentsPage.LastVisibleSet = true;
			navigatorAgentsPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorAgentsPage.Name = "navigatorAgentsPage";
			controlResource.SetResourceKey(navigatorAgentsPage, null);
			navigatorAgentsPage.Size = new System.Drawing.Size(608, 357);
			navigatorAgentsPage.Text = "";
			navigatorAgentsPage.ToolTipTitle = "Page ToolTip";
			navigatorAgentsPage.UniqueName = "baf18fe9861c467298b46645633c9221";
			// 
			// agentsPageControl
			// 
			agentsPageControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			agentsPageControl.BackColor = System.Drawing.Color.Transparent;
			agentsPageControl.Location = new System.Drawing.Point(0, 0);
			agentsPageControl.Name = "agentsPageControl";
			controlResource.SetResourceKey(agentsPageControl, null);
			agentsPageControl.Size = new System.Drawing.Size(610, 357);
			agentsPageControl.TabIndex = 0;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// applyButton
			// 
			applyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			applyButton.Location = new System.Drawing.Point(528, 399);
			applyButton.Name = "applyButton";
			applyButton.LocalCustomPalette = bigFontPalette;
			applyButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(applyButton, "IDS_SETTINGS_BUTTON_APPLY");
			applyButton.Size = new System.Drawing.Size(90, 25);
			applyButton.TabIndex = 3;
			applyButton.Values.Text = "Apply";
			applyButton.Click += ApplyButton_Click;
			// 
			// bigFontPalette
			// 
			bigFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			bigFontPalette.BaseFontSize = 9F;
			bigFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			bigFontPalette.ThemeName = "";
			bigFontPalette.UseKryptonFileDialogs = true;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			cancelButton.Location = new System.Drawing.Point(430, 399);
			cancelButton.Name = "cancelButton";
			cancelButton.LocalCustomPalette = bigFontPalette;
			cancelButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(cancelButton, "IDS_SETTINGS_BUTTON_CANCEL");
			cancelButton.Size = new System.Drawing.Size(90, 25);
			cancelButton.TabIndex = 2;
			cancelButton.Values.Text = "Cancel";
			cancelButton.Click += CancelButton_Click;
			// 
			// okButton
			// 
			okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			okButton.Location = new System.Drawing.Point(332, 399);
			okButton.Name = "okButton";
			okButton.LocalCustomPalette = bigFontPalette;
			okButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(okButton, "IDS_SETTINGS_BUTTON_OK");
			okButton.Size = new System.Drawing.Size(90, 25);
			okButton.TabIndex = 1;
			okButton.Values.Text = "Ok";
			okButton.Click += OkButton_Click;
			// 
			// SettingsWindowForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(626, 432);
			Controls.Add(okButton);
			Controls.Add(cancelButton);
			Controls.Add(applyButton);
			Controls.Add(navigator);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SettingsWindowForm";
			LocalCustomPalette = fontPalette;
			PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(this, null);
			FormClosed += SettingsWindowForm_FormClosed;
			((System.ComponentModel.ISupportInitialize)navigator).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorOptionsPage).EndInit();
			navigatorOptionsPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)navigatorModulesPage).EndInit();
			navigatorModulesPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)navigatorPathsPage).EndInit();
			navigatorPathsPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)navigatorMixerPage).EndInit();
			navigatorMixerPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)navigatorAgentsPage).EndInit();
			navigatorAgentsPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private Krypton.Navigator.KryptonNavigator navigator;
		private Krypton.Navigator.KryptonPage navigatorPathsPage;
		private Krypton.Navigator.KryptonPage navigatorMixerPage;
		private Krypton.Navigator.KryptonPage navigatorAgentsPage;
		private Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonButton applyButton;
		private Krypton.Toolkit.KryptonButton cancelButton;
		private Krypton.Toolkit.KryptonButton okButton;
		private Pages.PathsPageControl pathsPageControl;
		private Pages.MixerPageControl mixerPageControl;
		private Pages.AgentsPageControl agentsPageControl;
		private Krypton.Navigator.KryptonPage navigatorOptionsPage;
		private Pages.OptionsPageControl optionsPageControl;
		private GuiKit.Components.FontPalette fontPalette;
		private GuiKit.Components.FontPalette bigFontPalette;
		private Krypton.Navigator.KryptonPage navigatorModulesPage;
		private Pages.ModulesPageControl modulesPageControl;
	}
}