
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SampleInfoWindowForm));
			this.navigator = new Krypton.Navigator.KryptonNavigator();
			this.navigatorInstrumentPage = new Krypton.Navigator.KryptonPage();
			this.instrumentGroup = new Krypton.Toolkit.KryptonGroup();
			this.instrumentDataGridView = new Krypton.Toolkit.KryptonDataGridView();
			this.monoFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(this.components);
			this.navigatorSamplePage = new Krypton.Navigator.KryptonPage();
			this.saveFormatLabel = new Krypton.Toolkit.KryptonLabel();
			this.fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(this.components);
			this.saveFormatComboBox = new Krypton.Toolkit.KryptonComboBox();
			this.saveButton = new Krypton.Toolkit.KryptonButton();
			this.polyphonyLabel = new Krypton.Toolkit.KryptonLabel();
			this.sampleGroup = new Krypton.Toolkit.KryptonGroup();
			this.sampleDataGridView = new Polycode.NostalgicPlayer.Client.GuiPlayer.SampleInfoWindow.SampleInfoSamplesListControl();
			this.octaveLabel = new Krypton.Toolkit.KryptonLabel();
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			((System.ComponentModel.ISupportInitialize)(this.navigator)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorInstrumentPage)).BeginInit();
			this.navigatorInstrumentPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.instrumentGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.instrumentGroup.Panel)).BeginInit();
			this.instrumentGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.instrumentDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorSamplePage)).BeginInit();
			this.navigatorSamplePage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.saveFormatComboBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sampleGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sampleGroup.Panel)).BeginInit();
			this.sampleGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sampleDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			this.SuspendLayout();
			// 
			// navigator
			// 
			this.navigator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.navigator.Button.CloseButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			this.navigator.Button.ContextButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			this.navigator.Location = new System.Drawing.Point(8, 8);
			this.navigator.Name = "navigator";
			this.navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] {
            this.navigatorInstrumentPage,
            this.navigatorSamplePage});
			this.navigator.Palette = this.fontPalette;
			this.navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			this.controlResource.SetResourceKey(this.navigator, null);
			this.navigator.SelectedIndex = 0;
			this.navigator.Size = new System.Drawing.Size(432, 172);
			this.navigator.TabIndex = 0;
			this.navigator.SelectedPageChanged += new System.EventHandler(this.Navigator_SelectedPageChanged);
			// 
			// navigatorInstrumentPage
			// 
			this.navigatorInstrumentPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorInstrumentPage.Controls.Add(this.instrumentGroup);
			this.navigatorInstrumentPage.Flags = 65534;
			this.navigatorInstrumentPage.LastVisibleSet = true;
			this.navigatorInstrumentPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorInstrumentPage.Name = "navigatorInstrumentPage";
			this.controlResource.SetResourceKey(this.navigatorInstrumentPage, null);
			this.navigatorInstrumentPage.Size = new System.Drawing.Size(430, 146);
			this.navigatorInstrumentPage.Text = "";
			this.navigatorInstrumentPage.ToolTipTitle = "Page ToolTip";
			this.navigatorInstrumentPage.UniqueName = "5d888e6082d44d78aac10a8a0c09a21e";
			// 
			// instrumentGroup
			// 
			this.instrumentGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.instrumentGroup.Location = new System.Drawing.Point(8, 8);
			this.instrumentGroup.Name = "instrumentGroup";
			// 
			// 
			// 
			this.instrumentGroup.Panel.Controls.Add(this.instrumentDataGridView);
			this.controlResource.SetResourceKey(this.instrumentGroup, null);
			this.instrumentGroup.Size = new System.Drawing.Size(414, 130);
			this.instrumentGroup.TabIndex = 0;
			// 
			// instrumentDataGridView
			// 
			this.instrumentDataGridView.AllowUserToAddRows = false;
			this.instrumentDataGridView.AllowUserToDeleteRows = false;
			this.instrumentDataGridView.AllowUserToOrderColumns = true;
			this.instrumentDataGridView.AllowUserToResizeRows = false;
			this.instrumentDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.instrumentDataGridView.Location = new System.Drawing.Point(0, 0);
			this.instrumentDataGridView.Name = "instrumentDataGridView";
			this.instrumentDataGridView.Palette = this.monoFontPalette;
			this.instrumentDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.instrumentDataGridView.ReadOnly = true;
			this.controlResource.SetResourceKey(this.instrumentDataGridView, null);
			this.instrumentDataGridView.RowHeadersVisible = false;
			this.instrumentDataGridView.RowTemplate.Height = 25;
			this.instrumentDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.instrumentDataGridView.ShowCellErrors = false;
			this.instrumentDataGridView.ShowEditingIcon = false;
			this.instrumentDataGridView.ShowRowErrors = false;
			this.instrumentDataGridView.Size = new System.Drawing.Size(412, 128);
			this.instrumentDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			this.instrumentDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			this.instrumentDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			this.instrumentDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.instrumentDataGridView.TabIndex = 0;
			// 
			// monoFontPalette
			// 
			this.monoFontPalette.UseMonospaceOnGrid = true;
			// 
			// navigatorSamplePage
			// 
			this.navigatorSamplePage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorSamplePage.Controls.Add(this.saveFormatLabel);
			this.navigatorSamplePage.Controls.Add(this.saveFormatComboBox);
			this.navigatorSamplePage.Controls.Add(this.saveButton);
			this.navigatorSamplePage.Controls.Add(this.polyphonyLabel);
			this.navigatorSamplePage.Controls.Add(this.sampleGroup);
			this.navigatorSamplePage.Controls.Add(this.octaveLabel);
			this.navigatorSamplePage.Flags = 65534;
			this.navigatorSamplePage.LastVisibleSet = true;
			this.navigatorSamplePage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorSamplePage.Name = "navigatorSamplePage";
			this.controlResource.SetResourceKey(this.navigatorSamplePage, null);
			this.navigatorSamplePage.Size = new System.Drawing.Size(430, 146);
			this.navigatorSamplePage.Text = "";
			this.navigatorSamplePage.ToolTipTitle = "Page ToolTip";
			this.navigatorSamplePage.UniqueName = "6f932f4ccf4b4441a28afbd3a888c881";
			// 
			// saveFormatLabel
			// 
			this.saveFormatLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveFormatLabel.Location = new System.Drawing.Point(123, 125);
			this.saveFormatLabel.Name = "saveFormatLabel";
			this.saveFormatLabel.Palette = this.fontPalette;
			this.saveFormatLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.saveFormatLabel, "IDS_SAMPLE_INFO_SAMP_SAVEFORMAT");
			this.saveFormatLabel.Size = new System.Drawing.Size(69, 16);
			this.saveFormatLabel.TabIndex = 3;
			this.saveFormatLabel.Values.Text = "Save format";
			// 
			// saveFormatComboBox
			// 
			this.saveFormatComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveFormatComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.saveFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.saveFormatComboBox.DropDownWidth = 120;
			this.saveFormatComboBox.IntegralHeight = false;
			this.saveFormatComboBox.Location = new System.Drawing.Point(198, 123);
			this.saveFormatComboBox.Name = "saveFormatComboBox";
			this.saveFormatComboBox.Palette = this.fontPalette;
			this.saveFormatComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.saveFormatComboBox, null);
			this.saveFormatComboBox.Size = new System.Drawing.Size(160, 18);
			this.saveFormatComboBox.TabIndex = 4;
			this.saveFormatComboBox.SelectedIndexChanged += new System.EventHandler(this.SaveFormatComboBox_SelectedIndexChanged);
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButton.Enabled = false;
			this.saveButton.Location = new System.Drawing.Point(362, 121);
			this.saveButton.Name = "saveButton";
			this.saveButton.Palette = this.fontPalette;
			this.saveButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.saveButton, "IDS_SAMPLE_INFO_SAMP_SAVE");
			this.saveButton.Size = new System.Drawing.Size(60, 21);
			this.saveButton.TabIndex = 5;
			this.saveButton.Values.Text = "Save";
			this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// polyphonyLabel
			// 
			this.polyphonyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.polyphonyLabel.Location = new System.Drawing.Point(75, 125);
			this.polyphonyLabel.Name = "polyphonyLabel";
			this.polyphonyLabel.Palette = this.fontPalette;
			this.polyphonyLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.polyphonyLabel, null);
			this.polyphonyLabel.Size = new System.Drawing.Size(16, 16);
			this.polyphonyLabel.TabIndex = 2;
			this.polyphonyLabel.Values.Text = "?";
			// 
			// sampleGroup
			// 
			this.sampleGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.sampleGroup.Location = new System.Drawing.Point(8, 8);
			this.sampleGroup.Name = "sampleGroup";
			// 
			// 
			// 
			this.sampleGroup.Panel.Controls.Add(this.sampleDataGridView);
			this.controlResource.SetResourceKey(this.sampleGroup, null);
			this.sampleGroup.Size = new System.Drawing.Size(414, 108);
			this.sampleGroup.TabIndex = 1;
			// 
			// sampleDataGridView
			// 
			this.sampleDataGridView.AllowUserToAddRows = false;
			this.sampleDataGridView.AllowUserToDeleteRows = false;
			this.sampleDataGridView.AllowUserToOrderColumns = true;
			this.sampleDataGridView.AllowUserToResizeRows = false;
			this.sampleDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sampleDataGridView.GridStyles.StyleDataCells = Krypton.Toolkit.GridStyle.Sheet;
			this.sampleDataGridView.Location = new System.Drawing.Point(0, 0);
			this.sampleDataGridView.Name = "sampleDataGridView";
			this.sampleDataGridView.Palette = this.monoFontPalette;
			this.sampleDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.sampleDataGridView.ReadOnly = true;
			this.controlResource.SetResourceKey(this.sampleDataGridView, null);
			this.sampleDataGridView.RowHeadersVisible = false;
			this.sampleDataGridView.RowTemplate.Height = 25;
			this.sampleDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.sampleDataGridView.ShowCellErrors = false;
			this.sampleDataGridView.ShowEditingIcon = false;
			this.sampleDataGridView.ShowRowErrors = false;
			this.sampleDataGridView.Size = new System.Drawing.Size(412, 106);
			this.sampleDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			this.sampleDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			this.sampleDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			this.sampleDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.sampleDataGridView.TabIndex = 0;
			this.sampleDataGridView.SelectionChanged += new System.EventHandler(this.SampleDataGridView_SelectionChanged);
			this.sampleDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SamplesDataGridView_KeyDown);
			this.sampleDataGridView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SamplesDataGridView_KeyUp);
			// 
			// octaveLabel
			// 
			this.octaveLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.octaveLabel.Location = new System.Drawing.Point(4, 125);
			this.octaveLabel.Name = "octaveLabel";
			this.octaveLabel.Palette = this.fontPalette;
			this.octaveLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this.octaveLabel, null);
			this.octaveLabel.Size = new System.Drawing.Size(16, 16);
			this.octaveLabel.TabIndex = 1;
			this.octaveLabel.Values.Text = "?";
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// SampleInfoWindowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(448, 188);
			this.Controls.Add(this.navigator);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(464, 227);
			this.Name = "SampleInfoWindowForm";
			this.Palette = this.fontPalette;
			this.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			this.controlResource.SetResourceKey(this, null);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SampleInfoWindowForm_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.navigator)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorInstrumentPage)).EndInit();
			this.navigatorInstrumentPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.instrumentGroup.Panel)).EndInit();
			this.instrumentGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.instrumentGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.instrumentDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorSamplePage)).EndInit();
			this.navigatorSamplePage.ResumeLayout(false);
			this.navigatorSamplePage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.saveFormatComboBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sampleGroup.Panel)).EndInit();
			this.sampleGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.sampleGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sampleDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			this.ResumeLayout(false);

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