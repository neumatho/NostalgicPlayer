
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow
{
	partial class SampleInfoWindowForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SampleInfoWindowForm));
			tabControl = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTab();
			tabInstrumentPage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			instrumentPageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow.Pages.InstrumentPageControl();
			tabSamplePage = new Polycode.NostalgicPlayer.Controls.Containers.NostalgicTabPage();
			samplePageControl = new Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow.Pages.SamplePageControl();
			((System.ComponentModel.ISupportInitialize)tabControl).BeginInit();
			tabControl.SuspendLayout();
			tabInstrumentPage.SuspendLayout();
			tabSamplePage.SuspendLayout();
			SuspendLayout();
			// 
			// tabControl
			// 
			tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tabControl.Controls.Add(tabInstrumentPage);
			tabControl.Controls.Add(tabSamplePage);
			tabControl.Location = new System.Drawing.Point(8, 8);
			tabControl.Name = "tabControl";
			tabControl.SelectedIndex = 0;
			tabControl.Size = new System.Drawing.Size(432, 172);
			tabControl.TabIndex = 0;
			tabControl.SelectedIndexChanged += Tab_SelectedIndexChanged;
			// 
			// tabInstrumentPage
			// 
			tabInstrumentPage.BackColor = System.Drawing.Color.Transparent;
			tabInstrumentPage.Controls.Add(instrumentPageControl);
			tabInstrumentPage.Location = new System.Drawing.Point(1, 24);
			tabInstrumentPage.MinimumSize = new System.Drawing.Size(50, 50);
			tabInstrumentPage.Name = "tabInstrumentPage";
			tabInstrumentPage.Size = new System.Drawing.Size(430, 145);
			tabInstrumentPage.TabIndex = 0;
			// 
			// instrumentPageControl
			// 
			instrumentPageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			instrumentPageControl.Location = new System.Drawing.Point(0, 0);
			instrumentPageControl.Name = "instrumentPageControl";
			instrumentPageControl.Size = new System.Drawing.Size(430, 145);
			instrumentPageControl.TabIndex = 0;
			// 
			// tabSamplePage
			// 
			tabSamplePage.BackColor = System.Drawing.Color.Transparent;
			tabSamplePage.Controls.Add(samplePageControl);
			tabSamplePage.Location = new System.Drawing.Point(1, 24);
			tabSamplePage.MinimumSize = new System.Drawing.Size(50, 50);
			tabSamplePage.Name = "tabSamplePage";
			tabSamplePage.Size = new System.Drawing.Size(430, 145);
			tabSamplePage.TabIndex = 1;
			// 
			// samplePageControl
			// 
			samplePageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			samplePageControl.Location = new System.Drawing.Point(0, 0);
			samplePageControl.Name = "samplePageControl";
			samplePageControl.Size = new System.Drawing.Size(430, 145);
			samplePageControl.TabIndex = 0;
			// 
			// SampleInfoWindowForm
			// 
			ClientSize = new System.Drawing.Size(448, 188);
			Controls.Add(tabControl);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(464, 227);
			Name = "SampleInfoWindowForm";
			FormClosed += SampleInfoWindowForm_FormClosed;
			((System.ComponentModel.ISupportInitialize)tabControl).EndInit();
			tabControl.ResumeLayout(false);
			tabInstrumentPage.ResumeLayout(false);
			tabSamplePage.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private NostalgicPlayer.Controls.Containers.NostalgicTab tabControl;
		private NostalgicPlayer.Controls.Containers.NostalgicTabPage tabInstrumentPage;
		private NostalgicPlayer.Controls.Containers.NostalgicTabPage tabSamplePage;
		private Pages.InstrumentPageControl instrumentPageControl;
		private Pages.SamplePageControl samplePageControl;
	}
}