namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.SampleInfoWindow.Pages
{
	partial class SamplePageControl
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
			saveFormatLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			saveFormatComboBox = new Polycode.NostalgicPlayer.Controls.Lists.NostalgicComboBox();
			saveButton = new Polycode.NostalgicPlayer.Controls.Buttons.NostalgicButton();
			polyphonyLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			sampleDataGridView = new SampleInfoSamplesListControl();
			monoFontConfiguration = new Polycode.NostalgicPlayer.Controls.Components.FontConfiguration(components);
			octaveLabel = new Polycode.NostalgicPlayer.Controls.Texts.NostalgicLabel();
			controlResource = new Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource();
			((System.ComponentModel.ISupportInitialize)sampleDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)controlResource).BeginInit();
			SuspendLayout();
			// 
			// saveFormatLabel
			// 
			saveFormatLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			saveFormatLabel.AutoSize = true;
			saveFormatLabel.Location = new System.Drawing.Point(123, 124);
			saveFormatLabel.Name = "saveFormatLabel";
			controlResource.SetResourceKey(saveFormatLabel, "IDS_SAMPLE_INFO_SAMP_SAVEFORMAT");
			saveFormatLabel.Size = new System.Drawing.Size(64, 13);
			saveFormatLabel.TabIndex = 3;
			saveFormatLabel.Text = "Save format";
			// 
			// saveFormatComboBox
			// 
			saveFormatComboBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			saveFormatComboBox.DropDownWidth = 120;
			saveFormatComboBox.IntegralHeight = false;
			saveFormatComboBox.Location = new System.Drawing.Point(198, 121);
			saveFormatComboBox.Name = "saveFormatComboBox";
			saveFormatComboBox.Size = new System.Drawing.Size(160, 21);
			saveFormatComboBox.TabIndex = 4;
			saveFormatComboBox.SelectedIndexChanged += SaveFormatComboBox_SelectedIndexChanged;
			// 
			// saveButton
			// 
			saveButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			saveButton.Enabled = false;
			saveButton.Location = new System.Drawing.Point(362, 121);
			saveButton.Name = "saveButton";
			controlResource.SetResourceKey(saveButton, "IDS_SAMPLE_INFO_SAMP_SAVE");
			saveButton.Size = new System.Drawing.Size(60, 21);
			saveButton.TabIndex = 5;
			saveButton.Text = "Save";
			saveButton.Click += SaveButton_Click;
			// 
			// polyphonyLabel
			// 
			polyphonyLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			polyphonyLabel.AutoSize = true;
			polyphonyLabel.Location = new System.Drawing.Point(75, 124);
			polyphonyLabel.Name = "polyphonyLabel";
			polyphonyLabel.Size = new System.Drawing.Size(13, 13);
			polyphonyLabel.TabIndex = 2;
			polyphonyLabel.Text = "?";
			// 
			// sampleDataGridView
			// 
			sampleDataGridView.AllowUserToOrderColumns = true;
			sampleDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			sampleDataGridView.Location = new System.Drawing.Point(8, 8);
			sampleDataGridView.Name = "sampleDataGridView";
			sampleDataGridView.Size = new System.Drawing.Size(414, 108);
			sampleDataGridView.TabIndex = 1;
			sampleDataGridView.UseFont = monoFontConfiguration;
			sampleDataGridView.SelectionChanged += SampleDataGridView_SelectionChanged;
			sampleDataGridView.KeyDown += SamplesDataGridView_KeyDown;
			sampleDataGridView.KeyUp += SamplesDataGridView_KeyUp;
			// 
			// monoFontConfiguration
			// 
			monoFontConfiguration.FontType = NostalgicPlayer.Controls.FontType.Monospace;
			// 
			// octaveLabel
			// 
			octaveLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			octaveLabel.AutoSize = true;
			octaveLabel.Location = new System.Drawing.Point(4, 124);
			octaveLabel.Name = "octaveLabel";
			octaveLabel.Size = new System.Drawing.Size(13, 13);
			octaveLabel.TabIndex = 1;
			octaveLabel.Text = "?";
			// 
			// controlResource
			// 
			controlResource.ResourceClassName = "Polycode.NostalgicPlayer.Client.GuiPlayer.Resources";
			// 
			// SamplePageControl
			// 
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Controls.Add(saveFormatLabel);
			Controls.Add(saveFormatComboBox);
			Controls.Add(saveButton);
			Controls.Add(polyphonyLabel);
			Controls.Add(sampleDataGridView);
			Controls.Add(octaveLabel);
			Name = "SamplePageControl";
			Size = new System.Drawing.Size(430, 146);
			((System.ComponentModel.ISupportInitialize)sampleDataGridView).EndInit();
			((System.ComponentModel.ISupportInitialize)controlResource).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private NostalgicPlayer.Controls.Texts.NostalgicLabel saveFormatLabel;
		private NostalgicPlayer.Controls.Lists.NostalgicComboBox saveFormatComboBox;
		private NostalgicPlayer.Controls.Buttons.NostalgicButton saveButton;
		private NostalgicPlayer.Controls.Texts.NostalgicLabel polyphonyLabel;
		private SampleInfoSamplesListControl sampleDataGridView;
		private NostalgicPlayer.Controls.Texts.NostalgicLabel octaveLabel;
		private Polycode.NostalgicPlayer.Kit.Gui.Designer.ControlResource controlResource;
		private NostalgicPlayer.Controls.Components.FontConfiguration monoFontConfiguration;
	}
}
