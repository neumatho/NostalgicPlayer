
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
			this.components = new System.ComponentModel.Container();
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.generalGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(this.components);
			this.stereoSeparationPercentLabel = new Krypton.Toolkit.KryptonLabel();
			this.stereoSeparationTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.stereoSeparationLabel = new Krypton.Toolkit.KryptonLabel();
			this.visualsLatencyLabel = new Krypton.Toolkit.KryptonLabel();
			this.visualsLatencyTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.visualsLatencyMsLabel = new Krypton.Toolkit.KryptonLabel();
			this.amigaFilterCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.interpolationCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.swapSpeakersCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.surroundCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.outputGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.outputAgentSettingsButton = new Krypton.Toolkit.KryptonButton();
			this.outputAgentComboBox = new Krypton.Toolkit.KryptonComboBox();
			this.outputAgentLabel = new Krypton.Toolkit.KryptonLabel();
			this.channelsGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.channels48_63Button = new Krypton.Toolkit.KryptonButton();
			this.channels32_47Button = new Krypton.Toolkit.KryptonButton();
			this.channels0_15Button = new Krypton.Toolkit.KryptonButton();
			this.channels16_31Button = new Krypton.Toolkit.KryptonButton();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox.Panel)).BeginInit();
			this.generalGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.outputGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outputGroupBox.Panel)).BeginInit();
			this.outputGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.outputAgentComboBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.channelsGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.channelsGroupBox.Panel)).BeginInit();
			this.channelsGroupBox.Panel.SuspendLayout();
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
			this.generalGroupBox.Panel.Controls.Add(this.stereoSeparationPercentLabel);
			this.generalGroupBox.Panel.Controls.Add(this.stereoSeparationTrackBar);
			this.generalGroupBox.Panel.Controls.Add(this.stereoSeparationLabel);
			this.generalGroupBox.Panel.Controls.Add(this.visualsLatencyLabel);
			this.generalGroupBox.Panel.Controls.Add(this.visualsLatencyTrackBar);
			this.generalGroupBox.Panel.Controls.Add(this.visualsLatencyMsLabel);
			this.generalGroupBox.Panel.Controls.Add(this.amigaFilterCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.interpolationCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.swapSpeakersCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.surroundCheckBox);
			this.controlResource.SetResourceKey(this.generalGroupBox, "IDS_SETTINGS_MIXER_GENERAL");
			this.generalGroupBox.Size = new System.Drawing.Size(592, 135);
			this.generalGroupBox.TabIndex = 0;
			this.generalGroupBox.Values.Heading = "General";
			// 
			// stereoSeparationPercentLabel
			// 
			this.stereoSeparationPercentLabel.Location = new System.Drawing.Point(544, 5);
			this.stereoSeparationPercentLabel.Name = "stereoSeparationPercentLabel";
			this.stereoSeparationPercentLabel.Palette = this.fontPalette;
			this.stereoSeparationPercentLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.stereoSeparationPercentLabel, null);
			this.stereoSeparationPercentLabel.Size = new System.Drawing.Size(38, 16);
			this.stereoSeparationPercentLabel.TabIndex = 2;
			this.stereoSeparationPercentLabel.Values.Text = "100%";
			// 
			// stereoSeparationTrackBar
			// 
			this.stereoSeparationTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			this.stereoSeparationTrackBar.Location = new System.Drawing.Point(106, 0);
			this.stereoSeparationTrackBar.Maximum = 100;
			this.stereoSeparationTrackBar.Name = "stereoSeparationTrackBar";
			this.controlResource.SetResourceKey(this.stereoSeparationTrackBar, null);
			this.stereoSeparationTrackBar.Size = new System.Drawing.Size(434, 27);
			this.stereoSeparationTrackBar.TabIndex = 1;
			this.stereoSeparationTrackBar.TickFrequency = 10;
			this.stereoSeparationTrackBar.ValueChanged += new System.EventHandler(this.StereoSeparationTrackBar_ValueChanged);
			// 
			// stereoSeparationLabel
			// 
			this.stereoSeparationLabel.Location = new System.Drawing.Point(4, 5);
			this.stereoSeparationLabel.Name = "stereoSeparationLabel";
			this.stereoSeparationLabel.Palette = this.fontPalette;
			this.stereoSeparationLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.stereoSeparationLabel, "IDS_SETTINGS_MIXER_GENERAL_STEREOSEPARATION");
			this.stereoSeparationLabel.Size = new System.Drawing.Size(97, 16);
			this.stereoSeparationLabel.TabIndex = 0;
			this.stereoSeparationLabel.Values.Text = "Stereo separation";
			// 
			// visualsLatencyLabel
			// 
			this.visualsLatencyLabel.Location = new System.Drawing.Point(4, 40);
			this.visualsLatencyLabel.Name = "visualsLatencyLabel";
			this.visualsLatencyLabel.Palette = this.fontPalette;
			this.visualsLatencyLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.visualsLatencyLabel, "IDS_SETTINGS_MIXER_GENERAL_VISUALSLATENCY");
			this.visualsLatencyLabel.Size = new System.Drawing.Size(83, 16);
			this.visualsLatencyLabel.TabIndex = 3;
			this.visualsLatencyLabel.Values.Text = "Visuals latency";
			// 
			// visualsLatencyTrackBar
			// 
			this.visualsLatencyTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			this.visualsLatencyTrackBar.Location = new System.Drawing.Point(106, 35);
			this.visualsLatencyTrackBar.Maximum = 49;
			this.visualsLatencyTrackBar.Name = "visualsLatencyTrackBar";
			this.controlResource.SetResourceKey(this.visualsLatencyTrackBar, null);
			this.visualsLatencyTrackBar.Size = new System.Drawing.Size(434, 27);
			this.visualsLatencyTrackBar.TabIndex = 4;
			this.visualsLatencyTrackBar.TickFrequency = 2;
			this.visualsLatencyTrackBar.ValueChanged += new System.EventHandler(this.VisualsLatencyTrackBar_ValueChanged);
			// 
			// visualsLatencyMsLabel
			// 
			this.visualsLatencyMsLabel.Location = new System.Drawing.Point(544, 40);
			this.visualsLatencyMsLabel.Name = "visualsLatencyMsLabel";
			this.visualsLatencyMsLabel.Palette = this.fontPalette;
			this.visualsLatencyMsLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.visualsLatencyMsLabel, null);
			this.visualsLatencyMsLabel.Size = new System.Drawing.Size(34, 16);
			this.visualsLatencyMsLabel.TabIndex = 5;
			this.visualsLatencyMsLabel.Values.Text = "0 ms";
			// 
			// amigaFilterCheckBox
			// 
			this.amigaFilterCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.amigaFilterCheckBox.Location = new System.Drawing.Point(438, 70);
			this.amigaFilterCheckBox.Name = "amigaFilterCheckBox";
			this.amigaFilterCheckBox.Palette = this.fontPalette;
			this.amigaFilterCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.amigaFilterCheckBox, "IDS_SETTINGS_MIXER_GENERAL_AMIGALED");
			this.amigaFilterCheckBox.Size = new System.Drawing.Size(146, 16);
			this.amigaFilterCheckBox.TabIndex = 9;
			this.amigaFilterCheckBox.Values.Text = "Emulate Amiga LED filter";
			this.amigaFilterCheckBox.CheckedChanged += new System.EventHandler(this.AmigaFilterCheckBox_CheckedChanged);
			// 
			// interpolationCheckBox
			// 
			this.interpolationCheckBox.Location = new System.Drawing.Point(4, 70);
			this.interpolationCheckBox.Name = "interpolationCheckBox";
			this.interpolationCheckBox.Palette = this.fontPalette;
			this.interpolationCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.interpolationCheckBox, "IDS_SETTINGS_MIXER_GENERAL_INTERPOLATION");
			this.interpolationCheckBox.Size = new System.Drawing.Size(84, 16);
			this.interpolationCheckBox.TabIndex = 6;
			this.interpolationCheckBox.Values.Text = "Interpolation";
			this.interpolationCheckBox.CheckedChanged += new System.EventHandler(this.InterpolationCheckBox_CheckedChanged);
			// 
			// swapSpeakersCheckBox
			// 
			this.swapSpeakersCheckBox.Location = new System.Drawing.Point(4, 91);
			this.swapSpeakersCheckBox.Name = "swapSpeakersCheckBox";
			this.swapSpeakersCheckBox.Palette = this.fontPalette;
			this.swapSpeakersCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.swapSpeakersCheckBox, "IDS_SETTINGS_MIXER_GENERAL_SWAPSPEAKERS");
			this.swapSpeakersCheckBox.Size = new System.Drawing.Size(161, 16);
			this.swapSpeakersCheckBox.TabIndex = 7;
			this.swapSpeakersCheckBox.Values.Text = "Swap left and right speakers";
			this.swapSpeakersCheckBox.CheckedChanged += new System.EventHandler(this.SwapSpeakersCheckBox_CheckedChanged);
			// 
			// surroundCheckBox
			// 
			this.surroundCheckBox.Location = new System.Drawing.Point(200, 70);
			this.surroundCheckBox.Name = "surroundCheckBox";
			this.surroundCheckBox.Palette = this.fontPalette;
			this.surroundCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.surroundCheckBox, "IDS_SETTINGS_MIXER_GENERAL_SURROUND");
			this.surroundCheckBox.Size = new System.Drawing.Size(143, 16);
			this.surroundCheckBox.TabIndex = 8;
			this.surroundCheckBox.Values.Text = "Dolby Prologic  surround";
			this.surroundCheckBox.CheckedChanged += new System.EventHandler(this.SurroundCheckBox_CheckedChanged);
			// 
			// outputGroupBox
			// 
			this.outputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.outputGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.outputGroupBox.Location = new System.Drawing.Point(8, 143);
			this.outputGroupBox.Name = "outputGroupBox";
			this.outputGroupBox.Palette = this.fontPalette;
			this.outputGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			this.outputGroupBox.Panel.Controls.Add(this.outputAgentSettingsButton);
			this.outputGroupBox.Panel.Controls.Add(this.outputAgentComboBox);
			this.outputGroupBox.Panel.Controls.Add(this.outputAgentLabel);
			this.controlResource.SetResourceKey(this.outputGroupBox, "IDS_SETTINGS_MIXER_OUPUT");
			this.outputGroupBox.Size = new System.Drawing.Size(592, 50);
			this.outputGroupBox.TabIndex = 1;
			this.outputGroupBox.Values.Heading = "Mixer output";
			// 
			// outputAgentSettingsButton
			// 
			this.outputAgentSettingsButton.Enabled = false;
			this.outputAgentSettingsButton.Location = new System.Drawing.Point(230, 0);
			this.outputAgentSettingsButton.Name = "outputAgentSettingsButton";
			this.outputAgentSettingsButton.Palette = this.fontPalette;
			this.outputAgentSettingsButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.outputAgentSettingsButton, "IDS_SETTINGS_MIXER_OUPUT_SETTINGS");
			this.outputAgentSettingsButton.Size = new System.Drawing.Size(60, 22);
			this.outputAgentSettingsButton.TabIndex = 2;
			this.outputAgentSettingsButton.Values.Text = "Settings";
			this.outputAgentSettingsButton.Click += new System.EventHandler(this.OutputAgentSettingsButton_Click);
			// 
			// outputAgentComboBox
			// 
			this.outputAgentComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.outputAgentComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.outputAgentComboBox.DropDownWidth = 120;
			this.outputAgentComboBox.IntegralHeight = false;
			this.outputAgentComboBox.Location = new System.Drawing.Point(106, 2);
			this.outputAgentComboBox.Name = "outputAgentComboBox";
			this.outputAgentComboBox.Palette = this.fontPalette;
			this.outputAgentComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.outputAgentComboBox, null);
			this.outputAgentComboBox.Size = new System.Drawing.Size(120, 18);
			this.outputAgentComboBox.Sorted = true;
			this.outputAgentComboBox.TabIndex = 1;
			this.outputAgentComboBox.SelectedIndexChanged += new System.EventHandler(this.OutputAgentComboBox_SelectedIndexChanged);
			// 
			// outputAgentLabel
			// 
			this.outputAgentLabel.Location = new System.Drawing.Point(4, 3);
			this.outputAgentLabel.Name = "outputAgentLabel";
			this.outputAgentLabel.Palette = this.fontPalette;
			this.outputAgentLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.outputAgentLabel, "IDS_SETTINGS_MIXER_OUPUT_AGENT");
			this.outputAgentLabel.Size = new System.Drawing.Size(73, 16);
			this.outputAgentLabel.TabIndex = 0;
			this.outputAgentLabel.Values.Text = "Output agent";
			// 
			// channelsGroupBox
			// 
			this.channelsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.channelsGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.channelsGroupBox.Location = new System.Drawing.Point(8, 207);
			this.channelsGroupBox.Name = "channelsGroupBox";
			this.channelsGroupBox.Palette = this.fontPalette;
			this.channelsGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			// 
			// 
			// 
			this.channelsGroupBox.Panel.Controls.Add(this.channels48_63Button);
			this.channelsGroupBox.Panel.Controls.Add(this.channels32_47Button);
			this.channelsGroupBox.Panel.Controls.Add(this.channels0_15Button);
			this.channelsGroupBox.Panel.Controls.Add(this.channels16_31Button);
			this.controlResource.SetResourceKey(this.channelsGroupBox, "IDS_SETTINGS_MIXER_CHANNELS");
			this.channelsGroupBox.Size = new System.Drawing.Size(592, 141);
			this.channelsGroupBox.TabIndex = 2;
			this.channelsGroupBox.Values.Heading = "Channels";
			// 
			// channels48_63Button
			// 
			this.channels48_63Button.Location = new System.Drawing.Point(8, 87);
			this.channels48_63Button.Name = "channels48_63Button";
			this.channels48_63Button.Palette = this.fontPalette;
			this.channels48_63Button.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.channels48_63Button, "IDS_SETTINGS_MIXER_CHANNELS_48_63");
			this.channels48_63Button.Size = new System.Drawing.Size(70, 25);
			this.channels48_63Button.TabIndex = 3;
			this.channels48_63Button.Values.Text = "48 - 63";
			this.channels48_63Button.Click += new System.EventHandler(this.Channels48_63Button_Click);
			// 
			// channels32_47Button
			// 
			this.channels32_47Button.Location = new System.Drawing.Point(8, 58);
			this.channels32_47Button.Name = "channels32_47Button";
			this.channels32_47Button.Palette = this.fontPalette;
			this.channels32_47Button.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.channels32_47Button, "IDS_SETTINGS_MIXER_CHANNELS_32_47");
			this.channels32_47Button.Size = new System.Drawing.Size(70, 25);
			this.channels32_47Button.TabIndex = 2;
			this.channels32_47Button.Values.Text = "32 - 47";
			this.channels32_47Button.Click += new System.EventHandler(this.Channels32_47Button_Click);
			// 
			// channels0_15Button
			// 
			this.channels0_15Button.Location = new System.Drawing.Point(8, 0);
			this.channels0_15Button.Name = "channels0_15Button";
			this.channels0_15Button.Palette = this.fontPalette;
			this.channels0_15Button.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.channels0_15Button, "IDS_SETTINGS_MIXER_CHANNELS_0_15");
			this.channels0_15Button.Size = new System.Drawing.Size(70, 25);
			this.channels0_15Button.TabIndex = 0;
			this.channels0_15Button.Values.Text = "0 - 15";
			this.channels0_15Button.Click += new System.EventHandler(this.Channels0_15Button_Click);
			// 
			// channels16_31Button
			// 
			this.channels16_31Button.Location = new System.Drawing.Point(8, 29);
			this.channels16_31Button.Name = "channels16_31Button";
			this.channels16_31Button.Palette = this.fontPalette;
			this.channels16_31Button.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.channels16_31Button, "IDS_SETTINGS_MIXER_CHANNELS_16_31");
			this.channels16_31Button.Size = new System.Drawing.Size(70, 25);
			this.channels16_31Button.TabIndex = 1;
			this.channels16_31Button.Values.Text = "16 - 31";
			this.channels16_31Button.Click += new System.EventHandler(this.Channels16_31Button_Click);
			// 
			// MixerPageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.channelsGroupBox);
			this.Controls.Add(this.outputGroupBox);
			this.Controls.Add(this.generalGroupBox);
			this.Name = "MixerPageControl";
			this.controlResource.SetResourceKey(this, null);
			this.Size = new System.Drawing.Size(608, 356);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox.Panel)).EndInit();
			this.generalGroupBox.Panel.ResumeLayout(false);
			this.generalGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.generalGroupBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outputGroupBox.Panel)).EndInit();
			this.outputGroupBox.Panel.ResumeLayout(false);
			this.outputGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.outputGroupBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outputAgentComboBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.channelsGroupBox.Panel)).EndInit();
			this.channelsGroupBox.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.channelsGroupBox)).EndInit();
			this.ResumeLayout(false);

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
