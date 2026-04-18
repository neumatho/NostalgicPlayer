namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.MainWindow
{
	partial class SearchPopupControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private Polycode.NostalgicPlayer.Controls.Inputs.NostalgicTextBox searchTextBox;
		private Polycode.NostalgicPlayer.Controls.Lists.NostalgicModuleList resultsList;

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
			searchTextBox = new Polycode.NostalgicPlayer.Controls.Inputs.NostalgicTextBox();
			resultsList = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicModuleList();
			SuspendLayout();
			// 
			// searchTextBox
			// 
			searchTextBox.Dock = System.Windows.Forms.DockStyle.Top;
			searchTextBox.Location = new System.Drawing.Point(0, 0);
			searchTextBox.Multiline = false;
			searchTextBox.Name = "searchTextBox";
			searchTextBox.Size = new System.Drawing.Size(300, 23);
			searchTextBox.TabIndex = 0;
			searchTextBox.Text = "";
			// 
			// resultsList
			// 
			resultsList.Dock = System.Windows.Forms.DockStyle.Fill;
			resultsList.Location = new System.Drawing.Point(0, 23);
			resultsList.Name = "resultsList";
			resultsList.Size = new System.Drawing.Size(300, 377);
			resultsList.TabIndex = 1;
			// 
			// SearchPopupControl
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(resultsList);
			Controls.Add(searchTextBox);
			Name = "SearchPopupControl";
			Size = new System.Drawing.Size(300, 400);
			ResumeLayout(false);
		}

		#endregion
	}
}
