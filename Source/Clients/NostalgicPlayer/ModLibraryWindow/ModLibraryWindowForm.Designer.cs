namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModLibraryWindow
{
	partial class ModLibraryWindowForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.ToolStripButton cancelDownloadButton = null;

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
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			searchLabel = new Krypton.Toolkit.KryptonLabel();
			onlineTabPage = new System.Windows.Forms.TabPage();
			offlineTabPage = new System.Windows.Forms.TabPage();
			parentButton = new Krypton.Toolkit.KryptonButton();
			flatViewCheckBox = new Krypton.Toolkit.KryptonCheckBox();
			playImmediatelyCheckBox = new Krypton.Toolkit.KryptonCheckBox();
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
			batchDownloadContextMenu = new Krypton.Toolkit.KryptonContextMenu();
			batchDownloadContextMenuItems = new Krypton.Toolkit.KryptonContextMenuItems();
			downloadSelectedItem = new Krypton.Toolkit.KryptonContextMenuItem();
			searchPanel = new System.Windows.Forms.Panel();
			searchButton = new Krypton.Toolkit.KryptonButton();
			searchTextBox = new Krypton.Toolkit.KryptonTextBox();
			searchModeComboBox = new Krypton.Toolkit.KryptonComboBox();
			modeTabControl = new System.Windows.Forms.TabControl();
			breadcrumbPanel = new System.Windows.Forms.FlowLayoutPanel();
			moduleListView = new System.Windows.Forms.ListView();
			columnName = new System.Windows.Forms.ColumnHeader();
			columnPath = new System.Windows.Forms.ColumnHeader();
			columnSize = new System.Windows.Forms.ColumnHeader();
			statusStrip = new System.Windows.Forms.StatusStrip();
			cancelDownloadButton = new System.Windows.Forms.ToolStripButton();
			statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			progressBar = new System.Windows.Forms.ToolStripProgressBar();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
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
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// searchLabel
			// 
			searchLabel.Dock = System.Windows.Forms.DockStyle.Top;
			searchLabel.Location = new System.Drawing.Point(8, 8);
			searchLabel.Name = "searchLabel";
			controlResource.SetResourceKey(searchLabel, "IDS_MODLIBRARY_SEARCH_LABEL");
			searchLabel.Size = new System.Drawing.Size(884, 20);
			searchLabel.TabIndex = 0;
			searchLabel.Values.Text = "Search:";
			// 
			// onlineTabPage
			// 
			onlineTabPage.Location = new System.Drawing.Point(4, 28);
			onlineTabPage.Name = "onlineTabPage";
			onlineTabPage.Padding = new System.Windows.Forms.Padding(3);
			controlResource.SetResourceKey(onlineTabPage, "IDS_MODLIBRARY_TAB_ONLINE");
			onlineTabPage.Size = new System.Drawing.Size(876, 0);
			onlineTabPage.TabIndex = 0;
			onlineTabPage.Text = "🌐 Online";
			onlineTabPage.UseVisualStyleBackColor = true;
			// 
			// offlineTabPage
			// 
			offlineTabPage.Location = new System.Drawing.Point(4, 28);
			offlineTabPage.Name = "offlineTabPage";
			offlineTabPage.Padding = new System.Windows.Forms.Padding(3);
			controlResource.SetResourceKey(offlineTabPage, "IDS_MODLIBRARY_TAB_OFFLINE");
			offlineTabPage.Size = new System.Drawing.Size(876, 0);
			offlineTabPage.TabIndex = 1;
			offlineTabPage.Text = "💾 Local";
			offlineTabPage.UseVisualStyleBackColor = true;
			// 
			// parentButton
			// 
			parentButton.Dock = System.Windows.Forms.DockStyle.Top;
			parentButton.Enabled = false;
			parentButton.Location = new System.Drawing.Point(8, 59);
			parentButton.Name = "parentButton";
			controlResource.SetResourceKey(parentButton, "IDS_MODLIBRARY_PARENT_BUTTON");
			parentButton.Size = new System.Drawing.Size(884, 33);
			parentButton.TabIndex = 3;
			parentButton.Values.Text = "⬆ Parent Folder";
			parentButton.Click += ParentButton_Click;
			// 
			// flatViewCheckBox
			// 
			flatViewCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
			flatViewCheckBox.Location = new System.Drawing.Point(8, 127);
			flatViewCheckBox.Name = "flatViewCheckBox";
			controlResource.SetResourceKey(flatViewCheckBox, "IDS_MODLIBRARY_FLATVIEW_CHECKBOX");
			flatViewCheckBox.Size = new System.Drawing.Size(884, 20);
			flatViewCheckBox.TabIndex = 4;
			flatViewCheckBox.Values.Text = "Flat view (show all files recursively)";
			flatViewCheckBox.CheckedChanged += FlatViewCheckBox_CheckedChanged;
			// 
			// playImmediatelyCheckBox
			// 
			playImmediatelyCheckBox.Checked = true;
			playImmediatelyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			playImmediatelyCheckBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			playImmediatelyCheckBox.Location = new System.Drawing.Point(8, 558);
			playImmediatelyCheckBox.Name = "playImmediatelyCheckBox";
			controlResource.SetResourceKey(playImmediatelyCheckBox, "IDS_MODLIBRARY_PLAYIMMEDIATELY_CHECKBOX");
			playImmediatelyCheckBox.Size = new System.Drawing.Size(884, 20);
			playImmediatelyCheckBox.TabIndex = 5;
			playImmediatelyCheckBox.Values.Text = "Play Immediately";
			playImmediatelyCheckBox.CheckedChanged += PlayImmediatelyCheckBox_CheckedChanged;
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
			jumpToFolderItem.Text = "Jump to Folder";
			jumpToFolderItem.Click += JumpToFolderItem_Click;
			//
			// batchDownloadContextMenu
			//
			batchDownloadContextMenu.Items.AddRange(new Krypton.Toolkit.KryptonContextMenuItemBase[] { batchDownloadContextMenuItems });
			//
			// batchDownloadContextMenuItems
			//
			batchDownloadContextMenuItems.Items.AddRange(new Krypton.Toolkit.KryptonContextMenuItemBase[] { downloadSelectedItem });
			//
			// downloadSelectedItem
			//
			downloadSelectedItem.Text = "Download Selected";
			downloadSelectedItem.Click += DownloadSelectedItem_Click;
			//
			// searchPanel
			// 
			searchPanel.Controls.Add(searchButton);
			searchPanel.Controls.Add(searchTextBox);
			searchPanel.Controls.Add(searchModeComboBox);
			searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
			searchPanel.Location = new System.Drawing.Point(8, 28);
			searchPanel.Name = "searchPanel";
			searchPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
			searchPanel.Size = new System.Drawing.Size(884, 31);
			searchPanel.TabIndex = 1;
			// 
			// searchButton
			// 
			searchButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			searchButton.Location = new System.Drawing.Point(608, 1);
			searchButton.Name = "searchButton";
			searchButton.Size = new System.Drawing.Size(22, 22);
			searchButton.TabIndex = 1;
			searchButton.Values.Image = Resources.IDB_SEARCH;
			searchButton.Values.Text = "";
			searchButton.Click += SearchButton_Click;
			// 
			// searchTextBox
			// 
			searchTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			searchTextBox.Location = new System.Drawing.Point(0, 1);
			searchTextBox.Name = "searchTextBox";
			searchTextBox.Size = new System.Drawing.Size(604, 23);
			searchTextBox.TabIndex = 0;
			searchTextBox.TextChanged += SearchTextBox_TextChanged;
			// 
			// searchModeComboBox
			// 
			searchModeComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			searchModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			searchModeComboBox.DropDownWidth = 200;
			searchModeComboBox.IntegralHeight = false;
			searchModeComboBox.Items.AddRange(new object[] { "Filename and path", "Filename only", "Path only" });
			searchModeComboBox.Location = new System.Drawing.Point(634, 1);
			searchModeComboBox.Name = "searchModeComboBox";
			searchModeComboBox.Size = new System.Drawing.Size(250, 22);
			searchModeComboBox.TabIndex = 2;
			searchModeComboBox.SelectedIndexChanged += SearchModeComboBox_SelectedIndexChanged;
			// 
			// modeTabControl
			// 
			modeTabControl.Controls.Add(onlineTabPage);
			modeTabControl.Controls.Add(offlineTabPage);
			modeTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			modeTabControl.Location = new System.Drawing.Point(8, 528);
			modeTabControl.Name = "modeTabControl";
			modeTabControl.Padding = new System.Drawing.Point(10, 5);
			modeTabControl.SelectedIndex = 0;
			modeTabControl.Size = new System.Drawing.Size(884, 30);
			modeTabControl.TabIndex = 6;
			modeTabControl.SelectedIndexChanged += ModeTabControl_SelectedIndexChanged;
			// 
			// breadcrumbPanel
			// 
			breadcrumbPanel.AutoSize = true;
			breadcrumbPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			breadcrumbPanel.Dock = System.Windows.Forms.DockStyle.Top;
			breadcrumbPanel.Location = new System.Drawing.Point(8, 92);
			breadcrumbPanel.MinimumSize = new System.Drawing.Size(0, 35);
			breadcrumbPanel.Name = "breadcrumbPanel";
			breadcrumbPanel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
			breadcrumbPanel.Size = new System.Drawing.Size(884, 35);
			breadcrumbPanel.TabIndex = 2;
			breadcrumbPanel.WrapContents = false;
			// 
			// moduleListView
			// 
			moduleListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnName, columnPath, columnSize });
			moduleListView.Dock = System.Windows.Forms.DockStyle.Fill;
			moduleListView.FullRowSelect = true;
			moduleListView.Location = new System.Drawing.Point(8, 147);
			moduleListView.Name = "moduleListView";
			moduleListView.Size = new System.Drawing.Size(884, 381);
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
			columnSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			columnSize.Width = 100;
			// 
			// statusStrip
			//
			statusStrip.ImageScalingSize = new System.Drawing.Size(16, 16);
			statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { cancelDownloadButton, statusLabel, progressBar });
			statusStrip.Location = new System.Drawing.Point(8, 578);
			statusStrip.Name = "statusStrip";
			statusStrip.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
			statusStrip.Size = new System.Drawing.Size(884, 22);
			statusStrip.TabIndex = 8;
			//
			// cancelDownloadButton
			//
			cancelDownloadButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			cancelDownloadButton.Name = "cancelDownloadButton";
			cancelDownloadButton.Size = new System.Drawing.Size(50, 19);
			cancelDownloadButton.Text = "Cancel";
			cancelDownloadButton.Visible = false;
			cancelDownloadButton.Click += CancelDownloadButton_Click;
			//
			// statusLabel
			//
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new System.Drawing.Size(0, 0);
			statusLabel.Spring = true;
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
			Controls.Add(breadcrumbPanel);
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
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
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
		private Kit.Gui.Designer.ControlResource controlResource;
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
		private Krypton.Toolkit.KryptonContextMenu batchDownloadContextMenu;
		private Krypton.Toolkit.KryptonContextMenuItems batchDownloadContextMenuItems;
		private Krypton.Toolkit.KryptonContextMenuItem downloadSelectedItem;
		private Krypton.Toolkit.KryptonLabel searchLabel;
		private System.Windows.Forms.Panel searchPanel;
		private Krypton.Toolkit.KryptonTextBox searchTextBox;
		private Krypton.Toolkit.KryptonButton searchButton;
		private Krypton.Toolkit.KryptonComboBox searchModeComboBox;
		private System.Windows.Forms.TabControl modeTabControl;
		private System.Windows.Forms.TabPage onlineTabPage;
		private System.Windows.Forms.TabPage offlineTabPage;
		private Krypton.Toolkit.KryptonButton parentButton;
		private System.Windows.Forms.FlowLayoutPanel breadcrumbPanel;
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
