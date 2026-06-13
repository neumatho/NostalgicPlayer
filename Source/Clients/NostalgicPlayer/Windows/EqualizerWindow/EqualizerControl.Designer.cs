namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.EqualizerWindow
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
			equalizerGroupBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicGroupBox();
			presetContextMenu = new Polycode.NostalgicPlayer.Controls.Menus.NostalgicContextMenu(components);
			controlsPanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel();
			band0TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			band1TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			band2TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			band3TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			band4TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			band5TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			band6TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			band7TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			band8TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			band9TrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			preAmpTrackBar = new Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar();
			label0 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			preAmpLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			preAmpValueLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			label1 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			label2 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			label3 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			label4 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			label5 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			label6 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			label7 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			label8 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			label9 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel0 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel1 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel2 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel3 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel4 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel5 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel6 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel7 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel8 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			valueLabel9 = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			dbPlusLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			dbZeroLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			dbMinusLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			topPanel = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel();
			enableEqualizerCheckBox = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicCheckBox();
			presetComboBox = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicComboBox();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			equalizerGroupBox.SuspendLayout();
			controlsPanel.SuspendLayout();
			topPanel.SuspendLayout();
			SuspendLayout();
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// equalizerGroupBox
			// 
			equalizerGroupBox.ContextMenuStrip = presetContextMenu;
			equalizerGroupBox.Controls.Add(controlsPanel);
			equalizerGroupBox.Controls.Add(topPanel);
			equalizerGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			equalizerGroupBox.Location = new System.Drawing.Point(0, 0);
			equalizerGroupBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			equalizerGroupBox.Name = "equalizerGroupBox";
			controlResource.SetResourceKey(equalizerGroupBox, "IDS_EQUALIZER_FREQUENCY_BANDS");
			equalizerGroupBox.Size = new System.Drawing.Size(487, 218);
			equalizerGroupBox.TabIndex = 0;
			equalizerGroupBox.Text = "Frequency Bands";
			// 
			// presetContextMenu
			// 
			presetContextMenu.Name = "presetContextMenu";
			// 
			// controlsPanel
			// 
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
			controlsPanel.Location = new System.Drawing.Point(3, 49);
			controlsPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			controlsPanel.Name = "controlsPanel";
			controlsPanel.Size = new System.Drawing.Size(481, 166);
			controlsPanel.TabIndex = 1;
			// 
			// band0TrackBar
			// 
			band0TrackBar.Location = new System.Drawing.Point(83, 18);
			band0TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band0TrackBar.Maximum = 120;
			band0TrackBar.Minimum = -120;
			band0TrackBar.Name = "band0TrackBar";
			band0TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band0TrackBar.Size = new System.Drawing.Size(27, 117);
			band0TrackBar.TabIndex = 4;
			band0TrackBar.Tag = 0;
			band0TrackBar.TickFrequency = 30;
			band0TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// band1TrackBar
			// 
			band1TrackBar.Location = new System.Drawing.Point(124, 18);
			band1TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band1TrackBar.Maximum = 120;
			band1TrackBar.Minimum = -120;
			band1TrackBar.Name = "band1TrackBar";
			band1TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band1TrackBar.Size = new System.Drawing.Size(27, 117);
			band1TrackBar.TabIndex = 5;
			band1TrackBar.Tag = 1;
			band1TrackBar.TickFrequency = 30;
			band1TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// band2TrackBar
			// 
			band2TrackBar.Location = new System.Drawing.Point(164, 18);
			band2TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band2TrackBar.Maximum = 120;
			band2TrackBar.Minimum = -120;
			band2TrackBar.Name = "band2TrackBar";
			band2TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band2TrackBar.Size = new System.Drawing.Size(27, 117);
			band2TrackBar.TabIndex = 6;
			band2TrackBar.Tag = 2;
			band2TrackBar.TickFrequency = 30;
			band2TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// band3TrackBar
			// 
			band3TrackBar.Location = new System.Drawing.Point(206, 18);
			band3TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band3TrackBar.Maximum = 120;
			band3TrackBar.Minimum = -120;
			band3TrackBar.Name = "band3TrackBar";
			band3TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band3TrackBar.Size = new System.Drawing.Size(27, 117);
			band3TrackBar.TabIndex = 7;
			band3TrackBar.Tag = 3;
			band3TrackBar.TickFrequency = 30;
			band3TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// band4TrackBar
			// 
			band4TrackBar.Location = new System.Drawing.Point(246, 18);
			band4TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band4TrackBar.Maximum = 120;
			band4TrackBar.Minimum = -120;
			band4TrackBar.Name = "band4TrackBar";
			band4TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band4TrackBar.Size = new System.Drawing.Size(27, 117);
			band4TrackBar.TabIndex = 8;
			band4TrackBar.Tag = 4;
			band4TrackBar.TickFrequency = 30;
			band4TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// band5TrackBar
			// 
			band5TrackBar.Location = new System.Drawing.Point(286, 18);
			band5TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band5TrackBar.Maximum = 120;
			band5TrackBar.Minimum = -120;
			band5TrackBar.Name = "band5TrackBar";
			band5TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band5TrackBar.Size = new System.Drawing.Size(27, 117);
			band5TrackBar.TabIndex = 9;
			band5TrackBar.Tag = 5;
			band5TrackBar.TickFrequency = 30;
			band5TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// band6TrackBar
			// 
			band6TrackBar.Location = new System.Drawing.Point(326, 18);
			band6TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band6TrackBar.Maximum = 120;
			band6TrackBar.Minimum = -120;
			band6TrackBar.Name = "band6TrackBar";
			band6TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band6TrackBar.Size = new System.Drawing.Size(27, 117);
			band6TrackBar.TabIndex = 10;
			band6TrackBar.Tag = 6;
			band6TrackBar.TickFrequency = 30;
			band6TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// band7TrackBar
			// 
			band7TrackBar.Location = new System.Drawing.Point(366, 18);
			band7TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band7TrackBar.Maximum = 120;
			band7TrackBar.Minimum = -120;
			band7TrackBar.Name = "band7TrackBar";
			band7TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band7TrackBar.Size = new System.Drawing.Size(27, 117);
			band7TrackBar.TabIndex = 11;
			band7TrackBar.Tag = 7;
			band7TrackBar.TickFrequency = 30;
			band7TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// band8TrackBar
			// 
			band8TrackBar.Location = new System.Drawing.Point(408, 18);
			band8TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band8TrackBar.Maximum = 120;
			band8TrackBar.Minimum = -120;
			band8TrackBar.Name = "band8TrackBar";
			band8TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band8TrackBar.Size = new System.Drawing.Size(27, 117);
			band8TrackBar.TabIndex = 12;
			band8TrackBar.Tag = 8;
			band8TrackBar.TickFrequency = 30;
			band8TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// band9TrackBar
			// 
			band9TrackBar.Location = new System.Drawing.Point(448, 18);
			band9TrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			band9TrackBar.Maximum = 120;
			band9TrackBar.Minimum = -120;
			band9TrackBar.Name = "band9TrackBar";
			band9TrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			band9TrackBar.Size = new System.Drawing.Size(27, 117);
			band9TrackBar.TabIndex = 13;
			band9TrackBar.Tag = 9;
			band9TrackBar.TickFrequency = 30;
			band9TrackBar.ValueChanged += BandTrackBar_ValueChanged;
			// 
			// preAmpTrackBar
			// 
			preAmpTrackBar.Location = new System.Drawing.Point(10, 18);
			preAmpTrackBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			preAmpTrackBar.Maximum = 120;
			preAmpTrackBar.Minimum = -120;
			preAmpTrackBar.Name = "preAmpTrackBar";
			preAmpTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
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
			label0.Size = new System.Drawing.Size(22, 16);
			label0.TabIndex = 14;
			label0.Text = "60";
			label0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// preAmpLabel
			// 
			preAmpLabel.Location = new System.Drawing.Point(5, 140);
			preAmpLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			preAmpLabel.Name = "preAmpLabel";
			controlResource.SetResourceKey(preAmpLabel, "IDS_EQUALIZER_PRE");
			preAmpLabel.Size = new System.Drawing.Size(27, 16);
			preAmpLabel.TabIndex = 38;
			preAmpLabel.Text = "Pre";
			preAmpLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// preAmpValueLabel
			// 
			preAmpValueLabel.Location = new System.Drawing.Point(8, 0);
			preAmpValueLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			preAmpValueLabel.Name = "preAmpValueLabel";
			preAmpValueLabel.Size = new System.Drawing.Size(25, 16);
			preAmpValueLabel.TabIndex = 39;
			preAmpValueLabel.Text = "0.0";
			preAmpValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1
			// 
			label1.Location = new System.Drawing.Point(120, 140);
			label1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(28, 16);
			label1.TabIndex = 15;
			label1.Text = "170";
			label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label2
			// 
			label2.Location = new System.Drawing.Point(161, 140);
			label2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(28, 16);
			label2.TabIndex = 16;
			label2.Text = "310";
			label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label3
			// 
			label3.Location = new System.Drawing.Point(201, 140);
			label3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(28, 16);
			label3.TabIndex = 17;
			label3.Text = "600";
			label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label4
			// 
			label4.Location = new System.Drawing.Point(242, 140);
			label4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(23, 16);
			label4.TabIndex = 18;
			label4.Text = "1K";
			label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label5
			// 
			label5.Location = new System.Drawing.Point(282, 140);
			label5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(23, 16);
			label5.TabIndex = 19;
			label5.Text = "3K";
			label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label6
			// 
			label6.Location = new System.Drawing.Point(322, 140);
			label6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(23, 16);
			label6.TabIndex = 20;
			label6.Text = "6K";
			label6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label7
			// 
			label7.Location = new System.Drawing.Point(363, 140);
			label7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			label7.Name = "label7";
			label7.Size = new System.Drawing.Size(30, 16);
			label7.TabIndex = 21;
			label7.Text = "12K";
			label7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label8
			// 
			label8.Location = new System.Drawing.Point(403, 140);
			label8.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			label8.Name = "label8";
			label8.Size = new System.Drawing.Size(30, 16);
			label8.TabIndex = 22;
			label8.Text = "14K";
			label8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label9
			// 
			label9.Location = new System.Drawing.Point(444, 140);
			label9.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(30, 16);
			label9.TabIndex = 23;
			label9.Text = "16K";
			label9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel0
			// 
			valueLabel0.Location = new System.Drawing.Point(83, 0);
			valueLabel0.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel0.Name = "valueLabel0";
			valueLabel0.Size = new System.Drawing.Size(25, 16);
			valueLabel0.TabIndex = 24;
			valueLabel0.Text = "0.0";
			valueLabel0.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel1
			// 
			valueLabel1.Location = new System.Drawing.Point(124, 0);
			valueLabel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel1.Name = "valueLabel1";
			valueLabel1.Size = new System.Drawing.Size(25, 16);
			valueLabel1.TabIndex = 25;
			valueLabel1.Text = "0.0";
			valueLabel1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel2
			// 
			valueLabel2.Location = new System.Drawing.Point(164, 0);
			valueLabel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel2.Name = "valueLabel2";
			valueLabel2.Size = new System.Drawing.Size(25, 16);
			valueLabel2.TabIndex = 26;
			valueLabel2.Text = "0.0";
			valueLabel2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel3
			// 
			valueLabel3.Location = new System.Drawing.Point(206, 0);
			valueLabel3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel3.Name = "valueLabel3";
			valueLabel3.Size = new System.Drawing.Size(25, 16);
			valueLabel3.TabIndex = 27;
			valueLabel3.Text = "0.0";
			valueLabel3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel4
			// 
			valueLabel4.Location = new System.Drawing.Point(246, 0);
			valueLabel4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel4.Name = "valueLabel4";
			valueLabel4.Size = new System.Drawing.Size(25, 16);
			valueLabel4.TabIndex = 28;
			valueLabel4.Text = "0.0";
			valueLabel4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel5
			// 
			valueLabel5.Location = new System.Drawing.Point(286, 0);
			valueLabel5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel5.Name = "valueLabel5";
			valueLabel5.Size = new System.Drawing.Size(25, 16);
			valueLabel5.TabIndex = 29;
			valueLabel5.Text = "0.0";
			valueLabel5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel6
			// 
			valueLabel6.Location = new System.Drawing.Point(326, 0);
			valueLabel6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel6.Name = "valueLabel6";
			valueLabel6.Size = new System.Drawing.Size(25, 16);
			valueLabel6.TabIndex = 30;
			valueLabel6.Text = "0.0";
			valueLabel6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel7
			// 
			valueLabel7.Location = new System.Drawing.Point(366, 0);
			valueLabel7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel7.Name = "valueLabel7";
			valueLabel7.Size = new System.Drawing.Size(25, 16);
			valueLabel7.TabIndex = 31;
			valueLabel7.Text = "0.0";
			valueLabel7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel8
			// 
			valueLabel8.Location = new System.Drawing.Point(408, 0);
			valueLabel8.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel8.Name = "valueLabel8";
			valueLabel8.Size = new System.Drawing.Size(25, 16);
			valueLabel8.TabIndex = 32;
			valueLabel8.Text = "0.0";
			valueLabel8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// valueLabel9
			// 
			valueLabel9.Location = new System.Drawing.Point(448, 0);
			valueLabel9.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			valueLabel9.Name = "valueLabel9";
			valueLabel9.Size = new System.Drawing.Size(25, 16);
			valueLabel9.TabIndex = 33;
			valueLabel9.Text = "0.0";
			valueLabel9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// dbPlusLabel
			// 
			dbPlusLabel.Location = new System.Drawing.Point(49, 18);
			dbPlusLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			dbPlusLabel.Name = "dbPlusLabel";
			dbPlusLabel.Size = new System.Drawing.Size(29, 16);
			dbPlusLabel.TabIndex = 34;
			dbPlusLabel.Text = "+12";
			// 
			// dbZeroLabel
			// 
			dbZeroLabel.Location = new System.Drawing.Point(49, 70);
			dbZeroLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			dbZeroLabel.Name = "dbZeroLabel";
			dbZeroLabel.Size = new System.Drawing.Size(16, 16);
			dbZeroLabel.TabIndex = 35;
			dbZeroLabel.Text = "0";
			// 
			// dbMinusLabel
			// 
			dbMinusLabel.Location = new System.Drawing.Point(49, 122);
			dbMinusLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			dbMinusLabel.Name = "dbMinusLabel";
			dbMinusLabel.Size = new System.Drawing.Size(26, 16);
			dbMinusLabel.TabIndex = 36;
			dbMinusLabel.Text = "-12";
			// 
			// topPanel
			// 
			topPanel.Controls.Add(enableEqualizerCheckBox);
			topPanel.Controls.Add(presetComboBox);
			topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			topPanel.Location = new System.Drawing.Point(3, 17);
			topPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			topPanel.Name = "topPanel";
			topPanel.Size = new System.Drawing.Size(481, 32);
			topPanel.TabIndex = 0;
			// 
			// enableEqualizerCheckBox
			// 
			enableEqualizerCheckBox.Location = new System.Drawing.Point(3, 3);
			enableEqualizerCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			enableEqualizerCheckBox.Name = "enableEqualizerCheckBox";
			controlResource.SetResourceKey(enableEqualizerCheckBox, "IDS_EQUALIZER_ENABLE");
			enableEqualizerCheckBox.Size = new System.Drawing.Size(90, 16);
			enableEqualizerCheckBox.TabIndex = 0;
			enableEqualizerCheckBox.Text = "Use equalizer";
			enableEqualizerCheckBox.CheckedChanged += EnableEqualizerCheckBox_CheckedChanged;
			// 
			// presetComboBox
			// 
			presetComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			presetComboBox.DropDownWidth = 100;
			presetComboBox.IntegralHeight = false;
			presetComboBox.Location = new System.Drawing.Point(311, 1);
			presetComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			presetComboBox.Name = "presetComboBox";
			presetComboBox.Size = new System.Drawing.Size(167, 21);
			presetComboBox.TabIndex = 1;
			presetComboBox.SelectedIndexChanged += PresetComboBox_SelectedIndexChanged;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// EqualizerControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(equalizerGroupBox);
			Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			Name = "EqualizerControl";
			Size = new System.Drawing.Size(487, 218);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			equalizerGroupBox.ResumeLayout(false);
			controlsPanel.ResumeLayout(false);
			topPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource controlResource;
		private Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette fontPalette;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicGroupBox equalizerGroupBox;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel topPanel;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicCheckBox enableEqualizerCheckBox;
		private Polycode.NostalgicPlayer.Controls.Lists.NostalgicComboBox presetComboBox;
		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicPanel controlsPanel;
		private Polycode.NostalgicPlayer.Controls.Menus.NostalgicContextMenu presetContextMenu;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band0TrackBar;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band1TrackBar;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band2TrackBar;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band3TrackBar;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band4TrackBar;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band5TrackBar;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band6TrackBar;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band7TrackBar;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band8TrackBar;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar band9TrackBar;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label0;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label1;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label2;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label3;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label4;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label5;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label6;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label7;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label8;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel label9;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel0;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel1;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel2;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel3;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel4;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel5;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel6;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel7;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel8;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel valueLabel9;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel dbPlusLabel;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel dbZeroLabel;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel dbMinusLabel;
		private Polycode.NostalgicPlayer.Controls.Sliders.NostalgicTrackBar preAmpTrackBar;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel preAmpLabel;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel preAmpValueLabel;
	}
}
