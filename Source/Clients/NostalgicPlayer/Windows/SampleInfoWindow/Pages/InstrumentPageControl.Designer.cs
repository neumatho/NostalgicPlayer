namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow.Pages
{
	partial class InstrumentPageControl
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
			instrumentGroup = new Krypton.Toolkit.KryptonGroup();
			instrumentDataGridView = new Krypton.Toolkit.KryptonDataGridView();
			monoFontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			((System.ComponentModel.ISupportInitialize)instrumentGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)instrumentGroup.Panel).BeginInit();
			instrumentGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)instrumentDataGridView).BeginInit();
			SuspendLayout();
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
			// InstrumentPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(instrumentGroup);
			Name = "InstrumentPageControl";
			Size = new System.Drawing.Size(430, 146);
			((System.ComponentModel.ISupportInitialize)instrumentGroup.Panel).EndInit();
			instrumentGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)instrumentGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)instrumentDataGridView).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private Krypton.Toolkit.KryptonGroup instrumentGroup;
		private Krypton.Toolkit.KryptonDataGridView instrumentDataGridView;
		private Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette monoFontPalette;
	}
}
