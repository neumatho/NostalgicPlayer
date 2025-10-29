using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Library.Sound.Mixer;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	/// <summary>
	/// Equalizer control for the Mixer settings page
	/// </summary>
	public partial class EqualizerControl : UserControl
	{
		private bool suppressEvents = false;
		private KryptonTrackBar[] bandTrackBars;
		private KryptonLabel[] bandValueLabels;
		private KryptonContextMenuItem[] presetMenuItems;

		public event EventHandler EqualizerChanged;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EqualizerControl()
		{
			InitializeComponent();

			// Store track bars for easy access
			bandTrackBars = new[]
			{
				band0TrackBar, band1TrackBar, band2TrackBar, band3TrackBar, band4TrackBar,
				band5TrackBar, band6TrackBar, band7TrackBar, band8TrackBar, band9TrackBar
			};

			// Store value labels
			bandValueLabels = valueLabels;

			// Populate preset context menu
			BuildPresetContextMenu();
		}

		/********************************************************************/
		/// <summary>
		/// Build the preset context menu with all available presets
		/// </summary>
		/********************************************************************/
		private void BuildPresetContextMenu()
		{
			KryptonContextMenuItems menuItems = new KryptonContextMenuItems();
			EqualizerPreset[] allPresets = EqualizerPresets.GetAllPresets();
			presetMenuItems = new KryptonContextMenuItem[allPresets.Length];

			for (int i = 0; i < allPresets.Length; i++)
			{
				EqualizerPreset preset = allPresets[i];
				KryptonContextMenuItem item = new KryptonContextMenuItem
				{
					Text = preset.GetDisplayName(),
					Tag = preset,
					Checked = false
				};
				item.Click += PresetMenuItem_Click;
				menuItems.Items.Add(item);
				presetMenuItems[i] = item;
			}

			presetContextMenu.Items.Clear();
			presetContextMenu.Items.Add(menuItems);

			// Set initial checkmark on Custom preset
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
				if (preset == EqualizerPreset.Custom)
					return;

				double[] presetValues = EqualizerPresets.GetPreset(preset);
				if (presetValues != null)
				{
					SetBandValues(presetValues);
					UpdatePresetCheckmarks();
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

			// Update checkmarks
			foreach (KryptonContextMenuItem item in presetMenuItems)
			{
				if (item.Tag is EqualizerPreset preset)
				{
					item.Checked = (preset == matchingPreset);
				}
			}
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
				enableEqualizerCheckBox.Checked = value;
				UpdateControlStates();
				suppressEvents = false;
			}
		}

		/********************************************************************/
		/// <summary>
		/// Get current equalizer band values in dB
		/// </summary>
		/********************************************************************/
		public double[] GetBandValues()
		{
			double[] values = new double[10];
			for (int i = 0; i < 10; i++)
			{
				values[i] = bandTrackBars[i].Value / 10.0; // Convert from tenths of dB to dB
			}
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
			for (int i = 0; i < 10; i++)
			{
				// Convert dB to tenths of dB and clamp
				int value = (int)Math.Round(values[i] * 10);
				value = Math.Max(-120, Math.Min(120, value));
				bandTrackBars[i].Value = value;

				// Update value label
				bandValueLabels[i].Values.Text = (value / 10.0).ToString("F1");
			}
			suppressEvents = false;

			// Update preset checkmarks to show matching preset
			UpdatePresetCheckmarks();
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
			controlsPanel.Enabled = enableEqualizerCheckBox.Checked;
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
			{
				bandValueLabels[bandIndex].Values.Text = (trackBar.Value / 10.0).ToString("F1");
			}

			UpdatePresetCheckmarks();
			EqualizerChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
