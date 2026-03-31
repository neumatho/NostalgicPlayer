namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages
{
	partial class InfoPageControl
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
			moduleInfoInfoDataGridView = new ModuleInfoListControl();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			infoGroup = new Krypton.Toolkit.KryptonGroup();
			((System.ComponentModel.ISupportInitialize)moduleInfoInfoDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)infoGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)infoGroup.Panel).BeginInit();
			infoGroup.Panel.SuspendLayout();
			SuspendLayout();
			// 
			// moduleInfoInfoDataGridView
			// 
			moduleInfoInfoDataGridView.AllowUserToAddRows = false;
			moduleInfoInfoDataGridView.AllowUserToDeleteRows = false;
			moduleInfoInfoDataGridView.AllowUserToResizeRows = false;
			moduleInfoInfoDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			moduleInfoInfoDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			moduleInfoInfoDataGridView.Location = new System.Drawing.Point(0, 0);
			moduleInfoInfoDataGridView.Name = "moduleInfoInfoDataGridView";
			moduleInfoInfoDataGridView.Palette = fontPalette;
			moduleInfoInfoDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			moduleInfoInfoDataGridView.ReadOnly = true;
			moduleInfoInfoDataGridView.RowHeadersVisible = false;
			moduleInfoInfoDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			moduleInfoInfoDataGridView.ShowCellErrors = false;
			moduleInfoInfoDataGridView.ShowEditingIcon = false;
			moduleInfoInfoDataGridView.ShowRowErrors = false;
			moduleInfoInfoDataGridView.Size = new System.Drawing.Size(264, 140);
			moduleInfoInfoDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			moduleInfoInfoDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			moduleInfoInfoDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			moduleInfoInfoDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right;
			moduleInfoInfoDataGridView.TabIndex = 0;
			moduleInfoInfoDataGridView.CellContentClick += ModuleInfoInfoDataGridView_CellContentClick;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// infoGroup
			// 
			infoGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			infoGroup.Location = new System.Drawing.Point(8, 8);
			infoGroup.Name = "infoGroup";
			// 
			// 
			// 
			infoGroup.Panel.Controls.Add(moduleInfoInfoDataGridView);
			infoGroup.Size = new System.Drawing.Size(266, 142);
			infoGroup.TabIndex = 0;
			// 
			// InfoPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(infoGroup);
			Name = "InfoPageControl";
			Size = new System.Drawing.Size(282, 158);
			((System.ComponentModel.ISupportInitialize)moduleInfoInfoDataGridView).EndInit();
			((System.ComponentModel.ISupportInitialize)infoGroup.Panel).EndInit();
			infoGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)infoGroup).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private ModuleInfoListControl moduleInfoInfoDataGridView;
		private Kit.Gui.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonGroup infoGroup;
	}
}
