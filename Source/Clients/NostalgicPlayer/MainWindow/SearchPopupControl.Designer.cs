namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	partial class SearchPopupControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private Krypton.Toolkit.KryptonTextBox searchTextBox;
		private ModuleListControl resultsListControl;

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
			searchTextBox = new Krypton.Toolkit.KryptonTextBox();
			resultsListControl = new ModuleListControl();
			SuspendLayout();
			//
			// searchTextBox
			//
			searchTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			searchTextBox.Location = new System.Drawing.Point(0, 0);
			searchTextBox.Name = "searchTextBox";
			searchTextBox.Size = new System.Drawing.Size(300, 23);
			searchTextBox.StateCommon.Content.Font = new System.Drawing.Font("Microsoft Sans", 8F);
			searchTextBox.TabIndex = 0;
			//
			// resultsListControl
			//
			resultsListControl.AllowDrop = false;
			resultsListControl.Dock = System.Windows.Forms.DockStyle.Fill;
			resultsListControl.Location = new System.Drawing.Point(0, 23);
			resultsListControl.Name = "resultsListControl";
			resultsListControl.Size = new System.Drawing.Size(300, 377);
			resultsListControl.TabIndex = 1;
			//
			// SearchPopupControl
			//
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(resultsListControl);
			Controls.Add(searchTextBox);
			Name = "SearchPopupControl";
			Size = new System.Drawing.Size(300, 400);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
	}
}
