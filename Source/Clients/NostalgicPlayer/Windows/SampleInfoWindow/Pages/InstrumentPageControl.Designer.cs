namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow.Pages
{
	partial class InstrumentPageControl
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
			instrumentDataGridView = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicDataGridView();
			monoFontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			((System.ComponentModel.ISupportInitialize)instrumentDataGridView).BeginInit();
			SuspendLayout();
			// 
			// instrumentDataGridView
			// 
			instrumentDataGridView.AllowUserToOrderColumns = true;
			instrumentDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			instrumentDataGridView.Location = new System.Drawing.Point(8, 8);
			instrumentDataGridView.Name = "instrumentDataGridView";
			instrumentDataGridView.Size = new System.Drawing.Size(414, 130);
			instrumentDataGridView.TabIndex = 0;
			instrumentDataGridView.UseFont = monoFontConfiguration;
			// 
			// monoFontConfiguration
			// 
			monoFontConfiguration.FontType = NostalgicPlayer.Controls.FontType.Monospace;
			// 
			// InstrumentPageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(instrumentDataGridView);
			Name = "InstrumentPageControl";
			Size = new System.Drawing.Size(430, 146);
			((System.ComponentModel.ISupportInitialize)instrumentDataGridView).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private NostalgicPlayer.Controls.Lists.NostalgicDataGridView instrumentDataGridView;
		private NostalgicPlayer.Controls.Components.FontConfiguration monoFontConfiguration;
	}
}
