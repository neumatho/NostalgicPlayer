/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Library.Containers;
using System;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.EqualizerWindow
{
	/// <summary>
	///     Equalizer window form
	/// </summary>
	public partial class EqualizerWindowForm : WindowFormBase
	{
		private const string WindowSettingsName = "EqualizerWindow";
		private readonly EqualizerControl equalizerControl;

		private readonly ModuleHandler moduleHandler;
		private readonly SoundSettings soundSettings;

		/********************************************************************/
		/// <summary>
		///     Constructor
		/// </summary>
		/********************************************************************/
		public EqualizerWindowForm(ModuleHandler moduleHandler, IMainWindowApi mainWindow, OptionSettings optSettings,
			SoundSettings soundSettings)
		{
			InitializeComponent();

			this.moduleHandler = moduleHandler;
			this.soundSettings = soundSettings;

			// Set window title
			Text = Resources.IDS_SETTINGS_MIXER_EQUALIZER;

			// Initialize the window
			InitializeWindow(mainWindow, optSettings);
			LoadWindowSettings(WindowSettingsName);

			// Create and configure equalizer control
			equalizerControl = new EqualizerControl {Dock = DockStyle.Fill};
			equalizerPanel.Controls.Add(equalizerControl);

			// Load settings into control
			LoadEqualizerSettings();

			// Hook up event handler
			equalizerControl.EqualizerChanged += EqualizerControl_EqualizerChanged;
		}

		#region Private methods

		/********************************************************************/
		/// <summary>
		///     Load equalizer settings from SoundSettings
		/// </summary>
		/********************************************************************/
		private void LoadEqualizerSettings()
		{
			equalizerControl.IsEqualizerEnabled = soundSettings.EnableEqualizer;
			equalizerControl.SetBandValues(soundSettings.EqualizerBands);
			equalizerControl.PreAmpGain = soundSettings.EqualizerPreAmp;
		}

		/********************************************************************/
		/// <summary>
		///     Equalizer control values changed - apply to mixer in real-time
		/// </summary>
		/********************************************************************/
		private void EqualizerControl_EqualizerChanged(object sender, EventArgs e)
		{
			// Save to settings (in memory, not to disk)
			soundSettings.EnableEqualizer = equalizerControl.IsEqualizerEnabled;
			soundSettings.EqualizerBands = equalizerControl.GetBandValues();
			soundSettings.EqualizerPreAmp = equalizerControl.PreAmpGain;

			// Apply to mixer immediately (for live preview)
			ApplyEqualizerToMixer();
		}

		/********************************************************************/
		/// <summary>
		///     Apply equalizer settings to the mixer (uses current control
		///     values, not saved settings)
		/// </summary>
		/********************************************************************/
		private void ApplyEqualizerToMixer()
		{
			if (moduleHandler == null) return;

			MixerConfiguration configuration = new()
			{
				StereoSeparator = soundSettings.StereoSeparation,
				VisualsLatency = soundSettings.VisualsLatency * 20,
				EnableInterpolation = soundSettings.Interpolation,
				SwapSpeakers = soundSettings.SwapSpeakers,
				EnableAmigaFilter = soundSettings.AmigaFilter,
				EnableEqualizer = equalizerControl.IsEqualizerEnabled,
				EqualizerBands = equalizerControl.GetBandValues(),
				EqualizerPreAmp = soundSettings.EqualizerPreAmp
			};

			// Copy current channel configuration
			if (moduleHandler.IsModuleLoaded)
				Array.Copy(moduleHandler.GetEnabledChannels(), configuration.ChannelsEnabled,
					configuration.ChannelsEnabled.Length);

			moduleHandler.ChangeMixerSettings(configuration);
		}

		/********************************************************************/
		/// <summary>
		///     Form is being shown
		/// </summary>
		/********************************************************************/
		private void EqualizerWindowForm_Shown(object sender, EventArgs e) =>
			// Refresh settings when window is shown
			LoadEqualizerSettings();

		/********************************************************************/
		/// <summary>
		///     Form is being closed - save equalizer settings
		/// </summary>
		/********************************************************************/
		private void EqualizerWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			// Save equalizer settings to persistent storage
			soundSettings.EnableEqualizer = equalizerControl.IsEqualizerEnabled;
			soundSettings.EqualizerBands = equalizerControl.GetBandValues();
			soundSettings.EqualizerPreAmp = equalizerControl.PreAmpGain;
		}

		#endregion
	}
}