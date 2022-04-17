﻿
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
			this.components = new System.ComponentModel.Container();
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.generalGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(this.components);
			this.addJumpCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.addToListCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.rememberListCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.rememberListPanel = new System.Windows.Forms.Panel();
			this.rememberModulePositionCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.rememberListPositionCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.tooltipsCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.showNameInTitleCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.showListNumberCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.separateWindowsCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.windowPanel = new System.Windows.Forms.Panel();
			this.showWindowsInTaskBarCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.scanFilesCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.useDatabaseCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.loadingGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.doubleBufferingCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.doubleBufferingPanel = new System.Windows.Forms.Panel();
			this.earlyLoadLabel = new Krypton.Toolkit.KryptonLabel();
			this.doubleBufferingTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.moduleErrorLabel = new Krypton.Toolkit.KryptonLabel();
			this.moduleErrorComboBox = new Krypton.Toolkit.KryptonComboBox();
			this.playingGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.neverEndingCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.neverEndingNumberTextBox = new Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.NumberTextBox();
			this.neverEndingLabel = new Krypton.Toolkit.KryptonLabel();
			this.moduleListEndComboBox = new Krypton.Toolkit.KryptonComboBox();
			this.moduleListEndLabel = new Krypton.Toolkit.KryptonLabel();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox.Panel)).BeginInit();
			this.generalGroupBox.Panel.SuspendLayout();
			this.rememberListPanel.SuspendLayout();
			this.windowPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.loadingGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.loadingGroupBox.Panel)).BeginInit();
			this.loadingGroupBox.Panel.SuspendLayout();
			this.doubleBufferingPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.moduleErrorComboBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.playingGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.playingGroupBox.Panel)).BeginInit();
			this.playingGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.moduleListEndComboBox)).BeginInit();
			this.SuspendLayout();
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// generalGroupBox
			// 
			this.generalGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.generalGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.generalGroupBox.Location = new System.Drawing.Point(8, 4);
			this.generalGroupBox.Name = "generalGroupBox";
			this.generalGroupBox.Palette = this.fontPalette;
			this.generalGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			this.generalGroupBox.Panel.Controls.Add(this.addJumpCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.addToListCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.rememberListCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.rememberListPanel);
			this.generalGroupBox.Panel.Controls.Add(this.tooltipsCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.showNameInTitleCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.showListNumberCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.separateWindowsCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.windowPanel);
			this.generalGroupBox.Panel.Controls.Add(this.scanFilesCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.useDatabaseCheckBox);
			this.controlResource.SetResourceKey(this.generalGroupBox, "IDS_SETTINGS_OPTIONS_GENERAL");
			this.generalGroupBox.Size = new System.Drawing.Size(592, 154);
			this.generalGroupBox.TabIndex = 0;
			this.generalGroupBox.Values.Heading = "General";
			// 
			// addJumpCheckBox
			// 
			this.addJumpCheckBox.Location = new System.Drawing.Point(4, 5);
			this.addJumpCheckBox.Name = "addJumpCheckBox";
			this.addJumpCheckBox.Palette = this.fontPalette;
			this.addJumpCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.addJumpCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_ADDJUMP");
			this.addJumpCheckBox.Size = new System.Drawing.Size(134, 16);
			this.addJumpCheckBox.TabIndex = 0;
			this.addJumpCheckBox.Values.Text = "Jump to added module";
			// 
			// addToListCheckBox
			// 
			this.addToListCheckBox.Location = new System.Drawing.Point(4, 26);
			this.addToListCheckBox.Name = "addToListCheckBox";
			this.addToListCheckBox.Palette = this.fontPalette;
			this.addToListCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.addToListCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_ADDTOLIST");
			this.addToListCheckBox.Size = new System.Drawing.Size(122, 16);
			this.addToListCheckBox.TabIndex = 1;
			this.addToListCheckBox.Values.Text = "Add to list as default";
			// 
			// rememberListCheckBox
			// 
			this.rememberListCheckBox.Location = new System.Drawing.Point(4, 47);
			this.rememberListCheckBox.Name = "rememberListCheckBox";
			this.rememberListCheckBox.Palette = this.fontPalette;
			this.rememberListCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.rememberListCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERLIST");
			this.rememberListCheckBox.Size = new System.Drawing.Size(129, 16);
			this.rememberListCheckBox.TabIndex = 2;
			this.rememberListCheckBox.Values.Text = "Remember list on exit";
			this.rememberListCheckBox.CheckedChanged += new System.EventHandler(this.RememberListCheckBox_CheckedChanged);
			// 
			// rememberListPanel
			// 
			this.rememberListPanel.BackColor = System.Drawing.Color.Transparent;
			this.rememberListPanel.Controls.Add(this.rememberModulePositionCheckBox);
			this.rememberListPanel.Controls.Add(this.rememberListPositionCheckBox);
			this.rememberListPanel.Enabled = false;
			this.rememberListPanel.Location = new System.Drawing.Point(12, 68);
			this.rememberListPanel.Name = "rememberListPanel";
			this.controlResource.SetResourceKey(this.rememberListPanel, null);
			this.rememberListPanel.Size = new System.Drawing.Size(170, 38);
			this.rememberListPanel.TabIndex = 3;
			// 
			// rememberModulePositionCheckBox
			// 
			this.rememberModulePositionCheckBox.Location = new System.Drawing.Point(0, 21);
			this.rememberModulePositionCheckBox.Name = "rememberModulePositionCheckBox";
			this.rememberModulePositionCheckBox.Palette = this.fontPalette;
			this.rememberModulePositionCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.rememberModulePositionCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERMODULEPOSITION");
			this.rememberModulePositionCheckBox.Size = new System.Drawing.Size(157, 16);
			this.rememberModulePositionCheckBox.TabIndex = 1;
			this.rememberModulePositionCheckBox.Values.Text = "Remember module position";
			// 
			// rememberListPositionCheckBox
			// 
			this.rememberListPositionCheckBox.Location = new System.Drawing.Point(0, 0);
			this.rememberListPositionCheckBox.Name = "rememberListPositionCheckBox";
			this.rememberListPositionCheckBox.Palette = this.fontPalette;
			this.rememberListPositionCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.rememberListPositionCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERLISTPOSITION");
			this.rememberListPositionCheckBox.Size = new System.Drawing.Size(154, 16);
			this.rememberListPositionCheckBox.TabIndex = 0;
			this.rememberListPositionCheckBox.Values.Text = "Remember playing module";
			this.rememberListPositionCheckBox.CheckedChanged += new System.EventHandler(this.RememberListPositionCheckBox_CheckedChanged);
			// 
			// tooltipsCheckBox
			// 
			this.tooltipsCheckBox.Location = new System.Drawing.Point(200, 5);
			this.tooltipsCheckBox.Name = "tooltipsCheckBox";
			this.tooltipsCheckBox.Palette = this.fontPalette;
			this.tooltipsCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.tooltipsCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_TOOLTIPS");
			this.tooltipsCheckBox.Size = new System.Drawing.Size(96, 16);
			this.tooltipsCheckBox.TabIndex = 5;
			this.tooltipsCheckBox.Values.Text = "Button tool tips";
			// 
			// showNameInTitleCheckBox
			// 
			this.showNameInTitleCheckBox.Location = new System.Drawing.Point(200, 26);
			this.showNameInTitleCheckBox.Name = "showNameInTitleCheckBox";
			this.showNameInTitleCheckBox.Palette = this.fontPalette;
			this.showNameInTitleCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.showNameInTitleCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWNAME");
			this.showNameInTitleCheckBox.Size = new System.Drawing.Size(167, 16);
			this.showNameInTitleCheckBox.TabIndex = 6;
			this.showNameInTitleCheckBox.Values.Text = "Show module name in titlebar";
			// 
			// showListNumberCheckBox
			// 
			this.showListNumberCheckBox.Location = new System.Drawing.Point(4, 110);
			this.showListNumberCheckBox.Name = "showListNumberCheckBox";
			this.showListNumberCheckBox.Palette = this.fontPalette;
			this.showListNumberCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.showListNumberCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWLISTNUMBER");
			this.showListNumberCheckBox.Size = new System.Drawing.Size(142, 16);
			this.showListNumberCheckBox.TabIndex = 4;
			this.showListNumberCheckBox.Values.Text = "Show item number in list";
			// 
			// separateWindowsCheckBox
			// 
			this.separateWindowsCheckBox.Location = new System.Drawing.Point(200, 47);
			this.separateWindowsCheckBox.Name = "separateWindowsCheckBox";
			this.separateWindowsCheckBox.Palette = this.fontPalette;
			this.separateWindowsCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.separateWindowsCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SEPARATEWINDOWS");
			this.separateWindowsCheckBox.Size = new System.Drawing.Size(169, 28);
			this.separateWindowsCheckBox.TabIndex = 7;
			this.separateWindowsCheckBox.Values.Text = "Separate each window in task\r\nswitcher";
			this.separateWindowsCheckBox.CheckedChanged += new System.EventHandler(this.SeparateWindows_CheckedChanged);
			// 
			// windowPanel
			// 
			this.windowPanel.BackColor = System.Drawing.Color.Transparent;
			this.windowPanel.Controls.Add(this.showWindowsInTaskBarCheckBox);
			this.windowPanel.Enabled = false;
			this.windowPanel.Location = new System.Drawing.Point(208, 81);
			this.windowPanel.Name = "windowPanel";
			this.controlResource.SetResourceKey(this.windowPanel, null);
			this.windowPanel.Size = new System.Drawing.Size(186, 17);
			this.windowPanel.TabIndex = 8;
			// 
			// showWindowsInTaskBarCheckBox
			// 
			this.showWindowsInTaskBarCheckBox.Location = new System.Drawing.Point(0, 0);
			this.showWindowsInTaskBarCheckBox.Name = "showWindowsInTaskBarCheckBox";
			this.showWindowsInTaskBarCheckBox.Palette = this.fontPalette;
			this.showWindowsInTaskBarCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.showWindowsInTaskBarCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWWINDOWSINTASKBAR");
			this.showWindowsInTaskBarCheckBox.Size = new System.Drawing.Size(163, 16);
			this.showWindowsInTaskBarCheckBox.TabIndex = 0;
			this.showWindowsInTaskBarCheckBox.Values.Text = "Show all windows in task bar";
			// 
			// scanFilesCheckBox
			// 
			this.scanFilesCheckBox.Location = new System.Drawing.Point(400, 5);
			this.scanFilesCheckBox.Name = "scanFilesCheckBox";
			this.scanFilesCheckBox.Palette = this.fontPalette;
			this.scanFilesCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.scanFilesCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SCANFILES");
			this.scanFilesCheckBox.Size = new System.Drawing.Size(104, 16);
			this.scanFilesCheckBox.TabIndex = 9;
			this.scanFilesCheckBox.Values.Text = "Scan added files";
			// 
			// useDatabaseCheckBox
			// 
			this.useDatabaseCheckBox.Location = new System.Drawing.Point(400, 26);
			this.useDatabaseCheckBox.Name = "useDatabaseCheckBox";
			this.useDatabaseCheckBox.Palette = this.fontPalette;
			this.useDatabaseCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.useDatabaseCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_USEDATABASE");
			this.useDatabaseCheckBox.Size = new System.Drawing.Size(169, 28);
			this.useDatabaseCheckBox.TabIndex = 10;
			this.useDatabaseCheckBox.Values.Text = "Use database to store module\r\ninformation";
			// 
			// loadingGroupBox
			// 
			this.loadingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.loadingGroupBox.Location = new System.Drawing.Point(8, 162);
			this.loadingGroupBox.Name = "loadingGroupBox";
			this.loadingGroupBox.Palette = this.fontPalette;
			this.loadingGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			this.loadingGroupBox.Panel.Controls.Add(this.doubleBufferingCheckBox);
			this.loadingGroupBox.Panel.Controls.Add(this.doubleBufferingPanel);
			this.loadingGroupBox.Panel.Controls.Add(this.moduleErrorLabel);
			this.loadingGroupBox.Panel.Controls.Add(this.moduleErrorComboBox);
			this.controlResource.SetResourceKey(this.loadingGroupBox, "IDS_SETTINGS_OPTIONS_LOADING");
			this.loadingGroupBox.Size = new System.Drawing.Size(592, 108);
			this.loadingGroupBox.TabIndex = 1;
			this.loadingGroupBox.Values.Heading = "Loading";
			// 
			// doubleBufferingCheckBox
			// 
			this.doubleBufferingCheckBox.Location = new System.Drawing.Point(4, 5);
			this.doubleBufferingCheckBox.Name = "doubleBufferingCheckBox";
			this.doubleBufferingCheckBox.Palette = this.fontPalette;
			this.doubleBufferingCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.doubleBufferingCheckBox, "IDS_SETTINGS_OPTIONS_LOADING_DOUBLEBUFFERING");
			this.doubleBufferingCheckBox.Size = new System.Drawing.Size(104, 16);
			this.doubleBufferingCheckBox.TabIndex = 0;
			this.doubleBufferingCheckBox.Values.Text = "Double buffering";
			this.doubleBufferingCheckBox.CheckedChanged += new System.EventHandler(this.DoubleBufferingCheckBox_CheckedChanged);
			// 
			// doubleBufferingPanel
			// 
			this.doubleBufferingPanel.BackColor = System.Drawing.Color.Transparent;
			this.doubleBufferingPanel.Controls.Add(this.earlyLoadLabel);
			this.doubleBufferingPanel.Controls.Add(this.doubleBufferingTrackBar);
			this.doubleBufferingPanel.Enabled = false;
			this.doubleBufferingPanel.Location = new System.Drawing.Point(8, 26);
			this.doubleBufferingPanel.Name = "doubleBufferingPanel";
			this.controlResource.SetResourceKey(this.doubleBufferingPanel, null);
			this.doubleBufferingPanel.Size = new System.Drawing.Size(574, 30);
			this.doubleBufferingPanel.TabIndex = 1;
			// 
			// earlyLoadLabel
			// 
			this.earlyLoadLabel.Location = new System.Drawing.Point(494, 5);
			this.earlyLoadLabel.Name = "earlyLoadLabel";
			this.earlyLoadLabel.Palette = this.fontPalette;
			this.earlyLoadLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.earlyLoadLabel, "IDS_SETTINGS_OPTIONS_LOADING_EARLYLOAD");
			this.earlyLoadLabel.Size = new System.Drawing.Size(59, 16);
			this.earlyLoadLabel.TabIndex = 1;
			this.earlyLoadLabel.Values.Text = "Early load";
			// 
			// doubleBufferingTrackBar
			// 
			this.doubleBufferingTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			this.doubleBufferingTrackBar.Location = new System.Drawing.Point(0, 0);
			this.doubleBufferingTrackBar.Maximum = 8;
			this.doubleBufferingTrackBar.Name = "doubleBufferingTrackBar";
			this.controlResource.SetResourceKey(this.doubleBufferingTrackBar, null);
			this.doubleBufferingTrackBar.Size = new System.Drawing.Size(490, 27);
			this.doubleBufferingTrackBar.TabIndex = 0;
			// 
			// moduleErrorLabel
			// 
			this.moduleErrorLabel.Location = new System.Drawing.Point(4, 62);
			this.moduleErrorLabel.Name = "moduleErrorLabel";
			this.moduleErrorLabel.Palette = this.fontPalette;
			this.moduleErrorLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.moduleErrorLabel, "IDS_SETTINGS_OPTIONS_LOADING_MODULEERROR");
			this.moduleErrorLabel.Size = new System.Drawing.Size(166, 16);
			this.moduleErrorLabel.TabIndex = 2;
			this.moduleErrorLabel.Values.Text = "When a module error is reached";
			// 
			// moduleErrorComboBox
			// 
			this.moduleErrorComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.moduleErrorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.moduleErrorComboBox.DropDownWidth = 121;
			this.moduleErrorComboBox.IntegralHeight = false;
			this.moduleErrorComboBox.Location = new System.Drawing.Point(180, 60);
			this.moduleErrorComboBox.Name = "moduleErrorComboBox";
			this.moduleErrorComboBox.Palette = this.fontPalette;
			this.moduleErrorComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.moduleErrorComboBox, null);
			this.moduleErrorComboBox.Size = new System.Drawing.Size(180, 18);
			this.moduleErrorComboBox.TabIndex = 3;
			// 
			// playingGroupBox
			// 
			this.playingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.playingGroupBox.Location = new System.Drawing.Point(8, 274);
			this.playingGroupBox.Name = "playingGroupBox";
			this.playingGroupBox.Palette = this.fontPalette;
			this.playingGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			this.playingGroupBox.Panel.Controls.Add(this.neverEndingCheckBox);
			this.playingGroupBox.Panel.Controls.Add(this.neverEndingNumberTextBox);
			this.playingGroupBox.Panel.Controls.Add(this.neverEndingLabel);
			this.playingGroupBox.Panel.Controls.Add(this.moduleListEndComboBox);
			this.playingGroupBox.Panel.Controls.Add(this.moduleListEndLabel);
			this.controlResource.SetResourceKey(this.playingGroupBox, "IDS_SETTINGS_OPTIONS_PLAYING");
			this.playingGroupBox.Size = new System.Drawing.Size(592, 76);
			this.playingGroupBox.TabIndex = 2;
			this.playingGroupBox.Values.Heading = "Playing";
			// 
			// neverEndingCheckBox
			// 
			this.neverEndingCheckBox.Location = new System.Drawing.Point(4, 5);
			this.neverEndingCheckBox.Name = "neverEndingCheckBox";
			this.neverEndingCheckBox.Palette = this.fontPalette;
			this.neverEndingCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.neverEndingCheckBox, "IDS_SETTINGS_OPTIONS_PLAYING_NEVERENDING");
			this.neverEndingCheckBox.Size = new System.Drawing.Size(166, 16);
			this.neverEndingCheckBox.TabIndex = 0;
			this.neverEndingCheckBox.Values.Text = "Never ending module timeout";
			this.neverEndingCheckBox.CheckedChanged += new System.EventHandler(this.NeverEnding_CheckedChanged);
			// 
			// neverEndingNumberTextBox
			// 
			this.neverEndingNumberTextBox.Enabled = false;
			this.neverEndingNumberTextBox.Location = new System.Drawing.Point(180, 3);
			this.neverEndingNumberTextBox.MaxLength = 3;
			this.neverEndingNumberTextBox.Name = "neverEndingNumberTextBox";
			this.neverEndingNumberTextBox.Palette = this.fontPalette;
			this.neverEndingNumberTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.neverEndingNumberTextBox, null);
			this.neverEndingNumberTextBox.Size = new System.Drawing.Size(32, 20);
			this.neverEndingNumberTextBox.TabIndex = 1;
			// 
			// neverEndingLabel
			// 
			this.neverEndingLabel.Location = new System.Drawing.Point(216, 5);
			this.neverEndingLabel.Name = "neverEndingLabel";
			this.neverEndingLabel.Palette = this.fontPalette;
			this.neverEndingLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.neverEndingLabel, "IDS_SETTINGS_OPTIONS_PLAYING_NEVERENDING_SECONDS");
			this.neverEndingLabel.Size = new System.Drawing.Size(51, 16);
			this.neverEndingLabel.TabIndex = 2;
			this.neverEndingLabel.Values.Text = "seconds";
			// 
			// moduleListEndComboBox
			// 
			this.moduleListEndComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.moduleListEndComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.moduleListEndComboBox.DropDownWidth = 121;
			this.moduleListEndComboBox.IntegralHeight = false;
			this.moduleListEndComboBox.Location = new System.Drawing.Point(124, 28);
			this.moduleListEndComboBox.Name = "moduleListEndComboBox";
			this.moduleListEndComboBox.Palette = this.fontPalette;
			this.moduleListEndComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.moduleListEndComboBox, null);
			this.moduleListEndComboBox.Size = new System.Drawing.Size(100, 18);
			this.moduleListEndComboBox.TabIndex = 4;
			// 
			// moduleListEndLabel
			// 
			this.moduleListEndLabel.Location = new System.Drawing.Point(4, 30);
			this.moduleListEndLabel.Name = "moduleListEndLabel";
			this.moduleListEndLabel.Palette = this.fontPalette;
			this.moduleListEndLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.moduleListEndLabel, "IDS_SETTINGS_OPTIONS_PLAYING_MODULELISTEND");
			this.moduleListEndLabel.Size = new System.Drawing.Size(109, 16);
			this.moduleListEndLabel.TabIndex = 3;
			this.moduleListEndLabel.Values.Text = "At end of module list";
			// 
			// OptionsPageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.playingGroupBox);
			this.Controls.Add(this.loadingGroupBox);
			this.Controls.Add(this.generalGroupBox);
			this.Name = "OptionsPageControl";
			this.controlResource.SetResourceKey(this, null);
			this.Size = new System.Drawing.Size(608, 356);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox.Panel)).EndInit();
			this.generalGroupBox.Panel.ResumeLayout(false);
			this.generalGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox)).EndInit();
			this.rememberListPanel.ResumeLayout(false);
			this.rememberListPanel.PerformLayout();
			this.windowPanel.ResumeLayout(false);
			this.windowPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.loadingGroupBox.Panel)).EndInit();
			this.loadingGroupBox.Panel.ResumeLayout(false);
			this.loadingGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.loadingGroupBox)).EndInit();
			this.doubleBufferingPanel.ResumeLayout(false);
			this.doubleBufferingPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.moduleErrorComboBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.playingGroupBox.Panel)).EndInit();
			this.playingGroupBox.Panel.ResumeLayout(false);
			this.playingGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.playingGroupBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.moduleListEndComboBox)).EndInit();
			this.ResumeLayout(false);

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
	}
}
