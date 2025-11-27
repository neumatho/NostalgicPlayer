/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Library.Containers;
using Polycode.NostalgicPlayer.Library.Sound.Equalizer;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.EqualizerWindow
{
	/// <summary>
	/// Equalizer control for the Mixer settings page
	/// </summary>
	public partial class EqualizerControl : UserControl
	{
		private bool suppressEvents;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EqualizerControl()
		{
			InitializeComponent();

			// Populate preset context menu (calls UpdatePresetCheckmarks which
			// populates combo box)
			BuildPresetContextMenu();
		}



		/********************************************************************/
		/// <summary>
		/// Get whether equalizer is enabled
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsEqualizerEnabled
		{
			get => enableEqualizerCheckBox.Checked;

			set
			{
				suppressEvents = true;
				try
				{
					enableEqualizerCheckBox.Checked = value;
					UpdateControlStates();
				}
				finally
				{
					suppressEvents = false;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get/Set pre-amp gain in dB
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public double PreAmpGain
		{
			get => preAmpTrackBar.Value / 10.0; // Convert from tenths of dB to dB

			set
			{
				suppressEvents = true;
				try
				{
					// Convert dB to tenths of dB and clamp
					int trackBarValue = (int)Math.Round(value * 10);
					trackBarValue = Math.Max(-120, Math.Min(120, trackBarValue));
					preAmpTrackBar.Value = trackBarValue;
					preAmpValueLabel.Text = (trackBarValue / 10.0).ToString("F1");
				}
				finally
				{
					suppressEvents = false;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Event fired when equalizer settings change
		/// </summary>
		/********************************************************************/
		public event EventHandler EqualizerChanged;



		/********************************************************************/
		/// <summary>
		/// Get current equalizer band values in dB
		/// </summary>
		/********************************************************************/
		public double[] GetBandValues()
		{
			double[] values = new double[10];

			for (int i = 0; i < 10; i++)
				values[i] = GetTrackBar(i).Value / 10.0; // Convert from tenths of dB to dB

			return values;
		}



		/********************************************************************/
		/// <summary>
		/// Set equalizer band values
		/// </summary>
		/********************************************************************/
		public void SetBandValues(double[] values)
		{
			if (values == null || values.Length != 10)
				return;

			suppressEvents = true;

			try
			{
				for (int i = 0; i < 10; i++)
				{
					// Convert dB to tenths of dB and clamp
					int value = (int)Math.Round(values[i] * 10);
					value = Math.Max(-120, Math.Min(120, value));
					SetTrackBarValue(i, value);

					// Update value label
					SetValueLabelText(i, (value / 10.0).ToString("F1"));
				}
			}
			finally
			{
				suppressEvents = false;
			}

			// Update preset checkmarks to show matching preset
			UpdatePresetCheckmarks();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Get track bar for a specific band
		/// </summary>
		/********************************************************************/
		private KryptonTrackBar GetTrackBar(int index)
		{
			return index switch
			{
				0 => band0TrackBar,
				1 => band1TrackBar,
				2 => band2TrackBar,
				3 => band3TrackBar,
				4 => band4TrackBar,
				5 => band5TrackBar,
				6 => band6TrackBar,
				7 => band7TrackBar,
				8 => band8TrackBar,
				9 => band9TrackBar,
				_ => throw new ArgumentOutOfRangeException(nameof(index), index, "Band index must be between 0 and 9")
			};
		}



		/********************************************************************/
		/// <summary>
		/// Set track bar value for a specific band
		/// </summary>
		/********************************************************************/
		private void SetTrackBarValue(int index, int value)
		{
			switch (index)
			{
				case 0:
					band0TrackBar.Value = value;
					break;
				case 1:
					band1TrackBar.Value = value;
					break;
				case 2:
					band2TrackBar.Value = value;
					break;
				case 3:
					band3TrackBar.Value = value;
					break;
				case 4:
					band4TrackBar.Value = value;
					break;
				case 5:
					band5TrackBar.Value = value;
					break;
				case 6:
					band6TrackBar.Value = value;
					break;
				case 7:
					band7TrackBar.Value = value;
					break;
				case 8:
					band8TrackBar.Value = value;
					break;
				case 9:
					band9TrackBar.Value = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(index), index, "Band index must be between 0 and 9");
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set value label text for a specific band
		/// </summary>
		/********************************************************************/
		private void SetValueLabelText(int index, string text)
		{
			switch (index)
			{
				case 0:
					valueLabel0.Values.Text = text;
					break;
				case 1:
					valueLabel1.Values.Text = text;
					break;
				case 2:
					valueLabel2.Values.Text = text;
					break;
				case 3:
					valueLabel3.Values.Text = text;
					break;
				case 4:
					valueLabel4.Values.Text = text;
					break;
				case 5:
					valueLabel5.Values.Text = text;
					break;
				case 6:
					valueLabel6.Values.Text = text;
					break;
				case 7:
					valueLabel7.Values.Text = text;
					break;
				case 8:
					valueLabel8.Values.Text = text;
					break;
				case 9:
					valueLabel9.Values.Text = text;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(index), index, "Band index must be between 0 and 9");
			}
		}



		/********************************************************************/
		/// <summary>
		/// Build the preset context menu with all available presets
		/// </summary>
		/********************************************************************/
		private void BuildPresetContextMenu()
		{
			KryptonContextMenuItems menuItems = new();
			EqualizerPreset[] allPresets = EqualizerPresets.GetAllPresets();

			for (int i = 0; i < allPresets.Length; i++)
			{
				EqualizerPreset preset = allPresets[i];
				KryptonContextMenuItem item = new() {Text = preset.GetDisplayName(), Tag = preset, Checked = false};
				item.Click += PresetMenuItem_Click;
				menuItems.Items.Add(item);
			}

			presetContextMenu.Items.Clear();
			presetContextMenu.Items.Add(menuItems);
			presetContextMenu.Opening += PresetContextMenu_Opening;

			// Set initial checkmark on Custom preset
			UpdatePresetCheckmarks();
		}



		/********************************************************************/
		/// <summary>
		/// Handle preset context menu opening - update checkmarks
		/// </summary>
		/********************************************************************/
		private void PresetContextMenu_Opening(object sender, CancelEventArgs e)
		{
			UpdatePresetCheckmarks();
		}



		/********************************************************************/
		/// <summary>
		/// Handle preset menu item click
		/// </summary>
		/********************************************************************/
		private void PresetMenuItem_Click(object sender, EventArgs e)
		{
			if (sender is KryptonContextMenuItem menuItem && menuItem.Tag is EqualizerPreset preset)
			{
				// Skip if Custom is selected (Custom has no predefined values)
				if (preset == EqualizerPreset.Custom) return;

				double[] presetValues = EqualizerPresets.GetPreset(preset);
				if (presetValues != null)
				{
					SetBandValues(presetValues);
					EqualizerChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Update checkmarks in preset menu to show current preset
		/// </summary>
		/********************************************************************/
		private void UpdatePresetCheckmarks()
		{
			double[] currentValues = GetBandValues();
			EqualizerPreset matchingPreset = EqualizerPresets.FindMatchingPreset(currentValues);

			// Update context menu checkmarks
			if (presetContextMenu.Items.Count > 0 && presetContextMenu.Items[0] is KryptonContextMenuItems menuItems)
			{
				foreach (KryptonContextMenuItemBase menuItem in menuItems.Items)
				{
					if (menuItem is KryptonContextMenuItem item && item.Tag is EqualizerPreset preset)
						item.Checked = preset == matchingPreset;
				}
			}

			// Update combo box selection
			UpdatePresetComboBox(matchingPreset);
		}



		/********************************************************************/
		/// <summary>
		/// Update preset combo box to show current preset
		/// </summary>
		/********************************************************************/
		private void UpdatePresetComboBox(EqualizerPreset preset)
		{
			suppressEvents = true;

			try
			{
				presetComboBox.BeginUpdate();

				try
				{
					presetComboBox.Items.Clear();
					EqualizerPreset[] allPresets = EqualizerPresets.GetAllPresets();

					foreach (EqualizerPreset p in allPresets)
					{
						int index = presetComboBox.Items.Add(p.GetDisplayName());

						if (p == preset)
							presetComboBox.SelectedIndex = index;
					}
				}
				finally
				{
					presetComboBox.EndUpdate();
				}
			}
			finally
			{
				suppressEvents = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle preset combo box selection changed
		/// </summary>
		/********************************************************************/
		private void PresetComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;

			EqualizerPreset[] allPresets = EqualizerPresets.GetAllPresets();
			int selectedIndex = presetComboBox.SelectedIndex;

			if (selectedIndex >= 0 && selectedIndex < allPresets.Length)
			{
				EqualizerPreset preset = allPresets[selectedIndex];

				// Skip if Custom is selected (Custom has no predefined values)
				if (preset == EqualizerPreset.Custom)
					return;

				double[] presetValues = EqualizerPresets.GetPreset(preset);
				if (presetValues != null)
				{
					SetBandValues(presetValues);
					EqualizerChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Enable equalizer checkbox changed
		/// </summary>
		/********************************************************************/
		private void EnableEqualizerCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateControlStates();

			if (!suppressEvents)
				EqualizerChanged?.Invoke(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Update enabled state of controls
		/// </summary>
		/********************************************************************/
		private void UpdateControlStates()
		{
			controlsPanel.SuspendLayout();

			try
			{
				controlsPanel.Enabled = enableEqualizerCheckBox.Checked;
			}
			finally
			{
				controlsPanel.ResumeLayout();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Band track bar value changed
		/// </summary>
		/********************************************************************/
		private void BandTrackBar_ValueChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;

			// Update the value label for the changed trackbar
			if (sender is KryptonTrackBar trackBar && trackBar.Tag is int bandIndex)
				SetValueLabelText(bandIndex, (trackBar.Value / 10.0).ToString("F1"));

			UpdatePresetCheckmarks();
			EqualizerChanged?.Invoke(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// PreAmp track bar value changed
		/// </summary>
		/********************************************************************/
		private void PreAmpTrackBar_ValueChanged(object sender, EventArgs e)
		{
			if (suppressEvents)
				return;

			preAmpValueLabel.Text = (preAmpTrackBar.Value / 10.0).ToString("F1");
			EqualizerChanged?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
