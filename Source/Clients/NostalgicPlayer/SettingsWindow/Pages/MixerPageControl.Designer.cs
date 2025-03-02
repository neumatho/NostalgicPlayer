
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	partial class MixerPageControl
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
			controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			generalGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			stereoSeparationPercentLabel = new Krypton.Toolkit.KryptonLabel();
			stereoSeparationTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			stereoSeparationLabel = new Krypton.Toolkit.KryptonLabel();
			visualsLatencyLabel = new Krypton.Toolkit.KryptonLabel();
			visualsLatencyTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			visualsLatencyMsLabel = new Krypton.Toolkit.KryptonLabel();
			amigaFilterCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			interpolationCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			swapSpeakersCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			surroundCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			outputGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			outputAgentSettingsButton = new Krypton.Toolkit.KryptonButton();
			outputAgentComboBox = new Krypton.Toolkit.KryptonComboBox();
			outputAgentLabel = new Krypton.Toolkit.KryptonLabel();
			channelsGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			channels48_63Button = new Krypton.Toolkit.KryptonButton();
			channels32_47Button = new Krypton.Toolkit.KryptonButton();
			channels0_15Button = new Krypton.Toolkit.KryptonButton();
			channels16_31Button = new Krypton.Toolkit.KryptonButton();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)generalGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)generalGroupBox.Panel).BeginInit();
			generalGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)outputGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)outputGroupBox.Panel).BeginInit();
			outputGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)outputAgentComboBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)channelsGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)channelsGroupBox.Panel).BeginInit();
			channelsGroupBox.Panel.SuspendLayout();
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
			generalGroupBox.Panel.Controls.Add(stereoSeparationPercentLabel);
			generalGroupBox.Panel.Controls.Add(stereoSeparationTrackBar);
			generalGroupBox.Panel.Controls.Add(stereoSeparationLabel);
			generalGroupBox.Panel.Controls.Add(visualsLatencyLabel);
			generalGroupBox.Panel.Controls.Add(visualsLatencyTrackBar);
			generalGroupBox.Panel.Controls.Add(visualsLatencyMsLabel);
			generalGroupBox.Panel.Controls.Add(amigaFilterCheckBox);
			generalGroupBox.Panel.Controls.Add(interpolationCheckBox);
			generalGroupBox.Panel.Controls.Add(swapSpeakersCheckBox);
			generalGroupBox.Panel.Controls.Add(surroundCheckBox);
			controlResource.SetResourceKey(generalGroupBox, "IDS_SETTINGS_MIXER_GENERAL");
			generalGroupBox.Size = new System.Drawing.Size(592, 135);
			generalGroupBox.TabIndex = 0;
			generalGroupBox.Values.Heading = "General";
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// stereoSeparationPercentLabel
			// 
			stereoSeparationPercentLabel.Location = new System.Drawing.Point(544, 5);
			stereoSeparationPercentLabel.Name = "stereoSeparationPercentLabel";
			stereoSeparationPercentLabel.Palette = fontPalette;
			stereoSeparationPercentLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(stereoSeparationPercentLabel, null);
			stereoSeparationPercentLabel.Size = new System.Drawing.Size(38, 16);
			stereoSeparationPercentLabel.TabIndex = 2;
			stereoSeparationPercentLabel.Values.Text = "100%";
			// 
			// stereoSeparationTrackBar
			// 
			stereoSeparationTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			stereoSeparationTrackBar.Location = new System.Drawing.Point(106, 0);
			stereoSeparationTrackBar.Maximum = 100;
			stereoSeparationTrackBar.Name = "stereoSeparationTrackBar";
			controlResource.SetResourceKey(stereoSeparationTrackBar, null);
			stereoSeparationTrackBar.Size = new System.Drawing.Size(434, 27);
			stereoSeparationTrackBar.TabIndex = 1;
			stereoSeparationTrackBar.TickFrequency = 10;
			stereoSeparationTrackBar.ValueChanged += StereoSeparationTrackBar_ValueChanged;
			// 
			// stereoSeparationLabel
			// 
			stereoSeparationLabel.Location = new System.Drawing.Point(4, 5);
			stereoSeparationLabel.Name = "stereoSeparationLabel";
			stereoSeparationLabel.Palette = fontPalette;
			stereoSeparationLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(stereoSeparationLabel, "IDS_SETTINGS_MIXER_GENERAL_STEREOSEPARATION");
			stereoSeparationLabel.Size = new System.Drawing.Size(97, 16);
			stereoSeparationLabel.TabIndex = 0;
			stereoSeparationLabel.Values.Text = "Stereo separation";
			// 
			// visualsLatencyLabel
			// 
			visualsLatencyLabel.Location = new System.Drawing.Point(4, 40);
			visualsLatencyLabel.Name = "visualsLatencyLabel";
			visualsLatencyLabel.Palette = fontPalette;
			visualsLatencyLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(visualsLatencyLabel, "IDS_SETTINGS_MIXER_GENERAL_VISUALSLATENCY");
			visualsLatencyLabel.Size = new System.Drawing.Size(83, 16);
			visualsLatencyLabel.TabIndex = 3;
			visualsLatencyLabel.Values.Text = "Visuals latency";
			// 
			// visualsLatencyTrackBar
			// 
			visualsLatencyTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			visualsLatencyTrackBar.Location = new System.Drawing.Point(106, 35);
			visualsLatencyTrackBar.Maximum = 49;
			visualsLatencyTrackBar.Name = "visualsLatencyTrackBar";
			controlResource.SetResourceKey(visualsLatencyTrackBar, null);
			visualsLatencyTrackBar.Size = new System.Drawing.Size(434, 27);
			visualsLatencyTrackBar.TabIndex = 4;
			visualsLatencyTrackBar.TickFrequency = 2;
			visualsLatencyTrackBar.ValueChanged += VisualsLatencyTrackBar_ValueChanged;
			// 
			// visualsLatencyMsLabel
			// 
			visualsLatencyMsLabel.Location = new System.Drawing.Point(544, 40);
			visualsLatencyMsLabel.Name = "visualsLatencyMsLabel";
			visualsLatencyMsLabel.Palette = fontPalette;
			visualsLatencyMsLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(visualsLatencyMsLabel, null);
			visualsLatencyMsLabel.Size = new System.Drawing.Size(34, 16);
			visualsLatencyMsLabel.TabIndex = 5;
			visualsLatencyMsLabel.Values.Text = "0 ms";
			// 
			// amigaFilterCheckBox
			// 
			amigaFilterCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			amigaFilterCheckBox.Location = new System.Drawing.Point(438, 70);
			amigaFilterCheckBox.Name = "amigaFilterCheckBox";
			amigaFilterCheckBox.Palette = fontPalette;
			amigaFilterCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(amigaFilterCheckBox, "IDS_SETTINGS_MIXER_GENERAL_AMIGALED");
			amigaFilterCheckBox.Size = new System.Drawing.Size(146, 16);
			amigaFilterCheckBox.TabIndex = 9;
			amigaFilterCheckBox.Values.Text = "Emulate Amiga LED filter";
			amigaFilterCheckBox.CheckedChanged += AmigaFilterCheckBox_CheckedChanged;
			// 
			// interpolationCheckBox
			// 
			interpolationCheckBox.Location = new System.Drawing.Point(4, 70);
			interpolationCheckBox.Name = "interpolationCheckBox";
			interpolationCheckBox.Palette = fontPalette;
			interpolationCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(interpolationCheckBox, "IDS_SETTINGS_MIXER_GENERAL_INTERPOLATION");
			interpolationCheckBox.Size = new System.Drawing.Size(84, 16);
			interpolationCheckBox.TabIndex = 6;
			interpolationCheckBox.Values.Text = "Interpolation";
			interpolationCheckBox.CheckedChanged += InterpolationCheckBox_CheckedChanged;
			// 
			// swapSpeakersCheckBox
			// 
			swapSpeakersCheckBox.Location = new System.Drawing.Point(4, 91);
			swapSpeakersCheckBox.Name = "swapSpeakersCheckBox";
			swapSpeakersCheckBox.Palette = fontPalette;
			swapSpeakersCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(swapSpeakersCheckBox, "IDS_SETTINGS_MIXER_GENERAL_SWAPSPEAKERS");
			swapSpeakersCheckBox.Size = new System.Drawing.Size(161, 16);
			swapSpeakersCheckBox.TabIndex = 7;
			swapSpeakersCheckBox.Values.Text = "Swap left and right speakers";
			swapSpeakersCheckBox.CheckedChanged += SwapSpeakersCheckBox_CheckedChanged;
			// 
			// surroundCheckBox
			// 
			surroundCheckBox.Location = new System.Drawing.Point(200, 70);
			surroundCheckBox.Name = "surroundCheckBox";
			surroundCheckBox.Palette = fontPalette;
			surroundCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(surroundCheckBox, "IDS_SETTINGS_MIXER_GENERAL_SURROUND");
			surroundCheckBox.Size = new System.Drawing.Size(147, 16);
			surroundCheckBox.TabIndex = 8;
			surroundCheckBox.Values.Text = "Dolby Pro Logic surround";
			surroundCheckBox.CheckedChanged += SurroundCheckBox_CheckedChanged;
			// 
			// outputGroupBox
			// 
			outputGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			outputGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			outputGroupBox.Location = new System.Drawing.Point(8, 143);
			outputGroupBox.Name = "outputGroupBox";
			outputGroupBox.Palette = fontPalette;
			outputGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			outputGroupBox.Panel.Controls.Add(outputAgentSettingsButton);
			outputGroupBox.Panel.Controls.Add(outputAgentComboBox);
			outputGroupBox.Panel.Controls.Add(outputAgentLabel);
			controlResource.SetResourceKey(outputGroupBox, "IDS_SETTINGS_MIXER_OUPUT");
			outputGroupBox.Size = new System.Drawing.Size(592, 50);
			outputGroupBox.TabIndex = 1;
			outputGroupBox.Values.Heading = "Mixer output";
			// 
			// outputAgentSettingsButton
			// 
			outputAgentSettingsButton.Enabled = false;
			outputAgentSettingsButton.Location = new System.Drawing.Point(230, 0);
			outputAgentSettingsButton.Name = "outputAgentSettingsButton";
			outputAgentSettingsButton.Palette = fontPalette;
			outputAgentSettingsButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(outputAgentSettingsButton, "IDS_SETTINGS_MIXER_OUPUT_SETTINGS");
			outputAgentSettingsButton.Size = new System.Drawing.Size(60, 22);
			outputAgentSettingsButton.TabIndex = 2;
			outputAgentSettingsButton.Values.Text = "Settings";
			outputAgentSettingsButton.Click += OutputAgentSettingsButton_Click;
			// 
			// outputAgentComboBox
			// 
			outputAgentComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			outputAgentComboBox.DropDownWidth = 120;
			outputAgentComboBox.IntegralHeight = false;
			outputAgentComboBox.Location = new System.Drawing.Point(106, 2);
			outputAgentComboBox.Name = "outputAgentComboBox";
			outputAgentComboBox.Palette = fontPalette;
			outputAgentComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(outputAgentComboBox, null);
			outputAgentComboBox.Size = new System.Drawing.Size(120, 19);
			outputAgentComboBox.Sorted = true;
			outputAgentComboBox.TabIndex = 1;
			outputAgentComboBox.SelectedIndexChanged += OutputAgentComboBox_SelectedIndexChanged;
			// 
			// outputAgentLabel
			// 
			outputAgentLabel.Location = new System.Drawing.Point(4, 3);
			outputAgentLabel.Name = "outputAgentLabel";
			outputAgentLabel.Palette = fontPalette;
			outputAgentLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(outputAgentLabel, "IDS_SETTINGS_MIXER_OUPUT_AGENT");
			outputAgentLabel.Size = new System.Drawing.Size(73, 16);
			outputAgentLabel.TabIndex = 0;
			outputAgentLabel.Values.Text = "Output agent";
			// 
			// channelsGroupBox
			// 
			channelsGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			channelsGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			channelsGroupBox.Location = new System.Drawing.Point(8, 207);
			channelsGroupBox.Name = "channelsGroupBox";
			channelsGroupBox.Palette = fontPalette;
			channelsGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			channelsGroupBox.Panel.Controls.Add(channels48_63Button);
			channelsGroupBox.Panel.Controls.Add(channels32_47Button);
			channelsGroupBox.Panel.Controls.Add(channels0_15Button);
			channelsGroupBox.Panel.Controls.Add(channels16_31Button);
			controlResource.SetResourceKey(channelsGroupBox, "IDS_SETTINGS_MIXER_CHANNELS");
			channelsGroupBox.Size = new System.Drawing.Size(592, 141);
			channelsGroupBox.TabIndex = 2;
			channelsGroupBox.Values.Heading = "Channels";
			// 
			// channels48_63Button
			// 
			channels48_63Button.Location = new System.Drawing.Point(8, 87);
			channels48_63Button.Name = "channels48_63Button";
			channels48_63Button.Palette = fontPalette;
			channels48_63Button.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(channels48_63Button, "IDS_SETTINGS_MIXER_CHANNELS_48_63");
			channels48_63Button.Size = new System.Drawing.Size(70, 25);
			channels48_63Button.TabIndex = 3;
			channels48_63Button.Values.Text = "48 - 63";
			channels48_63Button.Click += Channels48_63Button_Click;
			// 
			// channels32_47Button
			// 
			channels32_47Button.Location = new System.Drawing.Point(8, 58);
			channels32_47Button.Name = "channels32_47Button";
			channels32_47Button.Palette = fontPalette;
			channels32_47Button.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(channels32_47Button, "IDS_SETTINGS_MIXER_CHANNELS_32_47");
			channels32_47Button.Size = new System.Drawing.Size(70, 25);
			channels32_47Button.TabIndex = 2;
			channels32_47Button.Values.Text = "32 - 47";
			channels32_47Button.Click += Channels32_47Button_Click;
			// 
			// channels0_15Button
			// 
			channels0_15Button.Location = new System.Drawing.Point(8, 0);
			channels0_15Button.Name = "channels0_15Button";
			channels0_15Button.Palette = fontPalette;
			channels0_15Button.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(channels0_15Button, "IDS_SETTINGS_MIXER_CHANNELS_0_15");
			channels0_15Button.Size = new System.Drawing.Size(70, 25);
			channels0_15Button.TabIndex = 0;
			channels0_15Button.Values.Text = "0 - 15";
			channels0_15Button.Click += Channels0_15Button_Click;
			// 
			// channels16_31Button
			// 
			channels16_31Button.Location = new System.Drawing.Point(8, 29);
			channels16_31Button.Name = "channels16_31Button";
			channels16_31Button.Palette = fontPalette;
			channels16_31Button.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(channels16_31Button, "IDS_SETTINGS_MIXER_CHANNELS_16_31");
			channels16_31Button.Size = new System.Drawing.Size(70, 25);
			channels16_31Button.TabIndex = 1;
			channels16_31Button.Values.Text = "16 - 31";
			channels16_31Button.Click += Channels16_31Button_Click;
			// 
			// MixerPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(channelsGroupBox);
			Controls.Add(outputGroupBox);
			Controls.Add(generalGroupBox);
			Name = "MixerPageControl";
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(608, 356);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			((System.ComponentModel.ISupportInitialize)generalGroupBox.Panel).EndInit();
			generalGroupBox.Panel.ResumeLayout(false);
			generalGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)generalGroupBox).EndInit();
			((System.ComponentModel.ISupportInitialize)outputGroupBox.Panel).EndInit();
			outputGroupBox.Panel.ResumeLayout(false);
			outputGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)outputGroupBox).EndInit();
			((System.ComponentModel.ISupportInitialize)outputAgentComboBox).EndInit();
			((System.ComponentModel.ISupportInitialize)channelsGroupBox.Panel).EndInit();
			channelsGroupBox.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)channelsGroupBox).EndInit();
			ResumeLayout(false);

		}

		#endregion
		private Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonGroupBox generalGroupBox;
		private Krypton.Toolkit.KryptonLabel stereoSeparationLabel;
		private Krypton.Toolkit.KryptonTrackBar stereoSeparationTrackBar;
		private Krypton.Toolkit.KryptonLabel stereoSeparationPercentLabel;
		private Krypton.Toolkit.KryptonCheckBox interpolationCheckBox;
		private Krypton.Toolkit.KryptonCheckBox amigaFilterCheckBox;
		private Krypton.Toolkit.KryptonGroupBox outputGroupBox;
		private Krypton.Toolkit.KryptonLabel outputAgentLabel;
		private Krypton.Toolkit.KryptonComboBox outputAgentComboBox;
		private Krypton.Toolkit.KryptonButton outputAgentSettingsButton;
		private Krypton.Toolkit.KryptonGroupBox channelsGroupBox;
		private Krypton.Toolkit.KryptonButton channels16_31Button;
		private Krypton.Toolkit.KryptonButton channels0_15Button;
		private Krypton.Toolkit.KryptonButton channels32_47Button;
		private Krypton.Toolkit.KryptonButton channels48_63Button;
		private Krypton.Toolkit.KryptonCheckBox swapSpeakersCheckBox;
		private Krypton.Toolkit.KryptonCheckBox surroundCheckBox;
		private GuiKit.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonLabel visualsLatencyLabel;
		private Krypton.Toolkit.KryptonTrackBar visualsLatencyTrackBar;
		private Krypton.Toolkit.KryptonLabel visualsLatencyMsLabel;
	}
}
