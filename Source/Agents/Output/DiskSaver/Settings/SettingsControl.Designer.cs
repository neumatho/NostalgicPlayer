﻿
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsControl));
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.pathLabel = new Krypton.Toolkit.KryptonLabel();
			this.fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(this.components);
			this.pathTextBox = new Krypton.Toolkit.KryptonTextBox();
			this.pathButton = new Krypton.Toolkit.KryptonButton();
			this.eigthBitRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.sixthteenBitRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.bitPanel = new System.Windows.Forms.Panel();
			this.thirtytwoRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.channelPanel = new System.Windows.Forms.Panel();
			this.stereoRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.monoRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.formatLabel = new Krypton.Toolkit.KryptonLabel();
			this.formatComboBox = new Krypton.Toolkit.KryptonComboBox();
			this.formatSettingsButton = new Krypton.Toolkit.KryptonButton();
			this.passThroughLabel = new Krypton.Toolkit.KryptonLabel();
			this.passThroughComboBox = new Krypton.Toolkit.KryptonComboBox();
			this.frequencyLabel = new Krypton.Toolkit.KryptonLabel();
			this.frequencyTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.frequencyValueLabel = new Krypton.Toolkit.KryptonLabel();
			this.frequencyPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			this.bitPanel.SuspendLayout();
			this.channelPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.formatComboBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.passThroughComboBox)).BeginInit();
			this.frequencyPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Agent.Output.DiskSaver.Resources";
			// 
			// pathLabel
			// 
			this.pathLabel.Location = new System.Drawing.Point(4, 10);
			this.pathLabel.Name = "pathLabel";
			this.pathLabel.LocalCustomPalette = this.fontPalette;
			this.pathLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.pathLabel, "IDS_SETTINGS_PATH");
			this.pathLabel.Size = new System.Drawing.Size(33, 16);
			this.pathLabel.TabIndex = 0;
			this.pathLabel.Values.Text = "Path";
			// 
			// pathTextBox
			// 
			this.pathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pathTextBox.Location = new System.Drawing.Point(41, 8);
			this.pathTextBox.Name = "pathTextBox";
			this.pathTextBox.LocalCustomPalette = this.fontPalette;
			this.pathTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.pathTextBox, null);
			this.pathTextBox.Size = new System.Drawing.Size(461, 20);
			this.pathTextBox.TabIndex = 1;
			// 
			// pathButton
			// 
			this.pathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pathButton.Location = new System.Drawing.Point(506, 7);
			this.pathButton.Name = "pathButton";
			this.controlResource.SetResourceKey(this.pathButton, null);
			this.pathButton.Size = new System.Drawing.Size(22, 22);
			this.pathButton.TabIndex = 2;
			this.pathButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("pathButton.Values.Image")));
			this.pathButton.Values.Text = "";
			this.pathButton.Click += new System.EventHandler(this.PathButton_Click);
			// 
			// eigthBitRadioButton
			// 
			this.eigthBitRadioButton.Location = new System.Drawing.Point(0, 0);
			this.eigthBitRadioButton.Name = "eigthBitRadioButton";
			this.eigthBitRadioButton.LocalCustomPalette = this.fontPalette;
			this.eigthBitRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.eigthBitRadioButton, "IDS_SETTINGS_8BIT");
			this.eigthBitRadioButton.Size = new System.Drawing.Size(43, 16);
			this.eigthBitRadioButton.TabIndex = 0;
			this.eigthBitRadioButton.Values.Text = "8 bit";
			// 
			// sixthteenBitRadioButton
			// 
			this.sixthteenBitRadioButton.Location = new System.Drawing.Point(0, 20);
			this.sixthteenBitRadioButton.Name = "sixthteenBitRadioButton";
			this.sixthteenBitRadioButton.LocalCustomPalette = this.fontPalette;
			this.sixthteenBitRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.sixthteenBitRadioButton, "IDS_SETTINGS_16BIT");
			this.sixthteenBitRadioButton.Size = new System.Drawing.Size(49, 16);
			this.sixthteenBitRadioButton.TabIndex = 1;
			this.sixthteenBitRadioButton.Values.Text = "16 bit";
			// 
			// bitPanel
			// 
			this.bitPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.bitPanel.Controls.Add(this.thirtytwoRadioButton);
			this.bitPanel.Controls.Add(this.sixthteenBitRadioButton);
			this.bitPanel.Controls.Add(this.eigthBitRadioButton);
			this.bitPanel.Location = new System.Drawing.Point(9, 38);
			this.bitPanel.Name = "bitPanel";
			this.controlResource.SetResourceKey(this.bitPanel, null);
			this.bitPanel.Size = new System.Drawing.Size(75, 58);
			this.bitPanel.TabIndex = 3;
			// 
			// thirtytwoRadioButton
			// 
			this.thirtytwoRadioButton.Location = new System.Drawing.Point(0, 40);
			this.thirtytwoRadioButton.Name = "thirtytwoRadioButton";
			this.thirtytwoRadioButton.LocalCustomPalette = this.fontPalette;
			this.thirtytwoRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.thirtytwoRadioButton, "IDS_SETTINGS_32BIT");
			this.thirtytwoRadioButton.Size = new System.Drawing.Size(49, 16);
			this.thirtytwoRadioButton.TabIndex = 2;
			this.thirtytwoRadioButton.Values.Text = "32 bit";
			// 
			// channelPanel
			// 
			this.channelPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.channelPanel.Controls.Add(this.stereoRadioButton);
			this.channelPanel.Controls.Add(this.monoRadioButton);
			this.channelPanel.Location = new System.Drawing.Point(91, 38);
			this.channelPanel.Name = "channelPanel";
			this.controlResource.SetResourceKey(this.channelPanel, null);
			this.channelPanel.Size = new System.Drawing.Size(73, 38);
			this.channelPanel.TabIndex = 4;
			// 
			// stereoRadioButton
			// 
			this.stereoRadioButton.Location = new System.Drawing.Point(0, 20);
			this.stereoRadioButton.Name = "stereoRadioButton";
			this.stereoRadioButton.LocalCustomPalette = this.fontPalette;
			this.stereoRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.stereoRadioButton, "IDS_SETTINGS_STEREO");
			this.stereoRadioButton.Size = new System.Drawing.Size(54, 16);
			this.stereoRadioButton.TabIndex = 1;
			this.stereoRadioButton.Values.Text = "Stereo";
			// 
			// monoRadioButton
			// 
			this.monoRadioButton.Location = new System.Drawing.Point(0, 0);
			this.monoRadioButton.Name = "monoRadioButton";
			this.monoRadioButton.LocalCustomPalette = this.fontPalette;
			this.monoRadioButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.monoRadioButton, "IDS_SETTINGS_MONO");
			this.monoRadioButton.Size = new System.Drawing.Size(50, 16);
			this.monoRadioButton.TabIndex = 0;
			this.monoRadioButton.Values.Text = "Mono";
			// 
			// formatLabel
			// 
			this.formatLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.formatLabel.Location = new System.Drawing.Point(171, 39);
			this.formatLabel.Name = "formatLabel";
			this.formatLabel.LocalCustomPalette = this.fontPalette;
			this.formatLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.formatLabel, "IDS_SETTINGS_FORMAT");
			this.formatLabel.Size = new System.Drawing.Size(77, 16);
			this.formatLabel.TabIndex = 5;
			this.formatLabel.Values.Text = "Output format";
			// 
			// formatComboBox
			// 
			this.formatComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.formatComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.formatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.formatComboBox.DropDownWidth = 120;
			this.formatComboBox.IntegralHeight = false;
			this.formatComboBox.Location = new System.Drawing.Point(175, 55);
			this.formatComboBox.Name = "formatComboBox";
			this.formatComboBox.LocalCustomPalette = this.fontPalette;
			this.formatComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.formatComboBox, null);
			this.formatComboBox.Size = new System.Drawing.Size(160, 18);
			this.formatComboBox.Sorted = true;
			this.formatComboBox.TabIndex = 6;
			this.formatComboBox.SelectedIndexChanged += new System.EventHandler(this.FormatComboBox_SelectedIndexChanged);
			// 
			// formatSettingsButton
			// 
			this.formatSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.formatSettingsButton.Enabled = false;
			this.formatSettingsButton.Location = new System.Drawing.Point(339, 53);
			this.formatSettingsButton.Name = "formatSettingsButton";
			this.controlResource.SetResourceKey(this.formatSettingsButton, null);
			this.formatSettingsButton.Size = new System.Drawing.Size(22, 22);
			this.formatSettingsButton.TabIndex = 7;
			this.formatSettingsButton.Values.Image = ((System.Drawing.Image)(resources.GetObject("formatSettingsButton.Values.Image")));
			this.formatSettingsButton.Values.Text = "";
			// 
			// passThroughLabel
			// 
			this.passThroughLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.passThroughLabel.Location = new System.Drawing.Point(373, 39);
			this.passThroughLabel.Name = "passThroughLabel";
			this.passThroughLabel.LocalCustomPalette = this.fontPalette;
			this.passThroughLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.passThroughLabel, "IDS_SETTINGS_PASSTHROUGH");
			this.passThroughLabel.Size = new System.Drawing.Size(139, 16);
			this.passThroughLabel.TabIndex = 8;
			this.passThroughLabel.Values.Text = "Pass through output agent";
			// 
			// passThroughComboBox
			// 
			this.passThroughComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.passThroughComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.passThroughComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.passThroughComboBox.DropDownWidth = 154;
			this.passThroughComboBox.IntegralHeight = false;
			this.passThroughComboBox.Location = new System.Drawing.Point(374, 55);
			this.passThroughComboBox.Name = "passThroughComboBox";
			this.passThroughComboBox.LocalCustomPalette = this.fontPalette;
			this.passThroughComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.passThroughComboBox, null);
			this.passThroughComboBox.Size = new System.Drawing.Size(154, 18);
			this.passThroughComboBox.Sorted = true;
			this.passThroughComboBox.TabIndex = 9;
			this.passThroughComboBox.SelectedIndexChanged += new System.EventHandler(this.PassThroughComboBox_SelectedIndexChanged);
			// 
			// frequencyLabel
			// 
			this.frequencyLabel.Location = new System.Drawing.Point(9, 5);
			this.frequencyLabel.Name = "frequencyLabel";
			this.frequencyLabel.LocalCustomPalette = this.fontPalette;
			this.frequencyLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.frequencyLabel, "IDS_SETTINGS_FREQUENCY");
			this.frequencyLabel.Size = new System.Drawing.Size(62, 16);
			this.frequencyLabel.TabIndex = 0;
			this.frequencyLabel.Values.Text = "Frequency";
			// 
			// frequencyTrackBar
			// 
			this.frequencyTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.frequencyTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			this.frequencyTrackBar.Location = new System.Drawing.Point(91, 0);
			this.frequencyTrackBar.Maximum = 5;
			this.frequencyTrackBar.Name = "frequencyTrackBar";
			this.controlResource.SetResourceKey(this.frequencyTrackBar, null);
			this.frequencyTrackBar.Size = new System.Drawing.Size(399, 27);
			this.frequencyTrackBar.TabIndex = 1;
			this.frequencyTrackBar.ValueChanged += new System.EventHandler(this.FrequencyTrackBar_ValueChanged);
			// 
			// frequencyValueLabel
			// 
			this.frequencyValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.frequencyValueLabel.Location = new System.Drawing.Point(494, 5);
			this.frequencyValueLabel.Name = "frequencyValueLabel";
			this.frequencyValueLabel.LocalCustomPalette = this.fontPalette;
			this.frequencyValueLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.frequencyValueLabel, null);
			this.frequencyValueLabel.Size = new System.Drawing.Size(41, 16);
			this.frequencyValueLabel.TabIndex = 2;
			this.frequencyValueLabel.Values.Text = "44100";
			// 
			// frequencyPanel
			// 
			this.frequencyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.frequencyPanel.Controls.Add(this.frequencyValueLabel);
			this.frequencyPanel.Controls.Add(this.frequencyTrackBar);
			this.frequencyPanel.Controls.Add(this.frequencyLabel);
			this.frequencyPanel.Location = new System.Drawing.Point(0, 97);
			this.frequencyPanel.Name = "frequencyPanel";
			this.controlResource.SetResourceKey(this.frequencyPanel, null);
			this.frequencyPanel.Size = new System.Drawing.Size(538, 27);
			this.frequencyPanel.TabIndex = 10;
			// 
			// SettingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.frequencyPanel);
			this.Controls.Add(this.passThroughComboBox);
			this.Controls.Add(this.passThroughLabel);
			this.Controls.Add(this.formatSettingsButton);
			this.Controls.Add(this.formatComboBox);
			this.Controls.Add(this.formatLabel);
			this.Controls.Add(this.channelPanel);
			this.Controls.Add(this.bitPanel);
			this.Controls.Add(this.pathButton);
			this.Controls.Add(this.pathTextBox);
			this.Controls.Add(this.pathLabel);
			this.MinimumSize = new System.Drawing.Size(538, 132);
			this.Name = "SettingsControl";
			this.controlResource.SetResourceKey(this, null);
			this.Size = new System.Drawing.Size(538, 132);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			this.bitPanel.ResumeLayout(false);
			this.bitPanel.PerformLayout();
			this.channelPanel.ResumeLayout(false);
			this.channelPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.formatComboBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.passThroughComboBox)).EndInit();
			this.frequencyPanel.ResumeLayout(false);
			this.frequencyPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private GuiKit.Designer.ControlResource controlResource;
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
		private GuiKit.Components.FontPalette fontPalette;
	}
}
