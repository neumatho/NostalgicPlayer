
namespace Polycode.NostalgicPlayer.Agent.Output.DiskSaver.Settings
{
	partial class SettingsControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsControl));
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			pathLabel = new Krypton.Toolkit.KryptonLabel();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			eigthBitRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			sixthteenBitRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			thirtytwoRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			stereoRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			monoRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			formatLabel = new Krypton.Toolkit.KryptonLabel();
			passThroughLabel = new Krypton.Toolkit.KryptonLabel();
			frequencyLabel = new Krypton.Toolkit.KryptonLabel();
			infoLabel = new Krypton.Toolkit.KryptonLabel();
			pathTextBox = new Krypton.Toolkit.KryptonTextBox();
			pathButton = new Krypton.Toolkit.KryptonButton();
			bitPanel = new System.Windows.Forms.Panel();
			channelPanel = new System.Windows.Forms.Panel();
			formatComboBox = new Krypton.Toolkit.KryptonComboBox();
			formatSettingsButton = new Krypton.Toolkit.KryptonButton();
			passThroughComboBox = new Krypton.Toolkit.KryptonComboBox();
			frequencyTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			frequencyValueLabel = new Krypton.Toolkit.KryptonLabel();
			frequencyPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			bitPanel.SuspendLayout();
			channelPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)formatComboBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)passThroughComboBox).BeginInit();
			frequencyPanel.SuspendLayout();
			SuspendLayout();
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Agent.Output.DiskSaver.Resources";
			// 
			// pathLabel
			// 
			pathLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			pathLabel.Location = new System.Drawing.Point(4, 38);
			pathLabel.Name = "pathLabel";
			pathLabel.Palette = fontPalette;
			pathLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(pathLabel, "IDS_SETTINGS_PATH");
			pathLabel.Size = new System.Drawing.Size(33, 16);
			pathLabel.TabIndex = 1;
			pathLabel.Values.Text = "Path";
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// eigthBitRadioButton
			// 
			eigthBitRadioButton.Location = new System.Drawing.Point(0, 0);
			eigthBitRadioButton.Name = "eigthBitRadioButton";
			eigthBitRadioButton.Palette = fontPalette;
			eigthBitRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(eigthBitRadioButton, "IDS_SETTINGS_8BIT");
			eigthBitRadioButton.Size = new System.Drawing.Size(43, 16);
			eigthBitRadioButton.TabIndex = 0;
			eigthBitRadioButton.Values.Text = "8 bit";
			// 
			// sixthteenBitRadioButton
			// 
			sixthteenBitRadioButton.Location = new System.Drawing.Point(0, 20);
			sixthteenBitRadioButton.Name = "sixthteenBitRadioButton";
			sixthteenBitRadioButton.Palette = fontPalette;
			sixthteenBitRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(sixthteenBitRadioButton, "IDS_SETTINGS_16BIT");
			sixthteenBitRadioButton.Size = new System.Drawing.Size(49, 16);
			sixthteenBitRadioButton.TabIndex = 1;
			sixthteenBitRadioButton.Values.Text = "16 bit";
			// 
			// thirtytwoRadioButton
			// 
			thirtytwoRadioButton.Location = new System.Drawing.Point(0, 40);
			thirtytwoRadioButton.Name = "thirtytwoRadioButton";
			thirtytwoRadioButton.Palette = fontPalette;
			thirtytwoRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(thirtytwoRadioButton, "IDS_SETTINGS_32BIT");
			thirtytwoRadioButton.Size = new System.Drawing.Size(49, 16);
			thirtytwoRadioButton.TabIndex = 2;
			thirtytwoRadioButton.Values.Text = "32 bit";
			// 
			// stereoRadioButton
			// 
			stereoRadioButton.Location = new System.Drawing.Point(0, 20);
			stereoRadioButton.Name = "stereoRadioButton";
			stereoRadioButton.Palette = fontPalette;
			stereoRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(stereoRadioButton, "IDS_SETTINGS_STEREO");
			stereoRadioButton.Size = new System.Drawing.Size(54, 16);
			stereoRadioButton.TabIndex = 1;
			stereoRadioButton.Values.Text = "Stereo";
			// 
			// monoRadioButton
			// 
			monoRadioButton.Location = new System.Drawing.Point(0, 0);
			monoRadioButton.Name = "monoRadioButton";
			monoRadioButton.Palette = fontPalette;
			monoRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(monoRadioButton, "IDS_SETTINGS_MONO");
			monoRadioButton.Size = new System.Drawing.Size(50, 16);
			monoRadioButton.TabIndex = 0;
			monoRadioButton.Values.Text = "Mono";
			// 
			// formatLabel
			// 
			formatLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			formatLabel.Location = new System.Drawing.Point(171, 67);
			formatLabel.Name = "formatLabel";
			formatLabel.Palette = fontPalette;
			formatLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(formatLabel, "IDS_SETTINGS_FORMAT");
			formatLabel.Size = new System.Drawing.Size(77, 16);
			formatLabel.TabIndex = 6;
			formatLabel.Values.Text = "Output format";
			// 
			// passThroughLabel
			// 
			passThroughLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			passThroughLabel.Location = new System.Drawing.Point(373, 67);
			passThroughLabel.Name = "passThroughLabel";
			passThroughLabel.Palette = fontPalette;
			passThroughLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(passThroughLabel, "IDS_SETTINGS_PASSTHROUGH");
			passThroughLabel.Size = new System.Drawing.Size(139, 16);
			passThroughLabel.TabIndex = 9;
			passThroughLabel.Values.Text = "Pass through output agent";
			// 
			// frequencyLabel
			// 
			frequencyLabel.Location = new System.Drawing.Point(9, 5);
			frequencyLabel.Name = "frequencyLabel";
			frequencyLabel.Palette = fontPalette;
			frequencyLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(frequencyLabel, "IDS_SETTINGS_FREQUENCY");
			frequencyLabel.Size = new System.Drawing.Size(62, 16);
			frequencyLabel.TabIndex = 0;
			frequencyLabel.Values.Text = "Frequency";
			// 
			// infoLabel
			// 
			infoLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			infoLabel.Location = new System.Drawing.Point(4, 8);
			infoLabel.Name = "infoLabel";
			infoLabel.Palette = fontPalette;
			infoLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(infoLabel, "IDS_SETTINGS_INFO");
			infoLabel.Size = new System.Drawing.Size(368, 16);
			infoLabel.TabIndex = 0;
			infoLabel.Values.Text = "Remember to switch output agent on the Mixer tab in the settings window.";
			// 
			// pathTextBox
			// 
			pathTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pathTextBox.Location = new System.Drawing.Point(41, 36);
			pathTextBox.Name = "pathTextBox";
			pathTextBox.Palette = fontPalette;
			pathTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			pathTextBox.Size = new System.Drawing.Size(461, 20);
			pathTextBox.TabIndex = 2;
			// 
			// pathButton
			// 
			pathButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			pathButton.Location = new System.Drawing.Point(506, 35);
			pathButton.Name = "pathButton";
			pathButton.Size = new System.Drawing.Size(22, 22);
			pathButton.TabIndex = 3;
			pathButton.Values.Image = (System.Drawing.Image)resources.GetObject("pathButton.Values.Image");
			pathButton.Values.Text = "";
			pathButton.Click += PathButton_Click;
			// 
			// bitPanel
			// 
			bitPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			bitPanel.Controls.Add(thirtytwoRadioButton);
			bitPanel.Controls.Add(sixthteenBitRadioButton);
			bitPanel.Controls.Add(eigthBitRadioButton);
			bitPanel.Location = new System.Drawing.Point(9, 66);
			bitPanel.Name = "bitPanel";
			bitPanel.Size = new System.Drawing.Size(75, 58);
			bitPanel.TabIndex = 4;
			// 
			// channelPanel
			// 
			channelPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			channelPanel.Controls.Add(stereoRadioButton);
			channelPanel.Controls.Add(monoRadioButton);
			channelPanel.Location = new System.Drawing.Point(91, 66);
			channelPanel.Name = "channelPanel";
			channelPanel.Size = new System.Drawing.Size(73, 38);
			channelPanel.TabIndex = 5;
			// 
			// formatComboBox
			// 
			formatComboBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			formatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			formatComboBox.DropDownWidth = 120;
			formatComboBox.IntegralHeight = false;
			formatComboBox.Location = new System.Drawing.Point(175, 83);
			formatComboBox.Name = "formatComboBox";
			formatComboBox.Palette = fontPalette;
			formatComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			formatComboBox.Size = new System.Drawing.Size(160, 19);
			formatComboBox.Sorted = true;
			formatComboBox.TabIndex = 7;
			formatComboBox.SelectedIndexChanged += FormatComboBox_SelectedIndexChanged;
			// 
			// formatSettingsButton
			// 
			formatSettingsButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			formatSettingsButton.Enabled = false;
			formatSettingsButton.Location = new System.Drawing.Point(339, 81);
			formatSettingsButton.Name = "formatSettingsButton";
			formatSettingsButton.Size = new System.Drawing.Size(22, 22);
			formatSettingsButton.TabIndex = 8;
			formatSettingsButton.Values.Image = (System.Drawing.Image)resources.GetObject("formatSettingsButton.Values.Image");
			formatSettingsButton.Values.Text = "";
			// 
			// passThroughComboBox
			// 
			passThroughComboBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			passThroughComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			passThroughComboBox.DropDownWidth = 154;
			passThroughComboBox.IntegralHeight = false;
			passThroughComboBox.Location = new System.Drawing.Point(374, 83);
			passThroughComboBox.Name = "passThroughComboBox";
			passThroughComboBox.Palette = fontPalette;
			passThroughComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			passThroughComboBox.Size = new System.Drawing.Size(154, 19);
			passThroughComboBox.Sorted = true;
			passThroughComboBox.TabIndex = 10;
			passThroughComboBox.SelectedIndexChanged += PassThroughComboBox_SelectedIndexChanged;
			// 
			// frequencyTrackBar
			// 
			frequencyTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			frequencyTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			frequencyTrackBar.Location = new System.Drawing.Point(91, 0);
			frequencyTrackBar.Maximum = 5;
			frequencyTrackBar.Name = "frequencyTrackBar";
			frequencyTrackBar.Size = new System.Drawing.Size(399, 27);
			frequencyTrackBar.TabIndex = 1;
			frequencyTrackBar.ValueChanged += FrequencyTrackBar_ValueChanged;
			// 
			// frequencyValueLabel
			// 
			frequencyValueLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			frequencyValueLabel.Location = new System.Drawing.Point(494, 5);
			frequencyValueLabel.Name = "frequencyValueLabel";
			frequencyValueLabel.Palette = fontPalette;
			frequencyValueLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			frequencyValueLabel.Size = new System.Drawing.Size(41, 16);
			frequencyValueLabel.TabIndex = 2;
			frequencyValueLabel.Values.Text = "44100";
			// 
			// frequencyPanel
			// 
			frequencyPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			frequencyPanel.Controls.Add(frequencyValueLabel);
			frequencyPanel.Controls.Add(frequencyTrackBar);
			frequencyPanel.Controls.Add(frequencyLabel);
			frequencyPanel.Location = new System.Drawing.Point(0, 125);
			frequencyPanel.Name = "frequencyPanel";
			frequencyPanel.Size = new System.Drawing.Size(538, 27);
			frequencyPanel.TabIndex = 11;
			// 
			// SettingsControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(infoLabel);
			Controls.Add(frequencyPanel);
			Controls.Add(passThroughComboBox);
			Controls.Add(passThroughLabel);
			Controls.Add(formatSettingsButton);
			Controls.Add(formatComboBox);
			Controls.Add(formatLabel);
			Controls.Add(channelPanel);
			Controls.Add(bitPanel);
			Controls.Add(pathButton);
			Controls.Add(pathTextBox);
			Controls.Add(pathLabel);
			MinimumSize = new System.Drawing.Size(538, 160);
			Name = "SettingsControl";
			Size = new System.Drawing.Size(538, 160);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			bitPanel.ResumeLayout(false);
			bitPanel.PerformLayout();
			channelPanel.ResumeLayout(false);
			channelPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)formatComboBox).EndInit();
			((System.ComponentModel.ISupportInitialize)passThroughComboBox).EndInit();
			frequencyPanel.ResumeLayout(false);
			frequencyPanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion
		private Kit.Gui.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonLabel pathLabel;
		private Krypton.Toolkit.KryptonTextBox pathTextBox;
		private Krypton.Toolkit.KryptonButton pathButton;
		private Krypton.Toolkit.KryptonRadioButton eigthBitRadioButton;
		private Krypton.Toolkit.KryptonRadioButton sixthteenBitRadioButton;
		private System.Windows.Forms.Panel bitPanel;
		private System.Windows.Forms.Panel channelPanel;
		private Krypton.Toolkit.KryptonRadioButton stereoRadioButton;
		private Krypton.Toolkit.KryptonRadioButton monoRadioButton;
		private Krypton.Toolkit.KryptonLabel formatLabel;
		private Krypton.Toolkit.KryptonComboBox formatComboBox;
		private Krypton.Toolkit.KryptonButton formatSettingsButton;
		private Krypton.Toolkit.KryptonLabel passThroughLabel;
		private Krypton.Toolkit.KryptonComboBox passThroughComboBox;
		private Krypton.Toolkit.KryptonLabel frequencyLabel;
		private Krypton.Toolkit.KryptonTrackBar frequencyTrackBar;
		private Krypton.Toolkit.KryptonLabel frequencyValueLabel;
		private Krypton.Toolkit.KryptonRadioButton thirtytwoRadioButton;
		private System.Windows.Forms.Panel frequencyPanel;
		private Kit.Gui.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonLabel infoLabel;
	}
}
