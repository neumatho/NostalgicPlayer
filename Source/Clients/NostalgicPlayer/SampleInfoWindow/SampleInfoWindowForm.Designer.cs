
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SampleInfoWindow
{
	partial class SampleInfoWindowForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SampleInfoWindowForm));
			navigator = new Krypton.Navigator.KryptonNavigator();
			navigatorInstrumentPage = new Krypton.Navigator.KryptonPage();
			instrumentGroup = new Krypton.Toolkit.KryptonGroup();
			instrumentDataGridView = new Krypton.Toolkit.KryptonDataGridView();
			monoFontPalette = new GuiKit.Components.FontPalette(components);
			navigatorSamplePage = new Krypton.Navigator.KryptonPage();
			saveFormatLabel = new Krypton.Toolkit.KryptonLabel();
			fontPalette = new GuiKit.Components.FontPalette(components);
			saveFormatComboBox = new Krypton.Toolkit.KryptonComboBox();
			saveButton = new Krypton.Toolkit.KryptonButton();
			polyphonyLabel = new Krypton.Toolkit.KryptonLabel();
			sampleGroup = new Krypton.Toolkit.KryptonGroup();
			sampleDataGridView = new SampleInfoSamplesListControl();
			octaveLabel = new Krypton.Toolkit.KryptonLabel();
			controlResource = new GuiKit.Designer.ControlResource();
			((System.ComponentModel.ISupportInitialize)navigator).BeginInit();
			((System.ComponentModel.ISupportInitialize)navigatorInstrumentPage).BeginInit();
			navigatorInstrumentPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)instrumentGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)instrumentGroup.Panel).BeginInit();
			instrumentGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)instrumentDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)navigatorSamplePage).BeginInit();
			navigatorSamplePage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)saveFormatComboBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)sampleGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)sampleGroup.Panel).BeginInit();
			sampleGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)sampleDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
			// 
			// navigator
			// 
			navigator.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			navigator.Button.ButtonDisplayLogic = Krypton.Navigator.ButtonDisplayLogic.Context;
			navigator.Button.CloseButtonAction = Krypton.Navigator.CloseButtonAction.RemovePageAndDispose;
			navigator.Button.CloseButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			navigator.Button.ContextButtonAction = Krypton.Navigator.ContextButtonAction.SelectPage;
			navigator.Button.ContextButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			navigator.Button.ContextMenuMapImage = Krypton.Navigator.MapKryptonPageImage.Small;
			navigator.Button.ContextMenuMapText = Krypton.Navigator.MapKryptonPageText.TextTitle;
			navigator.Button.NextButtonAction = Krypton.Navigator.DirectionButtonAction.ModeAppropriateAction;
			navigator.Button.NextButtonDisplay = Krypton.Navigator.ButtonDisplay.Logic;
			navigator.Button.PreviousButtonAction = Krypton.Navigator.DirectionButtonAction.ModeAppropriateAction;
			navigator.Button.PreviousButtonDisplay = Krypton.Navigator.ButtonDisplay.Logic;
			navigator.ControlKryptonFormFeatures = false;
			navigator.Location = new System.Drawing.Point(8, 8);
			navigator.Name = "navigator";
			navigator.NavigatorMode = Krypton.Navigator.NavigatorMode.BarTabGroup;
			navigator.Owner = null;
			navigator.PageBackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
			navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] { navigatorInstrumentPage, navigatorSamplePage });
			navigator.Palette = fontPalette;
			navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			controlResource.SetResourceKey(navigator, null);
			navigator.SelectedIndex = 0;
			navigator.Size = new System.Drawing.Size(432, 172);
			navigator.TabIndex = 0;
			navigator.SelectedPageChanged += Navigator_SelectedPageChanged;
			// 
			// navigatorInstrumentPage
			// 
			navigatorInstrumentPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorInstrumentPage.Controls.Add(instrumentGroup);
			navigatorInstrumentPage.Flags = 65534;
			navigatorInstrumentPage.LastVisibleSet = true;
			navigatorInstrumentPage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorInstrumentPage.Name = "navigatorInstrumentPage";
			controlResource.SetResourceKey(navigatorInstrumentPage, null);
			navigatorInstrumentPage.Size = new System.Drawing.Size(430, 146);
			navigatorInstrumentPage.Text = "";
			navigatorInstrumentPage.ToolTipTitle = "Page ToolTip";
			navigatorInstrumentPage.UniqueName = "5d888e6082d44d78aac10a8a0c09a21e";
			// 
			// instrumentGroup
			// 
			instrumentGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			instrumentGroup.Location = new System.Drawing.Point(8, 8);
			instrumentGroup.Name = "instrumentGroup";
			// 
			// 
			// 
			instrumentGroup.Panel.Controls.Add(instrumentDataGridView);
			controlResource.SetResourceKey(instrumentGroup, null);
			instrumentGroup.Size = new System.Drawing.Size(414, 130);
			instrumentGroup.TabIndex = 0;
			// 
			// instrumentDataGridView
			// 
			instrumentDataGridView.AllowUserToAddRows = false;
			instrumentDataGridView.AllowUserToDeleteRows = false;
			instrumentDataGridView.AllowUserToOrderColumns = true;
			instrumentDataGridView.AllowUserToResizeRows = false;
			instrumentDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			instrumentDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			instrumentDataGridView.Location = new System.Drawing.Point(0, 0);
			instrumentDataGridView.MultiSelect = false;
			instrumentDataGridView.Name = "instrumentDataGridView";
			instrumentDataGridView.Palette = monoFontPalette;
			instrumentDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			instrumentDataGridView.ReadOnly = true;
			controlResource.SetResourceKey(instrumentDataGridView, null);
			instrumentDataGridView.RowHeadersVisible = false;
			instrumentDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			instrumentDataGridView.ShowCellErrors = false;
			instrumentDataGridView.ShowEditingIcon = false;
			instrumentDataGridView.ShowRowErrors = false;
			instrumentDataGridView.Size = new System.Drawing.Size(412, 128);
			instrumentDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			instrumentDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			instrumentDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			instrumentDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right;
			instrumentDataGridView.TabIndex = 0;
			// 
			// monoFontPalette
			// 
			monoFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			monoFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			monoFontPalette.ThemeName = "";
			monoFontPalette.UseKryptonFileDialogs = true;
			monoFontPalette.UseMonospaceOnGrid = true;
			// 
			// navigatorSamplePage
			// 
			navigatorSamplePage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorSamplePage.Controls.Add(saveFormatLabel);
			navigatorSamplePage.Controls.Add(saveFormatComboBox);
			navigatorSamplePage.Controls.Add(saveButton);
			navigatorSamplePage.Controls.Add(polyphonyLabel);
			navigatorSamplePage.Controls.Add(sampleGroup);
			navigatorSamplePage.Controls.Add(octaveLabel);
			navigatorSamplePage.Flags = 65534;
			navigatorSamplePage.LastVisibleSet = true;
			navigatorSamplePage.MinimumSize = new System.Drawing.Size(50, 50);
			navigatorSamplePage.Name = "navigatorSamplePage";
			controlResource.SetResourceKey(navigatorSamplePage, null);
			navigatorSamplePage.Size = new System.Drawing.Size(430, 146);
			navigatorSamplePage.Text = "";
			navigatorSamplePage.ToolTipTitle = "Page ToolTip";
			navigatorSamplePage.UniqueName = "6f932f4ccf4b4441a28afbd3a888c881";
			// 
			// saveFormatLabel
			// 
			saveFormatLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			saveFormatLabel.Location = new System.Drawing.Point(123, 125);
			saveFormatLabel.Name = "saveFormatLabel";
			saveFormatLabel.Palette = fontPalette;
			saveFormatLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(saveFormatLabel, "IDS_SAMPLE_INFO_SAMP_SAVEFORMAT");
			saveFormatLabel.Size = new System.Drawing.Size(69, 16);
			saveFormatLabel.TabIndex = 3;
			saveFormatLabel.Values.Text = "Save format";
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// saveFormatComboBox
			// 
			saveFormatComboBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			saveFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			saveFormatComboBox.DropDownWidth = 120;
			saveFormatComboBox.IntegralHeight = false;
			saveFormatComboBox.Location = new System.Drawing.Point(198, 123);
			saveFormatComboBox.Name = "saveFormatComboBox";
			saveFormatComboBox.Palette = fontPalette;
			saveFormatComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(saveFormatComboBox, null);
			saveFormatComboBox.Size = new System.Drawing.Size(160, 19);
			saveFormatComboBox.TabIndex = 4;
			saveFormatComboBox.SelectedIndexChanged += SaveFormatComboBox_SelectedIndexChanged;
			// 
			// saveButton
			// 
			saveButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			saveButton.Enabled = false;
			saveButton.Location = new System.Drawing.Point(362, 121);
			saveButton.Name = "saveButton";
			saveButton.Palette = fontPalette;
			saveButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(saveButton, "IDS_SAMPLE_INFO_SAMP_SAVE");
			saveButton.Size = new System.Drawing.Size(60, 21);
			saveButton.TabIndex = 5;
			saveButton.Values.Text = "Save";
			saveButton.Click += SaveButton_Click;
			// 
			// polyphonyLabel
			// 
			polyphonyLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			polyphonyLabel.Location = new System.Drawing.Point(75, 125);
			polyphonyLabel.Name = "polyphonyLabel";
			polyphonyLabel.Palette = fontPalette;
			polyphonyLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(polyphonyLabel, null);
			polyphonyLabel.Size = new System.Drawing.Size(16, 16);
			polyphonyLabel.TabIndex = 2;
			polyphonyLabel.Values.Text = "?";
			// 
			// sampleGroup
			// 
			sampleGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			sampleGroup.Location = new System.Drawing.Point(8, 8);
			sampleGroup.Name = "sampleGroup";
			// 
			// 
			// 
			sampleGroup.Panel.Controls.Add(sampleDataGridView);
			controlResource.SetResourceKey(sampleGroup, null);
			sampleGroup.Size = new System.Drawing.Size(414, 108);
			sampleGroup.TabIndex = 1;
			// 
			// sampleDataGridView
			// 
			sampleDataGridView.AllowUserToAddRows = false;
			sampleDataGridView.AllowUserToDeleteRows = false;
			sampleDataGridView.AllowUserToOrderColumns = true;
			sampleDataGridView.AllowUserToResizeRows = false;
			sampleDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			sampleDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			sampleDataGridView.GridStyles.StyleDataCells = Krypton.Toolkit.GridStyle.Sheet;
			sampleDataGridView.Location = new System.Drawing.Point(0, 0);
			sampleDataGridView.MultiSelect = false;
			sampleDataGridView.Name = "sampleDataGridView";
			sampleDataGridView.Palette = monoFontPalette;
			sampleDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			sampleDataGridView.ReadOnly = true;
			controlResource.SetResourceKey(sampleDataGridView, null);
			sampleDataGridView.RowHeadersVisible = false;
			sampleDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			sampleDataGridView.ShowCellErrors = false;
			sampleDataGridView.ShowEditingIcon = false;
			sampleDataGridView.ShowRowErrors = false;
			sampleDataGridView.Size = new System.Drawing.Size(412, 106);
			sampleDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			sampleDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			sampleDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			sampleDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right;
			sampleDataGridView.TabIndex = 0;
			sampleDataGridView.SelectionChanged += SampleDataGridView_SelectionChanged;
			sampleDataGridView.KeyDown += SamplesDataGridView_KeyDown;
			sampleDataGridView.KeyUp += SamplesDataGridView_KeyUp;
			// 
			// octaveLabel
			// 
			octaveLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			octaveLabel.Location = new System.Drawing.Point(4, 125);
			octaveLabel.Name = "octaveLabel";
			octaveLabel.Palette = fontPalette;
			octaveLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(octaveLabel, null);
			octaveLabel.Size = new System.Drawing.Size(16, 16);
			octaveLabel.TabIndex = 1;
			octaveLabel.Values.Text = "?";
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// SampleInfoWindowForm
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			ClientSize = new System.Drawing.Size(448, 188);
			Controls.Add(navigator);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(464, 227);
			Name = "SampleInfoWindowForm";
			Palette = fontPalette;
			PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(this, null);
			FormClosed += SampleInfoWindowForm_FormClosed;
			((System.ComponentModel.ISupportInitialize)navigator).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorInstrumentPage).EndInit();
			navigatorInstrumentPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)instrumentGroup.Panel).EndInit();
			instrumentGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)instrumentGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)instrumentDataGridView).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorSamplePage).EndInit();
			navigatorSamplePage.ResumeLayout(false);
			navigatorSamplePage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)saveFormatComboBox).EndInit();
			((System.ComponentModel.ISupportInitialize)sampleGroup.Panel).EndInit();
			sampleGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)sampleGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)sampleDataGridView).EndInit();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private Krypton.Navigator.KryptonNavigator navigator;
		private Krypton.Navigator.KryptonPage navigatorInstrumentPage;
		private Krypton.Navigator.KryptonPage navigatorSamplePage;
		private Krypton.Toolkit.KryptonGroup instrumentGroup;
		private Krypton.Toolkit.KryptonDataGridView instrumentDataGridView;
		private Krypton.Toolkit.KryptonGroup sampleGroup;
		private SampleInfoSamplesListControl sampleDataGridView;
		private Krypton.Toolkit.KryptonLabel octaveLabel;
		private Krypton.Toolkit.KryptonLabel polyphonyLabel;
		private Krypton.Toolkit.KryptonButton saveButton;
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonComboBox saveFormatComboBox;
		private Krypton.Toolkit.KryptonLabel saveFormatLabel;
		private GuiKit.Components.FontPalette fontPalette;
		private GuiKit.Components.FontPalette monoFontPalette;
	}
}