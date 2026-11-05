namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow
{
	partial class ModLibraryDownloadControl
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
			progressBar = new Polycode.NostalgicPlayer.Controls.Progress.NostalgicProgressBar();
			cancelButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicButton();
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
			// 
			// progressBar
			// 
			progressBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			progressBar.Location = new System.Drawing.Point(0, 0);
			progressBar.Name = "progressBar";
			progressBar.Size = new System.Drawing.Size(421, 23);
			progressBar.TabIndex = 0;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			cancelButton.Location = new System.Drawing.Point(425, 0);
			cancelButton.Name = "cancelButton";
			controlResource.SetResourceKey(cancelButton, "IDS_MODLIBRARY_CANCEL");
			cancelButton.Size = new System.Drawing.Size(75, 23);
			cancelButton.TabIndex = 1;
			cancelButton.Text = "Cancel";
			cancelButton.Click += CancelButton_Click;
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// ModLibraryDownloadControl
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(cancelButton);
			Controls.Add(progressBar);
			Name = "ModLibraryDownloadControl";
			Size = new System.Drawing.Size(500, 23);
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private NostalgicPlayer.Controls.Buttons.NostalgicButton cancelButton;
		private Kit.Gui.Designer.ControlResource controlResource;
		private NostalgicPlayer.Controls.Progress.NostalgicProgressBar progressBar;
	}
}
