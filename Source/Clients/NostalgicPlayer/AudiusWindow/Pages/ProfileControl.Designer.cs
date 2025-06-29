namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AudiusWindow.Pages
{
	partial class ProfileControl
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
			infoPanel = new Polycode.NostalgicPlayer.GuiKit.Controls.ImprovedPanel();
			closeButton = new Krypton.Toolkit.KryptonButton();
			handleLabel = new Krypton.Toolkit.KryptonLabel();
			bigFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			nameLabel = new Krypton.Toolkit.KryptonLabel();
			extraBigBoldFontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			profilePictureBox = new System.Windows.Forms.PictureBox();
			navigator = new Krypton.Navigator.KryptonNavigator();
			navigatorTracksPage = new Krypton.Navigator.KryptonPage();
			profileTracksPageControl = new ProfileTracksPageControl();
			navigatorPlaylistsPage = new Krypton.Navigator.KryptonPage();
			profilePlaylistsPageControl = new ProfilePlaylistsPageControl();
			fontPalette = new Polycode.NostalgicPlayer.GuiKit.Components.FontPalette(components);
			infoPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)profilePictureBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)navigator).BeginInit();
			((System.ComponentModel.ISupportInitialize)navigatorTracksPage).BeginInit();
			navigatorTracksPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)navigatorPlaylistsPage).BeginInit();
			navigatorPlaylistsPage.SuspendLayout();
			SuspendLayout();
			// 
			// infoPanel
			// 
			infoPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			infoPanel.BackColor = System.Drawing.Color.Transparent;
			infoPanel.Controls.Add(closeButton);
			infoPanel.Controls.Add(handleLabel);
			infoPanel.Controls.Add(nameLabel);
			infoPanel.Controls.Add(profilePictureBox);
			infoPanel.Location = new System.Drawing.Point(0, 0);
			infoPanel.Name = "infoPanel";
			infoPanel.Size = new System.Drawing.Size(766, 224);
			infoPanel.TabIndex = 0;
			// 
			// closeButton
			// 
			closeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			closeButton.Location = new System.Drawing.Point(734, 8);
			closeButton.Name = "closeButton";
			closeButton.Size = new System.Drawing.Size(24, 24);
			closeButton.StateCommon.Back.Draw = Krypton.Toolkit.InheritBool.False;
			closeButton.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
			closeButton.TabIndex = 3;
			closeButton.Values.Image = Resources.IDB_CLOSE_WHITE;
			closeButton.Values.Text = "";
			closeButton.Click += Close_Click;
			closeButton.MouseEnter += Close_MouseEnter;
			closeButton.MouseLeave += Close_MouseLeave;
			// 
			// handleLabel
			// 
			handleLabel.Location = new System.Drawing.Point(144, 155);
			handleLabel.Name = "handleLabel";
			handleLabel.Palette = bigFontPalette;
			handleLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			handleLabel.Size = new System.Drawing.Size(6, 2);
			handleLabel.StateCommon.ShortText.Color1 = System.Drawing.Color.White;
			handleLabel.TabIndex = 2;
			handleLabel.Values.Text = "";
			// 
			// bigFontPalette
			// 
			bigFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			bigFontPalette.BaseFontSize = 10F;
			bigFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			bigFontPalette.ThemeName = "";
			bigFontPalette.UseKryptonFileDialogs = true;
			// 
			// nameLabel
			// 
			nameLabel.Location = new System.Drawing.Point(144, 126);
			nameLabel.Name = "nameLabel";
			nameLabel.Palette = extraBigBoldFontPalette;
			nameLabel.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			nameLabel.Size = new System.Drawing.Size(6, 2);
			nameLabel.StateCommon.ShortText.Color1 = System.Drawing.Color.White;
			nameLabel.TabIndex = 1;
			nameLabel.Values.Text = "";
			// 
			// extraBigBoldFontPalette
			// 
			extraBigBoldFontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			extraBigBoldFontPalette.BaseFontSize = 16F;
			extraBigBoldFontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			extraBigBoldFontPalette.FontStyle = System.Drawing.FontStyle.Bold;
			extraBigBoldFontPalette.ThemeName = "";
			extraBigBoldFontPalette.UseKryptonFileDialogs = true;
			// 
			// profilePictureBox
			// 
			profilePictureBox.Location = new System.Drawing.Point(8, 88);
			profilePictureBox.Name = "profilePictureBox";
			profilePictureBox.Size = new System.Drawing.Size(128, 128);
			profilePictureBox.TabIndex = 0;
			profilePictureBox.TabStop = false;
			// 
			// navigator
			// 
			navigator.AllowPageReorder = false;
			navigator.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			navigator.Button.ButtonDisplayLogic = Krypton.Navigator.ButtonDisplayLogic.Context;
			navigator.Button.CloseButtonAction = Krypton.Navigator.CloseButtonAction.RemovePageAndDispose;
			navigator.Button.CloseButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			navigator.Button.ContextButtonAction = Krypton.Navigator.ContextButtonAction.SelectPage;
			navigator.Button.ContextButtonDisplay = Krypton.Navigator.ButtonDisplay.Hide;
			navigator.Button.ContextMenuMapImage = Krypton.Navigator.MapKryptonPageImage.Small;
			navigator.Button.ContextMenuMapText = Krypton.Navigator.MapKryptonPageText.TextTitle;
			navigator.Button.NextButtonAction = Krypton.Navigator.DirectionButtonAction.ModeAppropriateAction;
			navigator.Button.NextButtonDisplay = Krypton.Navigator.ButtonDisplay.Logic;
			navigator.Button.PreviousButtonAction = Krypton.Navigator.DirectionButtonAction.ModeAppropriateAction;
			navigator.Button.PreviousButtonDisplay = Krypton.Navigator.ButtonDisplay.Logic;
			navigator.ControlKryptonFormFeatures = false;
			navigator.Location = new System.Drawing.Point(8, 232);
			navigator.Name = "navigator";
			navigator.NavigatorMode = Krypton.Navigator.NavigatorMode.BarTabGroup;
			navigator.Owner = null;
			navigator.PageBackStyle = Krypton.Toolkit.PaletteBackStyle.ControlClient;
			navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] { navigatorTracksPage, navigatorPlaylistsPage });
			navigator.Palette = fontPalette;
			navigator.PaletteMode = Krypton.Toolkit.PaletteMode.Custom;
			navigator.Panel.PanelBackStyle = Krypton.Toolkit.PaletteBackStyle.TabLowProfile;
			navigator.SelectedIndex = 0;
			navigator.Size = new System.Drawing.Size(750, 128);
			navigator.TabIndex = 1;
			navigator.SelectedPageChanged += Navigator_SelectedPageChanged;
			// 
			// navigatorTracksPage
			// 
			navigatorTracksPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorTracksPage.Controls.Add(profileTracksPageControl);
			navigatorTracksPage.Flags = 65534;
			navigatorTracksPage.LastVisibleSet = true;
			navigatorTracksPage.MinimumSize = new System.Drawing.Size(150, 50);
			navigatorTracksPage.Name = "navigatorTracksPage";
			navigatorTracksPage.Size = new System.Drawing.Size(748, 102);
			navigatorTracksPage.Text = "";
			navigatorTracksPage.ToolTipTitle = "Page ToolTip";
			navigatorTracksPage.UniqueName = "6782b14423d84ca9964882855f0cc4fa";
			// 
			// profileTracksPageControl
			// 
			profileTracksPageControl.BackColor = System.Drawing.Color.Transparent;
			profileTracksPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			profileTracksPageControl.Location = new System.Drawing.Point(0, 0);
			profileTracksPageControl.Name = "profileTracksPageControl";
			profileTracksPageControl.Size = new System.Drawing.Size(748, 102);
			profileTracksPageControl.TabIndex = 2;
			// 
			// navigatorPlaylistsPage
			// 
			navigatorPlaylistsPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			navigatorPlaylistsPage.Controls.Add(profilePlaylistsPageControl);
			navigatorPlaylistsPage.Flags = 65534;
			navigatorPlaylistsPage.LastVisibleSet = true;
			navigatorPlaylistsPage.MinimumSize = new System.Drawing.Size(150, 50);
			navigatorPlaylistsPage.Name = "navigatorPlaylistsPage";
			navigatorPlaylistsPage.Size = new System.Drawing.Size(748, 102);
			navigatorPlaylistsPage.Text = "";
			navigatorPlaylistsPage.ToolTipTitle = "Page ToolTip";
			navigatorPlaylistsPage.UniqueName = "b6d6a1ed5b054a11b1216b6bb36c44f1";
			// 
			// profilePlaylistsPageControl
			// 
			profilePlaylistsPageControl.BackColor = System.Drawing.Color.Transparent;
			profilePlaylistsPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			profilePlaylistsPageControl.Location = new System.Drawing.Point(0, 0);
			profilePlaylistsPageControl.Name = "profilePlaylistsPageControl";
			profilePlaylistsPageControl.Size = new System.Drawing.Size(748, 102);
			profilePlaylistsPageControl.TabIndex = 2;
			// 
			// fontPalette
			// 
			fontPalette.BaseFont = new System.Drawing.Font("Segoe UI", 9F);
			fontPalette.BasePaletteType = Krypton.Toolkit.BasePaletteType.Custom;
			fontPalette.ThemeName = "";
			fontPalette.UseKryptonFileDialogs = true;
			// 
			// ProfileControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(navigator);
			Controls.Add(infoPanel);
			Name = "ProfileControl";
			Size = new System.Drawing.Size(766, 368);
			infoPanel.ResumeLayout(false);
			infoPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)profilePictureBox).EndInit();
			((System.ComponentModel.ISupportInitialize)navigator).EndInit();
			((System.ComponentModel.ISupportInitialize)navigatorTracksPage).EndInit();
			navigatorTracksPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)navigatorPlaylistsPage).EndInit();
			navigatorPlaylistsPage.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Polycode.NostalgicPlayer.GuiKit.Controls.ImprovedPanel infoPanel;
		private System.Windows.Forms.PictureBox profilePictureBox;
		private GuiKit.Components.FontPalette extraBigBoldFontPalette;
		private GuiKit.Components.FontPalette bigFontPalette;
		private Krypton.Toolkit.KryptonLabel handleLabel;
		private Krypton.Toolkit.KryptonLabel nameLabel;
		private Krypton.Toolkit.KryptonButton closeButton;
		private Krypton.Navigator.KryptonNavigator navigator;
		private GuiKit.Components.FontPalette fontPalette;
		private Krypton.Navigator.KryptonPage navigatorTracksPage;
		private ProfileTracksPageControl profileTracksPageControl;
		private Krypton.Navigator.KryptonPage navigatorPlaylistsPage;
		private ProfilePlaylistsPageControl profilePlaylistsPageControl;
	}
}
