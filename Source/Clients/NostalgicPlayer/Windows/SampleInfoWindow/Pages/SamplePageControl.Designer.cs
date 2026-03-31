namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow.Pages
{
	partial class SamplePageControl
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
			saveFormatLabel = new Krypton.Toolkit.KryptonLabel();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			saveFormatComboBox = new Krypton.Toolkit.KryptonComboBox();
			saveButton = new Krypton.Toolkit.KryptonButton();
			polyphonyLabel = new Krypton.Toolkit.KryptonLabel();
			sampleGroup = new Krypton.Toolkit.KryptonGroup();
			sampleDataGridView = new SampleInfoSamplesListControl();
			monoFontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			octaveLabel = new Krypton.Toolkit.KryptonLabel();
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			((System.ComponentModel.ISupportInitialize)saveFormatComboBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)sampleGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)sampleGroup.Panel).BeginInit();
			sampleGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)sampleDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
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
			// monoFontPalette
			// 
			monoFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			monoFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			monoFontPalette.ThemeName = "";
			monoFontPalette.UseKryptonFileDialogs = true;
			monoFontPalette.UseMonospaceOnGrid = true;
			// 
			// octaveLabel
			// 
			octaveLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			octaveLabel.Location = new System.Drawing.Point(4, 125);
			octaveLabel.Name = "octaveLabel";
			octaveLabel.Palette = fontPalette;
			octaveLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			octaveLabel.Size = new System.Drawing.Size(16, 16);
			octaveLabel.TabIndex = 1;
			octaveLabel.Values.Text = "?";
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// SamplePageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(saveFormatLabel);
			Controls.Add(saveFormatComboBox);
			Controls.Add(saveButton);
			Controls.Add(polyphonyLabel);
			Controls.Add(sampleGroup);
			Controls.Add(octaveLabel);
			Name = "SamplePageControl";
			Size = new System.Drawing.Size(430, 146);
			((System.ComponentModel.ISupportInitialize)saveFormatComboBox).EndInit();
			((System.ComponentModel.ISupportInitialize)sampleGroup.Panel).EndInit();
			sampleGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)sampleGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)sampleDataGridView).EndInit();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private Krypton.Toolkit.KryptonLabel saveFormatLabel;
		private Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonComboBox saveFormatComboBox;
		private Krypton.Toolkit.KryptonButton saveButton;
		private Krypton.Toolkit.KryptonLabel polyphonyLabel;
		private Krypton.Toolkit.KryptonGroup sampleGroup;
		private SampleInfoSamplesListControl sampleDataGridView;
		private Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette monoFontPalette;
		private Krypton.Toolkit.KryptonLabel octaveLabel;
		private Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource controlResource;
	}
}
