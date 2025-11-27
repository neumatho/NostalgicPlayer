namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	partial class SearchPopupForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private SearchPopupControl searchPopupControl;

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
			searchPopupControl = new SearchPopupControl();
			SuspendLayout();
			//
			// searchPopupControl
			//
			searchPopupControl.Dock = System.Windows.Forms.DockStyle.Fill;
			searchPopupControl.Location = new System.Drawing.Point(0, 0);
			searchPopupControl.Name = "searchPopupControl";
			searchPopupControl.Size = new System.Drawing.Size(300, 400);
			searchPopupControl.TabIndex = 0;
			//
			// SearchPopupForm
			//
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(300, 100);
			Controls.Add(searchPopupControl);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SearchPopupForm";
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			TopMost = true;
			ResumeLayout(false);
		}

		#endregion
	}
}
