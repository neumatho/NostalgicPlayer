
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow
{
	partial class SettingsWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsWindowForm));
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.navigator = new Krypton.Navigator.KryptonNavigator();
			this.navigatorPathsPage = new Krypton.Navigator.KryptonPage();
			this.pathsPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.PathsPageControl();
			this.navigatorMixerPage = new Krypton.Navigator.KryptonPage();
			this.navigatorAgentsPage = new Krypton.Navigator.KryptonPage();
			this.controlResource = new Polycode.NostalgicPlayer.Client.GuiPlayer.Designer.ControlResource();
			this.applyButton = new Krypton.Toolkit.KryptonButton();
			this.cancelButton = new Krypton.Toolkit.KryptonButton();
			this.okButton = new Krypton.Toolkit.KryptonButton();
			((System.ComponentModel.ISupportInitialize)(this.navigator)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorPathsPage)).BeginInit();
			this.navigatorPathsPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.navigatorMixerPage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorAgentsPage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).BeginInit();
			this.SuspendLayout();
			// 
			// navigator
			// 
			this.navigator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.navigator.Location = new System.Drawing.Point(8, 8);
			this.navigator.Name = "navigator";
			this.navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] {
            this.navigatorPathsPage,
            this.navigatorMixerPage,
            this.navigatorAgentsPage});
			this.controlResource.SetResourceKey(this.navigator, null);
			this.navigator.SelectedIndex = 0;
			this.navigator.Size = new System.Drawing.Size(610, 375);
			this.navigator.StateCommon.Tab.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.navigator.TabIndex = 0;
			// 
			// navigatorPathsPage
			// 
			this.navigatorPathsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorPathsPage.Controls.Add(this.pathsPageControl);
			this.navigatorPathsPage.Flags = 65534;
			this.navigatorPathsPage.LastVisibleSet = true;
			this.navigatorPathsPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorPathsPage.Name = "navigatorPathsPage";
			this.controlResource.SetResourceKey(this.navigatorPathsPage, null);
			this.navigatorPathsPage.Size = new System.Drawing.Size(608, 348);
			this.navigatorPathsPage.Text = "";
			this.navigatorPathsPage.ToolTipTitle = "Page ToolTip";
			this.navigatorPathsPage.UniqueName = "690d92cb4fb74babaad9b326d3b4be6d";
			// 
			// pathsPageControl
			// 
			this.pathsPageControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pathsPageControl.BackColor = System.Drawing.Color.Transparent;
			this.pathsPageControl.Location = new System.Drawing.Point(0, 0);
			this.pathsPageControl.Name = "pathsPageControl";
			this.controlResource.SetResourceKey(this.pathsPageControl, null);
			this.pathsPageControl.Size = new System.Drawing.Size(608, 348);
			this.pathsPageControl.TabIndex = 0;
			// 
			// navigatorMixerPage
			// 
			this.navigatorMixerPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorMixerPage.Flags = 65534;
			this.navigatorMixerPage.LastVisibleSet = true;
			this.navigatorMixerPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorMixerPage.Name = "navigatorMixerPage";
			this.controlResource.SetResourceKey(this.navigatorMixerPage, null);
			this.navigatorMixerPage.Size = new System.Drawing.Size(606, 348);
			this.navigatorMixerPage.Text = "";
			this.navigatorMixerPage.ToolTipTitle = "Page ToolTip";
			this.navigatorMixerPage.UniqueName = "8f2b8ed82e5e492fba41a69c3687c8ed";
			// 
			// navigatorAgentsPage
			// 
			this.navigatorAgentsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorAgentsPage.Flags = 65534;
			this.navigatorAgentsPage.LastVisibleSet = true;
			this.navigatorAgentsPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorAgentsPage.Name = "navigatorAgentsPage";
			this.controlResource.SetResourceKey(this.navigatorAgentsPage, null);
			this.navigatorAgentsPage.Size = new System.Drawing.Size(606, 348);
			this.navigatorAgentsPage.Text = "";
			this.navigatorAgentsPage.ToolTipTitle = "Page ToolTip";
			this.navigatorAgentsPage.UniqueName = "baf18fe9861c467298b46645633c9221";
			// 
			// controlResource
			// 
			this.controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// applyButton
			// 
			this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.applyButton.Location = new System.Drawing.Point(528, 391);
			this.applyButton.Name = "applyButton";
			this.controlResource.SetResourceKey(this.applyButton, "IDS_SETTINGS_APPLY");
			this.applyButton.Size = new System.Drawing.Size(90, 25);
			this.applyButton.TabIndex = 3;
			this.applyButton.Values.Text = "Apply";
			this.applyButton.Click += new System.EventHandler(this.ApplyButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.Location = new System.Drawing.Point(430, 391);
			this.cancelButton.Name = "cancelButton";
			this.controlResource.SetResourceKey(this.cancelButton, "IDS_SETTINGS_CANCEL");
			this.cancelButton.Size = new System.Drawing.Size(90, 25);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Values.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(332, 391);
			this.okButton.Name = "okButton";
			this.controlResource.SetResourceKey(this.okButton, "IDS_SETTINGS_OK");
			this.okButton.Size = new System.Drawing.Size(90, 25);
			this.okButton.TabIndex = 1;
			this.okButton.Values.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// SettingsWindowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(626, 424);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.applyButton);
			this.Controls.Add(this.navigator);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsWindowForm";
			this.controlResource.SetResourceKey(this, null);
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingsWindowForm_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.navigator)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorPathsPage)).EndInit();
			this.navigatorPathsPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.navigatorMixerPage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorAgentsPage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.controlResource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private Krypton.Navigator.KryptonNavigator navigator;
		private Krypton.Navigator.KryptonPage navigatorPathsPage;
		private Krypton.Navigator.KryptonPage navigatorMixerPage;
		private Krypton.Navigator.KryptonPage navigatorAgentsPage;
		private Designer.ControlResource controlResource;
		private Krypton.Toolkit.KryptonButton applyButton;
		private Krypton.Toolkit.KryptonButton cancelButton;
		private Krypton.Toolkit.KryptonButton okButton;
		private Pages.PathsPageControl pathsPageControl;
	}
}