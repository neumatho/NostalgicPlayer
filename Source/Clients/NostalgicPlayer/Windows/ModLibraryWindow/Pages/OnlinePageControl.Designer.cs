namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Pages
{
	partial class OnlinePageControl
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
			listControl = new ModLibraryListControl();
			breadcrumbControl = new ModLibraryBreadcrumbControl();
			flatViewCheckBox = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicCheckBox();
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			sourceContextMenu = new Polycode.NostalgicPlayer.Controls.Menus.NostalgicContextMenu(components);
			contextMenu = new Polycode.NostalgicPlayer.Controls.Menus.NostalgicContextMenu(components);
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
			// 
			// listControl
			// 
			listControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			listControl.Location = new System.Drawing.Point(8, 66);
			listControl.Name = "listControl";
			listControl.Size = new System.Drawing.Size(484, 176);
			listControl.TabIndex = 2;
			listControl.ItemActivate += List_ItemActivate;
			listControl.NavigateUp += List_NavigateUp;
			listControl.SortOrderChanged += List_SortOrderChanged;
			listControl.OpenContextMenu += List_OpenContextMenu;
			// 
			// breadcrumbControl
			// 
			breadcrumbControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			breadcrumbControl.Location = new System.Drawing.Point(8, 8);
			breadcrumbControl.Name = "breadcrumbControl";
			breadcrumbControl.Size = new System.Drawing.Size(484, 23);
			breadcrumbControl.TabIndex = 0;
			breadcrumbControl.PathChanged += Breadcrumb_PathChanged;
			// 
			// flatViewCheckBox
			// 
			flatViewCheckBox.AutoSize = true;
			flatViewCheckBox.Location = new System.Drawing.Point(8, 39);
			flatViewCheckBox.Name = "flatViewCheckBox";
			controlResource.SetResourceKey(flatViewCheckBox, "IDS_MODLIBRARY_FLATVIEW_CHECKBOX");
			flatViewCheckBox.Size = new System.Drawing.Size(209, 19);
			flatViewCheckBox.TabIndex = 1;
			flatViewCheckBox.Text = "Flat view (show all files recursively)";
			flatViewCheckBox.CheckedChanged += FlatView_CheckedChanged;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// sourceContextMenu
			// 
			sourceContextMenu.Name = "sourceContextMenu";
			// 
			// contextMenu
			// 
			contextMenu.Name = "contextMenu";
			// 
			// OnlinePageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(flatViewCheckBox);
			Controls.Add(breadcrumbControl);
			Controls.Add(listControl);
			Name = "OnlinePageControl";
			Size = new System.Drawing.Size(500, 250);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ModLibraryListControl listControl;
		private ModLibraryBreadcrumbControl breadcrumbControl;
		private NostalgicPlayer.Controls.Buttons.NostalgicCheckBox flatViewCheckBox;
		private Kit.Gui.Designer.ControlResource controlResource;
		private NostalgicPlayer.Controls.Menus.NostalgicContextMenu sourceContextMenu;
		private NostalgicPlayer.Controls.Menus.NostalgicContextMenu contextMenu;
	}
}
