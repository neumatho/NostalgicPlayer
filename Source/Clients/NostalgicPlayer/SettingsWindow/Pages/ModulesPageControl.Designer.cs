namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	partial class ModulesPageControl
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
			fontPalette = new GuiKit.Components.FontPalette(components);
			controlResource = new GuiKit.Designer.ControlResource();
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
			moduleEndComboBox = new Krypton.Toolkit.KryptonComboBox();
			moduleEndLabel = new Krypton.Toolkit.KryptonLabel();
			moduleListEndComboBox = new Krypton.Toolkit.KryptonComboBox();
			moduleListEndLabel = new Krypton.Toolkit.KryptonLabel();
			showingGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			moduleInfoOrderDownButton = new Krypton.Toolkit.KryptonButton();
			moduleInfoOrderUpButton = new Krypton.Toolkit.KryptonButton();
			moduleInfoOrderLabel = new Krypton.Toolkit.KryptonLabel();
			moduleInfoOrderListBox = new Krypton.Toolkit.KryptonListBox();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)loadingGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)loadingGroupBox.Panel).BeginInit();
			loadingGroupBox.Panel.SuspendLayout();
			doubleBufferingPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)moduleErrorComboBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)playingGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)playingGroupBox.Panel).BeginInit();
			playingGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)moduleEndComboBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)moduleListEndComboBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)showingGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)showingGroupBox.Panel).BeginInit();
			showingGroupBox.Panel.SuspendLayout();
			SuspendLayout();
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// loadingGroupBox
			// 
			loadingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			loadingGroupBox.Location = new System.Drawing.Point(8, 4);
			loadingGroupBox.Name = "loadingGroupBox";
			loadingGroupBox.LocalCustomPalette = fontPalette;
			loadingGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			loadingGroupBox.Panel.Controls.Add(doubleBufferingCheckBox);
			loadingGroupBox.Panel.Controls.Add(doubleBufferingPanel);
			loadingGroupBox.Panel.Controls.Add(moduleErrorLabel);
			loadingGroupBox.Panel.Controls.Add(moduleErrorComboBox);
			controlResource.SetResourceKey(loadingGroupBox, "IDS_SETTINGS_MODULES_LOADING");
			loadingGroupBox.Size = new System.Drawing.Size(592, 108);
			loadingGroupBox.TabIndex = 0;
			loadingGroupBox.Values.Heading = "Loading";
			// 
			// doubleBufferingCheckBox
			// 
			doubleBufferingCheckBox.Location = new System.Drawing.Point(4, 5);
			doubleBufferingCheckBox.Name = "doubleBufferingCheckBox";
			doubleBufferingCheckBox.LocalCustomPalette = fontPalette;
			doubleBufferingCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(doubleBufferingCheckBox, "IDS_SETTINGS_MODULES_LOADING_DOUBLEBUFFERING");
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
			earlyLoadLabel.LocalCustomPalette = fontPalette;
			earlyLoadLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(earlyLoadLabel, "IDS_SETTINGS_MODULES_LOADING_EARLYLOAD");
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
			moduleErrorLabel.LocalCustomPalette = fontPalette;
			moduleErrorLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleErrorLabel, "IDS_SETTINGS_MODULES_LOADING_MODULEERROR");
			moduleErrorLabel.Size = new System.Drawing.Size(166, 16);
			moduleErrorLabel.TabIndex = 2;
			moduleErrorLabel.Values.Text = "When a module error is reached";
			// 
			// moduleErrorComboBox
			// 
			moduleErrorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			moduleErrorComboBox.DropDownWidth = 121;
			moduleErrorComboBox.IntegralHeight = false;
			moduleErrorComboBox.Location = new System.Drawing.Point(180, 60);
			moduleErrorComboBox.Name = "moduleErrorComboBox";
			moduleErrorComboBox.LocalCustomPalette = fontPalette;
			moduleErrorComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleErrorComboBox, null);
			moduleErrorComboBox.Size = new System.Drawing.Size(180, 19);
			moduleErrorComboBox.TabIndex = 3;
			// 
			// playingGroupBox
			// 
			playingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			playingGroupBox.Location = new System.Drawing.Point(8, 116);
			playingGroupBox.Name = "playingGroupBox";
			playingGroupBox.LocalCustomPalette = fontPalette;
			playingGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			playingGroupBox.Panel.Controls.Add(neverEndingCheckBox);
			playingGroupBox.Panel.Controls.Add(neverEndingNumberTextBox);
			playingGroupBox.Panel.Controls.Add(neverEndingLabel);
			playingGroupBox.Panel.Controls.Add(moduleEndComboBox);
			playingGroupBox.Panel.Controls.Add(moduleEndLabel);
			playingGroupBox.Panel.Controls.Add(moduleListEndComboBox);
			playingGroupBox.Panel.Controls.Add(moduleListEndLabel);
			controlResource.SetResourceKey(playingGroupBox, "IDS_SETTINGS_MODULES_PLAYING");
			playingGroupBox.Size = new System.Drawing.Size(592, 76);
			playingGroupBox.TabIndex = 1;
			playingGroupBox.Values.Heading = "Playing";
			// 
			// neverEndingCheckBox
			// 
			neverEndingCheckBox.Location = new System.Drawing.Point(4, 5);
			neverEndingCheckBox.Name = "neverEndingCheckBox";
			neverEndingCheckBox.LocalCustomPalette = fontPalette;
			neverEndingCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(neverEndingCheckBox, "IDS_SETTINGS_MODULES_PLAYING_NEVERENDING");
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
			neverEndingNumberTextBox.LocalCustomPalette = fontPalette;
			neverEndingNumberTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(neverEndingNumberTextBox, null);
			neverEndingNumberTextBox.Size = new System.Drawing.Size(32, 20);
			neverEndingNumberTextBox.TabIndex = 1;
			// 
			// neverEndingLabel
			// 
			neverEndingLabel.Location = new System.Drawing.Point(216, 5);
			neverEndingLabel.Name = "neverEndingLabel";
			neverEndingLabel.LocalCustomPalette = fontPalette;
			neverEndingLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(neverEndingLabel, "IDS_SETTINGS_MODULES_PLAYING_NEVERENDING_SECONDS");
			neverEndingLabel.Size = new System.Drawing.Size(51, 16);
			neverEndingLabel.TabIndex = 2;
			neverEndingLabel.Values.Text = "seconds";
			// 
			// moduleEndComboBox
			// 
			moduleEndComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			moduleEndComboBox.DropDownWidth = 121;
			moduleEndComboBox.IntegralHeight = false;
			moduleEndComboBox.Location = new System.Drawing.Point(108, 28);
			moduleEndComboBox.Name = "moduleEndComboBox";
			moduleEndComboBox.LocalCustomPalette = fontPalette;
			moduleEndComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleEndComboBox, null);
			moduleEndComboBox.Size = new System.Drawing.Size(180, 19);
			moduleEndComboBox.TabIndex = 4;
			// 
			// moduleEndLabel
			// 
			moduleEndLabel.Location = new System.Drawing.Point(4, 30);
			moduleEndLabel.Name = "moduleEndLabel";
			moduleEndLabel.LocalCustomPalette = fontPalette;
			moduleEndLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleEndLabel, "IDS_SETTINGS_MODULES_PLAYING_MODULEEND");
			moduleEndLabel.Size = new System.Drawing.Size(93, 16);
			moduleEndLabel.TabIndex = 3;
			moduleEndLabel.Values.Text = "At end of module";
			// 
			// moduleListEndComboBox
			// 
			moduleListEndComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			moduleListEndComboBox.DropDownWidth = 121;
			moduleListEndComboBox.IntegralHeight = false;
			moduleListEndComboBox.Location = new System.Drawing.Point(420, 28);
			moduleListEndComboBox.Name = "moduleListEndComboBox";
			moduleListEndComboBox.LocalCustomPalette = fontPalette;
			moduleListEndComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleListEndComboBox, null);
			moduleListEndComboBox.Size = new System.Drawing.Size(100, 19);
			moduleListEndComboBox.TabIndex = 6;
			// 
			// moduleListEndLabel
			// 
			moduleListEndLabel.Location = new System.Drawing.Point(300, 30);
			moduleListEndLabel.Name = "moduleListEndLabel";
			moduleListEndLabel.LocalCustomPalette = fontPalette;
			moduleListEndLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleListEndLabel, "IDS_SETTINGS_MODULES_PLAYING_MODULELISTEND");
			moduleListEndLabel.Size = new System.Drawing.Size(109, 16);
			moduleListEndLabel.TabIndex = 5;
			moduleListEndLabel.Values.Text = "At end of module list";
			// 
			// showingGroupBox
			// 
			showingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			showingGroupBox.Location = new System.Drawing.Point(8, 196);
			showingGroupBox.Name = "showingGroupBox";
			showingGroupBox.LocalCustomPalette = fontPalette;
			showingGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			showingGroupBox.Panel.Controls.Add(moduleInfoOrderDownButton);
			showingGroupBox.Panel.Controls.Add(moduleInfoOrderUpButton);
			showingGroupBox.Panel.Controls.Add(moduleInfoOrderLabel);
			showingGroupBox.Panel.Controls.Add(moduleInfoOrderListBox);
			controlResource.SetResourceKey(showingGroupBox, "IDS_SETTINGS_MODULES_SHOWING");
			showingGroupBox.Size = new System.Drawing.Size(592, 152);
			showingGroupBox.TabIndex = 2;
			showingGroupBox.Values.Heading = "Showing";
			// 
			// moduleInfoOrderDownButton
			// 
			moduleInfoOrderDownButton.Enabled = false;
			moduleInfoOrderDownButton.Location = new System.Drawing.Point(128, 62);
			moduleInfoOrderDownButton.Name = "moduleInfoOrderDownButton";
			moduleInfoOrderDownButton.LocalCustomPalette = fontPalette;
			moduleInfoOrderDownButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleInfoOrderDownButton, "IDS_SETTINGS_MODULES_SHOWING_MODULEINFO_DOWN");
			moduleInfoOrderDownButton.Size = new System.Drawing.Size(60, 25);
			moduleInfoOrderDownButton.TabIndex = 3;
			moduleInfoOrderDownButton.Values.Text = "Down";
			moduleInfoOrderDownButton.Click += ModuleInfoOrderDownButton_Click;
			// 
			// moduleInfoOrderUpButton
			// 
			moduleInfoOrderUpButton.Enabled = false;
			moduleInfoOrderUpButton.Location = new System.Drawing.Point(128, 33);
			moduleInfoOrderUpButton.Name = "moduleInfoOrderUpButton";
			moduleInfoOrderUpButton.LocalCustomPalette = fontPalette;
			moduleInfoOrderUpButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleInfoOrderUpButton, "IDS_SETTINGS_MODULES_SHOWING_MODULEINFO_UP");
			moduleInfoOrderUpButton.Size = new System.Drawing.Size(60, 25);
			moduleInfoOrderUpButton.TabIndex = 2;
			moduleInfoOrderUpButton.Values.Text = "Up";
			moduleInfoOrderUpButton.Click += ModuleInfoOrderUpButton_Click;
			// 
			// moduleInfoOrderLabel
			// 
			moduleInfoOrderLabel.Location = new System.Drawing.Point(4, 5);
			moduleInfoOrderLabel.Name = "moduleInfoOrderLabel";
			moduleInfoOrderLabel.LocalCustomPalette = fontPalette;
			moduleInfoOrderLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleInfoOrderLabel, "IDS_SETTINGS_MODULES_SHOWING_MODULEINFO_ORDER");
			moduleInfoOrderLabel.Size = new System.Drawing.Size(155, 28);
			moduleInfoOrderLabel.TabIndex = 0;
			moduleInfoOrderLabel.Values.Text = "In which order to activate tabs\r\nin Module Information window";
			// 
			// moduleInfoOrderListBox
			// 
			moduleInfoOrderListBox.Location = new System.Drawing.Point(4, 33);
			moduleInfoOrderListBox.Name = "moduleInfoOrderListBox";
			moduleInfoOrderListBox.LocalCustomPalette = fontPalette;
			moduleInfoOrderListBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleInfoOrderListBox, null);
			moduleInfoOrderListBox.Size = new System.Drawing.Size(120, 89);
			moduleInfoOrderListBox.TabIndex = 1;
			moduleInfoOrderListBox.SelectedIndexChanged += ModuleInfoOrderListBox_SelectedIndexChanged;
			// 
			// ModulesPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(showingGroupBox);
			Controls.Add(playingGroupBox);
			Controls.Add(loadingGroupBox);
			Name = "ModulesPageControl";
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(608, 356);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
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
			((System.ComponentModel.ISupportInitialize)moduleEndComboBox).EndInit();
			((System.ComponentModel.ISupportInitialize)moduleListEndComboBox).EndInit();
			((System.ComponentModel.ISupportInitialize)showingGroupBox.Panel).EndInit();
			showingGroupBox.Panel.ResumeLayout(false);
			showingGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)showingGroupBox).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private GuiKit.Components.FontPalette fontPalette;
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonGroupBox loadingGroupBox;
		private Krypton.Toolkit.KryptonCheckBox doubleBufferingCheckBox;
		private System.Windows.Forms.Panel doubleBufferingPanel;
		private Krypton.Toolkit.KryptonLabel earlyLoadLabel;
		private Krypton.Toolkit.KryptonTrackBar doubleBufferingTrackBar;
		private Krypton.Toolkit.KryptonLabel moduleErrorLabel;
		private Krypton.Toolkit.KryptonComboBox moduleErrorComboBox;
		private Krypton.Toolkit.KryptonGroupBox playingGroupBox;
		private Krypton.Toolkit.KryptonCheckBox neverEndingCheckBox;
		private Controls.NumberTextBox neverEndingNumberTextBox;
		private Krypton.Toolkit.KryptonLabel neverEndingLabel;
		private Krypton.Toolkit.KryptonComboBox moduleListEndComboBox;
		private Krypton.Toolkit.KryptonLabel moduleListEndLabel;
		private Krypton.Toolkit.KryptonGroupBox showingGroupBox;
		private Krypton.Toolkit.KryptonListBox moduleInfoOrderListBox;
		private Krypton.Toolkit.KryptonLabel moduleInfoOrderLabel;
		private Krypton.Toolkit.KryptonButton moduleInfoOrderUpButton;
		private Krypton.Toolkit.KryptonButton moduleInfoOrderDownButton;
		private Krypton.Toolkit.KryptonLabel moduleEndLabel;
		private Krypton.Toolkit.KryptonComboBox moduleEndComboBox;
	}
}
