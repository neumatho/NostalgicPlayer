/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.EqualizerWindow
{
	/// <summary>
	/// Equalizer window form
	/// </summary>
	public partial class EqualizerWindowForm : WindowFormBase
	{
		private const string WindowSettingsName = "EqualizerWindow";

		private readonly IMainWindowApi mainWindowApi;
		private readonly ModuleHandler moduleHandler;
		private readonly SoundSettings soundSettings;
		private EqualizerControl equalizerControl;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EqualizerWindowForm(ModuleHandler moduleHandler, IMainWindowApi mainWindow, OptionSettings optSettings, SoundSettings soundSettings)
		{
			InitializeComponent();

			this.moduleHandler = moduleHandler;
			mainWindowApi = mainWindow;
			this.soundSettings = soundSettings;

			// Initialize the window
			InitializeWindow(mainWindow, optSettings);
			LoadWindowSettings(WindowSettingsName);

			// Create and configure equalizer control
			equalizerControl = new EqualizerControl
			{
				Dock = DockStyle.Fill
			};
			equalizerPanel.Controls.Add(equalizerControl);

			// Load settings into control
			LoadEqualizerSettings();

			// Hook up event handler
			equalizerControl.EqualizerChanged += EqualizerControl_EqualizerChanged;
		}

		/********************************************************************/
		/// <summary>
		/// Load equalizer settings from SoundSettings
		/// </summary>
		/********************************************************************/
		private void LoadEqualizerSettings()
		{
			equalizerControl.IsEqualizerEnabled = soundSettings.EnableEqualizer;
			equalizerControl.SetBandValues(soundSettings.EqualizerBands);
		}

		/********************************************************************/
		/// <summary>
		/// Equalizer control values changed - save to settings and apply
		/// </summary>
		/********************************************************************/
		private void EqualizerControl_EqualizerChanged(object sender, EventArgs e)
		{
			// Save to settings
			soundSettings.EnableEqualizer = equalizerControl.IsEqualizerEnabled;
			soundSettings.EqualizerBands = equalizerControl.GetBandValues();

			// Apply to mixer immediately
			ApplyEqualizerToMixer();
		}

		/********************************************************************/
		/// <summary>
		/// Apply equalizer settings to the mixer
		/// </summary>
		/********************************************************************/
		private void ApplyEqualizerToMixer()
		{
			if (moduleHandler == null)
				return;

			MixerConfiguration configuration = new MixerConfiguration
			{
				StereoSeparator = soundSettings.StereoSeparation,
				VisualsLatency = soundSettings.VisualsLatency * 20,
				EnableInterpolation = soundSettings.Interpolation,
				SwapSpeakers = soundSettings.SwapSpeakers,
				EnableAmigaFilter = soundSettings.AmigaFilter,
				EnableEqualizer = soundSettings.EnableEqualizer,
				EqualizerBands = soundSettings.EqualizerBands
			};

			// Copy current channel configuration
			if (moduleHandler.IsModuleLoaded)
			{
				Array.Copy(moduleHandler.GetEnabledChannels(), configuration.ChannelsEnabled, configuration.ChannelsEnabled.Length);
			}

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
		/// Form is being closed
		/// </summary>
		/********************************************************************/
		private void EqualizerWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			// Save window settings
			UpdateWindowSettings();
		}
	}
}
