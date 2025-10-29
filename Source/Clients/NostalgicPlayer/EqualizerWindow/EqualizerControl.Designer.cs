namespace Polycode.NostalgicPlayer.Client.GuiPlayer.EqualizerWindow
{
	partial class EqualizerControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private Krypton.Toolkit.KryptonTrackBar preAmpTrackBar;
		private Krypton.Toolkit.KryptonLabel preAmpLabel;
		private Krypton.Toolkit.KryptonLabel preAmpValueLabel;

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
            equalizerGroupBox = new Krypton.Toolkit.KryptonGroupBox();
            presetContextMenu = new Krypton.Toolkit.KryptonContextMenu();
            fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
            controlsPanel = new System.Windows.Forms.Panel();
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
            preAmpTrackBar = new Krypton.Toolkit.KryptonTrackBar();
            label0 = new Krypton.Toolkit.KryptonLabel();
            preAmpLabel = new Krypton.Toolkit.KryptonLabel();
            preAmpValueLabel = new Krypton.Toolkit.KryptonLabel();
            label1 = new Krypton.Toolkit.KryptonLabel();
            label2 = new Krypton.Toolkit.KryptonLabel();
            label3 = new Krypton.Toolkit.KryptonLabel();
            label4 = new Krypton.Toolkit.KryptonLabel();
            label5 = new Krypton.Toolkit.KryptonLabel();
            label6 = new Krypton.Toolkit.KryptonLabel();
            label7 = new Krypton.Toolkit.KryptonLabel();
            label8 = new Krypton.Toolkit.KryptonLabel();
            label9 = new Krypton.Toolkit.KryptonLabel();
            valueLabel0 = new Krypton.Toolkit.KryptonLabel();
            valueLabel1 = new Krypton.Toolkit.KryptonLabel();
            valueLabel2 = new Krypton.Toolkit.KryptonLabel();
            valueLabel3 = new Krypton.Toolkit.KryptonLabel();
            valueLabel4 = new Krypton.Toolkit.KryptonLabel();
            valueLabel5 = new Krypton.Toolkit.KryptonLabel();
            valueLabel6 = new Krypton.Toolkit.KryptonLabel();
            valueLabel7 = new Krypton.Toolkit.KryptonLabel();
            valueLabel8 = new Krypton.Toolkit.KryptonLabel();
            valueLabel9 = new Krypton.Toolkit.KryptonLabel();
            dbPlusLabel = new Krypton.Toolkit.KryptonLabel();
            dbZeroLabel = new Krypton.Toolkit.KryptonLabel();
            dbMinusLabel = new Krypton.Toolkit.KryptonLabel();
            topPanel = new System.Windows.Forms.Panel();
            enableEqualizerCheckBox = new Krypton.Toolkit.KryptonCheckBox();
            presetComboBox = new Krypton.Toolkit.KryptonComboBox();
            ((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)equalizerGroupBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)equalizerGroupBox.Panel).BeginInit();
            equalizerGroupBox.Panel.SuspendLayout();
            controlsPanel.SuspendLayout();
            topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)presetComboBox).BeginInit();
            SuspendLayout();
            // 
            // controlResource
            // 
            controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
            // 
            // equalizerGroupBox
            // 
            equalizerGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            equalizerGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            equalizerGroupBox.KryptonContextMenu = presetContextMenu;
            equalizerGroupBox.Location = new System.Drawing.Point(0, 0);
            equalizerGroupBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            equalizerGroupBox.Name = "equalizerGroupBox";
            equalizerGroupBox.Palette = fontPalette;
            equalizerGroupBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            // 
            // 
            // 
            equalizerGroupBox.Panel.Controls.Add(controlsPanel);
            equalizerGroupBox.Panel.Controls.Add(topPanel);
            controlResource.SetResourceKey(equalizerGroupBox, "IDS_SETTINGS_MIXER_EQUALIZER_FREQUENCY_BANDS");
            equalizerGroupBox.Size = new System.Drawing.Size(487, 218);
            equalizerGroupBox.StateCommon.Back.Color1 = System.Drawing.Color.White;
            equalizerGroupBox.StateCommon.Back.Color2 = System.Drawing.Color.White;
            equalizerGroupBox.TabIndex = 0;
            equalizerGroupBox.Values.Heading = "Frequency Bands";
            // 
            // fontPalette
            // 
            fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
            fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
            fontPalette.ThemeName = "";
            fontPalette.UseKryptonFileDialogs = true;
            // 
            // controlsPanel
            // 
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
            controlsPanel.Controls.Add(preAmpTrackBar);
            controlsPanel.Controls.Add(label0);
            controlsPanel.Controls.Add(preAmpLabel);
            controlsPanel.Controls.Add(preAmpValueLabel);
            controlsPanel.Controls.Add(label1);
            controlsPanel.Controls.Add(label2);
            controlsPanel.Controls.Add(label3);
            controlsPanel.Controls.Add(label4);
            controlsPanel.Controls.Add(label5);
            controlsPanel.Controls.Add(label6);
            controlsPanel.Controls.Add(label7);
            controlsPanel.Controls.Add(label8);
            controlsPanel.Controls.Add(label9);
            controlsPanel.Controls.Add(valueLabel0);
            controlsPanel.Controls.Add(valueLabel1);
            controlsPanel.Controls.Add(valueLabel2);
            controlsPanel.Controls.Add(valueLabel3);
            controlsPanel.Controls.Add(valueLabel4);
            controlsPanel.Controls.Add(valueLabel5);
            controlsPanel.Controls.Add(valueLabel6);
            controlsPanel.Controls.Add(valueLabel7);
            controlsPanel.Controls.Add(valueLabel8);
            controlsPanel.Controls.Add(valueLabel9);
            controlsPanel.Controls.Add(dbPlusLabel);
            controlsPanel.Controls.Add(dbZeroLabel);
            controlsPanel.Controls.Add(dbMinusLabel);
            controlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            controlsPanel.Location = new System.Drawing.Point(0, 32);
            controlsPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            controlsPanel.Name = "controlsPanel";
            controlsPanel.Size = new System.Drawing.Size(483, 164);
            controlsPanel.TabIndex = 1;
            // 
            // band0TrackBar
            // 
            band0TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band0TrackBar.KryptonContextMenu = presetContextMenu;
            band0TrackBar.Location = new System.Drawing.Point(83, 18);
            band0TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band0TrackBar.Maximum = 120;
            band0TrackBar.Minimum = -120;
            band0TrackBar.Name = "band0TrackBar";
            band0TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band0TrackBar.Palette = fontPalette;
            band0TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band0TrackBar.Size = new System.Drawing.Size(27, 117);
            band0TrackBar.TabIndex = 4;
            band0TrackBar.Tag = 0;
            band0TrackBar.TickFrequency = 30;
            band0TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            // 
            // band1TrackBar
            // 
            band1TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band1TrackBar.KryptonContextMenu = presetContextMenu;
            band1TrackBar.Location = new System.Drawing.Point(124, 18);
            band1TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band1TrackBar.Maximum = 120;
            band1TrackBar.Minimum = -120;
            band1TrackBar.Name = "band1TrackBar";
            band1TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band1TrackBar.Palette = fontPalette;
            band1TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band1TrackBar.Size = new System.Drawing.Size(27, 117);
            band1TrackBar.TabIndex = 5;
            band1TrackBar.Tag = 1;
            band1TrackBar.TickFrequency = 30;
            band1TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            // 
            // band2TrackBar
            // 
            band2TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band2TrackBar.KryptonContextMenu = presetContextMenu;
            band2TrackBar.Location = new System.Drawing.Point(164, 18);
            band2TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band2TrackBar.Maximum = 120;
            band2TrackBar.Minimum = -120;
            band2TrackBar.Name = "band2TrackBar";
            band2TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band2TrackBar.Palette = fontPalette;
            band2TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band2TrackBar.Size = new System.Drawing.Size(27, 117);
            band2TrackBar.TabIndex = 6;
            band2TrackBar.Tag = 2;
            band2TrackBar.TickFrequency = 30;
            band2TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            // 
            // band3TrackBar
            // 
            band3TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band3TrackBar.KryptonContextMenu = presetContextMenu;
            band3TrackBar.Location = new System.Drawing.Point(206, 18);
            band3TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band3TrackBar.Maximum = 120;
            band3TrackBar.Minimum = -120;
            band3TrackBar.Name = "band3TrackBar";
            band3TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band3TrackBar.Palette = fontPalette;
            band3TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band3TrackBar.Size = new System.Drawing.Size(27, 117);
            band3TrackBar.TabIndex = 7;
            band3TrackBar.Tag = 3;
            band3TrackBar.TickFrequency = 30;
            band3TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            // 
            // band4TrackBar
            // 
            band4TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band4TrackBar.KryptonContextMenu = presetContextMenu;
            band4TrackBar.Location = new System.Drawing.Point(246, 18);
            band4TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band4TrackBar.Maximum = 120;
            band4TrackBar.Minimum = -120;
            band4TrackBar.Name = "band4TrackBar";
            band4TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band4TrackBar.Palette = fontPalette;
            band4TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band4TrackBar.Size = new System.Drawing.Size(27, 117);
            band4TrackBar.TabIndex = 8;
            band4TrackBar.Tag = 4;
            band4TrackBar.TickFrequency = 30;
            band4TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            // 
            // band5TrackBar
            // 
            band5TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band5TrackBar.KryptonContextMenu = presetContextMenu;
            band5TrackBar.Location = new System.Drawing.Point(286, 18);
            band5TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band5TrackBar.Maximum = 120;
            band5TrackBar.Minimum = -120;
            band5TrackBar.Name = "band5TrackBar";
            band5TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band5TrackBar.Palette = fontPalette;
            band5TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band5TrackBar.Size = new System.Drawing.Size(27, 117);
            band5TrackBar.TabIndex = 9;
            band5TrackBar.Tag = 5;
            band5TrackBar.TickFrequency = 30;
            band5TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            // 
            // band6TrackBar
            // 
            band6TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band6TrackBar.KryptonContextMenu = presetContextMenu;
            band6TrackBar.Location = new System.Drawing.Point(326, 18);
            band6TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band6TrackBar.Maximum = 120;
            band6TrackBar.Minimum = -120;
            band6TrackBar.Name = "band6TrackBar";
            band6TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band6TrackBar.Palette = fontPalette;
            band6TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band6TrackBar.Size = new System.Drawing.Size(27, 117);
            band6TrackBar.TabIndex = 10;
            band6TrackBar.Tag = 6;
            band6TrackBar.TickFrequency = 30;
            band6TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            // 
            // band7TrackBar
            // 
            band7TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band7TrackBar.KryptonContextMenu = presetContextMenu;
            band7TrackBar.Location = new System.Drawing.Point(366, 18);
            band7TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band7TrackBar.Maximum = 120;
            band7TrackBar.Minimum = -120;
            band7TrackBar.Name = "band7TrackBar";
            band7TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band7TrackBar.Palette = fontPalette;
            band7TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band7TrackBar.Size = new System.Drawing.Size(27, 117);
            band7TrackBar.TabIndex = 11;
            band7TrackBar.Tag = 7;
            band7TrackBar.TickFrequency = 30;
            band7TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            // 
            // band8TrackBar
            // 
            band8TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band8TrackBar.KryptonContextMenu = presetContextMenu;
            band8TrackBar.Location = new System.Drawing.Point(408, 18);
            band8TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band8TrackBar.Maximum = 120;
            band8TrackBar.Minimum = -120;
            band8TrackBar.Name = "band8TrackBar";
            band8TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band8TrackBar.Palette = fontPalette;
            band8TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band8TrackBar.Size = new System.Drawing.Size(27, 117);
            band8TrackBar.TabIndex = 12;
            band8TrackBar.Tag = 8;
            band8TrackBar.TickFrequency = 30;
            band8TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            // 
            // band9TrackBar
            // 
            band9TrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            band9TrackBar.KryptonContextMenu = presetContextMenu;
            band9TrackBar.Location = new System.Drawing.Point(448, 18);
            band9TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            band9TrackBar.Maximum = 120;
            band9TrackBar.Minimum = -120;
            band9TrackBar.Name = "band9TrackBar";
            band9TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            band9TrackBar.Palette = fontPalette;
            band9TrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            band9TrackBar.Size = new System.Drawing.Size(27, 117);
            band9TrackBar.TabIndex = 13;
            band9TrackBar.Tag = 9;
            band9TrackBar.TickFrequency = 30;
            band9TrackBar.ValueChanged += BandTrackBar_ValueChanged;
            //
            // preAmpTrackBar
            //
            preAmpTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
            preAmpTrackBar.Location = new System.Drawing.Point(10, 18);
            preAmpTrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            preAmpTrackBar.Maximum = 120;
            preAmpTrackBar.Minimum = -120;
            preAmpTrackBar.Name = "preAmpTrackBar";
            preAmpTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            preAmpTrackBar.Palette = fontPalette;
            preAmpTrackBar.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            preAmpTrackBar.Size = new System.Drawing.Size(27, 117);
            preAmpTrackBar.TabIndex = 37;
            preAmpTrackBar.TickFrequency = 30;
            preAmpTrackBar.ValueChanged += PreAmpTrackBar_ValueChanged;
            //
            // label0
            // 
            label0.Location = new System.Drawing.Point(80, 140);
            label0.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label0.Name = "label0";
            label0.Palette = fontPalette;
            label0.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label0.Size = new System.Drawing.Size(22, 16);
            label0.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label0.TabIndex = 14;
            label0.Values.Text = "60";
            //
            // preAmpLabel
            //
            preAmpLabel.Location = new System.Drawing.Point(5, 140);
            preAmpLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            preAmpLabel.Name = "preAmpLabel";
            preAmpLabel.Palette = fontPalette;
            preAmpLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            preAmpLabel.Size = new System.Drawing.Size(30, 16);
            preAmpLabel.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            preAmpLabel.TabIndex = 38;
            preAmpLabel.Values.Text = "Pre";
            //
            // preAmpValueLabel
            //
            preAmpValueLabel.Location = new System.Drawing.Point(8, 0);
            preAmpValueLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            preAmpValueLabel.Name = "preAmpValueLabel";
            preAmpValueLabel.Palette = fontPalette;
            preAmpValueLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            preAmpValueLabel.Size = new System.Drawing.Size(25, 16);
            preAmpValueLabel.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            preAmpValueLabel.TabIndex = 39;
            preAmpValueLabel.Values.Text = "0.0";
            //
            // label1
            // 
            label1.Location = new System.Drawing.Point(120, 140);
            label1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label1.Name = "label1";
            label1.Palette = fontPalette;
            label1.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label1.Size = new System.Drawing.Size(28, 16);
            label1.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label1.TabIndex = 15;
            label1.Values.Text = "170";
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(161, 140);
            label2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label2.Name = "label2";
            label2.Palette = fontPalette;
            label2.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label2.Size = new System.Drawing.Size(28, 16);
            label2.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label2.TabIndex = 16;
            label2.Values.Text = "310";
            // 
            // label3
            // 
            label3.Location = new System.Drawing.Point(201, 140);
            label3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label3.Name = "label3";
            label3.Palette = fontPalette;
            label3.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label3.Size = new System.Drawing.Size(28, 16);
            label3.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label3.TabIndex = 17;
            label3.Values.Text = "600";
            // 
            // label4
            // 
            label4.Location = new System.Drawing.Point(242, 140);
            label4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label4.Name = "label4";
            label4.Palette = fontPalette;
            label4.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label4.Size = new System.Drawing.Size(23, 16);
            label4.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label4.TabIndex = 18;
            label4.Values.Text = "1K";
            // 
            // label5
            // 
            label5.Location = new System.Drawing.Point(282, 140);
            label5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label5.Name = "label5";
            label5.Palette = fontPalette;
            label5.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label5.Size = new System.Drawing.Size(23, 16);
            label5.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label5.TabIndex = 19;
            label5.Values.Text = "3K";
            // 
            // label6
            // 
            label6.Location = new System.Drawing.Point(322, 140);
            label6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label6.Name = "label6";
            label6.Palette = fontPalette;
            label6.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label6.Size = new System.Drawing.Size(23, 16);
            label6.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label6.TabIndex = 20;
            label6.Values.Text = "6K";
            // 
            // label7
            // 
            label7.Location = new System.Drawing.Point(363, 140);
            label7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label7.Name = "label7";
            label7.Palette = fontPalette;
            label7.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label7.Size = new System.Drawing.Size(30, 16);
            label7.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label7.TabIndex = 21;
            label7.Values.Text = "12K";
            // 
            // label8
            // 
            label8.Location = new System.Drawing.Point(403, 140);
            label8.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label8.Name = "label8";
            label8.Palette = fontPalette;
            label8.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label8.Size = new System.Drawing.Size(30, 16);
            label8.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label8.TabIndex = 22;
            label8.Values.Text = "14K";
            // 
            // label9
            // 
            label9.Location = new System.Drawing.Point(444, 140);
            label9.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            label9.Name = "label9";
            label9.Palette = fontPalette;
            label9.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            label9.Size = new System.Drawing.Size(30, 16);
            label9.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            label9.TabIndex = 23;
            label9.Values.Text = "16K";
            // 
            // valueLabel0
            // 
            valueLabel0.Location = new System.Drawing.Point(83, 0);
            valueLabel0.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel0.Name = "valueLabel0";
            valueLabel0.Palette = fontPalette;
            valueLabel0.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel0.Size = new System.Drawing.Size(25, 16);
            valueLabel0.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel0.TabIndex = 24;
            valueLabel0.Values.Text = "0.0";
            // 
            // valueLabel1
            // 
            valueLabel1.Location = new System.Drawing.Point(124, 0);
            valueLabel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel1.Name = "valueLabel1";
            valueLabel1.Palette = fontPalette;
            valueLabel1.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel1.Size = new System.Drawing.Size(25, 16);
            valueLabel1.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel1.TabIndex = 25;
            valueLabel1.Values.Text = "0.0";
            // 
            // valueLabel2
            // 
            valueLabel2.Location = new System.Drawing.Point(164, 0);
            valueLabel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel2.Name = "valueLabel2";
            valueLabel2.Palette = fontPalette;
            valueLabel2.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel2.Size = new System.Drawing.Size(25, 16);
            valueLabel2.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel2.TabIndex = 26;
            valueLabel2.Values.Text = "0.0";
            // 
            // valueLabel3
            // 
            valueLabel3.Location = new System.Drawing.Point(206, 0);
            valueLabel3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel3.Name = "valueLabel3";
            valueLabel3.Palette = fontPalette;
            valueLabel3.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel3.Size = new System.Drawing.Size(25, 16);
            valueLabel3.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel3.TabIndex = 27;
            valueLabel3.Values.Text = "0.0";
            // 
            // valueLabel4
            // 
            valueLabel4.Location = new System.Drawing.Point(246, 0);
            valueLabel4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel4.Name = "valueLabel4";
            valueLabel4.Palette = fontPalette;
            valueLabel4.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel4.Size = new System.Drawing.Size(25, 16);
            valueLabel4.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel4.TabIndex = 28;
            valueLabel4.Values.Text = "0.0";
            // 
            // valueLabel5
            // 
            valueLabel5.Location = new System.Drawing.Point(286, 0);
            valueLabel5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel5.Name = "valueLabel5";
            valueLabel5.Palette = fontPalette;
            valueLabel5.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel5.Size = new System.Drawing.Size(25, 16);
            valueLabel5.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel5.TabIndex = 29;
            valueLabel5.Values.Text = "0.0";
            // 
            // valueLabel6
            // 
            valueLabel6.Location = new System.Drawing.Point(326, 0);
            valueLabel6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel6.Name = "valueLabel6";
            valueLabel6.Palette = fontPalette;
            valueLabel6.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel6.Size = new System.Drawing.Size(25, 16);
            valueLabel6.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel6.TabIndex = 30;
            valueLabel6.Values.Text = "0.0";
            // 
            // valueLabel7
            // 
            valueLabel7.Location = new System.Drawing.Point(366, 0);
            valueLabel7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel7.Name = "valueLabel7";
            valueLabel7.Palette = fontPalette;
            valueLabel7.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel7.Size = new System.Drawing.Size(25, 16);
            valueLabel7.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel7.TabIndex = 31;
            valueLabel7.Values.Text = "0.0";
            // 
            // valueLabel8
            // 
            valueLabel8.Location = new System.Drawing.Point(408, 0);
            valueLabel8.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel8.Name = "valueLabel8";
            valueLabel8.Palette = fontPalette;
            valueLabel8.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel8.Size = new System.Drawing.Size(25, 16);
            valueLabel8.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel8.TabIndex = 32;
            valueLabel8.Values.Text = "0.0";
            // 
            // valueLabel9
            // 
            valueLabel9.Location = new System.Drawing.Point(448, 0);
            valueLabel9.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            valueLabel9.Name = "valueLabel9";
            valueLabel9.Palette = fontPalette;
            valueLabel9.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            valueLabel9.Size = new System.Drawing.Size(25, 16);
            valueLabel9.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Center;
            valueLabel9.TabIndex = 33;
            valueLabel9.Values.Text = "0.0";
            // 
            // dbPlusLabel
            // 
            dbPlusLabel.Location = new System.Drawing.Point(49, 18);
            dbPlusLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            dbPlusLabel.Name = "dbPlusLabel";
            dbPlusLabel.Palette = fontPalette;
            dbPlusLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            dbPlusLabel.Size = new System.Drawing.Size(29, 16);
            dbPlusLabel.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Far;
            dbPlusLabel.TabIndex = 34;
            dbPlusLabel.Values.Text = "+12";
            // 
            // dbZeroLabel
            // 
            dbZeroLabel.Location = new System.Drawing.Point(49, 72);
            dbZeroLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            dbZeroLabel.Name = "dbZeroLabel";
            dbZeroLabel.Palette = fontPalette;
            dbZeroLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            dbZeroLabel.Size = new System.Drawing.Size(16, 16);
            dbZeroLabel.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Far;
            dbZeroLabel.TabIndex = 35;
            dbZeroLabel.Values.Text = "0";
            // 
            // dbMinusLabel
            // 
            dbMinusLabel.Location = new System.Drawing.Point(49, 122);
            dbMinusLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            dbMinusLabel.Name = "dbMinusLabel";
            dbMinusLabel.Palette = fontPalette;
            dbMinusLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            dbMinusLabel.Size = new System.Drawing.Size(26, 16);
            dbMinusLabel.StateCommon.ShortText.TextH = Krypton.Toolkit.PaletteRelativeAlign.Far;
            dbMinusLabel.TabIndex = 36;
            dbMinusLabel.Values.Text = "-12";
            // 
            // topPanel
            // 
            topPanel.BackColor = System.Drawing.Color.White;
            topPanel.Controls.Add(enableEqualizerCheckBox);
            topPanel.Controls.Add(presetComboBox);
            topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            topPanel.Location = new System.Drawing.Point(0, 0);
            topPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            topPanel.Name = "topPanel";
            topPanel.Size = new System.Drawing.Size(442, 32);
            topPanel.TabIndex = 0;
            // 
            // enableEqualizerCheckBox
            // 
            enableEqualizerCheckBox.Location = new System.Drawing.Point(3, 3);
            enableEqualizerCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            enableEqualizerCheckBox.Name = "enableEqualizerCheckBox";
            enableEqualizerCheckBox.Palette = fontPalette;
            enableEqualizerCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            controlResource.SetResourceKey(enableEqualizerCheckBox, "IDS_SETTINGS_MIXER_EQUALIZER_ENABLE");
            enableEqualizerCheckBox.Size = new System.Drawing.Size(90, 16);
            enableEqualizerCheckBox.TabIndex = 0;
            enableEqualizerCheckBox.Values.Text = "Use equalizer";
            enableEqualizerCheckBox.CheckedChanged += EnableEqualizerCheckBox_CheckedChanged;
            // 
            // presetComboBox
            // 
            presetComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            presetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            presetComboBox.DropDownWidth = 100;
            presetComboBox.IntegralHeight = false;
            presetComboBox.Location = new System.Drawing.Point(272, 1);
            presetComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            presetComboBox.Name = "presetComboBox";
            presetComboBox.Palette = fontPalette;
            presetComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            presetComboBox.Size = new System.Drawing.Size(167, 19);
            presetComboBox.TabIndex = 1;
            presetComboBox.SelectedIndexChanged += PresetComboBox_SelectedIndexChanged;
            // 
            // EqualizerControl
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            Controls.Add(equalizerGroupBox);
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "EqualizerControl";
            Size = new System.Drawing.Size(487, 218);
            ((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
            ((System.ComponentModel.ISupportInitialize)equalizerGroupBox.Panel).EndInit();
            equalizerGroupBox.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)equalizerGroupBox).EndInit();
            controlsPanel.ResumeLayout(false);
            controlsPanel.PerformLayout();
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)presetComboBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource controlResource;
		private Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonGroupBox equalizerGroupBox;
		private System.Windows.Forms.Panel topPanel;
		private Krypton.Toolkit.KryptonCheckBox enableEqualizerCheckBox;
		private Krypton.Toolkit.KryptonComboBox presetComboBox;
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
		private Krypton.Toolkit.KryptonLabel valueLabel0;
		private Krypton.Toolkit.KryptonLabel valueLabel1;
		private Krypton.Toolkit.KryptonLabel valueLabel2;
		private Krypton.Toolkit.KryptonLabel valueLabel3;
		private Krypton.Toolkit.KryptonLabel valueLabel4;
		private Krypton.Toolkit.KryptonLabel valueLabel5;
		private Krypton.Toolkit.KryptonLabel valueLabel6;
		private Krypton.Toolkit.KryptonLabel valueLabel7;
		private Krypton.Toolkit.KryptonLabel valueLabel8;
		private Krypton.Toolkit.KryptonLabel valueLabel9;
		private Krypton.Toolkit.KryptonLabel dbPlusLabel;
		private Krypton.Toolkit.KryptonLabel dbZeroLabel;
		private Krypton.Toolkit.KryptonLabel dbMinusLabel;
	}
}
