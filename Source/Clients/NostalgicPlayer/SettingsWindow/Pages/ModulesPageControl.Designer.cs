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
			moduleListEndComboBox = new Krypton.Toolkit.KryptonComboBox();
			moduleListEndLabel = new Krypton.Toolkit.KryptonLabel();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
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
			// loadingGroupBox
			// 
			loadingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			loadingGroupBox.Location = new System.Drawing.Point(8, 4);
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
			controlResource.SetResourceKey(loadingGroupBox, "IDS_SETTINGS_MODULES_LOADING");
			loadingGroupBox.Size = new System.Drawing.Size(592, 108);
			loadingGroupBox.TabIndex = 0;
			loadingGroupBox.Values.Heading = "Loading";
			// 
			// doubleBufferingCheckBox
			// 
			doubleBufferingCheckBox.Location = new System.Drawing.Point(4, 5);
			doubleBufferingCheckBox.Name = "doubleBufferingCheckBox";
			doubleBufferingCheckBox.Palette = fontPalette;
			doubleBufferingCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(doubleBufferingCheckBox, "IDS_SETTINGS_MODULES_LOADING_DOUBLEBUFFERING");
			doubleBufferingCheckBox.Size = new System.Drawing.Size(104, 16);
			doubleBufferingCheckBox.TabIndex = 4;
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
			doubleBufferingPanel.TabIndex = 5;
			// 
			// earlyLoadLabel
			// 
			earlyLoadLabel.Location = new System.Drawing.Point(494, 5);
			earlyLoadLabel.Name = "earlyLoadLabel";
			earlyLoadLabel.Palette = fontPalette;
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
			moduleErrorLabel.Palette = fontPalette;
			moduleErrorLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleErrorLabel, "IDS_SETTINGS_MODULES_LOADING_MODULEERROR");
			moduleErrorLabel.Size = new System.Drawing.Size(166, 16);
			moduleErrorLabel.TabIndex = 6;
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
			moduleErrorComboBox.TabIndex = 7;
			// 
			// playingGroupBox
			// 
			playingGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			playingGroupBox.Location = new System.Drawing.Point(8, 116);
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
			controlResource.SetResourceKey(playingGroupBox, "IDS_SETTINGS_MODULES_PLAYING");
			playingGroupBox.Size = new System.Drawing.Size(592, 76);
			playingGroupBox.TabIndex = 1;
			playingGroupBox.Values.Heading = "Playing";
			// 
			// neverEndingCheckBox
			// 
			neverEndingCheckBox.Location = new System.Drawing.Point(4, 5);
			neverEndingCheckBox.Name = "neverEndingCheckBox";
			neverEndingCheckBox.Palette = fontPalette;
			neverEndingCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(neverEndingCheckBox, "IDS_SETTINGS_MODULES_PLAYING_NEVERENDING");
			neverEndingCheckBox.Size = new System.Drawing.Size(166, 16);
			neverEndingCheckBox.TabIndex = 5;
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
			neverEndingNumberTextBox.TabIndex = 6;
			// 
			// neverEndingLabel
			// 
			neverEndingLabel.Location = new System.Drawing.Point(216, 5);
			neverEndingLabel.Name = "neverEndingLabel";
			neverEndingLabel.Palette = fontPalette;
			neverEndingLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(neverEndingLabel, "IDS_SETTINGS_MODULES_PLAYING_NEVERENDING_SECONDS");
			neverEndingLabel.Size = new System.Drawing.Size(51, 16);
			neverEndingLabel.TabIndex = 7;
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
			moduleListEndComboBox.TabIndex = 9;
			// 
			// moduleListEndLabel
			// 
			moduleListEndLabel.Location = new System.Drawing.Point(4, 30);
			moduleListEndLabel.Name = "moduleListEndLabel";
			moduleListEndLabel.Palette = fontPalette;
			moduleListEndLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(moduleListEndLabel, "IDS_SETTINGS_MODULES_PLAYING_MODULELISTEND");
			moduleListEndLabel.Size = new System.Drawing.Size(109, 16);
			moduleListEndLabel.TabIndex = 8;
			moduleListEndLabel.Values.Text = "At end of module list";
			// 
			// ModulesPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
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
			((System.ComponentModel.ISupportInitialize)moduleListEndComboBox).EndInit();
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
	}
}
