namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.FavoriteSongSystemWindow
{
	partial class FavoriteSongSystemForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FavoriteSongSystemForm));
			favoriteDataGridView = new Krypton.Toolkit.KryptonDataGridView();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			addButton = new Krypton.Toolkit.KryptonButton();
			bigFontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			removeButton = new Krypton.Toolkit.KryptonButton();
			resetButton = new Krypton.Toolkit.KryptonButton();
			favoriteGroup = new Krypton.Toolkit.KryptonGroup();
			showComboBox = new Krypton.Toolkit.KryptonComboBox();
			otherNumberTextBox = new Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.NumberTextBox();
			toolTip = new System.Windows.Forms.ToolTip(components);
			((System.ComponentModel.ISupportInitialize)favoriteDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)favoriteGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)favoriteGroup.Panel).BeginInit();
			favoriteGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)showComboBox).BeginInit();
			SuspendLayout();
			// 
			// favoriteDataGridView
			// 
			favoriteDataGridView.AllowUserToAddRows = false;
			favoriteDataGridView.AllowUserToDeleteRows = false;
			favoriteDataGridView.AllowUserToOrderColumns = true;
			favoriteDataGridView.AllowUserToResizeRows = false;
			favoriteDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			favoriteDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			favoriteDataGridView.GridStyles.StyleDataCells = Krypton.Toolkit.GridStyle.Sheet;
			favoriteDataGridView.Location = new System.Drawing.Point(0, 0);
			favoriteDataGridView.Name = "favoriteDataGridView";
			favoriteDataGridView.Palette = fontPalette;
			favoriteDataGridView.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			favoriteDataGridView.ReadOnly = true;
			controlResource.SetResourceKey(favoriteDataGridView, null);
			favoriteDataGridView.RowHeadersVisible = false;
			favoriteDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			favoriteDataGridView.ShowCellErrors = false;
			favoriteDataGridView.ShowEditingIcon = false;
			favoriteDataGridView.ShowRowErrors = false;
			favoriteDataGridView.Size = new System.Drawing.Size(404, 182);
			favoriteDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			favoriteDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			favoriteDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			favoriteDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right;
			favoriteDataGridView.TabIndex = 0;
			favoriteDataGridView.SelectionChanged += FavoriteDataGridView_SelectionChanged;
			favoriteDataGridView.CellDoubleClick += FavoriteDataGridView_CellDoubleClick;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// addButton
			// 
			addButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			addButton.Enabled = false;
			addButton.Location = new System.Drawing.Point(8, 200);
			addButton.Name = "addButton";
			addButton.Palette = bigFontPalette;
			addButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(addButton, "IDS_FAVORITE_BUTTON_ADD");
			addButton.Size = new System.Drawing.Size(90, 21);
			addButton.TabIndex = 1;
			addButton.Values.Text = "Add";
			addButton.Click += AddButton_Click;
			// 
			// bigFontPalette
			// 
			bigFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			bigFontPalette.BaseFontSize = 9F;
			bigFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			bigFontPalette.ThemeName = "";
			bigFontPalette.UseKryptonFileDialogs = true;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// removeButton
			// 
			removeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			removeButton.Enabled = false;
			removeButton.Location = new System.Drawing.Point(102, 200);
			removeButton.Name = "removeButton";
			removeButton.Palette = bigFontPalette;
			removeButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(removeButton, "IDS_FAVORITE_BUTTON_REMOVE");
			removeButton.Size = new System.Drawing.Size(90, 21);
			removeButton.TabIndex = 2;
			removeButton.Values.Text = "Remove";
			removeButton.Click += RemoveButton_Click;
			// 
			// resetButton
			// 
			resetButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			resetButton.Location = new System.Drawing.Point(324, 200);
			resetButton.Name = "resetButton";
			resetButton.Palette = bigFontPalette;
			resetButton.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(resetButton, "IDS_FAVORITE_BUTTON_RESET");
			resetButton.Size = new System.Drawing.Size(90, 21);
			resetButton.TabIndex = 5;
			resetButton.Values.Text = "Reset";
			resetButton.Click += ResetButton_Click;
			// 
			// favoriteGroup
			// 
			favoriteGroup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			favoriteGroup.Location = new System.Drawing.Point(8, 8);
			favoriteGroup.Name = "favoriteGroup";
			// 
			// 
			// 
			favoriteGroup.Panel.Controls.Add(favoriteDataGridView);
			controlResource.SetResourceKey(favoriteGroup, null);
			favoriteGroup.Size = new System.Drawing.Size(406, 184);
			favoriteGroup.TabIndex = 0;
			// 
			// showComboBox
			// 
			showComboBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			showComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			showComboBox.DropDownWidth = 121;
			showComboBox.IntegralHeight = false;
			showComboBox.Location = new System.Drawing.Point(196, 202);
			showComboBox.Name = "showComboBox";
			showComboBox.Palette = fontPalette;
			showComboBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(showComboBox, null);
			showComboBox.Size = new System.Drawing.Size(90, 19);
			showComboBox.TabIndex = 3;
			showComboBox.SelectedIndexChanged += ShowComboBox_SelectedIndexChanged;
			// 
			// otherNumberTextBox
			// 
			otherNumberTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			otherNumberTextBox.Enabled = false;
			otherNumberTextBox.Location = new System.Drawing.Point(290, 201);
			otherNumberTextBox.Name = "otherNumberTextBox";
			otherNumberTextBox.Palette = fontPalette;
			otherNumberTextBox.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			controlResource.SetResourceKey(otherNumberTextBox, null);
			otherNumberTextBox.Size = new System.Drawing.Size(30, 20);
			otherNumberTextBox.TabIndex = 4;
			otherNumberTextBox.TextChanged += OtherNumberTextBox_TextChanged;
			// 
			// FavoriteSongSystemForm
			// 
			ClientSize = new System.Drawing.Size(422, 225);
			Controls.Add(otherNumberTextBox);
			Controls.Add(showComboBox);
			Controls.Add(favoriteGroup);
			Controls.Add(resetButton);
			Controls.Add(removeButton);
			Controls.Add(addButton);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(438, 264);
			Name = "FavoriteSongSystemForm";
			controlResource.SetResourceKey(this, null);
			FormClosed += FavoriteSongSystemWindowForm_FormClosed;
			((System.ComponentModel.ISupportInitialize)favoriteDataGridView).EndInit();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			((System.ComponentModel.ISupportInitialize)favoriteGroup.Panel).EndInit();
			favoriteGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)favoriteGroup).EndInit();
			((System.ComponentModel.ISupportInitialize)showComboBox).EndInit();
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion
		private Krypton.Toolkit.KryptonDataGridView favoriteDataGridView;
		private Krypton.Toolkit.KryptonButton addButton;
		private Kit.Gui.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonButton removeButton;
		private Krypton.Toolkit.KryptonButton resetButton;
		private Krypton.Toolkit.KryptonGroup favoriteGroup;
		private Krypton.Toolkit.KryptonComboBox showComboBox;
		private Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.NumberTextBox otherNumberTextBox;
		private System.Windows.Forms.ToolTip toolTip;
		private Kit.Gui.Components.FontPalette fontPalette;
		private Kit.Gui.Components.FontPalette bigFontPalette;
	}
}