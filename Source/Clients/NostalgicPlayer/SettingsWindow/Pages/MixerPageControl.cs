/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Helpers;
using Polycode.NostalgicPlayer.Library.Agent;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.SettingsWindow.Pages
{
	/// <summary>
	/// Holds all the controls for the Mixer tab
	/// </summary>
	public partial class MixerPageControl : UserControl, ISettingsPage
	{
		private const int MaxNumberOfChannels = 64;

		private Manager manager;
		private ModuleHandler moduleHandler;
		private IMainWindowApi mainWindowApi;

		private SoundSettings soundSettings;

		private int originalStereoSeparation;
		private int originalVisualsLatency;
		private bool originalInterpolation;
		private bool originalSwapSpeakers;
		private bool originalAmigaFilter;

		private int channelsUsed;

		private readonly KryptonCheckBox[] channelCheckBoxes;
		private bool doNotTrigChannelChanged = false;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MixerPageControl()
		{
			InitializeComponent();

			// Add items to the combo controls
			surroundModeComboBox.Items.AddRange(
			[
				Resources.IDS_SETTINGS_MIXER_GENERAL_SURROUND_NONE,
				Resources.IDS_SETTINGS_MIXER_GENERAL_SURROUND_PROLOGIC,
				Resources.IDS_SETTINGS_MIXER_GENERAL_SURROUND_REAL
			]);

			// Create channel checkboxes
			channelCheckBoxes = new KryptonCheckBox[MaxNumberOfChannels];

			int y = 6;
			for (int i = 0; i < 4; i++)
			{
				int x = 106;

				for (int j = 0; j < 16; j++)
				{
					int channelNum = i * 16 + j;

					KryptonCheckBox checkBox = new KryptonCheckBox();
					checkBox.Name = "channel" + channelNum;
					checkBox.Location = new Point(x, y);
					checkBox.Size = new Size(19, 13);
					checkBox.Text = string.Empty;
					checkBox.Tag = channelNum;
					checkBox.TabIndex = 4 + channelNum;
					checkBox.CheckedChanged += ChannelCheckBox_CheckedChanged;

					channelCheckBoxes[channelNum] = checkBox;

					x += 23;
				}

				y += 29;
			}

			channelsGroupBox.Panel.Controls.AddRange(channelCheckBoxes);
		}

		#region ISettingsPage implementation
		/********************************************************************/
		/// <summary>
		/// Will prepare to handle the settings
		/// </summary>
		/********************************************************************/
		public void InitSettings(Manager agentManager, ModuleHandler modHandler, IMainWindowApi mainWindow, ISettings userSettings, ISettings windowSettings)
		{
			manager = agentManager;
			moduleHandler = modHandler;
			mainWindowApi = mainWindow;

			soundSettings = new SoundSettings(userSettings);
		}



		/********************************************************************/
		/// <summary>
		/// Will make a backup of settings that can be changed in real-time
		/// </summary>
		/********************************************************************/
		public void MakeBackup()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will read the settings and set all the controls
		/// </summary>
		/********************************************************************/
		public void ReadSettings()
		{
			// Fill the combobox with available agents
			outputAgentComboBox.Items.Clear();

			foreach (AgentInfo agentInfo in manager.GetAllAgents(Manager.AgentType.Output))
			{
				KryptonListItem listItem = new KryptonListItem(agentInfo.TypeName);
				listItem.Tag = agentInfo;

				outputAgentComboBox.Items.Add(listItem);
			}

			// Find the selected output agent
			Guid outputAgent = soundSettings.OutputAgent;

			for (int i = outputAgentComboBox.Items.Count - 1; i >= 0; i--)
			{
				if (((AgentInfo)((KryptonListItem)outputAgentComboBox.Items[i]).Tag).TypeId == outputAgent)
				{
					outputAgentComboBox.SelectedIndex = i;
					break;
				}
			}

			// Setup the rest of the settings
			originalStereoSeparation = stereoSeparationTrackBar.Value = soundSettings.StereoSeparation;
			originalVisualsLatency = visualsLatencyTrackBar.Value = soundSettings.VisualsLatency;
			originalInterpolation = interpolationCheckBox.Checked = soundSettings.Interpolation;
			originalSwapSpeakers = swapSpeakersCheckBox.Checked = soundSettings.SwapSpeakers;
			originalAmigaFilter = amigaFilterCheckBox.Checked = soundSettings.AmigaFilter;

			surroundModeComboBox.SelectedIndex = (int)soundSettings.SurroundMode;
			disableCenterSpeakerCheckBox.Checked = soundSettings.DisableCenterSpeaker;

			// Setup channels
			doNotTrigChannelChanged = true;

			try
			{
				bool[] enabled = moduleHandler.GetEnabledChannels();
				for (int i = Math.Min(enabled.Length, MaxNumberOfChannels) - 1; i >= 0; i--)
					channelCheckBoxes[i].Checked = enabled[i];
			}
			finally
			{
				doNotTrigChannelChanged = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will read the window settings
		/// </summary>
		/********************************************************************/
		public void ReadWindowSettings()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		/********************************************************************/
		public void WriteSettings()
		{
			originalStereoSeparation = soundSettings.StereoSeparation = stereoSeparationTrackBar.Value;
			originalVisualsLatency = soundSettings.VisualsLatency = visualsLatencyTrackBar.Value;
			originalInterpolation = soundSettings.Interpolation = interpolationCheckBox.Checked;
			originalSwapSpeakers = soundSettings.SwapSpeakers = swapSpeakersCheckBox.Checked;
			originalAmigaFilter = soundSettings.AmigaFilter = amigaFilterCheckBox.Checked;

			soundSettings.SurroundMode = (SurroundMode)surroundModeComboBox.SelectedIndex;
			soundSettings.DisableCenterSpeaker = disableCenterSpeakerCheckBox.Checked;

			soundSettings.OutputAgent = ((AgentInfo)((KryptonListItem)outputAgentComboBox.SelectedItem).Tag).TypeId;
		}



		/********************************************************************/
		/// <summary>
		/// Will store window specific settings
		/// </summary>
		/********************************************************************/
		public void WriteWindowSettings()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will restore real-time values
		/// </summary>
		/********************************************************************/
		public void CancelSettings()
		{
			soundSettings.StereoSeparation = originalStereoSeparation;
			soundSettings.VisualsLatency = originalVisualsLatency;
			soundSettings.Interpolation = originalInterpolation;
			soundSettings.SwapSpeakers = originalSwapSpeakers;
			soundSettings.AmigaFilter = originalAmigaFilter;

			SetMixerSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the page when a module is loaded/ejected
		/// </summary>
		/********************************************************************/
		public void RefreshWindow()
		{
			SetChannels();
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the user change the stereo separation
		/// </summary>
		/********************************************************************/
		private void StereoSeparationTrackBar_ValueChanged(object sender, EventArgs e)
		{
			stereoSeparationPercentLabel.Text = stereoSeparationTrackBar.Value + "%";
			soundSettings.StereoSeparation = stereoSeparationTrackBar.Value;

			SetMixerSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the visuals latency
		/// </summary>
		/********************************************************************/
		private void VisualsLatencyTrackBar_ValueChanged(object sender, EventArgs e)
		{
			soundSettings.VisualsLatency = visualsLatencyTrackBar.Value;
			visualsLatencyMsLabel.Text = $"{soundSettings.VisualsLatency * 20} ms";

			SetMixerSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the interpolation
		/// </summary>
		/********************************************************************/
		private void InterpolationCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			soundSettings.Interpolation = interpolationCheckBox.Checked;

			SetMixerSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user swap speakers
		/// </summary>
		/********************************************************************/
		private void SwapSpeakersCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			soundSettings.SwapSpeakers = swapSpeakersCheckBox.Checked;

			SetMixerSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the Amiga filter
		/// </summary>
		/********************************************************************/
		private void AmigaFilterCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			soundSettings.AmigaFilter = amigaFilterCheckBox.Checked;

			SetMixerSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the output agent
		/// </summary>
		/********************************************************************/
		private void OutputAgentComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			AgentInfo agentInfo = (AgentInfo)((KryptonListItem)outputAgentComboBox.SelectedItem).Tag;
			outputAgentSettingsButton.Enabled = agentInfo.HasSettings && agentInfo.Enabled;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user click the settings button
		/// </summary>
		/********************************************************************/
		private void OutputAgentSettingsButton_Click(object sender, EventArgs e)
		{
			AgentInfo agentInfo = (AgentInfo)((KryptonListItem)outputAgentComboBox.SelectedItem).Tag;

			if (!agentInfo.Enabled)
				mainWindowApi.ShowSimpleErrorMessage(Resources.IDS_ERR_AGENT_DISABLED);
			else
				mainWindowApi.OpenAgentSettingsWindow(agentInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when one of the channel checkboxes is clicked by the
		/// user
		/// </summary>
		/********************************************************************/
		private void ChannelCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!doNotTrigChannelChanged)
			{
				KryptonCheckBox checkBox = (KryptonCheckBox)sender;
				moduleHandler.EnableChannels(checkBox.Checked, (int)checkBox.Tag);

				SetMixerSettings();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked on the channel 0-15 button
		/// </summary>
		/********************************************************************/
		private void Channels0_15Button_Click(object sender, EventArgs e)
		{
			EnableChannels(0);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked on the channel 16-31 button
		/// </summary>
		/********************************************************************/
		private void Channels16_31Button_Click(object sender, EventArgs e)
		{
			EnableChannels(16);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked on the channel 32-47 button
		/// </summary>
		/********************************************************************/
		private void Channels32_47Button_Click(object sender, EventArgs e)
		{
			EnableChannels(32);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked on the channel 48-63 button
		/// </summary>
		/********************************************************************/
		private void Channels48_63Button_Click(object sender, EventArgs e)
		{
			EnableChannels(48);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Give real-time settings to the mixer
		/// </summary>
		/********************************************************************/
		private void SetMixerSettings()
		{
			MixerConfiguration configuration = new MixerConfiguration
			{
				StereoSeparator = soundSettings.StereoSeparation,
				VisualsLatency = soundSettings.VisualsLatency * 20,
				EnableInterpolation = soundSettings.Interpolation,
				SwapSpeakers = soundSettings.SwapSpeakers,
				EnableAmigaFilter = soundSettings.AmigaFilter,
				EnableEqualizer = soundSettings.EnableEqualizer,
				EqualizerBands = soundSettings.EqualizerBands,
				EqualizerPreAmp = soundSettings.EqualizerPreAmp
			};

			Array.Copy(moduleHandler.GetEnabledChannels(), configuration.ChannelsEnabled, configuration.ChannelsEnabled.Length);

			moduleHandler.ChangeMixerSettings(configuration);
		}



		/********************************************************************/
		/// <summary>
		/// Enable and disable the channel checkboxes
		/// </summary>
		/********************************************************************/
		private void SetChannels()
		{
			// Get the number of channels in use right now
			channelsUsed = moduleHandler.IsModuleLoaded ? moduleHandler.StaticModuleInformation.Channels : 0;

			// First enable the used channels
			for (int i = 0; i < channelsUsed; i++)
				channelCheckBoxes[i].Enabled = true;

			// Now disable the rest
			for (int i = channelsUsed; i < MaxNumberOfChannels; i++)
				channelCheckBoxes[i].Enabled = false;

			// Enable and disable the channel buttons
			channels0_15Button.Enabled = channelsUsed > 0;
			channels16_31Button.Enabled = channelsUsed > 16;
			channels32_47Button.Enabled = channelsUsed > 32;
			channels48_63Button.Enabled = channelsUsed > 48;
		}



		/********************************************************************/
		/// <summary>
		/// Enable or disable a bunch of channels
		/// </summary>
		/********************************************************************/
		private void EnableChannels(int startChannel)
		{
			doNotTrigChannelChanged = true;

			try
			{
				bool[] channelsEnabled = moduleHandler.GetEnabledChannels();
				bool enable = false;

				// Find the maximum channels to check
				int stopChannel = Math.Min(channelsUsed, startChannel + 16) - 1;

				// Find out if we need to enable or disable all the channels
				for (int i = startChannel; i <= stopChannel; i++)
				{
					if (!channelsEnabled[i])
					{
						enable = true;
						break;
					}
				}

				// Enable or disable the channels
				for (int i = startChannel; i <= stopChannel; i++)
					channelCheckBoxes[i].Checked = enable;

				// Change the channels
				moduleHandler.EnableChannels(enable, startChannel, stopChannel);

				SetMixerSettings();
			}
			finally
			{
				doNotTrigChannelChanged = false;
			}
		}
		#endregion
	}
}
