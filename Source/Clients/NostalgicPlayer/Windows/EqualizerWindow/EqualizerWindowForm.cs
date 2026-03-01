/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Factories;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Services;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.EqualizerWindow
{
	/// <summary>
	/// Equalizer window form
	/// </summary>
	public partial class EqualizerWindowForm : WindowFormBase
	{
		private const string WindowSettingsName = "EqualizerWindow";
		private readonly EqualizerControl equalizerControl;

		private IMixerConfigurationFactory mixerConfigurationFactory;
		private IModuleHandlerService moduleHandler;
		private SoundSettings soundSettings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EqualizerWindowForm()
		{
			InitializeComponent();

			// Create and configure equalizer control
			equalizerControl = new EqualizerControl {Dock = DockStyle.Fill};
			equalizerPanel.Controls.Add(equalizerControl);

			// Hook up event handler
			equalizerControl.EqualizerChanged += EqualizerControl_EqualizerChanged;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the form
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeForm(IMixerConfigurationFactory mixerConfigurationFactory, IModuleHandlerService moduleHandlerService, SoundSettings soundSettings)
		{
			// Remember the arguments
			this.mixerConfigurationFactory = mixerConfigurationFactory;
			moduleHandler = moduleHandlerService;
			this.soundSettings = soundSettings;

			// Set window title
			Text = Resources.IDS_SETTINGS_MIXER_EQUALIZER;

			// Initialize the window
			InitializeWindow();
			LoadWindowSettings(WindowSettingsName);

			// Load settings into control
			LoadEqualizerSettings();
		}

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected override string HelpUrl => "equalizer.html";
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load equalizer settings from SoundSettings
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
		/// Equalizer control values changed - apply to mixer in real-time
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
		/// Apply equalizer settings to the mixer (uses current control
		/// values, not saved settings)
		/// </summary>
		/********************************************************************/
		private void ApplyEqualizerToMixer()
		{
			if (moduleHandler == null)
				return;

			MixerConfiguration configuration = mixerConfigurationFactory.Create();
			configuration.EnableEqualizer = equalizerControl.IsEqualizerEnabled;
			configuration.EqualizerBands = equalizerControl.GetBandValues();

			// Copy current channel configuration
			if (moduleHandler.IsModuleLoaded)
				Array.Copy(moduleHandler.GetEnabledChannels(), configuration.ChannelsEnabled, configuration.ChannelsEnabled.Length);

			moduleHandler.ChangeMixerSettings(configuration);
		}



		/********************************************************************/
		/// <summary>
		/// Form is being shown
		/// </summary>
		/********************************************************************/
		private void EqualizerWindowForm_Shown(object sender, EventArgs e)
		{
			// Refresh settings when window is shown
			LoadEqualizerSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Form is being closed - save equalizer settings
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
