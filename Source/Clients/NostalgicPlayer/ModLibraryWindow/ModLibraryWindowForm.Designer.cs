namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	partial class ModLibraryWindowForm
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
            fontPalette = new Polycode.NostalgicPlayer.Kit.Gui.Components.FontPalette(components);
            serviceContextMenu = new Krypton.Toolkit.KryptonContextMenu();
            updateDatabaseMenuItem = new Krypton.Toolkit.KryptonContextMenuItems();
            updateDatabaseItem = new Krypton.Toolkit.KryptonContextMenuItem();
            clearDatabaseItem = new Krypton.Toolkit.KryptonContextMenuItem();
            offlineContextMenu = new Krypton.Toolkit.KryptonContextMenu();
            offlineContextMenuItems = new Krypton.Toolkit.KryptonContextMenuItems();
            deleteItem = new Krypton.Toolkit.KryptonContextMenuItem();
            flatViewContextMenu = new Krypton.Toolkit.KryptonContextMenu();
            flatViewContextMenuItems = new Krypton.Toolkit.KryptonContextMenuItems();
            jumpToFolderItem = new Krypton.Toolkit.KryptonContextMenuItem();
            searchLabel = new Krypton.Toolkit.KryptonLabel();
            searchPanel = new System.Windows.Forms.Panel();
            searchTextBox = new Krypton.Toolkit.KryptonTextBox();
            searchModeComboBox = new Krypton.Toolkit.KryptonComboBox();
            modeTabControl = new System.Windows.Forms.TabControl();
            onlineTabPage = new System.Windows.Forms.TabPage();
            offlineTabPage = new System.Windows.Forms.TabPage();
            parentButton = new Krypton.Toolkit.KryptonButton();
            breadcrumbLabel = new Krypton.Toolkit.KryptonLabel();
            flatViewCheckBox = new Krypton.Toolkit.KryptonCheckBox();
            moduleListView = new System.Windows.Forms.ListView();
            columnName = new System.Windows.Forms.ColumnHeader();
            columnPath = new System.Windows.Forms.ColumnHeader();
            columnSize = new System.Windows.Forms.ColumnHeader();
            playImmediatelyCheckBox = new Krypton.Toolkit.KryptonCheckBox();
            statusStrip = new System.Windows.Forms.StatusStrip();
            statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            progressBar = new System.Windows.Forms.ToolStripProgressBar();
            searchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)searchModeComboBox).BeginInit();
            modeTabControl.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // fontPalette
            // 
            fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
            fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
            fontPalette.ThemeName = "";
            fontPalette.UseKryptonFileDialogs = true;
            // 
            // serviceContextMenu
            // 
            serviceContextMenu.Items.AddRange(new Krypton.Toolkit.KryptonContextMenuItemBase[] { updateDatabaseMenuItem });
            // 
            // updateDatabaseMenuItem
            // 
            updateDatabaseMenuItem.Items.AddRange(new Krypton.Toolkit.KryptonContextMenuItemBase[] { updateDatabaseItem, clearDatabaseItem });
            // 
            // updateDatabaseItem
            // 
            updateDatabaseItem.Text = "Update Database";
            updateDatabaseItem.Click += UpdateDatabaseItem_Click;
            // 
            // clearDatabaseItem
            // 
            clearDatabaseItem.Text = "Clear Database";
            clearDatabaseItem.Click += ClearDatabaseItem_Click;
            // 
            // offlineContextMenu
            // 
            offlineContextMenu.Items.AddRange(new Krypton.Toolkit.KryptonContextMenuItemBase[] { offlineContextMenuItems });
            // 
            // offlineContextMenuItems
            // 
            offlineContextMenuItems.Items.AddRange(new Krypton.Toolkit.KryptonContextMenuItemBase[] { deleteItem });
            // 
            // deleteItem
            // 
            deleteItem.Text = "Delete";
            deleteItem.Click += DeleteItem_Click;
            // 
            // flatViewContextMenu
            // 
            flatViewContextMenu.Items.AddRange(new Krypton.Toolkit.KryptonContextMenuItemBase[] { flatViewContextMenuItems });
            // 
            // flatViewContextMenuItems
            // 
            flatViewContextMenuItems.Items.AddRange(new Krypton.Toolkit.KryptonContextMenuItemBase[] { jumpToFolderItem });
            // 
            // jumpToFolderItem
            // 
            jumpToFolderItem.Text = "Jump to folder";
            jumpToFolderItem.Click += JumpToFolderItem_Click;
            // 
            // searchLabel
            // 
            searchLabel.Dock = System.Windows.Forms.DockStyle.Top;
            searchLabel.Location = new System.Drawing.Point(8, 8);
            searchLabel.Name = "searchLabel";
            searchLabel.Size = new System.Drawing.Size(884, 29);
            searchLabel.TabIndex = 0;
            searchLabel.Values.Text = "🔍 Search:";
            // 
            // searchPanel
            // 
            searchPanel.Controls.Add(searchTextBox);
            searchPanel.Controls.Add(searchModeComboBox);
            searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            searchPanel.Location = new System.Drawing.Point(8, 37);
            searchPanel.Name = "searchPanel";
            searchPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            searchPanel.Size = new System.Drawing.Size(884, 31);
            searchPanel.TabIndex = 1;
            // 
            // searchTextBox
            // 
            searchTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            searchTextBox.Location = new System.Drawing.Point(0, 0);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new System.Drawing.Size(634, 31);
            searchTextBox.TabIndex = 0;
            searchTextBox.TextChanged += SearchTextBox_TextChanged;
            // 
            // searchModeComboBox
            // 
            searchModeComboBox.Dock = System.Windows.Forms.DockStyle.Right;
            searchModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            searchModeComboBox.DropDownWidth = 200;
            searchModeComboBox.IntegralHeight = false;
            searchModeComboBox.Items.AddRange(new object[] { "Filename and path", "Filename only", "Path only" });
            searchModeComboBox.Location = new System.Drawing.Point(634, 0);
            searchModeComboBox.Name = "searchModeComboBox";
            searchModeComboBox.Size = new System.Drawing.Size(250, 30);
            searchModeComboBox.TabIndex = 1;
            searchModeComboBox.Text = "Filename and path";
            searchModeComboBox.SelectedIndexChanged += SearchModeComboBox_SelectedIndexChanged;
            // 
            // modeTabControl
            // 
            modeTabControl.Controls.Add(onlineTabPage);
            modeTabControl.Controls.Add(offlineTabPage);
            modeTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            modeTabControl.Location = new System.Drawing.Point(8, 501);
            modeTabControl.Name = "modeTabControl";
            modeTabControl.Padding = new System.Drawing.Point(10, 5);
            modeTabControl.SelectedIndex = 0;
            modeTabControl.Size = new System.Drawing.Size(884, 30);
            modeTabControl.TabIndex = 6;
            modeTabControl.SelectedIndexChanged += ModeTabControl_SelectedIndexChanged;
            // 
            // onlineTabPage
            // 
            onlineTabPage.Location = new System.Drawing.Point(4, 38);
            onlineTabPage.Name = "onlineTabPage";
            onlineTabPage.Padding = new System.Windows.Forms.Padding(3);
            onlineTabPage.Size = new System.Drawing.Size(876, 0);
            onlineTabPage.TabIndex = 0;
            onlineTabPage.Text = "🌐 Online";
            onlineTabPage.UseVisualStyleBackColor = true;
            // 
            // offlineTabPage
            // 
            offlineTabPage.Location = new System.Drawing.Point(4, 38);
            offlineTabPage.Name = "offlineTabPage";
            offlineTabPage.Padding = new System.Windows.Forms.Padding(3);
            offlineTabPage.Size = new System.Drawing.Size(876, 0);
            offlineTabPage.TabIndex = 1;
            offlineTabPage.Text = "💾 Local";
            offlineTabPage.UseVisualStyleBackColor = true;
            // 
            // parentButton
            // 
            parentButton.Dock = System.Windows.Forms.DockStyle.Top;
            parentButton.Enabled = false;
            parentButton.Location = new System.Drawing.Point(8, 68);
            parentButton.Name = "parentButton";
            parentButton.Size = new System.Drawing.Size(884, 33);
            parentButton.TabIndex = 3;
            parentButton.Values.Text = "⬆ Parent";
            parentButton.Click += ParentButton_Click;
            // 
            // breadcrumbLabel
            // 
            breadcrumbLabel.Dock = System.Windows.Forms.DockStyle.Top;
            breadcrumbLabel.Location = new System.Drawing.Point(8, 101);
            breadcrumbLabel.Name = "breadcrumbLabel";
            breadcrumbLabel.Size = new System.Drawing.Size(884, 2);
            breadcrumbLabel.TabIndex = 2;
            breadcrumbLabel.Values.Text = "";
            // 
            // flatViewCheckBox
            // 
            flatViewCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            flatViewCheckBox.Location = new System.Drawing.Point(8, 103);
            flatViewCheckBox.Name = "flatViewCheckBox";
            flatViewCheckBox.Size = new System.Drawing.Size(884, 29);
            flatViewCheckBox.TabIndex = 4;
            flatViewCheckBox.Values.Text = "📋 Flat view (show all files without folder structure)";
            flatViewCheckBox.CheckedChanged += FlatViewCheckBox_CheckedChanged;
            // 
            // moduleListView
            // 
            moduleListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnName, columnPath, columnSize });
            moduleListView.Dock = System.Windows.Forms.DockStyle.Fill;
            moduleListView.FullRowSelect = true;
            moduleListView.Location = new System.Drawing.Point(8, 132);
            moduleListView.Name = "moduleListView";
            moduleListView.Size = new System.Drawing.Size(884, 369);
            moduleListView.TabIndex = 4;
            moduleListView.UseCompatibleStateImageBehavior = false;
            moduleListView.View = System.Windows.Forms.View.Details;
            moduleListView.VirtualMode = true;
            moduleListView.ColumnClick += ModuleListView_ColumnClick;
            moduleListView.RetrieveVirtualItem += ModuleListView_RetrieveVirtualItem;
            moduleListView.DoubleClick += ModuleListView_DoubleClick;
            moduleListView.KeyDown += ModuleListView_KeyDown;
            moduleListView.KeyPress += ModuleListView_KeyPress;
            moduleListView.MouseClick += ModuleListView_MouseClick;
            // 
            // columnName
            // 
            columnName.Text = "Name";
            columnName.Width = 400;
            // 
            // columnPath
            // 
            columnPath.Text = "Path";
            columnPath.Width = 0;
            // 
            // columnSize
            // 
            columnSize.Text = "Size";
            columnSize.Width = 100;
            // 
            // playImmediatelyCheckBox
            // 
            playImmediatelyCheckBox.Checked = true;
            playImmediatelyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            playImmediatelyCheckBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            playImmediatelyCheckBox.Location = new System.Drawing.Point(8, 531);
            playImmediatelyCheckBox.Name = "playImmediatelyCheckBox";
            playImmediatelyCheckBox.Size = new System.Drawing.Size(884, 29);
            playImmediatelyCheckBox.TabIndex = 5;
            playImmediatelyCheckBox.Values.Text = "▶ Play immediately (otherwise just add to playlist)";
            playImmediatelyCheckBox.CheckedChanged += PlayImmediatelyCheckBox_CheckedChanged;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusLabel, progressBar });
            statusStrip.Location = new System.Drawing.Point(8, 560);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new System.Windows.Forms.Padding(0, 3, 0, 5);
            statusStrip.Size = new System.Drawing.Size(884, 40);
            statusStrip.TabIndex = 8;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new System.Drawing.Size(884, 25);
            statusLabel.Spring = true;
            statusLabel.Text = "Right-click on a service to update its database";
            statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(200, 24);
            progressBar.Visible = false;
            // 
            // ModLibraryWindowForm
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(900, 600);
            Controls.Add(moduleListView);
            Controls.Add(flatViewCheckBox);
            Controls.Add(breadcrumbLabel);
            Controls.Add(parentButton);
            Controls.Add(searchPanel);
            Controls.Add(searchLabel);
            Controls.Add(modeTabControl);
            Controls.Add(playImmediatelyCheckBox);
            Controls.Add(statusStrip);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(600, 480);
            Name = "ModLibraryWindowForm";
            Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
            Palette = fontPalette;
            PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
            FormClosed += ModLibraryForm_FormClosed;
            Shown += ModLibraryForm_Shown;
            searchPanel.ResumeLayout(false);
            searchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)searchModeComboBox).EndInit();
            modeTabControl.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Kit.Gui.Components.FontPalette fontPalette;
		private Krypton.Toolkit.KryptonContextMenu serviceContextMenu;
		private Krypton.Toolkit.KryptonContextMenuItems updateDatabaseMenuItem;
		private Krypton.Toolkit.KryptonContextMenuItem updateDatabaseItem;
		private Krypton.Toolkit.KryptonContextMenuItem clearDatabaseItem;
		private Krypton.Toolkit.KryptonContextMenu offlineContextMenu;
		private Krypton.Toolkit.KryptonContextMenuItems offlineContextMenuItems;
		private Krypton.Toolkit.KryptonContextMenuItem deleteItem;
		private Krypton.Toolkit.KryptonContextMenu flatViewContextMenu;
		private Krypton.Toolkit.KryptonContextMenuItems flatViewContextMenuItems;
		private Krypton.Toolkit.KryptonContextMenuItem jumpToFolderItem;
		private Krypton.Toolkit.KryptonLabel searchLabel;
		private System.Windows.Forms.Panel searchPanel;
		private Krypton.Toolkit.KryptonTextBox searchTextBox;
		private Krypton.Toolkit.KryptonComboBox searchModeComboBox;
		private System.Windows.Forms.TabControl modeTabControl;
		private System.Windows.Forms.TabPage onlineTabPage;
		private System.Windows.Forms.TabPage offlineTabPage;
		private Krypton.Toolkit.KryptonButton parentButton;
		private Krypton.Toolkit.KryptonLabel breadcrumbLabel;
		private System.Windows.Forms.ListView moduleListView;
		private System.Windows.Forms.ColumnHeader columnName;
		private System.Windows.Forms.ColumnHeader columnPath;
		private System.Windows.Forms.ColumnHeader columnSize;
		private Krypton.Toolkit.KryptonCheckBox flatViewCheckBox;
		private Krypton.Toolkit.KryptonCheckBox playImmediatelyCheckBox;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel statusLabel;
		private System.Windows.Forms.ToolStripProgressBar progressBar;
	}
}
