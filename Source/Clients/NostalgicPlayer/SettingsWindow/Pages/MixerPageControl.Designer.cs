
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
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.generalGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.stereoSeparationPercentLabel = new Krypton.Toolkit.KryptonLabel();
			this.stereoSeparationTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.stereoSeparationLabel = new Krypton.Toolkit.KryptonLabel();
			this.amigaFilterCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.interpolationCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.outputGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.outputAgentButton = new Krypton.Toolkit.KryptonButton();
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
			this.generalGroupBox.Location = new System.Drawing.Point(8, 4);
			this.generalGroupBox.Name = "generalGroupBox";
			// 
			// 
			// 
			this.generalGroupBox.Panel.Controls.Add(this.stereoSeparationPercentLabel);
			this.generalGroupBox.Panel.Controls.Add(this.stereoSeparationTrackBar);
			this.generalGroupBox.Panel.Controls.Add(this.stereoSeparationLabel);
			this.generalGroupBox.Panel.Controls.Add(this.amigaFilterCheckBox);
			this.generalGroupBox.Panel.Controls.Add(this.interpolationCheckBox);
			this.controlResource.SetResourceKey(this.generalGroupBox, "IDS_SETTINGS_MIXER_GENERAL");
			this.generalGroupBox.Size = new System.Drawing.Size(592, 79);
			this.generalGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.generalGroupBox.TabIndex = 0;
			this.generalGroupBox.Values.Heading = "General";
			// 
			// stereoSeparationPercentLabel
			// 
			this.stereoSeparationPercentLabel.Location = new System.Drawing.Point(544, 5);
			this.stereoSeparationPercentLabel.Name = "stereoSeparationPercentLabel";
			this.controlResource.SetResourceKey(this.stereoSeparationPercentLabel, null);
			this.stereoSeparationPercentLabel.Size = new System.Drawing.Size(40, 17);
			this.stereoSeparationPercentLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.stereoSeparationPercentLabel.TabIndex = 2;
			this.stereoSeparationPercentLabel.Values.Text = "100%";
			// 
			// stereoSeparationTrackBar
			// 
			this.stereoSeparationTrackBar.DrawBackground = true;
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
			this.controlResource.SetResourceKey(this.stereoSeparationLabel, "IDS_SETTINGS_MIXER_GENERAL_STEREOSEPARATION");
			this.stereoSeparationLabel.Size = new System.Drawing.Size(98, 17);
			this.stereoSeparationLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.stereoSeparationLabel.TabIndex = 0;
			this.stereoSeparationLabel.Values.Text = "Stereo separation";
			// 
			// amigaFilterCheckBox
			// 
			this.amigaFilterCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.amigaFilterCheckBox.Location = new System.Drawing.Point(436, 35);
			this.amigaFilterCheckBox.Name = "amigaFilterCheckBox";
			this.controlResource.SetResourceKey(this.amigaFilterCheckBox, "IDS_SETTINGS_MIXER_GENERAL_AMIGALED");
			this.amigaFilterCheckBox.Size = new System.Drawing.Size(148, 17);
			this.amigaFilterCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.amigaFilterCheckBox.TabIndex = 4;
			this.amigaFilterCheckBox.Values.Text = "Emulate Amiga LED filter";
			this.amigaFilterCheckBox.CheckedChanged += new System.EventHandler(this.AmigaFilterCheckBox_CheckedChanged);
			// 
			// interpolationCheckBox
			// 
			this.interpolationCheckBox.Location = new System.Drawing.Point(4, 35);
			this.interpolationCheckBox.Name = "interpolationCheckBox";
			this.controlResource.SetResourceKey(this.interpolationCheckBox, "IDS_SETTINGS_MIXER_GENERAL_INTERPOLATION");
			this.interpolationCheckBox.Size = new System.Drawing.Size(87, 17);
			this.interpolationCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.interpolationCheckBox.TabIndex = 3;
			this.interpolationCheckBox.Values.Text = "Interpolation";
			this.interpolationCheckBox.CheckedChanged += new System.EventHandler(this.InterpolationCheckBox_CheckedChanged);
			// 
			// outputGroupBox
			// 
			this.outputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.outputGroupBox.Location = new System.Drawing.Point(8, 87);
			this.outputGroupBox.Name = "outputGroupBox";
			// 
			// 
			// 
			this.outputGroupBox.Panel.Controls.Add(this.outputAgentButton);
			this.outputGroupBox.Panel.Controls.Add(this.outputAgentComboBox);
			this.outputGroupBox.Panel.Controls.Add(this.outputAgentLabel);
			this.controlResource.SetResourceKey(this.outputGroupBox, "IDS_SETTINGS_MIXER_OUPUT");
			this.outputGroupBox.Size = new System.Drawing.Size(592, 50);
			this.outputGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.outputGroupBox.TabIndex = 1;
			this.outputGroupBox.Values.Heading = "Mixer output";
			// 
			// outputAgentButton
			// 
			this.outputAgentButton.Enabled = false;
			this.outputAgentButton.Location = new System.Drawing.Point(230, 0);
			this.outputAgentButton.Name = "outputAgentButton";
			this.controlResource.SetResourceKey(this.outputAgentButton, "IDS_SETTINGS_MIXER_OUPUT_SETTINGS");
			this.outputAgentButton.Size = new System.Drawing.Size(60, 22);
			this.outputAgentButton.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.outputAgentButton.TabIndex = 2;
			this.outputAgentButton.Values.Text = "Settings";
			// 
			// outputAgentComboBox
			// 
			this.outputAgentComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.outputAgentComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.outputAgentComboBox.DropDownWidth = 120;
			this.outputAgentComboBox.IntegralHeight = false;
			this.outputAgentComboBox.Location = new System.Drawing.Point(106, 1);
			this.outputAgentComboBox.Name = "outputAgentComboBox";
			this.controlResource.SetResourceKey(this.outputAgentComboBox, null);
			this.outputAgentComboBox.Size = new System.Drawing.Size(120, 20);
			this.outputAgentComboBox.Sorted = true;
			this.outputAgentComboBox.StateCommon.ComboBox.Content.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.outputAgentComboBox.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
			this.outputAgentComboBox.StateCommon.Item.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.outputAgentComboBox.TabIndex = 1;
			// 
			// outputAgentLabel
			// 
			this.outputAgentLabel.Location = new System.Drawing.Point(4, 3);
			this.outputAgentLabel.Name = "outputAgentLabel";
			this.controlResource.SetResourceKey(this.outputAgentLabel, "IDS_SETTINGS_MIXER_OUPUT_AGENT");
			this.outputAgentLabel.Size = new System.Drawing.Size(76, 17);
			this.outputAgentLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.outputAgentLabel.TabIndex = 0;
			this.outputAgentLabel.Values.Text = "Output agent";
			// 
			// channelsGroupBox
			// 
			this.channelsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.channelsGroupBox.Location = new System.Drawing.Point(8, 199);
			this.channelsGroupBox.Name = "channelsGroupBox";
			// 
			// 
			// 
			this.channelsGroupBox.Panel.Controls.Add(this.channels48_63Button);
			this.channelsGroupBox.Panel.Controls.Add(this.channels32_47Button);
			this.channelsGroupBox.Panel.Controls.Add(this.channels0_15Button);
			this.channelsGroupBox.Panel.Controls.Add(this.channels16_31Button);
			this.controlResource.SetResourceKey(this.channelsGroupBox, "IDS_SETTINGS_MIXER_CHANNELS");
			this.channelsGroupBox.Size = new System.Drawing.Size(592, 141);
			this.channelsGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.channelsGroupBox.TabIndex = 2;
			this.channelsGroupBox.Values.Heading = "Channels";
			// 
			// channels48_63Button
			// 
			this.channels48_63Button.Location = new System.Drawing.Point(8, 87);
			this.channels48_63Button.Name = "channels48_63Button";
			this.controlResource.SetResourceKey(this.channels48_63Button, "IDS_SETTINGS_MIXER_CHANNELS_48_63");
			this.channels48_63Button.Size = new System.Drawing.Size(70, 25);
			this.channels48_63Button.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.channels48_63Button.TabIndex = 3;
			this.channels48_63Button.Values.Text = "48 - 63";
			this.channels48_63Button.Click += new System.EventHandler(this.Channels48_63Button_Click);
			// 
			// channels32_47Button
			// 
			this.channels32_47Button.Location = new System.Drawing.Point(8, 58);
			this.channels32_47Button.Name = "channels32_47Button";
			this.controlResource.SetResourceKey(this.channels32_47Button, "IDS_SETTINGS_MIXER_CHANNELS_32_47");
			this.channels32_47Button.Size = new System.Drawing.Size(70, 25);
			this.channels32_47Button.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.channels32_47Button.TabIndex = 2;
			this.channels32_47Button.Values.Text = "32 - 47";
			this.channels32_47Button.Click += new System.EventHandler(this.Channels32_47Button_Click);
			// 
			// channels0_15Button
			// 
			this.channels0_15Button.Location = new System.Drawing.Point(8, 0);
			this.channels0_15Button.Name = "channels0_15Button";
			this.controlResource.SetResourceKey(this.channels0_15Button, "IDS_SETTINGS_MIXER_CHANNELS_0_15");
			this.channels0_15Button.Size = new System.Drawing.Size(70, 25);
			this.channels0_15Button.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.channels0_15Button.TabIndex = 0;
			this.channels0_15Button.Values.Text = "0 - 15";
			this.channels0_15Button.Click += new System.EventHandler(this.Channels0_15Button_Click);
			// 
			// channels16_31Button
			// 
			this.channels16_31Button.Location = new System.Drawing.Point(8, 29);
			this.channels16_31Button.Name = "channels16_31Button";
			this.controlResource.SetResourceKey(this.channels16_31Button, "IDS_SETTINGS_MIXER_CHANNELS_16_31");
			this.channels16_31Button.Size = new System.Drawing.Size(70, 25);
			this.channels16_31Button.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
			this.Size = new System.Drawing.Size(608, 348);
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

		private Krypton.Toolkit.KryptonManager kryptonManager;
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
		private Krypton.Toolkit.KryptonButton outputAgentButton;
		private Krypton.Toolkit.KryptonGroupBox channelsGroupBox;
		private Krypton.Toolkit.KryptonButton channels16_31Button;
		private Krypton.Toolkit.KryptonButton channels0_15Button;
		private Krypton.Toolkit.KryptonButton channels32_47Button;
		private Krypton.Toolkit.KryptonButton channels48_63Button;
	}
}
