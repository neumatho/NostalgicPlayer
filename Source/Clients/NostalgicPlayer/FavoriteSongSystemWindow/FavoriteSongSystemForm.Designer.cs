namespace Polycode.NostalgicPlayer.Client.GuiPlayer.FavoriteSongSystemWindow
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FavoriteSongSystemForm));
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.favoriteDataGridView = new Krypton.Toolkit.KryptonDataGridView();
			this.addButton = new Krypton.Toolkit.KryptonButton();
			this.controlResource = new Polycode.NostalgicPlayer.GuiKit.Designer.ControlResource();
			this.removeButton = new Krypton.Toolkit.KryptonButton();
			this.resetButton = new Krypton.Toolkit.KryptonButton();
			this.favoriteGroup = new Krypton.Toolkit.KryptonGroup();
			this.showComboBox = new Krypton.Toolkit.KryptonComboBox();
			this.otherNumberTextBox = new Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.NumberTextBox();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.favoriteDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.favoriteGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.favoriteGroup.Panel)).BeginInit();
			this.favoriteGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.showComboBox)).BeginInit();
			this.SuspendLayout();
			// 
			// kryptonManager
			// 
			this.kryptonManager.GlobalPaletteMode = Krypton.Toolkit.PaletteModeManager.Office2010Blue;
			// 
			// favoriteDataGridView
			// 
			this.favoriteDataGridView.AllowUserToAddRows = false;
			this.favoriteDataGridView.AllowUserToDeleteRows = false;
			this.favoriteDataGridView.AllowUserToOrderColumns = true;
			this.favoriteDataGridView.AllowUserToResizeRows = false;
			this.favoriteDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.favoriteDataGridView.GridStyles.StyleDataCells = Krypton.Toolkit.GridStyle.Sheet;
			this.favoriteDataGridView.Location = new System.Drawing.Point(0, 0);
			this.favoriteDataGridView.Name = "favoriteDataGridView";
			this.favoriteDataGridView.ReadOnly = true;
			this.controlResource.SetResourceKey(this.favoriteDataGridView, null);
			this.favoriteDataGridView.RowHeadersVisible = false;
			this.favoriteDataGridView.RowTemplate.Height = 25;
			this.favoriteDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.favoriteDataGridView.ShowCellErrors = false;
			this.favoriteDataGridView.ShowEditingIcon = false;
			this.favoriteDataGridView.ShowRowErrors = false;
			this.favoriteDataGridView.Size = new System.Drawing.Size(404, 182);
			this.favoriteDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			this.favoriteDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			this.favoriteDataGridView.StateCommon.DataCell.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			this.favoriteDataGridView.StateCommon.DataCell.Content.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.favoriteDataGridView.StateCommon.DataCell.Content.Padding = new System.Windows.Forms.Padding(0);
			this.favoriteDataGridView.StateCommon.HeaderColumn.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Right)));
			this.favoriteDataGridView.StateCommon.HeaderColumn.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.favoriteDataGridView.TabIndex = 0;
			this.favoriteDataGridView.SelectionChanged += new System.EventHandler(this.FavoriteDataGridView_SelectionChanged);
			this.favoriteDataGridView.DoubleClick += new System.EventHandler(this.FavoriteDataGridView_DoubleClick);
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addButton.Enabled = false;
			this.addButton.Location = new System.Drawing.Point(8, 196);
			this.addButton.Name = "addButton";
			this.controlResource.SetResourceKey(this.addButton, "IDS_FAVORITE_BUTTON_ADD");
			this.addButton.Size = new System.Drawing.Size(90, 21);
			this.addButton.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.addButton.TabIndex = 1;
			this.addButton.Values.Text = "Add";
			this.addButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// removeButton
			// 
			this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.removeButton.Enabled = false;
			this.removeButton.Location = new System.Drawing.Point(102, 196);
			this.removeButton.Name = "removeButton";
			this.controlResource.SetResourceKey(this.removeButton, "IDS_FAVORITE_BUTTON_REMOVE");
			this.removeButton.Size = new System.Drawing.Size(90, 21);
			this.removeButton.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.removeButton.TabIndex = 2;
			this.removeButton.Values.Text = "Remove";
			this.removeButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// resetButton
			// 
			this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.resetButton.Location = new System.Drawing.Point(324, 196);
			this.resetButton.Name = "resetButton";
			this.controlResource.SetResourceKey(this.resetButton, "IDS_FAVORITE_BUTTON_RESET");
			this.resetButton.Size = new System.Drawing.Size(90, 21);
			this.resetButton.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.resetButton.TabIndex = 5;
			this.resetButton.Values.Text = "Reset";
			this.resetButton.Click += new System.EventHandler(this.ResetButton_Click);
			// 
			// favoriteGroup
			// 
			this.favoriteGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.favoriteGroup.Location = new System.Drawing.Point(8, 8);
			this.favoriteGroup.Name = "favoriteGroup";
			// 
			// 
			// 
			this.favoriteGroup.Panel.Controls.Add(this.favoriteDataGridView);
			this.controlResource.SetResourceKey(this.favoriteGroup, null);
			this.favoriteGroup.Size = new System.Drawing.Size(406, 184);
			this.favoriteGroup.TabIndex = 0;
			// 
			// showComboBox
			// 
			this.showComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.showComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.showComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.showComboBox.DropDownWidth = 121;
			this.showComboBox.IntegralHeight = false;
			this.showComboBox.Location = new System.Drawing.Point(196, 197);
			this.showComboBox.Name = "showComboBox";
			this.controlResource.SetResourceKey(this.showComboBox, null);
			this.showComboBox.Size = new System.Drawing.Size(90, 19);
			this.showComboBox.StateCommon.ComboBox.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.showComboBox.StateCommon.ComboBox.Content.TextH = Krypton.Toolkit.PaletteRelativeAlign.Near;
			this.showComboBox.StateCommon.Item.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.showComboBox.TabIndex = 3;
			this.showComboBox.SelectedIndexChanged += new System.EventHandler(this.ShowComboBox_SelectedIndexChanged);
			// 
			// otherNumberTextBox
			// 
			this.otherNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.otherNumberTextBox.Enabled = false;
			this.otherNumberTextBox.Location = new System.Drawing.Point(290, 196);
			this.otherNumberTextBox.Name = "otherNumberTextBox";
			this.controlResource.SetResourceKey(this.otherNumberTextBox, null);
			this.otherNumberTextBox.Size = new System.Drawing.Size(30, 21);
			this.otherNumberTextBox.StateCommon.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.otherNumberTextBox.TabIndex = 4;
			this.otherNumberTextBox.TextChanged += new System.EventHandler(this.OtherNumberTextBox_TextChanged);
			// 
			// FavoriteSongSystemForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(422, 221);
			this.Controls.Add(this.otherNumberTextBox);
			this.Controls.Add(this.showComboBox);
			this.Controls.Add(this.favoriteGroup);
			this.Controls.Add(this.resetButton);
			this.Controls.Add(this.removeButton);
			this.Controls.Add(this.addButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(438, 260);
			this.Name = "FavoriteSongSystemForm";
			this.controlResource.SetResourceKey(this, null);
			this.Text = "FavoriteSongSystemForm";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FavoriteSongSystemWindowForm_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.favoriteDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.favoriteGroup.Panel)).EndInit();
			this.favoriteGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.favoriteGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.showComboBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private Krypton.Toolkit.KryptonDataGridView favoriteDataGridView;
		private Krypton.Toolkit.KryptonButton addButton;
		private GuiKit.Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonButton removeButton;
		private Krypton.Toolkit.KryptonButton resetButton;
		private Krypton.Toolkit.KryptonGroup favoriteGroup;
		private Krypton.Toolkit.KryptonComboBox showComboBox;
		private Polycode.NostalgicPlayer.Client.GuiPlayer.Controls.NumberTextBox otherNumberTextBox;
		private System.Windows.Forms.ToolTip toolTip;
	}
}