namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.ListItems
{
	partial class AudiusProfileListItemControl
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
			controlBox = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox();
			handleLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			nameLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			bigBoldFontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			positionLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			bigFontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			itemPictureBox = new System.Windows.Forms.PictureBox();
			separator = new Polycode.NostalgicPlayer.Controls.Separators.NostalgicSeparator();
			showInfoButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			controlBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)itemPictureBox).BeginInit();
			SuspendLayout();
			// 
			// controlBox
			// 
			controlBox.Controls.Add(handleLabel);
			controlBox.Controls.Add(nameLabel);
			controlBox.Controls.Add(positionLabel);
			controlBox.Controls.Add(itemPictureBox);
			controlBox.Controls.Add(separator);
			controlBox.Controls.Add(showInfoButton);
			controlBox.Dock = System.Windows.Forms.DockStyle.Fill;
			controlBox.Location = new System.Drawing.Point(0, 0);
			controlBox.Name = "controlBox";
			controlBox.Size = new System.Drawing.Size(463, 144);
			controlBox.TabIndex = 0;
			// 
			// handleLabel
			// 
			handleLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			handleLabel.AutoSize = false;
			handleLabel.Location = new System.Drawing.Point(171, 36);
			handleLabel.Name = "handleLabel";
			handleLabel.Size = new System.Drawing.Size(247, 20);
			handleLabel.TabIndex = 2;
			// 
			// nameLabel
			// 
			nameLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nameLabel.AutoSize = false;
			nameLabel.Location = new System.Drawing.Point(171, 8);
			nameLabel.Name = "nameLabel";
			nameLabel.Size = new System.Drawing.Size(230, 19);
			nameLabel.TabIndex = 1;
			nameLabel.UseFont = bigBoldFontConfiguration;
			// 
			// bigBoldFontConfiguration
			// 
			bigBoldFontConfiguration.FontSize = 2;
			bigBoldFontConfiguration.FontStyle = System.Drawing.FontStyle.Bold;
			// 
			// positionLabel
			// 
			positionLabel.Location = new System.Drawing.Point(4, 63);
			positionLabel.Name = "positionLabel";
			positionLabel.Size = new System.Drawing.Size(0, 17);
			positionLabel.TabIndex = 0;
			positionLabel.UseFont = bigFontConfiguration;
			// 
			// bigFontConfiguration
			// 
			bigFontConfiguration.FontSize = 2;
			// 
			// itemPictureBox
			// 
			itemPictureBox.Location = new System.Drawing.Point(39, 8);
			itemPictureBox.Name = "itemPictureBox";
			itemPictureBox.Size = new System.Drawing.Size(128, 128);
			itemPictureBox.TabIndex = 1;
			itemPictureBox.TabStop = false;
			// 
			// separator
			// 
			separator.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			separator.Location = new System.Drawing.Point(171, 104);
			separator.Name = "separator";
			separator.Size = new System.Drawing.Size(284, 2);
			separator.TabIndex = 3;
			// 
			// showInfoButton
			// 
			showInfoButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			showInfoButton.ImageArea = NostalgicPlayer.Controls.Types.ImageBankArea.Audius;
			showInfoButton.ImageName = "ShowProfileInfo";
			showInfoButton.Location = new System.Drawing.Point(171, 112);
			showInfoButton.Name = "showInfoButton";
			showInfoButton.Size = new System.Drawing.Size(24, 24);
			showInfoButton.TabIndex = 4;
			showInfoButton.Click += ShowInfo_Click;
			// 
			// AudiusProfileListItemControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.Transparent;
			Controls.Add(controlBox);
			DoubleBuffered = true;
			Margin = new System.Windows.Forms.Padding(8);
			Name = "AudiusProfileListItemControl";
			Size = new System.Drawing.Size(463, 144);
			controlBox.ResumeLayout(false);
			controlBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)itemPictureBox).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Polycode.NostalgicPlayer.Controls.Containers.NostalgicBox controlBox;
		private System.Windows.Forms.PictureBox itemPictureBox;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel positionLabel;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel nameLabel;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel handleLabel;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton showInfoButton;
		private Polycode.NostalgicPlayer.Controls.Separators.NostalgicSeparator separator;
		private NostalgicPlayer.Controls.Components.FontConfiguration bigBoldFontConfiguration;
		private NostalgicPlayer.Controls.Components.FontConfiguration bigFontConfiguration;
	}
}
