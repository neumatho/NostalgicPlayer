namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	partial class ModLibraryBreadcrumbControl
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
			countLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			parentButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicButton();
			breadcrumb = new Polycode.NostalgicPlayer.Controls.Navigation.NostalgicBreadcrumb();
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
			// 
			// countLabel
			// 
			countLabel.Location = new System.Drawing.Point(222, 4);
			countLabel.Name = "countLabel";
			countLabel.Size = new System.Drawing.Size(0, 13);
			countLabel.TabIndex = 2;
			// 
			// parentButton
			// 
			parentButton.Location = new System.Drawing.Point(0, 0);
			parentButton.Name = "parentButton";
			controlResource.SetResourceKey(parentButton, "IDS_MODLIBRARY_PARENT_BUTTON");
			parentButton.Size = new System.Drawing.Size(75, 23);
			parentButton.TabIndex = 0;
			parentButton.Text = "Parent";
			parentButton.Click += Parent_Click;
			// 
			// breadcrumb
			// 
			breadcrumb.Location = new System.Drawing.Point(79, 1);
			breadcrumb.Name = "breadcrumb";
			breadcrumb.Size = new System.Drawing.Size(156, 20);
			breadcrumb.TabIndex = 1;
			breadcrumb.ItemClicked += Breadcrumb_ItemClicked;
			breadcrumb.PathChanged += Breadcrumb_PathChanged;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// ModLibraryBreadcrumbControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(countLabel);
			Controls.Add(parentButton);
			Controls.Add(breadcrumb);
			Name = "ModLibraryBreadcrumbControl";
			Size = new System.Drawing.Size(500, 23);
			Layout += ModLibraryBreadcrumbControl_Layout;
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private NostalgicPlayer.Controls.Texts.NostalgicLabel countLabel;
		private NostalgicPlayer.Controls.Buttons.NostalgicButton parentButton;
		private NostalgicPlayer.Controls.Navigation.NostalgicBreadcrumb breadcrumb;
		private Kit.Gui.Designer.ControlResource controlResource;
	}
}
