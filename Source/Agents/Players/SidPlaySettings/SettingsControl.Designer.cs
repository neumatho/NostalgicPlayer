
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlaySettings
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
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.memoryGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.transparentRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.fullBankRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.playSidRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.realC64RadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.clockGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.ntscRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.palRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.clockPanel = new System.Windows.Forms.Panel();
			this.clockAlwaysRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.clockNotKnownRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.modelGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.model8580RadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.model6581RadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.modelPanel = new System.Windows.Forms.Panel();
			this.modelAlwaysRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.modelNotKnownRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.hvscGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.hvscLabel = new Krypton.Toolkit.KryptonLabel();
			this.hvscvPathTextBox = new Krypton.Toolkit.KryptonTextBox();
			this.hvscPathButton = new Krypton.Toolkit.KryptonButton();
			this.songLengthCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.stilCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.bugListCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.filterGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			this.enableFilterCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			this.filterPanel = new System.Windows.Forms.Panel();
			this.filterModelRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.filterCustomRadioButton = new Krypton.Toolkit.KryptonRadioButton();
			this.filterAdjustmentPanel = new System.Windows.Forms.Panel();
			this.filterResetButton = new Krypton.Toolkit.KryptonButton();
			this.filterFtLabel = new Krypton.Toolkit.KryptonLabel();
			this.filterFmLabel = new Krypton.Toolkit.KryptonLabel();
			this.filterFtTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.filterFmTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			this.filterControl = new Polycode.NostalgicPlayer.Agent.Player.SidPlaySettings.FilterControl();
			this.filterFsLabel = new Krypton.Toolkit.KryptonLabel();
			this.filterFsTrackBar = new Krypton.Toolkit.KryptonTrackBar();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.memoryGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.memoryGroupBox.Panel)).BeginInit();
			this.memoryGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.clockGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.clockGroupBox.Panel)).BeginInit();
			this.clockGroupBox.Panel.SuspendLayout();
			this.clockPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.modelGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.modelGroupBox.Panel)).BeginInit();
			this.modelGroupBox.Panel.SuspendLayout();
			this.modelPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.hvscGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.hvscGroupBox.Panel)).BeginInit();
			this.hvscGroupBox.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.filterGroupBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.filterGroupBox.Panel)).BeginInit();
			this.filterGroupBox.Panel.SuspendLayout();
			this.filterPanel.SuspendLayout();
			this.filterAdjustmentPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// kryptonManager
			// 
			this.kryptonManager.GlobalPaletteMode = Krypton.Toolkit.PaletteModeManager.Office2010Blue;
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Agent.Player.SidPlaySettings.Resources";
			// 
			// memoryGroupBox
			// 
			this.memoryGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.memoryGroupBox.Location = new System.Drawing.Point(8, 4);
			this.memoryGroupBox.Name = "memoryGroupBox";
			// 
			// 
			// 
			this.memoryGroupBox.Panel.Controls.Add(this.transparentRadioButton);
			this.memoryGroupBox.Panel.Controls.Add(this.fullBankRadioButton);
			this.memoryGroupBox.Panel.Controls.Add(this.playSidRadioButton);
			this.memoryGroupBox.Panel.Controls.Add(this.realC64RadioButton);
			this.controlResource.SetResourceKey(this.memoryGroupBox, "IDS_SETTINGS_MEMORY");
			this.memoryGroupBox.Size = new System.Drawing.Size(192, 112);
			this.memoryGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.memoryGroupBox.TabIndex = 0;
			this.memoryGroupBox.Values.Heading = "Memory model";
			// 
			// transparentRadioButton
			// 
			this.transparentRadioButton.Location = new System.Drawing.Point(4, 26);
			this.transparentRadioButton.Name = "transparentRadioButton";
			this.controlResource.SetResourceKey(this.transparentRadioButton, "IDS_SETTINGS_MEMORY_TRANSPARENT");
			this.transparentRadioButton.Size = new System.Drawing.Size(110, 17);
			this.transparentRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.transparentRadioButton.TabIndex = 1;
			this.transparentRadioButton.Values.Text = "Transparent ROM";
			// 
			// fullBankRadioButton
			// 
			this.fullBankRadioButton.Location = new System.Drawing.Point(4, 47);
			this.fullBankRadioButton.Name = "fullBankRadioButton";
			this.controlResource.SetResourceKey(this.fullBankRadioButton, "IDS_SETTINGS_MEMORY_FULL_BANK");
			this.fullBankRadioButton.Size = new System.Drawing.Size(118, 17);
			this.fullBankRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.fullBankRadioButton.TabIndex = 2;
			this.fullBankRadioButton.Values.Text = "Full bank-switching";
			// 
			// playSidRadioButton
			// 
			this.playSidRadioButton.Location = new System.Drawing.Point(4, 5);
			this.playSidRadioButton.Name = "playSidRadioButton";
			this.controlResource.SetResourceKey(this.playSidRadioButton, "IDS_SETTINGS_MEMORY_PLAYSID");
			this.playSidRadioButton.Size = new System.Drawing.Size(127, 17);
			this.playSidRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.playSidRadioButton.TabIndex = 0;
			this.playSidRadioButton.Values.Text = "PlaySID environment";
			// 
			// realC64RadioButton
			// 
			this.realC64RadioButton.Location = new System.Drawing.Point(4, 68);
			this.realC64RadioButton.Name = "realC64RadioButton";
			this.controlResource.SetResourceKey(this.realC64RadioButton, "IDS_SETTINGS_MEMORY_REAL_C64");
			this.realC64RadioButton.Size = new System.Drawing.Size(133, 17);
			this.realC64RadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.realC64RadioButton.TabIndex = 3;
			this.realC64RadioButton.Values.Text = "Real C64 environment";
			// 
			// clockGroupBox
			// 
			this.clockGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.clockGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.clockGroupBox.Location = new System.Drawing.Point(208, 4);
			this.clockGroupBox.Name = "clockGroupBox";
			// 
			// 
			// 
			this.clockGroupBox.Panel.Controls.Add(this.ntscRadioButton);
			this.clockGroupBox.Panel.Controls.Add(this.palRadioButton);
			this.clockGroupBox.Panel.Controls.Add(this.clockPanel);
			this.controlResource.SetResourceKey(this.clockGroupBox, "IDS_SETTINGS_CLOCK");
			this.clockGroupBox.Size = new System.Drawing.Size(192, 112);
			this.clockGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.clockGroupBox.TabIndex = 1;
			this.clockGroupBox.Values.Heading = "Clock speed";
			// 
			// ntscRadioButton
			// 
			this.ntscRadioButton.Location = new System.Drawing.Point(50, 5);
			this.ntscRadioButton.Name = "ntscRadioButton";
			this.controlResource.SetResourceKey(this.ntscRadioButton, "IDS_SETTINGS_CLOCK_NTSC");
			this.ntscRadioButton.Size = new System.Drawing.Size(49, 17);
			this.ntscRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.ntscRadioButton.TabIndex = 1;
			this.ntscRadioButton.Values.Text = "NTSC";
			// 
			// palRadioButton
			// 
			this.palRadioButton.Location = new System.Drawing.Point(4, 5);
			this.palRadioButton.Name = "palRadioButton";
			this.controlResource.SetResourceKey(this.palRadioButton, "IDS_SETTINGS_CLOCK_PAL");
			this.palRadioButton.Size = new System.Drawing.Size(41, 17);
			this.palRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.palRadioButton.TabIndex = 0;
			this.palRadioButton.Values.Text = "PAL";
			// 
			// clockPanel
			// 
			this.clockPanel.Controls.Add(this.clockAlwaysRadioButton);
			this.clockPanel.Controls.Add(this.clockNotKnownRadioButton);
			this.clockPanel.Location = new System.Drawing.Point(12, 26);
			this.clockPanel.Name = "clockPanel";
			this.controlResource.SetResourceKey(this.clockPanel, null);
			this.clockPanel.Size = new System.Drawing.Size(178, 37);
			this.clockPanel.TabIndex = 2;
			// 
			// clockAlwaysRadioButton
			// 
			this.clockAlwaysRadioButton.Location = new System.Drawing.Point(0, 21);
			this.clockAlwaysRadioButton.Name = "clockAlwaysRadioButton";
			this.controlResource.SetResourceKey(this.clockAlwaysRadioButton, "IDS_SETTINGS_CLOCK_ALWAYS");
			this.clockAlwaysRadioButton.Size = new System.Drawing.Size(148, 17);
			this.clockAlwaysRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.clockAlwaysRadioButton.TabIndex = 1;
			this.clockAlwaysRadioButton.Values.Text = "Always play at this speed";
			// 
			// clockNotKnownRadioButton
			// 
			this.clockNotKnownRadioButton.Location = new System.Drawing.Point(0, 0);
			this.clockNotKnownRadioButton.Name = "clockNotKnownRadioButton";
			this.controlResource.SetResourceKey(this.clockNotKnownRadioButton, "IDS_SETTINGS_CLOCK_NOT_KNOWN");
			this.clockNotKnownRadioButton.Size = new System.Drawing.Size(175, 16);
			this.clockNotKnownRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.clockNotKnownRadioButton.TabIndex = 0;
			this.clockNotKnownRadioButton.Values.Text = "Only when speed is not known";
			// 
			// modelGroupBox
			// 
			this.modelGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.modelGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.modelGroupBox.Location = new System.Drawing.Point(408, 4);
			this.modelGroupBox.Name = "modelGroupBox";
			// 
			// 
			// 
			this.modelGroupBox.Panel.Controls.Add(this.model8580RadioButton);
			this.modelGroupBox.Panel.Controls.Add(this.model6581RadioButton);
			this.modelGroupBox.Panel.Controls.Add(this.modelPanel);
			this.controlResource.SetResourceKey(this.modelGroupBox, "IDS_SETTINGS_MODEL");
			this.modelGroupBox.Size = new System.Drawing.Size(192, 112);
			this.modelGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.modelGroupBox.TabIndex = 2;
			this.modelGroupBox.Values.Heading = "SID model";
			// 
			// model8580RadioButton
			// 
			this.model8580RadioButton.Location = new System.Drawing.Point(80, 5);
			this.model8580RadioButton.Name = "model8580RadioButton";
			this.controlResource.SetResourceKey(this.model8580RadioButton, "IDS_SETTINGS_MODEL_8580");
			this.model8580RadioButton.Size = new System.Drawing.Size(74, 17);
			this.model8580RadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.model8580RadioButton.TabIndex = 1;
			this.model8580RadioButton.Values.Text = "MOS-8580";
			// 
			// model6581RadioButton
			// 
			this.model6581RadioButton.Location = new System.Drawing.Point(4, 5);
			this.model6581RadioButton.Name = "model6581RadioButton";
			this.controlResource.SetResourceKey(this.model6581RadioButton, "IDS_SETTINGS_MODEL_6581");
			this.model6581RadioButton.Size = new System.Drawing.Size(74, 17);
			this.model6581RadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.model6581RadioButton.TabIndex = 0;
			this.model6581RadioButton.Values.Text = "MOS-6581";
			// 
			// modelPanel
			// 
			this.modelPanel.Controls.Add(this.modelAlwaysRadioButton);
			this.modelPanel.Controls.Add(this.modelNotKnownRadioButton);
			this.modelPanel.Location = new System.Drawing.Point(12, 26);
			this.modelPanel.Name = "modelPanel";
			this.controlResource.SetResourceKey(this.modelPanel, null);
			this.modelPanel.Size = new System.Drawing.Size(178, 37);
			this.modelPanel.TabIndex = 3;
			// 
			// modelAlwaysRadioButton
			// 
			this.modelAlwaysRadioButton.Location = new System.Drawing.Point(0, 21);
			this.modelAlwaysRadioButton.Name = "modelAlwaysRadioButton";
			this.controlResource.SetResourceKey(this.modelAlwaysRadioButton, "IDS_SETTINGS_MODEL_ALWAYS");
			this.modelAlwaysRadioButton.Size = new System.Drawing.Size(133, 17);
			this.modelAlwaysRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.modelAlwaysRadioButton.TabIndex = 1;
			this.modelAlwaysRadioButton.Values.Text = "Always use this model";
			// 
			// modelNotKnownRadioButton
			// 
			this.modelNotKnownRadioButton.Location = new System.Drawing.Point(0, 0);
			this.modelNotKnownRadioButton.Name = "modelNotKnownRadioButton";
			this.controlResource.SetResourceKey(this.modelNotKnownRadioButton, "IDS_SETTINGS_MODEL_NOT_KNOWN");
			this.modelNotKnownRadioButton.Size = new System.Drawing.Size(177, 17);
			this.modelNotKnownRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.modelNotKnownRadioButton.TabIndex = 0;
			this.modelNotKnownRadioButton.Values.Text = "Only when model is not known";
			// 
			// hvscGroupBox
			// 
			this.hvscGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.hvscGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.hvscGroupBox.Location = new System.Drawing.Point(8, 314);
			this.hvscGroupBox.Name = "hvscGroupBox";
			// 
			// 
			// 
			this.hvscGroupBox.Panel.Controls.Add(this.hvscLabel);
			this.hvscGroupBox.Panel.Controls.Add(this.hvscvPathTextBox);
			this.hvscGroupBox.Panel.Controls.Add(this.hvscPathButton);
			this.hvscGroupBox.Panel.Controls.Add(this.songLengthCheckBox);
			this.hvscGroupBox.Panel.Controls.Add(this.stilCheckBox);
			this.hvscGroupBox.Panel.Controls.Add(this.bugListCheckBox);
			this.controlResource.SetResourceKey(this.hvscGroupBox, "IDS_SETTINGS_HVSC");
			this.hvscGroupBox.Size = new System.Drawing.Size(592, 73);
			this.hvscGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.hvscGroupBox.TabIndex = 4;
			this.hvscGroupBox.Values.Heading = "HVSC";
			// 
			// hvscLabel
			// 
			this.hvscLabel.Location = new System.Drawing.Point(4, 5);
			this.hvscLabel.Name = "hvscLabel";
			this.controlResource.SetResourceKey(this.hvscLabel, "IDS_SETTINGS_HVSC_PATH");
			this.hvscLabel.Size = new System.Drawing.Size(127, 17);
			this.hvscLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.hvscLabel.TabIndex = 0;
			this.hvscLabel.Values.Text = "Path to HVSC collection";
			// 
			// hvscvPathTextBox
			// 
			this.hvscvPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.hvscvPathTextBox.Location = new System.Drawing.Point(140, 3);
			this.hvscvPathTextBox.Name = "hvscvPathTextBox";
			this.controlResource.SetResourceKey(this.hvscvPathTextBox, null);
			this.hvscvPathTextBox.Size = new System.Drawing.Size(418, 21);
			this.hvscvPathTextBox.StateCommon.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.hvscvPathTextBox.TabIndex = 1;
			// 
			// hvscPathButton
			// 
			this.hvscPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.hvscPathButton.Location = new System.Drawing.Point(562, 2);
			this.hvscPathButton.Name = "hvscPathButton";
			this.controlResource.SetResourceKey(this.hvscPathButton, null);
			this.hvscPathButton.Size = new System.Drawing.Size(22, 22);
			this.hvscPathButton.TabIndex = 2;
			this.hvscPathButton.Values.Image = global::Polycode.NostalgicPlayer.Agent.Player.SidPlaySettings.Resources.IDB_OPEN;
			this.hvscPathButton.Values.Text = "kryptonButton1";
			this.hvscPathButton.Click += new System.EventHandler(this.HvscPathButton_Click);
			// 
			// songLengthCheckBox
			// 
			this.songLengthCheckBox.Location = new System.Drawing.Point(348, 28);
			this.songLengthCheckBox.Name = "songLengthCheckBox";
			this.controlResource.SetResourceKey(this.songLengthCheckBox, "IDS_SETTINGS_HVSC_SONGLENGTH");
			this.songLengthCheckBox.Size = new System.Drawing.Size(167, 17);
			this.songLengthCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.songLengthCheckBox.TabIndex = 5;
			this.songLengthCheckBox.Values.Text = "Enable song length database";
			// 
			// stilCheckBox
			// 
			this.stilCheckBox.Location = new System.Drawing.Point(140, 28);
			this.stilCheckBox.Name = "stilCheckBox";
			this.controlResource.SetResourceKey(this.stilCheckBox, "IDS_SETTINGS_HVSC_STIL");
			this.stilCheckBox.Size = new System.Drawing.Size(83, 17);
			this.stilCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.stilCheckBox.TabIndex = 3;
			this.stilCheckBox.Values.Text = "Enable STIL";
			// 
			// bugListCheckBox
			// 
			this.bugListCheckBox.Location = new System.Drawing.Point(237, 28);
			this.bugListCheckBox.Name = "bugListCheckBox";
			this.controlResource.SetResourceKey(this.bugListCheckBox, "IDS_SETTINGS_HVSC_BUGLIST");
			this.bugListCheckBox.Size = new System.Drawing.Size(97, 17);
			this.bugListCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.bugListCheckBox.TabIndex = 4;
			this.bugListCheckBox.Values.Text = "Enable bug list";
			// 
			// filterGroupBox
			// 
			this.filterGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterGroupBox.GroupBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.filterGroupBox.Location = new System.Drawing.Point(8, 120);
			this.filterGroupBox.Name = "filterGroupBox";
			// 
			// 
			// 
			this.filterGroupBox.Panel.Controls.Add(this.enableFilterCheckBox);
			this.filterGroupBox.Panel.Controls.Add(this.filterPanel);
			this.filterGroupBox.Panel.Controls.Add(this.filterAdjustmentPanel);
			this.controlResource.SetResourceKey(this.filterGroupBox, "IDS_SETTINGS_FILTER");
			this.filterGroupBox.Size = new System.Drawing.Size(592, 190);
			this.filterGroupBox.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.filterGroupBox.TabIndex = 3;
			this.filterGroupBox.Values.Heading = "Filter adjustment";
			// 
			// enableFilterCheckBox
			// 
			this.enableFilterCheckBox.Location = new System.Drawing.Point(4, 5);
			this.enableFilterCheckBox.Name = "enableFilterCheckBox";
			this.controlResource.SetResourceKey(this.enableFilterCheckBox, "IDS_SETTINGS_FILTER_ENABLE");
			this.enableFilterCheckBox.Size = new System.Drawing.Size(83, 17);
			this.enableFilterCheckBox.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.enableFilterCheckBox.TabIndex = 0;
			this.enableFilterCheckBox.Values.Text = "Enable filter";
			this.enableFilterCheckBox.CheckedChanged += new System.EventHandler(this.EnableFilterCheckBox_CheckedChanged);
			// 
			// filterPanel
			// 
			this.filterPanel.Controls.Add(this.filterModelRadioButton);
			this.filterPanel.Controls.Add(this.filterCustomRadioButton);
			this.filterPanel.Location = new System.Drawing.Point(100, 5);
			this.filterPanel.Name = "filterPanel";
			this.controlResource.SetResourceKey(this.filterPanel, null);
			this.filterPanel.Size = new System.Drawing.Size(484, 18);
			this.filterPanel.TabIndex = 1;
			// 
			// filterModelRadioButton
			// 
			this.filterModelRadioButton.Location = new System.Drawing.Point(0, 0);
			this.filterModelRadioButton.Name = "filterModelRadioButton";
			this.controlResource.SetResourceKey(this.filterModelRadioButton, "IDS_SETTINGS_FILTER_MODEL");
			this.filterModelRadioButton.Size = new System.Drawing.Size(92, 17);
			this.filterModelRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.filterModelRadioButton.TabIndex = 0;
			this.filterModelRadioButton.Values.Text = "Model specific";
			this.filterModelRadioButton.CheckedChanged += new System.EventHandler(this.FilterModelRadioButton_CheckedChanged);
			// 
			// filterCustomRadioButton
			// 
			this.filterCustomRadioButton.Location = new System.Drawing.Point(105, 0);
			this.filterCustomRadioButton.Name = "filterCustomRadioButton";
			this.controlResource.SetResourceKey(this.filterCustomRadioButton, "IDS_SETTINGS_FILTER_CUSTOM");
			this.filterCustomRadioButton.Size = new System.Drawing.Size(60, 17);
			this.filterCustomRadioButton.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.filterCustomRadioButton.TabIndex = 1;
			this.filterCustomRadioButton.Values.Text = "Custom";
			this.filterCustomRadioButton.CheckedChanged += new System.EventHandler(this.FilterCustomRadioButton_CheckedChanged);
			// 
			// filterAdjustmentPanel
			// 
			this.filterAdjustmentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterAdjustmentPanel.Controls.Add(this.filterResetButton);
			this.filterAdjustmentPanel.Controls.Add(this.filterFtLabel);
			this.filterAdjustmentPanel.Controls.Add(this.filterFmLabel);
			this.filterAdjustmentPanel.Controls.Add(this.filterFtTrackBar);
			this.filterAdjustmentPanel.Controls.Add(this.filterFmTrackBar);
			this.filterAdjustmentPanel.Controls.Add(this.filterControl);
			this.filterAdjustmentPanel.Controls.Add(this.filterFsLabel);
			this.filterAdjustmentPanel.Controls.Add(this.filterFsTrackBar);
			this.filterAdjustmentPanel.Location = new System.Drawing.Point(4, 30);
			this.filterAdjustmentPanel.Name = "filterAdjustmentPanel";
			this.controlResource.SetResourceKey(this.filterAdjustmentPanel, null);
			this.filterAdjustmentPanel.Size = new System.Drawing.Size(584, 136);
			this.filterAdjustmentPanel.TabIndex = 2;
			// 
			// filterResetButton
			// 
			this.filterResetButton.Location = new System.Drawing.Point(0, 100);
			this.filterResetButton.Name = "filterResetButton";
			this.controlResource.SetResourceKey(this.filterResetButton, "IDS_SETTINGS_FILTER_RESET");
			this.filterResetButton.Size = new System.Drawing.Size(90, 25);
			this.filterResetButton.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.filterResetButton.TabIndex = 7;
			this.filterResetButton.Values.Text = "Reset";
			this.filterResetButton.Click += new System.EventHandler(this.FilterResetButton_Click);
			// 
			// filterFtLabel
			// 
			this.filterFtLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterFtLabel.Location = new System.Drawing.Point(364, 67);
			this.filterFtLabel.Name = "filterFtLabel";
			this.controlResource.SetResourceKey(this.filterFtLabel, "IDS_SETTINGS_FILTER_FT");
			this.filterFtLabel.Size = new System.Drawing.Size(78, 17);
			this.filterFtLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.filterFtLabel.TabIndex = 5;
			this.filterFtLabel.Values.Text = "FT parameter";
			// 
			// filterFmLabel
			// 
			this.filterFmLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterFmLabel.Location = new System.Drawing.Point(364, 36);
			this.filterFmLabel.Name = "filterFmLabel";
			this.controlResource.SetResourceKey(this.filterFmLabel, "IDS_SETTINGS_FILTER_FM");
			this.filterFmLabel.Size = new System.Drawing.Size(80, 17);
			this.filterFmLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.filterFmLabel.TabIndex = 3;
			this.filterFmLabel.Values.Text = "FM parameter";
			// 
			// filterFtTrackBar
			// 
			this.filterFtTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterFtTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			this.filterFtTrackBar.DrawBackground = true;
			this.filterFtTrackBar.Location = new System.Drawing.Point(0, 62);
			this.filterFtTrackBar.Maximum = 50;
			this.filterFtTrackBar.Name = "filterFtTrackBar";
			this.controlResource.SetResourceKey(this.filterFtTrackBar, null);
			this.filterFtTrackBar.Size = new System.Drawing.Size(360, 27);
			this.filterFtTrackBar.TabIndex = 4;
			this.filterFtTrackBar.TickFrequency = 5;
			this.filterFtTrackBar.Value = 10;
			this.filterFtTrackBar.ValueChanged += new System.EventHandler(this.FilterFtTrackBar_ValueChanged);
			// 
			// filterFmTrackBar
			// 
			this.filterFmTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterFmTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			this.filterFmTrackBar.DrawBackground = true;
			this.filterFmTrackBar.Location = new System.Drawing.Point(0, 31);
			this.filterFmTrackBar.Maximum = 120;
			this.filterFmTrackBar.Minimum = 2;
			this.filterFmTrackBar.Name = "filterFmTrackBar";
			this.controlResource.SetResourceKey(this.filterFmTrackBar, null);
			this.filterFmTrackBar.Size = new System.Drawing.Size(360, 27);
			this.filterFmTrackBar.TabIndex = 2;
			this.filterFmTrackBar.TickFrequency = 13;
			this.filterFmTrackBar.Value = 10;
			this.filterFmTrackBar.ValueChanged += new System.EventHandler(this.FilterFmTrackBar_ValueChanged);
			// 
			// filterControl
			// 
			this.filterControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterControl.BackColor = System.Drawing.Color.White;
			this.filterControl.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.filterControl.Location = new System.Drawing.Point(453, 4);
			this.filterControl.Name = "filterControl";
			this.controlResource.SetResourceKey(this.filterControl, null);
			this.filterControl.Size = new System.Drawing.Size(128, 128);
			this.filterControl.TabIndex = 6;
			// 
			// filterFsLabel
			// 
			this.filterFsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterFsLabel.Location = new System.Drawing.Point(364, 5);
			this.filterFsLabel.Name = "filterFsLabel";
			this.controlResource.SetResourceKey(this.filterFsLabel, "IDS_SETTINGS_FILTER_FS");
			this.filterFsLabel.Size = new System.Drawing.Size(77, 17);
			this.filterFsLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.filterFsLabel.TabIndex = 1;
			this.filterFsLabel.Values.Text = "FS parameter";
			// 
			// filterFsTrackBar
			// 
			this.filterFsTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterFsTrackBar.BackStyle = Krypton.Toolkit.PaletteBackStyle.InputControlStandalone;
			this.filterFsTrackBar.DrawBackground = true;
			this.filterFsTrackBar.Location = new System.Drawing.Point(0, 0);
			this.filterFsTrackBar.Maximum = 700;
			this.filterFsTrackBar.Minimum = 10;
			this.filterFsTrackBar.Name = "filterFsTrackBar";
			this.controlResource.SetResourceKey(this.filterFsTrackBar, null);
			this.filterFsTrackBar.Size = new System.Drawing.Size(360, 27);
			this.filterFsTrackBar.TabIndex = 0;
			this.filterFsTrackBar.TickFrequency = 75;
			this.filterFsTrackBar.Value = 10;
			this.filterFsTrackBar.ValueChanged += new System.EventHandler(this.FilterFsTrackBar_ValueChanged);
			// 
			// SettingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.filterGroupBox);
			this.Controls.Add(this.hvscGroupBox);
			this.Controls.Add(this.modelGroupBox);
			this.Controls.Add(this.clockGroupBox);
			this.Controls.Add(this.memoryGroupBox);
			this.MinimumSize = new System.Drawing.Size(608, 395);
			this.Name = "SettingsControl";
			this.controlResource.SetResourceKey(this, null);
			this.Size = new System.Drawing.Size(608, 395);
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.memoryGroupBox.Panel)).EndInit();
			this.memoryGroupBox.Panel.ResumeLayout(false);
			this.memoryGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.memoryGroupBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.clockGroupBox.Panel)).EndInit();
			this.clockGroupBox.Panel.ResumeLayout(false);
			this.clockGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.clockGroupBox)).EndInit();
			this.clockPanel.ResumeLayout(false);
			this.clockPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.modelGroupBox.Panel)).EndInit();
			this.modelGroupBox.Panel.ResumeLayout(false);
			this.modelGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.modelGroupBox)).EndInit();
			this.modelPanel.ResumeLayout(false);
			this.modelPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.hvscGroupBox.Panel)).EndInit();
			this.hvscGroupBox.Panel.ResumeLayout(false);
			this.hvscGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.hvscGroupBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.filterGroupBox.Panel)).EndInit();
			this.filterGroupBox.Panel.ResumeLayout(false);
			this.filterGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.filterGroupBox)).EndInit();
			this.filterPanel.ResumeLayout(false);
			this.filterPanel.PerformLayout();
			this.filterAdjustmentPanel.ResumeLayout(false);
			this.filterAdjustmentPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonGroupBox memoryGroupBox;
		private Krypton.Toolkit.KryptonRadioButton realC64RadioButton;
		private Krypton.Toolkit.KryptonRadioButton fullBankRadioButton;
		private Krypton.Toolkit.KryptonRadioButton transparentRadioButton;
		private Krypton.Toolkit.KryptonRadioButton playSidRadioButton;
		private Krypton.Toolkit.KryptonGroupBox clockGroupBox;
		private Krypton.Toolkit.KryptonRadioButton palRadioButton;
		private Krypton.Toolkit.KryptonRadioButton ntscRadioButton;
		private System.Windows.Forms.Panel clockPanel;
		private Krypton.Toolkit.KryptonRadioButton clockNotKnownRadioButton;
		private Krypton.Toolkit.KryptonRadioButton clockAlwaysRadioButton;
		private Krypton.Toolkit.KryptonGroupBox modelGroupBox;
		private Krypton.Toolkit.KryptonRadioButton model6581RadioButton;
		private Krypton.Toolkit.KryptonRadioButton model8580RadioButton;
		private System.Windows.Forms.Panel modelPanel;
		private Krypton.Toolkit.KryptonRadioButton modelAlwaysRadioButton;
		private Krypton.Toolkit.KryptonRadioButton modelNotKnownRadioButton;
		private Krypton.Toolkit.KryptonGroupBox hvscGroupBox;
		private Krypton.Toolkit.KryptonLabel hvscLabel;
		private Krypton.Toolkit.KryptonGroupBox filterGroupBox;
		private Krypton.Toolkit.KryptonTextBox hvscvPathTextBox;
		private Krypton.Toolkit.KryptonButton hvscPathButton;
		private Krypton.Toolkit.KryptonCheckBox stilCheckBox;
		private Krypton.Toolkit.KryptonCheckBox songLengthCheckBox;
		private Krypton.Toolkit.KryptonCheckBox enableFilterCheckBox;
		private Krypton.Toolkit.KryptonRadioButton filterModelRadioButton;
		private Krypton.Toolkit.KryptonRadioButton filterCustomRadioButton;
		private System.Windows.Forms.Panel filterPanel;
		private System.Windows.Forms.Panel filterAdjustmentPanel;
		private Krypton.Toolkit.KryptonTrackBar filterFsTrackBar;
		private Krypton.Toolkit.KryptonLabel filterFsLabel;
		private FilterControl filterControl;
		private Krypton.Toolkit.KryptonLabel filterFtLabel;
		private Krypton.Toolkit.KryptonLabel filterFmLabel;
		private Krypton.Toolkit.KryptonTrackBar filterFtTrackBar;
		private Krypton.Toolkit.KryptonTrackBar filterFmTrackBar;
		private Krypton.Toolkit.KryptonButton filterResetButton;
		private Krypton.Toolkit.KryptonCheckBox bugListCheckBox;
	}
}
