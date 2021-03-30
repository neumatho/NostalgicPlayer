
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	partial class AgentsPageControl
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
			this.components = new System.ComponentModel.Container();
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.navigator = new Krypton.Navigator.KryptonNavigator();
			this.navigatorPlayersPage = new Krypton.Navigator.KryptonPage();
			this.playersListControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.AgentsListUserControl();
			this.navigatorOutputPage = new Krypton.Navigator.KryptonPage();
			this.outputListControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.AgentsListUserControl();
			this.navigatorSampleConvertersPage = new Krypton.Navigator.KryptonPage();
			this.sampleConvertersListControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.AgentsListUserControl();
			this.navigatorVisualsPage = new Krypton.Navigator.KryptonPage();
			this.visualsListControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages.AgentsListUserControl();
			((System.ComponentModel.ISupportInitialize)(this.navigator)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorPlayersPage)).BeginInit();
			this.navigatorPlayersPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.navigatorOutputPage)).BeginInit();
			this.navigatorOutputPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.navigatorSampleConvertersPage)).BeginInit();
			this.navigatorSampleConvertersPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.navigatorVisualsPage)).BeginInit();
			this.navigatorVisualsPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// navigator
			// 
			this.navigator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.navigator.Bar.BarOrientation = Krypton.Toolkit.VisualOrientation.Bottom;
			this.navigator.Bar.ItemAlignment = Krypton.Toolkit.RelativePositionAlign.Center;
			this.navigator.Location = new System.Drawing.Point(8, 8);
			this.navigator.Name = "navigator";
			this.navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] {
            this.navigatorPlayersPage,
            this.navigatorOutputPage,
            this.navigatorSampleConvertersPage,
            this.navigatorVisualsPage});
			this.navigator.SelectedIndex = 0;
			this.navigator.Size = new System.Drawing.Size(592, 332);
			this.navigator.StateCommon.Tab.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.navigator.TabIndex = 0;
			// 
			// navigatorPlayersPage
			// 
			this.navigatorPlayersPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorPlayersPage.Controls.Add(this.playersListControl);
			this.navigatorPlayersPage.Flags = 65534;
			this.navigatorPlayersPage.LastVisibleSet = true;
			this.navigatorPlayersPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorPlayersPage.Name = "navigatorPlayersPage";
			this.navigatorPlayersPage.Size = new System.Drawing.Size(590, 307);
			this.navigatorPlayersPage.Text = "";
			this.navigatorPlayersPage.ToolTipTitle = "Page ToolTip";
			this.navigatorPlayersPage.UniqueName = "0c5b0509367e444183bfe52fd16df5cc";
			// 
			// playersListControl
			// 
			this.playersListControl.BackColor = System.Drawing.Color.Transparent;
			this.playersListControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.playersListControl.Location = new System.Drawing.Point(0, 0);
			this.playersListControl.Name = "playersListControl";
			this.playersListControl.Size = new System.Drawing.Size(590, 307);
			this.playersListControl.TabIndex = 0;
			// 
			// navigatorOutputPage
			// 
			this.navigatorOutputPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorOutputPage.Controls.Add(this.outputListControl);
			this.navigatorOutputPage.Flags = 65534;
			this.navigatorOutputPage.LastVisibleSet = true;
			this.navigatorOutputPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorOutputPage.Name = "navigatorOutputPage";
			this.navigatorOutputPage.Size = new System.Drawing.Size(100, 100);
			this.navigatorOutputPage.Text = "";
			this.navigatorOutputPage.ToolTipTitle = "Page ToolTip";
			this.navigatorOutputPage.UniqueName = "c9a5bb69346c46ad86650b9049d61d44";
			// 
			// outputListControl
			// 
			this.outputListControl.BackColor = System.Drawing.Color.Transparent;
			this.outputListControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outputListControl.Location = new System.Drawing.Point(0, 0);
			this.outputListControl.Name = "outputListControl";
			this.outputListControl.Size = new System.Drawing.Size(100, 100);
			this.outputListControl.TabIndex = 0;
			// 
			// navigatorSampleConvertersPage
			// 
			this.navigatorSampleConvertersPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorSampleConvertersPage.Controls.Add(this.sampleConvertersListControl);
			this.navigatorSampleConvertersPage.Flags = 65534;
			this.navigatorSampleConvertersPage.LastVisibleSet = true;
			this.navigatorSampleConvertersPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorSampleConvertersPage.Name = "navigatorSampleConvertersPage";
			this.navigatorSampleConvertersPage.Size = new System.Drawing.Size(100, 100);
			this.navigatorSampleConvertersPage.Text = "";
			this.navigatorSampleConvertersPage.ToolTipTitle = "Page ToolTip";
			this.navigatorSampleConvertersPage.UniqueName = "2d2e33220f4349f8b63cdee8e9fe571f";
			// 
			// sampleConvertersListControl
			// 
			this.sampleConvertersListControl.BackColor = System.Drawing.Color.Transparent;
			this.sampleConvertersListControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sampleConvertersListControl.EnableCheckColumn = false;
			this.sampleConvertersListControl.Location = new System.Drawing.Point(0, 0);
			this.sampleConvertersListControl.Name = "sampleConvertersListControl";
			this.sampleConvertersListControl.Size = new System.Drawing.Size(100, 100);
			this.sampleConvertersListControl.TabIndex = 0;
			// 
			// navigatorVisualsPage
			// 
			this.navigatorVisualsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorVisualsPage.Controls.Add(this.visualsListControl);
			this.navigatorVisualsPage.Flags = 65534;
			this.navigatorVisualsPage.LastVisibleSet = true;
			this.navigatorVisualsPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorVisualsPage.Name = "navigatorVisualsPage";
			this.navigatorVisualsPage.Size = new System.Drawing.Size(100, 100);
			this.navigatorVisualsPage.Text = "";
			this.navigatorVisualsPage.ToolTipTitle = "Page ToolTip";
			this.navigatorVisualsPage.UniqueName = "bdf89da9d454418ca56b12f0cefd7e3d";
			// 
			// visualsListControl
			// 
			this.visualsListControl.BackColor = System.Drawing.Color.Transparent;
			this.visualsListControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.visualsListControl.Location = new System.Drawing.Point(0, 0);
			this.visualsListControl.Name = "visualsListControl";
			this.visualsListControl.Size = new System.Drawing.Size(100, 100);
			this.visualsListControl.TabIndex = 0;
			// 
			// AgentsPageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.navigator);
			this.Name = "AgentsPageControl";
			this.Size = new System.Drawing.Size(608, 348);
			((System.ComponentModel.ISupportInitialize)(this.navigator)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorPlayersPage)).EndInit();
			this.navigatorPlayersPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.navigatorOutputPage)).EndInit();
			this.navigatorOutputPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.navigatorSampleConvertersPage)).EndInit();
			this.navigatorSampleConvertersPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.navigatorVisualsPage)).EndInit();
			this.navigatorVisualsPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private Krypton.Navigator.KryptonNavigator navigator;
		private AgentsListUserControl playersListControl;
		private AgentsListUserControl outputListControl;
		private Krypton.Navigator.KryptonPage navigatorPlayersPage;
		private Krypton.Navigator.KryptonPage navigatorOutputPage;
		private Krypton.Navigator.KryptonPage navigatorSampleConvertersPage;
		private AgentsListUserControl sampleConvertersListControl;
		private Krypton.Navigator.KryptonPage navigatorVisualsPage;
		private AgentsListUserControl visualsListControl;
	}
}
