
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	partial class OptionsPageControl
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
			generalGroupBox = new Krypton.Toolkit.KryptonGroupBox();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			addJumpCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			addToListCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			rememberListCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			rememberListPanel = new System.Windows.Forms.Panel();
			rememberModulePositionCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			rememberListPositionCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			tooltipsCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			showNameInTitleCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			showListNumberCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			showFullPathCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			separateWindowsCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			windowPanel = new System.Windows.Forms.Panel();
			showWindowsInTaskBarCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			useDatabaseCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			scanFilesCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			scanFilesPanel = new System.Windows.Forms.Panel();
			extractPlayingTimeCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			removeUnknownCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)generalGroupBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)generalGroupBox.Panel).BeginInit();
			generalGroupBox.Panel.SuspendLayout();
			rememberListPanel.SuspendLayout();
			windowPanel.SuspendLayout();
			scanFilesPanel.SuspendLayout();
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
			generalGroupBox.Panel.Controls.Add(addJumpCheckBox);
			generalGroupBox.Panel.Controls.Add(addToListCheckBox);
			generalGroupBox.Panel.Controls.Add(rememberListCheckBox);
			generalGroupBox.Panel.Controls.Add(rememberListPanel);
			generalGroupBox.Panel.Controls.Add(tooltipsCheckBox);
			generalGroupBox.Panel.Controls.Add(showNameInTitleCheckBox);
			generalGroupBox.Panel.Controls.Add(showListNumberCheckBox);
			generalGroupBox.Panel.Controls.Add(showFullPathCheckBox);
			generalGroupBox.Panel.Controls.Add(separateWindowsCheckBox);
			generalGroupBox.Panel.Controls.Add(windowPanel);
			generalGroupBox.Panel.Controls.Add(useDatabaseCheckBox);
			generalGroupBox.Panel.Controls.Add(scanFilesCheckBox);
			generalGroupBox.Panel.Controls.Add(scanFilesPanel);
			controlResource.SetResourceKey(generalGroupBox, "IDS_SETTINGS_OPTIONS_GENERAL");
			generalGroupBox.Size = new System.Drawing.Size(592, 175);
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
			// addJumpCheckBox
			// 
			addJumpCheckBox.Location = new System.Drawing.Point(4, 5);
			addJumpCheckBox.Name = "addJumpCheckBox";
			addJumpCheckBox.Palette = fontPalette;
			addJumpCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(addJumpCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_ADDJUMP");
			addJumpCheckBox.Size = new System.Drawing.Size(134, 16);
			addJumpCheckBox.TabIndex = 0;
			addJumpCheckBox.Values.Text = "Jump to added module";
			// 
			// addToListCheckBox
			// 
			addToListCheckBox.Location = new System.Drawing.Point(4, 26);
			addToListCheckBox.Name = "addToListCheckBox";
			addToListCheckBox.Palette = fontPalette;
			addToListCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(addToListCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_ADDTOLIST");
			addToListCheckBox.Size = new System.Drawing.Size(122, 16);
			addToListCheckBox.TabIndex = 1;
			addToListCheckBox.Values.Text = "Add to list as default";
			// 
			// rememberListCheckBox
			// 
			rememberListCheckBox.Location = new System.Drawing.Point(4, 47);
			rememberListCheckBox.Name = "rememberListCheckBox";
			rememberListCheckBox.Palette = fontPalette;
			rememberListCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(rememberListCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERLIST");
			rememberListCheckBox.Size = new System.Drawing.Size(129, 16);
			rememberListCheckBox.TabIndex = 2;
			rememberListCheckBox.Values.Text = "Remember list on exit";
			rememberListCheckBox.CheckedChanged += RememberListCheckBox_CheckedChanged;
			// 
			// rememberListPanel
			// 
			rememberListPanel.BackColor = System.Drawing.Color.Transparent;
			rememberListPanel.Controls.Add(rememberModulePositionCheckBox);
			rememberListPanel.Controls.Add(rememberListPositionCheckBox);
			rememberListPanel.Enabled = false;
			rememberListPanel.Location = new System.Drawing.Point(12, 68);
			rememberListPanel.Name = "rememberListPanel";
			controlResource.SetResourceKey(rememberListPanel, null);
			rememberListPanel.Size = new System.Drawing.Size(170, 38);
			rememberListPanel.TabIndex = 3;
			// 
			// rememberModulePositionCheckBox
			// 
			rememberModulePositionCheckBox.Location = new System.Drawing.Point(0, 21);
			rememberModulePositionCheckBox.Name = "rememberModulePositionCheckBox";
			rememberModulePositionCheckBox.Palette = fontPalette;
			rememberModulePositionCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(rememberModulePositionCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERMODULEPOSITION");
			rememberModulePositionCheckBox.Size = new System.Drawing.Size(157, 16);
			rememberModulePositionCheckBox.TabIndex = 1;
			rememberModulePositionCheckBox.Values.Text = "Remember module position";
			// 
			// rememberListPositionCheckBox
			// 
			rememberListPositionCheckBox.Location = new System.Drawing.Point(0, 0);
			rememberListPositionCheckBox.Name = "rememberListPositionCheckBox";
			rememberListPositionCheckBox.Palette = fontPalette;
			rememberListPositionCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(rememberListPositionCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMEMBERLISTPOSITION");
			rememberListPositionCheckBox.Size = new System.Drawing.Size(154, 16);
			rememberListPositionCheckBox.TabIndex = 0;
			rememberListPositionCheckBox.Values.Text = "Remember playing module";
			rememberListPositionCheckBox.CheckedChanged += RememberListPositionCheckBox_CheckedChanged;
			// 
			// tooltipsCheckBox
			// 
			tooltipsCheckBox.Location = new System.Drawing.Point(200, 5);
			tooltipsCheckBox.Name = "tooltipsCheckBox";
			tooltipsCheckBox.Palette = fontPalette;
			tooltipsCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(tooltipsCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_TOOLTIPS");
			tooltipsCheckBox.Size = new System.Drawing.Size(96, 16);
			tooltipsCheckBox.TabIndex = 6;
			tooltipsCheckBox.Values.Text = "Button tool tips";
			// 
			// showNameInTitleCheckBox
			// 
			showNameInTitleCheckBox.Location = new System.Drawing.Point(200, 26);
			showNameInTitleCheckBox.Name = "showNameInTitleCheckBox";
			showNameInTitleCheckBox.Palette = fontPalette;
			showNameInTitleCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(showNameInTitleCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWNAME");
			showNameInTitleCheckBox.Size = new System.Drawing.Size(167, 16);
			showNameInTitleCheckBox.TabIndex = 7;
			showNameInTitleCheckBox.Values.Text = "Show module name in titlebar";
			// 
			// showListNumberCheckBox
			// 
			showListNumberCheckBox.Location = new System.Drawing.Point(4, 110);
			showListNumberCheckBox.Name = "showListNumberCheckBox";
			showListNumberCheckBox.Palette = fontPalette;
			showListNumberCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(showListNumberCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWLISTNUMBER");
			showListNumberCheckBox.Size = new System.Drawing.Size(142, 16);
			showListNumberCheckBox.TabIndex = 4;
			showListNumberCheckBox.Values.Text = "Show item number in list";
			// 
			// showFullPathCheckBox
			// 
			showFullPathCheckBox.Location = new System.Drawing.Point(4, 131);
			showFullPathCheckBox.Name = "showFullPathCheckBox";
			showFullPathCheckBox.Palette = fontPalette;
			showFullPathCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(showFullPathCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWFULLPATH");
			showFullPathCheckBox.Size = new System.Drawing.Size(120, 16);
			showFullPathCheckBox.TabIndex = 5;
			showFullPathCheckBox.Values.Text = "Show full path in list";
			// 
			// separateWindowsCheckBox
			// 
			separateWindowsCheckBox.Location = new System.Drawing.Point(200, 47);
			separateWindowsCheckBox.Name = "separateWindowsCheckBox";
			separateWindowsCheckBox.Palette = fontPalette;
			separateWindowsCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(separateWindowsCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SEPARATEWINDOWS");
			separateWindowsCheckBox.Size = new System.Drawing.Size(169, 28);
			separateWindowsCheckBox.TabIndex = 8;
			separateWindowsCheckBox.Values.Text = "Separate each window in task\r\nswitcher";
			separateWindowsCheckBox.CheckedChanged += SeparateWindows_CheckedChanged;
			// 
			// windowPanel
			// 
			windowPanel.BackColor = System.Drawing.Color.Transparent;
			windowPanel.Controls.Add(showWindowsInTaskBarCheckBox);
			windowPanel.Enabled = false;
			windowPanel.Location = new System.Drawing.Point(208, 81);
			windowPanel.Name = "windowPanel";
			controlResource.SetResourceKey(windowPanel, null);
			windowPanel.Size = new System.Drawing.Size(186, 17);
			windowPanel.TabIndex = 9;
			// 
			// showWindowsInTaskBarCheckBox
			// 
			showWindowsInTaskBarCheckBox.Location = new System.Drawing.Point(0, 0);
			showWindowsInTaskBarCheckBox.Name = "showWindowsInTaskBarCheckBox";
			showWindowsInTaskBarCheckBox.Palette = fontPalette;
			showWindowsInTaskBarCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(showWindowsInTaskBarCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SHOWWINDOWSINTASKBAR");
			showWindowsInTaskBarCheckBox.Size = new System.Drawing.Size(163, 16);
			showWindowsInTaskBarCheckBox.TabIndex = 0;
			showWindowsInTaskBarCheckBox.Values.Text = "Show all windows in task bar";
			// 
			// useDatabaseCheckBox
			// 
			useDatabaseCheckBox.Location = new System.Drawing.Point(400, 5);
			useDatabaseCheckBox.Name = "useDatabaseCheckBox";
			useDatabaseCheckBox.Palette = fontPalette;
			useDatabaseCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(useDatabaseCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_USEDATABASE");
			useDatabaseCheckBox.Size = new System.Drawing.Size(169, 28);
			useDatabaseCheckBox.TabIndex = 10;
			useDatabaseCheckBox.Values.Text = "Use database to store module\r\ninformation";
			// 
			// scanFilesCheckBox
			// 
			scanFilesCheckBox.Location = new System.Drawing.Point(400, 38);
			scanFilesCheckBox.Name = "scanFilesCheckBox";
			scanFilesCheckBox.Palette = fontPalette;
			scanFilesCheckBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(scanFilesCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_SCANFILES");
			scanFilesCheckBox.Size = new System.Drawing.Size(104, 16);
			scanFilesCheckBox.TabIndex = 11;
			scanFilesCheckBox.Values.Text = "Scan added files";
			scanFilesCheckBox.CheckedChanged += ScanFiles_CheckedChanged;
			// 
			// scanFilesPanel
			// 
			scanFilesPanel.Controls.Add(extractPlayingTimeCheckBox);
			scanFilesPanel.Controls.Add(removeUnknownCheckBox);
			scanFilesPanel.Enabled = false;
			scanFilesPanel.Location = new System.Drawing.Point(408, 59);
			scanFilesPanel.Name = "scanFilesPanel";
			controlResource.SetResourceKey(scanFilesPanel, null);
			scanFilesPanel.Size = new System.Drawing.Size(177, 38);
			scanFilesPanel.TabIndex = 12;
			// 
			// extractPlayingTimeCheckBox
			// 
			extractPlayingTimeCheckBox.Location = new System.Drawing.Point(0, 21);
			extractPlayingTimeCheckBox.Name = "extractPlayingTimeCheckBox";
			controlResource.SetResourceKey(extractPlayingTimeCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_EXTRACTPLAYINGTIME");
			extractPlayingTimeCheckBox.Size = new System.Drawing.Size(131, 20);
			extractPlayingTimeCheckBox.TabIndex = 1;
			extractPlayingTimeCheckBox.Values.Text = "Extract playing time";
			// 
			// removeUnknownCheckBox
			// 
			removeUnknownCheckBox.Location = new System.Drawing.Point(0, 0);
			removeUnknownCheckBox.Name = "removeUnknownCheckBox";
			controlResource.SetResourceKey(removeUnknownCheckBox, "IDS_SETTINGS_OPTIONS_GENERAL_REMOVEUNKNOWN");
			removeUnknownCheckBox.Size = new System.Drawing.Size(172, 20);
			removeUnknownCheckBox.TabIndex = 0;
			removeUnknownCheckBox.Values.Text = "Remove unknown modules";
			// 
			// OptionsPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(generalGroupBox);
			Name = "OptionsPageControl";
			controlResource.SetResourceKey(this, null);
			Size = new System.Drawing.Size(608, 356);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			((System.ComponentModel.ISupportInitialize)generalGroupBox.Panel).EndInit();
			generalGroupBox.Panel.ResumeLayout(false);
			generalGroupBox.Panel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)generalGroupBox).EndInit();
			rememberListPanel.ResumeLayout(false);
			rememberListPanel.PerformLayout();
			windowPanel.ResumeLayout(false);
			windowPanel.PerformLayout();
			scanFilesPanel.ResumeLayout(false);
			scanFilesPanel.PerformLayout();
			ResumeLayout(false);
		}

		#endregion
		private Kit.Gui.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonGroupBox generalGroupBox;
		private Krypton.Toolkit.KryptonCheckBox addJumpCheckBox;
		private Krypton.Toolkit.KryptonCheckBox addToListCheckBox;
		private Krypton.Toolkit.KryptonCheckBox rememberListCheckBox;
		private System.Windows.Forms.Panel rememberListPanel;
		private Krypton.Toolkit.KryptonCheckBox rememberModulePositionCheckBox;
		private Krypton.Toolkit.KryptonCheckBox rememberListPositionCheckBox;
		private Krypton.Toolkit.KryptonCheckBox tooltipsCheckBox;
		private Krypton.Toolkit.KryptonCheckBox showNameInTitleCheckBox;
		private Krypton.Toolkit.KryptonCheckBox showListNumberCheckBox;
		private Krypton.Toolkit.KryptonCheckBox scanFilesCheckBox;
		private Krypton.Toolkit.KryptonCheckBox useDatabaseCheckBox;
		private Krypton.Toolkit.KryptonCheckBox separateWindowsCheckBox;
		private Krypton.Toolkit.KryptonCheckBox showWindowsInTaskBarCheckBox;
		private System.Windows.Forms.Panel windowPanel;
		private Kit.Gui.Components.FontPalette fontPalette;
		private System.Windows.Forms.Panel scanFilesPanel;
		private Krypton.Toolkit.KryptonCheckBox extractPlayingTimeCheckBox;
		private Krypton.Toolkit.KryptonCheckBox removeUnknownCheckBox;
		private Krypton.Toolkit.KryptonCheckBox showFullPathCheckBox;
	}
}
