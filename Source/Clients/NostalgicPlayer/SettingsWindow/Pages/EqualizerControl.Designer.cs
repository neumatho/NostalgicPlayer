namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	partial class EqualizerControl
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
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			equalizerGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			checkBoxPanel = new System.Windows.Forms.Panel();
			enableEqualizerCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			controlsPanel = new System.Windows.Forms.Panel();
			presetContextMenu = new Krypton.Toolkit.KryptonContextMenu();
			band0TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			band1TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			band2TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			band3TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			band4TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			band5TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			band6TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			band7TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			band8TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			band9TrackBar = new Krypton.Toolkit.KryptonTrackBar();
			label0 = new Krypton.Toolkit.KryptonLabel();
			label1 = new Krypton.Toolkit.KryptonLabel();
			label2 = new Krypton.Toolkit.KryptonLabel();
			label3 = new Krypton.Toolkit.KryptonLabel();
			label4 = new Krypton.Toolkit.KryptonLabel();
			label5 = new Krypton.Toolkit.KryptonLabel();
			label6 = new Krypton.Toolkit.KryptonLabel();
			label7 = new Krypton.Toolkit.KryptonLabel();
			label8 = new Krypton.Toolkit.KryptonLabel();
			label9 = new Krypton.Toolkit.KryptonLabel();
			valueLabels = new Krypton.Toolkit.KryptonLabel[10];
			valueLabels[0] = new Krypton.Toolkit.KryptonLabel();
			valueLabels[1] = new Krypton.Toolkit.KryptonLabel();
			valueLabels[2] = new Krypton.Toolkit.KryptonLabel();
			valueLabels[3] = new Krypton.Toolkit.KryptonLabel();
			valueLabels[4] = new Krypton.Toolkit.KryptonLabel();
			valueLabels[5] = new Krypton.Toolkit.KryptonLabel();
			valueLabels[6] = new Krypton.Toolkit.KryptonLabel();
			valueLabels[7] = new Krypton.Toolkit.KryptonLabel();
			valueLabels[8] = new Krypton.Toolkit.KryptonLabel();
			valueLabels[9] = new Krypton.Toolkit.KryptonLabel();
			dbPlusLabel = new Krypton.Toolkit.KryptonLabel();
			dbZeroLabel = new Krypton.Toolkit.KryptonLabel();
			dbMinusLabel = new Krypton.Toolkit.KryptonLabel();

			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)equalizerGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)equalizerGroupBox.Panel).BeginInit();
			equalizerGroupBox.Panel.SuspendLayout();
			SuspendLayout();

			//
			// controlResource
			//
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";

			//
			// fontPalette
			//
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;

			//
			// equalizerGroupBox
			//
			equalizerGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			equalizerGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
			equalizerGroupBox.Location = new System.Drawing.Point(0, 0);
			equalizerGroupBox.Name = "equalizerGroupBox";
			equalizerGroupBox.Palette = fontPalette;
			equalizerGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			equalizerGroupBox.StateCommon.Back.Color1 = System.Drawing.Color.White;
			equalizerGroupBox.StateCommon.Back.Color2 = System.Drawing.Color.White;
			equalizerGroupBox.Panel.Controls.Add(checkBoxPanel);
			equalizerGroupBox.Panel.Controls.Add(controlsPanel);

			//
			// checkBoxPanel
			//
			checkBoxPanel.Dock = System.Windows.Forms.DockStyle.Top;
			checkBoxPanel.Location = new System.Drawing.Point(0, 0);
			checkBoxPanel.Name = "checkBoxPanel";
			checkBoxPanel.Size = new System.Drawing.Size(429, 30);
			checkBoxPanel.TabIndex = 0;
			checkBoxPanel.BackColor = System.Drawing.Color.White;
			checkBoxPanel.Controls.Add(enableEqualizerCheckBox);

			//
			// controlsPanel
			//
			controlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			controlsPanel.Location = new System.Drawing.Point(0, 30);
			controlsPanel.Name = "controlsPanel";
			controlsPanel.Size = new System.Drawing.Size(429, 141);
			controlsPanel.TabIndex = 1;
			controlsPanel.BackColor = System.Drawing.Color.White;

			controlsPanel.Controls.Add(band0TrackBar);
			controlsPanel.Controls.Add(band1TrackBar);
			controlsPanel.Controls.Add(band2TrackBar);
			controlsPanel.Controls.Add(band3TrackBar);
			controlsPanel.Controls.Add(band4TrackBar);
			controlsPanel.Controls.Add(band5TrackBar);
			controlsPanel.Controls.Add(band6TrackBar);
			controlsPanel.Controls.Add(band7TrackBar);
			controlsPanel.Controls.Add(band8TrackBar);
			controlsPanel.Controls.Add(band9TrackBar);
			controlsPanel.Controls.Add(label0);
			controlsPanel.Controls.Add(label1);
			controlsPanel.Controls.Add(label2);
			controlsPanel.Controls.Add(label3);
			controlsPanel.Controls.Add(label4);
			controlsPanel.Controls.Add(label5);
			controlsPanel.Controls.Add(label6);
			controlsPanel.Controls.Add(label7);
			controlsPanel.Controls.Add(label8);
			controlsPanel.Controls.Add(label9);
			controlResource.SetResourceKey(equalizerGroupBox, "IDS_SETTINGS_MIXER_EQUALIZER_FREQUENCY_BANDS");
			equalizerGroupBox.Size = new System.Drawing.Size(429, 192);
			equalizerGroupBox.TabIndex = 0;
			equalizerGroupBox.Values.Heading = "Frequency Bands";

			//
			// enableEqualizerCheckBox
			//
			enableEqualizerCheckBox.Location = new System.Drawing.Point(10, 5);
			enableEqualizerCheckBox.Name = "enableEqualizerCheckBox";
			enableEqualizerCheckBox.Palette = fontPalette;
			enableEqualizerCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(enableEqualizerCheckBox, "IDS_SETTINGS_MIXER_EQUALIZER_ENABLE");
			enableEqualizerCheckBox.Size = new System.Drawing.Size(130, 20);
			enableEqualizerCheckBox.TabIndex = 0;
			enableEqualizerCheckBox.Values.Text = "Enable";
			enableEqualizerCheckBox.CheckedChanged += EnableEqualizerCheckBox_CheckedChanged;

			//
			// presetContextMenu
			//
			equalizerGroupBox.KryptonContextMenu = presetContextMenu;

			//
			// band0TrackBar
			//
			band0TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band0TrackBar.Location = new System.Drawing.Point(38, 14);
			band0TrackBar.Maximum = 120;
			band0TrackBar.Minimum = -120;
			band0TrackBar.Value = 0;
			band0TrackBar.TickFrequency = 30;
			band0TrackBar.Size = new System.Drawing.Size(31, 91);
			band0TrackBar.Name = "band0TrackBar";
			band0TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band0TrackBar.Palette = fontPalette;
			band0TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band0TrackBar.TabIndex = 4;
			band0TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band0TrackBar.Tag = 0;
			band0TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// band1TrackBar
			//
			band1TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band1TrackBar.Location = new System.Drawing.Point(75, 14);
			band1TrackBar.Maximum = 120;
			band1TrackBar.Minimum = -120;
			band1TrackBar.Value = 0;
			band1TrackBar.TickFrequency = 30;
			band1TrackBar.Size = new System.Drawing.Size(31, 91);
			band1TrackBar.Name = "band1TrackBar";
			band1TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band1TrackBar.Palette = fontPalette;
			band1TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band1TrackBar.TabIndex = 5;
			band1TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band1TrackBar.Tag = 1;
			band1TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// band2TrackBar
			//
			band2TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band2TrackBar.Location = new System.Drawing.Point(111, 14);
			band2TrackBar.Maximum = 120;
			band2TrackBar.Minimum = -120;
			band2TrackBar.Value = 0;
			band2TrackBar.TickFrequency = 30;
			band2TrackBar.Size = new System.Drawing.Size(31, 91);
			band2TrackBar.Name = "band2TrackBar";
			band2TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band2TrackBar.Palette = fontPalette;
			band2TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band2TrackBar.TabIndex = 6;
			band2TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band2TrackBar.Tag = 2;
			band2TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// band3TrackBar
			//
			band3TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band3TrackBar.Location = new System.Drawing.Point(148, 14);
			band3TrackBar.Maximum = 120;
			band3TrackBar.Minimum = -120;
			band3TrackBar.Value = 0;
			band3TrackBar.TickFrequency = 30;
			band3TrackBar.Size = new System.Drawing.Size(31, 91);
			band3TrackBar.Name = "band3TrackBar";
			band3TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band3TrackBar.Palette = fontPalette;
			band3TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band3TrackBar.TabIndex = 7;
			band3TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band3TrackBar.Tag = 3;
			band3TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// band4TrackBar
			//
			band4TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band4TrackBar.Location = new System.Drawing.Point(184, 14);
			band4TrackBar.Maximum = 120;
			band4TrackBar.Minimum = -120;
			band4TrackBar.Value = 0;
			band4TrackBar.TickFrequency = 30;
			band4TrackBar.Size = new System.Drawing.Size(31, 91);
			band4TrackBar.Name = "band4TrackBar";
			band4TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band4TrackBar.Palette = fontPalette;
			band4TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band4TrackBar.TabIndex = 8;
			band4TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band4TrackBar.Tag = 4;
			band4TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// band5TrackBar
			//
			band5TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band5TrackBar.Location = new System.Drawing.Point(221, 14);
			band5TrackBar.Maximum = 120;
			band5TrackBar.Minimum = -120;
			band5TrackBar.Value = 0;
			band5TrackBar.TickFrequency = 30;
			band5TrackBar.Size = new System.Drawing.Size(31, 91);
			band5TrackBar.Name = "band5TrackBar";
			band5TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band5TrackBar.Palette = fontPalette;
			band5TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band5TrackBar.TabIndex = 9;
			band5TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band5TrackBar.Tag = 5;
			band5TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// band6TrackBar
			//
			band6TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band6TrackBar.Location = new System.Drawing.Point(257, 14);
			band6TrackBar.Maximum = 120;
			band6TrackBar.Minimum = -120;
			band6TrackBar.Value = 0;
			band6TrackBar.TickFrequency = 30;
			band6TrackBar.Size = new System.Drawing.Size(31, 91);
			band6TrackBar.Name = "band6TrackBar";
			band6TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band6TrackBar.Palette = fontPalette;
			band6TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band6TrackBar.TabIndex = 10;
			band6TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band6TrackBar.Tag = 6;
			band6TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// band7TrackBar
			//
			band7TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band7TrackBar.Location = new System.Drawing.Point(293, 14);
			band7TrackBar.Maximum = 120;
			band7TrackBar.Minimum = -120;
			band7TrackBar.Value = 0;
			band7TrackBar.TickFrequency = 30;
			band7TrackBar.Size = new System.Drawing.Size(31, 91);
			band7TrackBar.Name = "band7TrackBar";
			band7TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band7TrackBar.Palette = fontPalette;
			band7TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band7TrackBar.TabIndex = 11;
			band7TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band7TrackBar.Tag = 7;
			band7TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// band8TrackBar
			//
			band8TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band8TrackBar.Location = new System.Drawing.Point(330, 14);
			band8TrackBar.Maximum = 120;
			band8TrackBar.Minimum = -120;
			band8TrackBar.Value = 0;
			band8TrackBar.TickFrequency = 30;
			band8TrackBar.Size = new System.Drawing.Size(31, 91);
			band8TrackBar.Name = "band8TrackBar";
			band8TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band8TrackBar.Palette = fontPalette;
			band8TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band8TrackBar.TabIndex = 12;
			band8TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band8TrackBar.Tag = 8;
			band8TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// band9TrackBar
			//
			band9TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band9TrackBar.Location = new System.Drawing.Point(366, 14);
			band9TrackBar.Maximum = 120;
			band9TrackBar.Minimum = -120;
			band9TrackBar.Value = 0;
			band9TrackBar.TickFrequency = 30;
			band9TrackBar.Size = new System.Drawing.Size(31, 91);
			band9TrackBar.Name = "band9TrackBar";
			band9TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			band9TrackBar.Palette = fontPalette;
			band9TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			band9TrackBar.TabIndex = 13;
			band9TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			band9TrackBar.Tag = 9;
			band9TrackBar.KryptonContextMenu = presetContextMenu;

			//
			// valueLabels[0]
			//
			valueLabels[0].Location = new System.Drawing.Point(38, 0);
			valueLabels[0].Name = "valueLabel0";
			valueLabels[0].Palette = fontPalette;
			valueLabels[0].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[0].Size = new System.Drawing.Size(31, 16);
			valueLabels[0].TabIndex = 24;
			valueLabels[0].Values.Text = "0.0";
			valueLabels[0].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[0], null);
			controlsPanel.Controls.Add(valueLabels[0]);

			//
			// valueLabels[1]
			//
			valueLabels[1].Location = new System.Drawing.Point(75, 0);
			valueLabels[1].Name = "valueLabel1";
			valueLabels[1].Palette = fontPalette;
			valueLabels[1].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[1].Size = new System.Drawing.Size(31, 16);
			valueLabels[1].TabIndex = 25;
			valueLabels[1].Values.Text = "0.0";
			valueLabels[1].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[1], null);
			controlsPanel.Controls.Add(valueLabels[1]);

			//
			// valueLabels[2]
			//
			valueLabels[2].Location = new System.Drawing.Point(111, 0);
			valueLabels[2].Name = "valueLabel2";
			valueLabels[2].Palette = fontPalette;
			valueLabels[2].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[2].Size = new System.Drawing.Size(31, 16);
			valueLabels[2].TabIndex = 26;
			valueLabels[2].Values.Text = "0.0";
			valueLabels[2].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[2], null);
			controlsPanel.Controls.Add(valueLabels[2]);

			//
			// valueLabels[3]
			//
			valueLabels[3].Location = new System.Drawing.Point(148, 0);
			valueLabels[3].Name = "valueLabel3";
			valueLabels[3].Palette = fontPalette;
			valueLabels[3].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[3].Size = new System.Drawing.Size(31, 16);
			valueLabels[3].TabIndex = 27;
			valueLabels[3].Values.Text = "0.0";
			valueLabels[3].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[3], null);
			controlsPanel.Controls.Add(valueLabels[3]);

			//
			// valueLabels[4]
			//
			valueLabels[4].Location = new System.Drawing.Point(184, 0);
			valueLabels[4].Name = "valueLabel4";
			valueLabels[4].Palette = fontPalette;
			valueLabels[4].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[4].Size = new System.Drawing.Size(31, 16);
			valueLabels[4].TabIndex = 28;
			valueLabels[4].Values.Text = "0.0";
			valueLabels[4].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[4], null);
			controlsPanel.Controls.Add(valueLabels[4]);

			//
			// valueLabels[5]
			//
			valueLabels[5].Location = new System.Drawing.Point(221, 0);
			valueLabels[5].Name = "valueLabel5";
			valueLabels[5].Palette = fontPalette;
			valueLabels[5].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[5].Size = new System.Drawing.Size(31, 16);
			valueLabels[5].TabIndex = 29;
			valueLabels[5].Values.Text = "0.0";
			valueLabels[5].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[5], null);
			controlsPanel.Controls.Add(valueLabels[5]);

			//
			// valueLabels[6]
			//
			valueLabels[6].Location = new System.Drawing.Point(257, 0);
			valueLabels[6].Name = "valueLabel6";
			valueLabels[6].Palette = fontPalette;
			valueLabels[6].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[6].Size = new System.Drawing.Size(31, 16);
			valueLabels[6].TabIndex = 30;
			valueLabels[6].Values.Text = "0.0";
			valueLabels[6].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[6], null);
			controlsPanel.Controls.Add(valueLabels[6]);

			//
			// valueLabels[7]
			//
			valueLabels[7].Location = new System.Drawing.Point(293, 0);
			valueLabels[7].Name = "valueLabel7";
			valueLabels[7].Palette = fontPalette;
			valueLabels[7].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[7].Size = new System.Drawing.Size(31, 16);
			valueLabels[7].TabIndex = 31;
			valueLabels[7].Values.Text = "0.0";
			valueLabels[7].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[7], null);
			controlsPanel.Controls.Add(valueLabels[7]);

			//
			// valueLabels[8]
			//
			valueLabels[8].Location = new System.Drawing.Point(330, 0);
			valueLabels[8].Name = "valueLabel8";
			valueLabels[8].Palette = fontPalette;
			valueLabels[8].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[8].Size = new System.Drawing.Size(31, 16);
			valueLabels[8].TabIndex = 32;
			valueLabels[8].Values.Text = "0.0";
			valueLabels[8].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[8], null);
			controlsPanel.Controls.Add(valueLabels[8]);

			//
			// valueLabels[9]
			//
			valueLabels[9].Location = new System.Drawing.Point(366, 0);
			valueLabels[9].Name = "valueLabel9";
			valueLabels[9].Palette = fontPalette;
			valueLabels[9].PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			valueLabels[9].Size = new System.Drawing.Size(31, 16);
			valueLabels[9].TabIndex = 33;
			valueLabels[9].Values.Text = "0.0";
			valueLabels[9].StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(valueLabels[9], null);
			controlsPanel.Controls.Add(valueLabels[9]);

			//
			// label0
			//
			label0.Location = new System.Drawing.Point(35, 109);
			label0.Name = "label0";
			label0.Palette = fontPalette;
			label0.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label0.Size = new System.Drawing.Size(35, 16);
			label0.TabIndex = 14;
			label0.Values.Text = "60";
			label0.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label0, null);

			//
			// label1
			//
			label1.Location = new System.Drawing.Point(71, 109);
			label1.Name = "label1";
			label1.Palette = fontPalette;
			label1.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label1.Size = new System.Drawing.Size(35, 16);
			label1.TabIndex = 15;
			label1.Values.Text = "170";
			label1.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label1, null);

			//
			// label2
			//
			label2.Location = new System.Drawing.Point(108, 109);
			label2.Name = "label2";
			label2.Palette = fontPalette;
			label2.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label2.Size = new System.Drawing.Size(35, 16);
			label2.TabIndex = 16;
			label2.Values.Text = "310";
			label2.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label2, null);

			//
			// label3
			//
			label3.Location = new System.Drawing.Point(144, 109);
			label3.Name = "label3";
			label3.Palette = fontPalette;
			label3.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label3.Size = new System.Drawing.Size(35, 16);
			label3.TabIndex = 17;
			label3.Values.Text = "600";
			label3.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label3, null);

			//
			// label4
			//
			label4.Location = new System.Drawing.Point(181, 109);
			label4.Name = "label4";
			label4.Palette = fontPalette;
			label4.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label4.Size = new System.Drawing.Size(35, 16);
			label4.TabIndex = 18;
			label4.Values.Text = "1K";
			label4.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label4, null);

			//
			// label5
			//
			label5.Location = new System.Drawing.Point(217, 109);
			label5.Name = "label5";
			label5.Palette = fontPalette;
			label5.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label5.Size = new System.Drawing.Size(35, 16);
			label5.TabIndex = 19;
			label5.Values.Text = "3K";
			label5.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label5, null);

			//
			// label6
			//
			label6.Location = new System.Drawing.Point(253, 109);
			label6.Name = "label6";
			label6.Palette = fontPalette;
			label6.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label6.Size = new System.Drawing.Size(35, 16);
			label6.TabIndex = 20;
			label6.Values.Text = "6K";
			label6.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label6, null);

			//
			// label7
			//
			label7.Location = new System.Drawing.Point(290, 109);
			label7.Name = "label7";
			label7.Palette = fontPalette;
			label7.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label7.Size = new System.Drawing.Size(35, 16);
			label7.TabIndex = 21;
			label7.Values.Text = "12K";
			label7.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label7, null);

			//
			// label8
			//
			label8.Location = new System.Drawing.Point(326, 109);
			label8.Name = "label8";
			label8.Palette = fontPalette;
			label8.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label8.Size = new System.Drawing.Size(35, 16);
			label8.TabIndex = 22;
			label8.Values.Text = "14K";
			label8.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label8, null);

			//
			// label9
			//
			label9.Location = new System.Drawing.Point(363, 109);
			label9.Name = "label9";
			label9.Palette = fontPalette;
			label9.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			label9.Size = new System.Drawing.Size(35, 16);
			label9.TabIndex = 23;
			label9.Values.Text = "16K";
			label9.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
			controlResource.SetResourceKey(label9, null);

			// Add dB scale labels on the left
			dbPlusLabel.Location = new System.Drawing.Point(7, 14);
			dbPlusLabel.Name = "dbPlusLabel";
			dbPlusLabel.Palette = fontPalette;
			dbPlusLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			dbPlusLabel.Size = new System.Drawing.Size(25, 16);
			dbPlusLabel.TabIndex = 34;
			dbPlusLabel.Values.Text = "+12";
			dbPlusLabel.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Far;
			controlResource.SetResourceKey(dbPlusLabel, null);
			controlsPanel.Controls.Add(dbPlusLabel);

			dbZeroLabel.Location = new System.Drawing.Point(7, 56);
			dbZeroLabel.Name = "dbZeroLabel";
			dbZeroLabel.Palette = fontPalette;
			dbZeroLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			dbZeroLabel.Size = new System.Drawing.Size(25, 16);
			dbZeroLabel.TabIndex = 35;
			dbZeroLabel.Values.Text = "0";
			dbZeroLabel.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Far;
			controlResource.SetResourceKey(dbZeroLabel, null);
			controlsPanel.Controls.Add(dbZeroLabel);

			dbMinusLabel.Location = new System.Drawing.Point(7, 95);
			dbMinusLabel.Name = "dbMinusLabel";
			dbMinusLabel.Palette = fontPalette;
			dbMinusLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			dbMinusLabel.Size = new System.Drawing.Size(25, 16);
			dbMinusLabel.TabIndex = 36;
			dbMinusLabel.Values.Text = "-12";
			dbMinusLabel.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Far;
			controlResource.SetResourceKey(dbMinusLabel, null);
			controlsPanel.Controls.Add(dbMinusLabel);

			//
			// EqualizerControl
			//
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(equalizerGroupBox);
			Name = "EqualizerControl";
			Size = new System.Drawing.Size(429, 192);
			controlResource.SetResourceKey(this, null);
			((System.ComponentModel.ISupportInitialize)equalizerGroupBox.Panel).EndInit();
			equalizerGroupBox.Panel.ResumeLayout(false);
			equalizerGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)equalizerGroupBox).EndInit();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource controlResource;
		private Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonGroupBox equalizerGroupBox;
		private System.Windows.Forms.Panel checkBoxPanel;
		private Krypton.Toolkit.KryptonCheckBox enableEqualizerCheckBox;
		private System.Windows.Forms.Panel controlsPanel;
		private Krypton.Toolkit.KryptonContextMenu presetContextMenu;
		private Krypton.Toolkit.KryptonTrackBar band0TrackBar;
		private Krypton.Toolkit.KryptonTrackBar band1TrackBar;
		private Krypton.Toolkit.KryptonTrackBar band2TrackBar;
		private Krypton.Toolkit.KryptonTrackBar band3TrackBar;
		private Krypton.Toolkit.KryptonTrackBar band4TrackBar;
		private Krypton.Toolkit.KryptonTrackBar band5TrackBar;
		private Krypton.Toolkit.KryptonTrackBar band6TrackBar;
		private Krypton.Toolkit.KryptonTrackBar band7TrackBar;
		private Krypton.Toolkit.KryptonTrackBar band8TrackBar;
		private Krypton.Toolkit.KryptonTrackBar band9TrackBar;
		private Krypton.Toolkit.KryptonLabel label0;
		private Krypton.Toolkit.KryptonLabel label1;
		private Krypton.Toolkit.KryptonLabel label2;
		private Krypton.Toolkit.KryptonLabel label3;
		private Krypton.Toolkit.KryptonLabel label4;
		private Krypton.Toolkit.KryptonLabel label5;
		private Krypton.Toolkit.KryptonLabel label6;
		private Krypton.Toolkit.KryptonLabel label7;
		private Krypton.Toolkit.KryptonLabel label8;
		private Krypton.Toolkit.KryptonLabel label9;
		private Krypton.Toolkit.KryptonLabel[] valueLabels;
		private Krypton.Toolkit.KryptonLabel dbPlusLabel;
		private Krypton.Toolkit.KryptonLabel dbZeroLabel;
		private Krypton.Toolkit.KryptonLabel dbMinusLabel;
	}
}
