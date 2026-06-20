namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.ListItems
{
	partial class AudiusTrackListItemControl
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
			playButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			addButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton();
			durationLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			titleLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			SuspendLayout();
			// 
			// playButton
			// 
			playButton.ImageArea = NostalgicPlayer.Controls.Types.ImageBankArea.Main;
			playButton.ImageName = "Play";
			playButton.Location = new System.Drawing.Point(4, 4);
			playButton.Name = "playButton";
			playButton.Size = new System.Drawing.Size(24, 24);
			playButton.TabIndex = 0;
			playButton.Click += Play_Click;
			// 
			// addButton
			// 
			addButton.ImageArea = NostalgicPlayer.Controls.Types.ImageBankArea.Main;
			addButton.ImageName = "Add";
			addButton.Location = new System.Drawing.Point(32, 4);
			addButton.Name = "addButton";
			addButton.Size = new System.Drawing.Size(24, 24);
			addButton.TabIndex = 1;
			addButton.Click += Add_Click;
			// 
			// durationLabel
			// 
			durationLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			durationLabel.Location = new System.Drawing.Point(449, 8);
			durationLabel.Name = "durationLabel";
			durationLabel.Size = new System.Drawing.Size(6, 2);
			durationLabel.TabIndex = 3;
			// 
			// titleLabel
			// 
			titleLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			titleLabel.AutoSize = false;
			titleLabel.Location = new System.Drawing.Point(56, 8);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new System.Drawing.Size(358, 19);
			titleLabel.TabIndex = 2;
			// 
			// AudiusTrackListItemControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			BackColor = System.Drawing.Color.FromArgb(252, 252, 252);
			Controls.Add(playButton);
			Controls.Add(addButton);
			Controls.Add(titleLabel);
			Controls.Add(durationLabel);
			Margin = new System.Windows.Forms.Padding(0);
			Name = "AudiusTrackListItemControl";
			Size = new System.Drawing.Size(463, 32);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton playButton;
		private Polycode.NostalgicPlayer.Controls.Buttons.NostalgicImageButton addButton;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel titleLabel;
		private Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel durationLabel;
	}
}
