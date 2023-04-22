
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	partial class OptionsPageControl
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
			controlResource = new GuiKit.Designer.ControlResource();
			generalGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			fontPalette = new GuiKit.Components.FontPalette(components);
			addJumpCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			addToListCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			rememberListCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			rememberListPanel = new System.Windows.Forms.Panel();
			rememberModulePositionCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			rememberListPositionCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			tooltipsCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			showNameInTitleCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			showListNumberCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			separateWindowsCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			windowPanel = new System.Windows.Forms.Panel();
			showWindowsInTaskBarCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			useDatabaseCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			scanFilesCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			scanFilesPanel = new System.Windows.Forms.Panel();
			extractPlayingTimeCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			removeUnknownCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			loadingGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			doubleBufferingCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			doubleBufferingPanel = new System.Windows.Forms.Panel();
			earlyLoadLabel = new Krypton.Toolkit.KryptonLabel();
			doubleBufferingTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			moduleErrorLabel = new Krypton.Toolkit.KryptonLabel();
			moduleErrorComboBox = new Krypton.Toolkit.KryptonComboBox();
			playingGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			neverEndingCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			neverEndingNumberTextBox = new Controls.NumberTextBox();
			neverEndingLabel = new Krypton.Toolkit.KryptonLabel();
			moduleListEndComboBox = new Krypton.Toolkit.KryptonComboBox();
			moduleListEndLabel = new Krypton.Toolkit.KryptonLabel();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)generalGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)generalGroupBox.Panel).BeginInit();
			generalGroupBox.Panel.SuspendLayout();
			rememberListPanel.SuspendLayout();
			windowPanel.SuspendLayout();
			scanFilesPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)loadingGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)loadingGroupBox.Panel).BeginInit();
			loadingGroupBox.Panel.SuspendLayout();
			doubleBufferingPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)moduleErrorComboBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)playingGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)playingGroupBox.Panel).BeginInit();
			playingGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)moduleListEndComboBox).BeginInit();
			SuspendLayout();
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// generalGroupBox
			// 
			generalGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			generalGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			generalGroupBox.Location = new System.Drawing.Point(8, 4);
			generalGroupBox.Name = "generalGroupBox";
			generalGroupBox.Palette = fontPalette;
			generalGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			generalGroupBox.Panel.Controls.Add(addJumpCheckBox);
			generalGroupBox.Panel.Controls.Add(addToListCheckBox);
			generalGroupBox.Panel.Controls.Add(rememberListCheckBox);
			generalGroupBox.Panel.Controls.Add(rememberListPanel);
			generalGroupBox.Panel.Controls.Add(tooltipsCheckBox);
			generalGroupBox.Panel.Controls.Add(showNameInTitleCheckBox);
			generalGroupBox.Panel.Controls.Add(showListNumberCheckBox);
			generalGroupBox.Panel.Controls.Add(separateWindowsCheckBox);
			generalGroupBox.Panel.Controls.Add(windowPanel);
			generalGroupBox.Panel.Controls.Add(useDatabaseCheckBox);
			generalGroupBox.Panel.Controls.Add(scanFilesCheckBox);
			generalGroupBox.Panel.Controls.Add(scanFilesPanel);
			controlResource.SetResourceKey(generalGroupBox, "IDS_SETTINGS_OPTIONS_GENERAL");
			generalGroupBox.Size = new System.Drawing.Size(592, 154);
			generalGroupBox.TabIndex = 0;
			generalGroupBox.Values.Heading = "General";
			// 
			// addJumpCheckBox
			// 
			addJumpCheckBox.Location = new System.Drawing.Point(4, 5);
			addJumpCheckBox.Name = "addJumpCheckBox";
			addJumpCheckBox.Palette = fontPalette;
			addJumpCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(addJumpCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_ADDJUMP");
			addJumpCheckBox.Size = new System.Drawing.Size(134, 16);
			addJumpCheckBox.TabIndex = 0;
			addJumpCheckBox.Values.Text = "Jump to added module";
			// 
			// addToListCheckBox
			// 
			addToListCheckBox.Location = new System.Drawing.Point(4, 26);
			addToListCheckBox.Name = "addToListCheckBox";
			addToListCheckBox.Palette = fontPalette;
			addToListCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(addToListCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_ADDTOLIST");
			addToListCheckBox.Size = new System.Drawing.Size(122, 16);
			addToListCheckBox.TabIndex = 1;
			addToListCheckBox.Values.Text = "Add to list as default";
			// 
			// rememberListCheckBox
			// 
			rememberListCheckBox.Location = new System.Drawing.Point(4, 47);
			rememberListCheckBox.Name = "rememberListCheckBox";
			rememberListCheckBox.Palette = fontPalette;
			rememberListCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(rememberListCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERLIST");
			rememberListCheckBox.Size = new System.Drawing.Size(129, 16);
			rememberListCheckBox.TabIndex = 2;
			rememberListCheckBox.Values.Text = "Remember list on exit";
			rememberListCheckBox.CheckedChanged += RememberListCheckBox_CheckedChanged;
			// 
			// rememberListPanel
			// 
			rememberListPanel.BackColor = System.Drawing.Color.Transparent;
			rememberListPanel.Controls.Add(rememberModulePositionCheckBox);
			rememberListPanel.Controls.Add(rememberListPositionCheckBox);
			rememberListPanel.Enabled = false;
			rememberListPanel.Location = new System.Drawing.Point(12, 68);
			rememberListPanel.Name = "rememberListPanel";
			controlResource.SetResourceKey(rememberListPanel, null);
			rememberListPanel.Size = new System.Drawing.Size(170, 38);
			rememberListPanel.TabIndex = 3;
			// 
			// rememberModulePositionCheckBox
			// 
			rememberModulePositionCheckBox.Location = new System.Drawing.Point(0, 21);
			rememberModulePositionCheckBox.Name = "rememberModulePositionCheckBox";
			rememberModulePositionCheckBox.Palette = fontPalette;
			rememberModulePositionCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(rememberModulePositionCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERMODULEPOSITION");
			rememberModulePositionCheckBox.Size = new System.Drawing.Size(157, 16);
			rememberModulePositionCheckBox.TabIndex = 1;
			rememberModulePositionCheckBox.Values.Text = "Remember module position";
			// 
			// rememberListPositionCheckBox
			// 
			rememberListPositionCheckBox.Location = new System.Drawing.Point(0, 0);
			rememberListPositionCheckBox.Name = "rememberListPositionCheckBox";
			rememberListPositionCheckBox.Palette = fontPalette;
			rememberListPositionCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(rememberListPositionCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERLISTPOSITION");
			rememberListPositionCheckBox.Size = new System.Drawing.Size(154, 16);
			rememberListPositionCheckBox.TabIndex = 0;
			rememberListPositionCheckBox.Values.Text = "Remember playing module";
			rememberListPositionCheckBox.CheckedChanged += RememberListPositionCheckBox_CheckedChanged;
			// 
			// tooltipsCheckBox
			// 
			tooltipsCheckBox.Location = new System.Drawing.Point(200, 5);
			tooltipsCheckBox.Name = "tooltipsCheckBox";
			tooltipsCheckBox.Palette = fontPalette;
			tooltipsCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(tooltipsCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_TOOLTIPS");
			tooltipsCheckBox.Size = new System.Drawing.Size(96, 16);
			tooltipsCheckBox.TabIndex = 5;
			tooltipsCheckBox.Values.Text = "Button tool tips";
			// 
			// showNameInTitleCheckBox
			// 
			showNameInTitleCheckBox.Location = new System.Drawing.Point(200, 26);
			showNameInTitleCheckBox.Name = "showNameInTitleCheckBox";
			showNameInTitleCheckBox.Palette = fontPalette;
			showNameInTitleCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(showNameInTitleCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWNAME");
			showNameInTitleCheckBox.Size = new System.Drawing.Size(167, 16);
			showNameInTitleCheckBox.TabIndex = 6;
			showNameInTitleCheckBox.Values.Text = "Show module name in titlebar";
			// 
			// showListNumberCheckBox
			// 
			showListNumberCheckBox.Location = new System.Drawing.Point(4, 110);
			showListNumberCheckBox.Name = "showListNumberCheckBox";
			showListNumberCheckBox.Palette = fontPalette;
			showListNumberCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(showListNumberCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWLISTNUMBER");
			showListNumberCheckBox.Size = new System.Drawing.Size(142, 16);
			showListNumberCheckBox.TabIndex = 4;
			showListNumberCheckBox.Values.Text = "Show item number in list";
			// 
			// separateWindowsCheckBox
			// 
			separateWindowsCheckBox.Location = new System.Drawing.Point(200, 47);
			separateWindowsCheckBox.Name = "separateWindowsCheckBox";
			separateWindowsCheckBox.Palette = fontPalette;
			separateWindowsCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(separateWindowsCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SEPARATEWINDOWS");
			separateWindowsCheckBox.Size = new System.Drawing.Size(169, 28);
			separateWindowsCheckBox.TabIndex = 7;
			separateWindowsCheckBox.Values.Text = "Separate each window in task\r\nswitcher";
			separateWindowsCheckBox.CheckedChanged += SeparateWindows_CheckedChanged;
			// 
			// windowPanel
			// 
			windowPanel.BackColor = System.Drawing.Color.Transparent;
			windowPanel.Controls.Add(showWindowsInTaskBarCheckBox);
			windowPanel.Enabled = false;
			windowPanel.Location = new System.Drawing.Point(208, 81);
			windowPanel.Name = "windowPanel";
			controlResource.SetResourceKey(windowPanel, null);
			windowPanel.Size = new System.Drawing.Size(186, 17);
			windowPanel.TabIndex = 8;
			// 
			// showWindowsInTaskBarCheckBox
			// 
			showWindowsInTaskBarCheckBox.Location = new System.Drawing.Point(0, 0);
			showWindowsInTaskBarCheckBox.Name = "showWindowsInTaskBarCheckBox";
			showWindowsInTaskBarCheckBox.Palette = fontPalette;
			showWindowsInTaskBarCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(showWindowsInTaskBarCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWWINDOWSINTASKBAR");
			showWindowsInTaskBarCheckBox.Size = new System.Drawing.Size(163, 16);
			showWindowsInTaskBarCheckBox.TabIndex = 0;
			showWindowsInTaskBarCheckBox.Values.Text = "Show all windows in task bar";
			// 
			// useDatabaseCheckBox
			// 
			useDatabaseCheckBox.Location = new System.Drawing.Point(400, 5);
			useDatabaseCheckBox.Name = "useDatabaseCheckBox";
			useDatabaseCheckBox.Palette = fontPalette;
			useDatabaseCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(useDatabaseCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_USEDATABASE");
			useDatabaseCheckBox.Size = new System.Drawing.Size(169, 28);
			useDatabaseCheckBox.TabIndex = 9;
			useDatabaseCheckBox.Values.Text = "Use database to store module\r\ninformation";
			// 
			// scanFilesCheckBox
			// 
			scanFilesCheckBox.Location = new System.Drawing.Point(400, 38);
			scanFilesCheckBox.Name = "scanFilesCheckBox";
			scanFilesCheckBox.Palette = fontPalette;
			scanFilesCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(scanFilesCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SCANFILES");
			scanFilesCheckBox.Size = new System.Drawing.Size(104, 16);
			scanFilesCheckBox.TabIndex = 10;
			scanFilesCheckBox.Values.Text = "Scan added files";
			scanFilesCheckBox.CheckedChanged += ScanFiles_CheckedChanged;
			// 
			// scanFilesPanel
			// 
			scanFilesPanel.Controls.Add(extractPlayingTimeCheckBox);
			scanFilesPanel.Controls.Add(removeUnknownCheckBox);
			scanFilesPanel.Enabled = false;
			scanFilesPanel.Location = new System.Drawing.Point(408, 59);
			scanFilesPanel.Name = "scanFilesPanel";
			controlResource.SetResourceKey(scanFilesPanel, null);
			scanFilesPanel.Size = new System.Drawing.Size(177, 38);
			scanFilesPanel.TabIndex = 11;
			// 
			// extractPlayingTimeCheckBox
			// 
			extractPlayingTimeCheckBox.Location = new System.Drawing.Point(0, 21);
			extractPlayingTimeCheckBox.Name = "extractPlayingTimeCheckBox";
			controlResource.SetResourceKey(extractPlayingTimeCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_EXTRACTPLAYINGTIME");
			extractPlayingTimeCheckBox.Size = new System.Drawing.Size(131, 20);
			extractPlayingTimeCheckBox.TabIndex = 1;
			extractPlayingTimeCheckBox.Values.Text = "Extract playing time";
			// 
			// removeUnknownCheckBox
			// 
			removeUnknownCheckBox.Location = new System.Drawing.Point(0, 0);
			removeUnknownCheckBox.Name = "removeUnknownCheckBox";
			controlResource.SetResourceKey(removeUnknownCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMOVEUNKNOWN");
			removeUnknownCheckBox.Size = new System.Drawing.Size(172, 20);
			removeUnknownCheckBox.TabIndex = 0;
			removeUnknownCheckBox.Values.Text = "Remove unknown modules";
			// 
			// loadingGroupBox
			// 
			loadingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			loadingGroupBox.Location = new System.Drawing.Point(8, 162);
			loadingGroupBox.Name = "loadingGroupBox";
			loadingGroupBox.Palette = fontPalette;
			loadingGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			loadingGroupBox.Panel.Controls.Add(doubleBufferingCheckBox);
			loadingGroupBox.Panel.Controls.Add(doubleBufferingPanel);
			loadingGroupBox.Panel.Controls.Add(moduleErrorLabel);
			loadingGroupBox.Panel.Controls.Add(moduleErrorComboBox);
			controlResource.SetResourceKey(loadingGroupBox, "IDS_SETTINGS_OPTIONS_LOADING");
			loadingGroupBox.Size = new System.Drawing.Size(592, 108);
			loadingGroupBox.TabIndex = 1;
			loadingGroupBox.Values.Heading = "Loading";
			// 
			// doubleBufferingCheckBox
			// 
			doubleBufferingCheckBox.Location = new System.Drawing.Point(4, 5);
			doubleBufferingCheckBox.Name = "doubleBufferingCheckBox";
			doubleBufferingCheckBox.Palette = fontPalette;
			doubleBufferingCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(doubleBufferingCheckBox, "IDS_SETTINGS_OPTIONS_LOADING_DOUBLEBUFFERING");
			doubleBufferingCheckBox.Size = new System.Drawing.Size(104, 16);
			doubleBufferingCheckBox.TabIndex = 0;
			doubleBufferingCheckBox.Values.Text = "Double buffering";
			doubleBufferingCheckBox.CheckedChanged += DoubleBufferingCheckBox_CheckedChanged;
			// 
			// doubleBufferingPanel
			// 
			doubleBufferingPanel.BackColor = System.Drawing.Color.Transparent;
			doubleBufferingPanel.Controls.Add(earlyLoadLabel);
			doubleBufferingPanel.Controls.Add(doubleBufferingTrackBar);
			doubleBufferingPanel.Enabled = false;
			doubleBufferingPanel.Location = new System.Drawing.Point(8, 26);
			doubleBufferingPanel.Name = "doubleBufferingPanel";
			controlResource.SetResourceKey(doubleBufferingPanel, null);
			doubleBufferingPanel.Size = new System.Drawing.Size(574, 30);
			doubleBufferingPanel.TabIndex = 1;
			// 
			// earlyLoadLabel
			// 
			earlyLoadLabel.Location = new System.Drawing.Point(494, 5);
			earlyLoadLabel.Name = "earlyLoadLabel";
			earlyLoadLabel.Palette = fontPalette;
			earlyLoadLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(earlyLoadLabel, "IDS_SETTINGS_OPTIONS_LOADING_EARLYLOAD");
			earlyLoadLabel.Size = new System.Drawing.Size(59, 16);
			earlyLoadLabel.TabIndex = 1;
			earlyLoadLabel.Values.Text = "Early load";
			// 
			// doubleBufferingTrackBar
			// 
			doubleBufferingTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			doubleBufferingTrackBar.Location = new System.Drawing.Point(0, 0);
			doubleBufferingTrackBar.Maximum = 8;
			doubleBufferingTrackBar.Name = "doubleBufferingTrackBar";
			controlResource.SetResourceKey(doubleBufferingTrackBar, null);
			doubleBufferingTrackBar.Size = new System.Drawing.Size(490, 27);
			doubleBufferingTrackBar.TabIndex = 0;
			// 
			// moduleErrorLabel
			// 
			moduleErrorLabel.Location = new System.Drawing.Point(4, 62);
			moduleErrorLabel.Name = "moduleErrorLabel";
			moduleErrorLabel.Palette = fontPalette;
			moduleErrorLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleErrorLabel, "IDS_SETTINGS_OPTIONS_LOADING_MODULEERROR");
			moduleErrorLabel.Size = new System.Drawing.Size(166, 16);
			moduleErrorLabel.TabIndex = 2;
			moduleErrorLabel.Values.Text = "When a module error is reached";
			// 
			// moduleErrorComboBox
			// 
			moduleErrorComboBox.CornerRoundingRadius = -1F;
			moduleErrorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			moduleErrorComboBox.DropDownWidth = 121;
			moduleErrorComboBox.IntegralHeight = false;
			moduleErrorComboBox.Location = new System.Drawing.Point(180, 60);
			moduleErrorComboBox.Name = "moduleErrorComboBox";
			moduleErrorComboBox.Palette = fontPalette;
			moduleErrorComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleErrorComboBox, null);
			moduleErrorComboBox.Size = new System.Drawing.Size(180, 18);
			moduleErrorComboBox.TabIndex = 3;
			// 
			// playingGroupBox
			// 
			playingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			playingGroupBox.Location = new System.Drawing.Point(8, 274);
			playingGroupBox.Name = "playingGroupBox";
			playingGroupBox.Palette = fontPalette;
			playingGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			playingGroupBox.Panel.Controls.Add(neverEndingCheckBox);
			playingGroupBox.Panel.Controls.Add(neverEndingNumberTextBox);
			playingGroupBox.Panel.Controls.Add(neverEndingLabel);
			playingGroupBox.Panel.Controls.Add(moduleListEndComboBox);
			playingGroupBox.Panel.Controls.Add(moduleListEndLabel);
			controlResource.SetResourceKey(playingGroupBox, "IDS_SETTINGS_OPTIONS_PLAYING");
			playingGroupBox.Size = new System.Drawing.Size(592, 76);
			playingGroupBox.TabIndex = 2;
			playingGroupBox.Values.Heading = "Playing";
			// 
			// neverEndingCheckBox
			// 
			neverEndingCheckBox.Location = new System.Drawing.Point(4, 5);
			neverEndingCheckBox.Name = "neverEndingCheckBox";
			neverEndingCheckBox.Palette = fontPalette;
			neverEndingCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(neverEndingCheckBox, "IDS_SETTINGS_OPTIONS_PLAYING_NEVERENDING");
			neverEndingCheckBox.Size = new System.Drawing.Size(166, 16);
			neverEndingCheckBox.TabIndex = 0;
			neverEndingCheckBox.Values.Text = "Never ending module timeout";
			neverEndingCheckBox.CheckedChanged += NeverEnding_CheckedChanged;
			// 
			// neverEndingNumberTextBox
			// 
			neverEndingNumberTextBox.Enabled = false;
			neverEndingNumberTextBox.Location = new System.Drawing.Point(180, 3);
			neverEndingNumberTextBox.MaxLength = 3;
			neverEndingNumberTextBox.Name = "neverEndingNumberTextBox";
			neverEndingNumberTextBox.Palette = fontPalette;
			neverEndingNumberTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(neverEndingNumberTextBox, null);
			neverEndingNumberTextBox.Size = new System.Drawing.Size(32, 20);
			neverEndingNumberTextBox.TabIndex = 1;
			// 
			// neverEndingLabel
			// 
			neverEndingLabel.Location = new System.Drawing.Point(216, 5);
			neverEndingLabel.Name = "neverEndingLabel";
			neverEndingLabel.Palette = fontPalette;
			neverEndingLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(neverEndingLabel, "IDS_SETTINGS_OPTIONS_PLAYING_NEVERENDING_SECONDS");
			neverEndingLabel.Size = new System.Drawing.Size(51, 16);
			neverEndingLabel.TabIndex = 2;
			neverEndingLabel.Values.Text = "seconds";
			// 
			// moduleListEndComboBox
			// 
			moduleListEndComboBox.CornerRoundingRadius = -1F;
			moduleListEndComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			moduleListEndComboBox.DropDownWidth = 121;
			moduleListEndComboBox.IntegralHeight = false;
			moduleListEndComboBox.Location = new System.Drawing.Point(124, 28);
			moduleListEndComboBox.Name = "moduleListEndComboBox";
			moduleListEndComboBox.Palette = fontPalette;
			moduleListEndComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleListEndComboBox, null);
			moduleListEndComboBox.Size = new System.Drawing.Size(100, 18);
			moduleListEndComboBox.TabIndex = 4;
			// 
			// moduleListEndLabel
			// 
			moduleListEndLabel.Location = new System.Drawing.Point(4, 30);
			moduleListEndLabel.Name = "moduleListEndLabel";
			moduleListEndLabel.Palette = fontPalette;
			moduleListEndLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleListEndLabel, "IDS_SETTINGS_OPTIONS_PLAYING_MODULELISTEND");
			moduleListEndLabel.Size = new System.Drawing.Size(109, 16);
			moduleListEndLabel.TabIndex = 3;
			moduleListEndLabel.Values.Text = "At end of module list";
			// 
			// OptionsPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(playingGroupBox);
			Controls.Add(loadingGroupBox);
			Controls.Add(generalGroupBox);
			Name = "OptionsPageControl";
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(608, 356);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			((System.ComponentModel.ISupportInitialize)generalGroupBox.Panel).EndInit();
			generalGroupBox.Panel.ResumeLayout(false);
			generalGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)generalGroupBox).EndInit();
			rememberListPanel.ResumeLayout(false);
			rememberListPanel.PerformLayout();
			windowPanel.ResumeLayout(false);
			windowPanel.PerformLayout();
			scanFilesPanel.ResumeLayout(false);
			scanFilesPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)loadingGroupBox.Panel).EndInit();
			loadingGroupBox.Panel.ResumeLayout(false);
			loadingGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)loadingGroupBox).EndInit();
			doubleBufferingPanel.ResumeLayout(false);
			doubleBufferingPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)moduleErrorComboBox).EndInit();
			((System.ComponentModel.ISupportInitialize)playingGroupBox.Panel).EndInit();
			playingGroupBox.Panel.ResumeLayout(false);
			playingGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)playingGroupBox).EndInit();
			((System.ComponentModel.ISupportInitialize)moduleListEndComboBox).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonGroupBox generalGroupBox;
		private Krypton.Toolkit.KryptonCheckBox addJumpCheckBox;
		private Krypton.Toolkit.KryptonCheckBox addToListCheckBox;
		private Krypton.Toolkit.KryptonCheckBox rememberListCheckBox;
		private System.Windows.Forms.Panel rememberListPanel;
		private Krypton.Toolkit.KryptonCheckBox rememberModulePositionCheckBox;
		private Krypton.Toolkit.KryptonCheckBox rememberListPositionCheckBox;
		private Krypton.Toolkit.KryptonCheckBox tooltipsCheckBox;
		private Krypton.Toolkit.KryptonCheckBox showNameInTitleCheckBox;
		private Krypton.Toolkit.KryptonCheckBox showListNumberCheckBox;
		private Krypton.Toolkit.KryptonCheckBox scanFilesCheckBox;
		private Krypton.Toolkit.KryptonCheckBox useDatabaseCheckBox;
		private Krypton.Toolkit.KryptonGroupBox loadingGroupBox;
		private Krypton.Toolkit.KryptonCheckBox doubleBufferingCheckBox;
		private System.Windows.Forms.Panel doubleBufferingPanel;
		private Krypton.Toolkit.KryptonLabel earlyLoadLabel;
		private Krypton.Toolkit.KryptonTrackBar doubleBufferingTrackBar;
		private Krypton.Toolkit.KryptonComboBox moduleErrorComboBox;
		private Krypton.Toolkit.KryptonLabel moduleErrorLabel;
		private Krypton.Toolkit.KryptonGroupBox playingGroupBox;
		private Krypton.Toolkit.KryptonLabel moduleListEndLabel;
		private Krypton.Toolkit.KryptonComboBox moduleListEndComboBox;
		private Krypton.Toolkit.KryptonCheckBox separateWindowsCheckBox;
		private Krypton.Toolkit.KryptonCheckBox showWindowsInTaskBarCheckBox;
		private System.Windows.Forms.Panel windowPanel;
		private Krypton.Toolkit.KryptonCheckBox neverEndingCheckBox;
		private Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.NumberTextBox neverEndingNumberTextBox;
		private Krypton.Toolkit.KryptonLabel neverEndingLabel;
		private GuiKit.Components.FontPalette fontPalette;
		private System.Windows.Forms.Panel scanFilesPanel;
		private Krypton.Toolkit.KryptonCheckBox extractPlayingTimeCheckBox;
		private Krypton.Toolkit.KryptonCheckBox removeUnknownCheckBox;
	}
}
