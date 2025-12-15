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
			favoriteDataGridView = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicDataGridView();
			fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
			addButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicButton();
			bigFontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			removeButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicButton();
			resetButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicButton();
			favoriteGroup = new Krypton.Toolkit.KryptonGroup();
			showComboBox = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicComboBox();
			otherNumberTextBox = new Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.NumberTextBox();
			toolTip = new System.Windows.Forms.ToolTip(components);
			((System.ComponentModel.ISupportInitialize)favoriteDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			((System.ComponentModel.ISupportInitialize)favoriteGroup).BeginInit();
			((System.ComponentModel.ISupportInitialize)favoriteGroup.Panel).BeginInit();
			favoriteGroup.Panel.SuspendLayout();
			SuspendLayout();
			// 
			// favoriteDataGridView
			// 
			favoriteDataGridView.AllowUserToOrderColumns = true;
			favoriteDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			favoriteDataGridView.Location = new System.Drawing.Point(0, 0);
			favoriteDataGridView.Name = "favoriteDataGridView";
			favoriteDataGridView.Size = new System.Drawing.Size(404, 182);
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
			controlResource.SetResourceKey(addButton, "IDS_FAVORITE_BUTTON_ADD");
			addButton.Size = new System.Drawing.Size(90, 21);
			addButton.TabIndex = 1;
			addButton.Text = "Add";
			addButton.UseFont = bigFontConfiguration;
			addButton.Click += AddButton_Click;
			// 
			// bigFontConfiguration
			// 
			bigFontConfiguration.FontSize = 1;
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
			controlResource.SetResourceKey(removeButton, "IDS_FAVORITE_BUTTON_REMOVE");
			removeButton.Size = new System.Drawing.Size(90, 21);
			removeButton.TabIndex = 2;
			removeButton.Text = "Remove";
			removeButton.UseFont = bigFontConfiguration;
			removeButton.Click += RemoveButton_Click;
			// 
			// resetButton
			// 
			resetButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			resetButton.Location = new System.Drawing.Point(324, 200);
			resetButton.Name = "resetButton";
			controlResource.SetResourceKey(resetButton, "IDS_FAVORITE_BUTTON_RESET");
			resetButton.Size = new System.Drawing.Size(90, 21);
			resetButton.TabIndex = 5;
			resetButton.Text = "Reset";
			resetButton.UseFont = bigFontConfiguration;
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
			favoriteGroup.Size = new System.Drawing.Size(406, 184);
			favoriteGroup.TabIndex = 0;
			// 
			// showComboBox
			// 
			showComboBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			showComboBox.DropDownWidth = 121;
			showComboBox.IntegralHeight = false;
			showComboBox.Location = new System.Drawing.Point(196, 200);
			showComboBox.Name = "showComboBox";
			showComboBox.Size = new System.Drawing.Size(90, 21);
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
			FormClosed += FavoriteSongSystemWindowForm_FormClosed;
			((System.ComponentModel.ISupportInitialize)favoriteDataGridView).EndInit();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			((System.ComponentModel.ISupportInitialize)favoriteGroup.Panel).EndInit();
			favoriteGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)favoriteGroup).EndInit();
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion
		private Polycode.NostalgicPlayer.Controls.Lists.NostalgicDataGridView favoriteDataGridView;
		private NostalgicPlayer.Controls.Buttons.NostalgicButton addButton;
		private Kit.Gui.Designer.ControlResource controlResource;
		private NostalgicPlayer.Controls.Buttons.NostalgicButton removeButton;
		private NostalgicPlayer.Controls.Buttons.NostalgicButton resetButton;
		private Krypton.Toolkit.KryptonGroup favoriteGroup;
		private NostalgicPlayer.Controls.Lists.NostalgicComboBox showComboBox;
		private Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.NumberTextBox otherNumberTextBox;
		private System.Windows.Forms.ToolTip toolTip;
		private Kit.Gui.Components.FontPalette fontPalette;
		private NostalgicPlayer.Controls.Components.FontConfiguration bigFontConfiguration;
	}
}