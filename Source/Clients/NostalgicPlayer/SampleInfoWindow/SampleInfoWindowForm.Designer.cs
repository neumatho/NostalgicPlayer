
namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SampleInfoWindow
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SampleInfoWindowForm));
			this.kryptonManager = new Krypton.Toolkit.KryptonManager(this.components);
			this.navigator = new Krypton.Navigator.KryptonNavigator();
			this.navigatorInstrumentPage = new Krypton.Navigator.KryptonPage();
			this.instrumentGroup = new Krypton.Toolkit.KryptonGroup();
			this.instrumentDataGridView = new Krypton.Toolkit.KryptonDataGridView();
			this.navigatorSamplePage = new Krypton.Navigator.KryptonPage();
			this.sampleGroup = new Krypton.Toolkit.KryptonGroup();
			this.sampleDataGridView = new Krypton.Toolkit.KryptonDataGridView();
			((System.ComponentModel.ISupportInitialize)(this.navigator)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorInstrumentPage)).BeginInit();
			this.navigatorInstrumentPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.instrumentGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.instrumentGroup.Panel)).BeginInit();
			this.instrumentGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.instrumentDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorSamplePage)).BeginInit();
			this.navigatorSamplePage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sampleGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sampleGroup.Panel)).BeginInit();
			this.sampleGroup.Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sampleDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// navigator
			// 
			this.navigator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.navigator.Location = new System.Drawing.Point(8, 8);
			this.navigator.Name = "navigator";
			this.navigator.Pages.AddRange(new Krypton.Navigator.KryptonPage[] {
            this.navigatorInstrumentPage,
            this.navigatorSamplePage});
			this.navigator.SelectedIndex = 0;
			this.navigator.Size = new System.Drawing.Size(282, 172);
			this.navigator.StateCommon.Tab.Content.ShortText.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.navigator.TabIndex = 0;
			// 
			// navigatorInstrumentPage
			// 
			this.navigatorInstrumentPage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorInstrumentPage.Controls.Add(this.instrumentGroup);
			this.navigatorInstrumentPage.Flags = 65534;
			this.navigatorInstrumentPage.LastVisibleSet = true;
			this.navigatorInstrumentPage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorInstrumentPage.Name = "navigatorInstrumentPage";
			this.navigatorInstrumentPage.Size = new System.Drawing.Size(280, 145);
			this.navigatorInstrumentPage.Text = "";
			this.navigatorInstrumentPage.ToolTipTitle = "Page ToolTip";
			this.navigatorInstrumentPage.UniqueName = "5d888e6082d44d78aac10a8a0c09a21e";
			// 
			// instrumentGroup
			// 
			this.instrumentGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.instrumentGroup.Location = new System.Drawing.Point(8, 8);
			this.instrumentGroup.Name = "instrumentGroup";
			// 
			// 
			// 
			this.instrumentGroup.Panel.Controls.Add(this.instrumentDataGridView);
			this.instrumentGroup.Size = new System.Drawing.Size(264, 129);
			this.instrumentGroup.TabIndex = 0;
			// 
			// instrumentDataGridView
			// 
			this.instrumentDataGridView.AllowUserToAddRows = false;
			this.instrumentDataGridView.AllowUserToDeleteRows = false;
			this.instrumentDataGridView.AllowUserToOrderColumns = true;
			this.instrumentDataGridView.AllowUserToResizeRows = false;
			this.instrumentDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.instrumentDataGridView.Location = new System.Drawing.Point(0, 0);
			this.instrumentDataGridView.Name = "instrumentDataGridView";
			this.instrumentDataGridView.ReadOnly = true;
			this.instrumentDataGridView.RowHeadersVisible = false;
			this.instrumentDataGridView.RowTemplate.Height = 25;
			this.instrumentDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.instrumentDataGridView.ShowCellErrors = false;
			this.instrumentDataGridView.ShowEditingIcon = false;
			this.instrumentDataGridView.ShowRowErrors = false;
			this.instrumentDataGridView.Size = new System.Drawing.Size(262, 127);
			this.instrumentDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			this.instrumentDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			this.instrumentDataGridView.StateCommon.DataCell.Content.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.instrumentDataGridView.StateCommon.DataCell.Content.Padding = new System.Windows.Forms.Padding(0);
			this.instrumentDataGridView.StateCommon.HeaderColumn.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.instrumentDataGridView.TabIndex = 0;
			// 
			// navigatorSamplePage
			// 
			this.navigatorSamplePage.AutoHiddenSlideSize = new System.Drawing.Size(200, 200);
			this.navigatorSamplePage.Controls.Add(this.sampleGroup);
			this.navigatorSamplePage.Flags = 65534;
			this.navigatorSamplePage.LastVisibleSet = true;
			this.navigatorSamplePage.MinimumSize = new System.Drawing.Size(50, 50);
			this.navigatorSamplePage.Name = "navigatorSamplePage";
			this.navigatorSamplePage.Size = new System.Drawing.Size(680, 145);
			this.navigatorSamplePage.Text = "";
			this.navigatorSamplePage.ToolTipTitle = "Page ToolTip";
			this.navigatorSamplePage.UniqueName = "6f932f4ccf4b4441a28afbd3a888c881";
			// 
			// sampleGroup
			// 
			this.sampleGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.sampleGroup.Location = new System.Drawing.Point(8, 8);
			this.sampleGroup.Name = "sampleGroup";
			// 
			// 
			// 
			this.sampleGroup.Panel.Controls.Add(this.sampleDataGridView);
			this.sampleGroup.Size = new System.Drawing.Size(664, 129);
			this.sampleGroup.TabIndex = 1;
			// 
			// sampleDataGridView
			// 
			this.sampleDataGridView.AllowUserToAddRows = false;
			this.sampleDataGridView.AllowUserToDeleteRows = false;
			this.sampleDataGridView.AllowUserToOrderColumns = true;
			this.sampleDataGridView.AllowUserToResizeRows = false;
			this.sampleDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sampleDataGridView.Location = new System.Drawing.Point(0, 0);
			this.sampleDataGridView.Name = "sampleDataGridView";
			this.sampleDataGridView.ReadOnly = true;
			this.sampleDataGridView.RowHeadersVisible = false;
			this.sampleDataGridView.RowTemplate.Height = 25;
			this.sampleDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.sampleDataGridView.ShowCellErrors = false;
			this.sampleDataGridView.ShowEditingIcon = false;
			this.sampleDataGridView.ShowRowErrors = false;
			this.sampleDataGridView.Size = new System.Drawing.Size(662, 127);
			this.sampleDataGridView.StateCommon.Background.Color1 = System.Drawing.Color.White;
			this.sampleDataGridView.StateCommon.BackStyle = Krypton.Toolkit.PaletteBackStyle.GridBackgroundList;
			this.sampleDataGridView.StateCommon.DataCell.Content.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.sampleDataGridView.StateCommon.DataCell.Content.Padding = new System.Windows.Forms.Padding(0);
			this.sampleDataGridView.StateCommon.HeaderColumn.Content.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			this.sampleDataGridView.TabIndex = 0;
			// 
			// SampleInfoWindowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(298, 188);
			this.Controls.Add(this.navigator);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(314, 227);
			this.Name = "SampleInfoWindowForm";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SampleInfoWindowForm_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.navigator)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorInstrumentPage)).EndInit();
			this.navigatorInstrumentPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.instrumentGroup.Panel)).EndInit();
			this.instrumentGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.instrumentGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.instrumentDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.navigatorSamplePage)).EndInit();
			this.navigatorSamplePage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.sampleGroup.Panel)).EndInit();
			this.sampleGroup.Panel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.sampleGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sampleDataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Krypton.Toolkit.KryptonManager kryptonManager;
		private Krypton.Navigator.KryptonNavigator navigator;
		private Krypton.Navigator.KryptonPage navigatorInstrumentPage;
		private Krypton.Navigator.KryptonPage navigatorSamplePage;
		private Krypton.Toolkit.KryptonGroup instrumentGroup;
		private Krypton.Toolkit.KryptonDataGridView instrumentDataGridView;
		private Krypton.Toolkit.KryptonGroup sampleGroup;
		private Krypton.Toolkit.KryptonDataGridView sampleDataGridView;
	}
}