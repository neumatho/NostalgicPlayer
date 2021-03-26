/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Threading;
using System.Windows.Forms;
using Krypton.Toolkit;
using NAudio.CoreAudioApi;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Output.CoreAudioSettings
{
	/// <summary>
	/// The core audio settings
	/// </summary>
	internal partial class SettingsControl : UserControl, ISettingsControl
	{
		private CoreAudio.CoreAudioSettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsControl()
		{
			InitializeComponent();

			// Add "Default" device
			deviceComboBox.Items.Add(new KryptonListItem(Resources.IDS_SETTINGS_DEVICE_DEFAULT));

			// Find all available devices
			using (MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator())
			{
				foreach (MMDevice device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
				{
					KryptonListItem listItem = new KryptonListItem(device.FriendlyName);
					listItem.Tag = device.ID;

					deviceComboBox.Items.Add(listItem);
				}
			}
		}

		#region ISettingsControl implementation
		/********************************************************************/
		/// <summary>
		/// Return the user control holding the settings
		/// </summary>
		/********************************************************************/
		public UserControl GetUserControl()
		{
			settings = new CoreAudio.CoreAudioSettings();

			return this;
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
			string searchFor = settings.OutputDevice;

			for (int i = 0, cnt = deviceComboBox.Items.Count; i < cnt; i++)
			{
				KryptonListItem listItem = (KryptonListItem)deviceComboBox.Items[i];
				if (searchFor.Equals(listItem.Tag))
				{
					deviceComboBox.SelectedIndex = i;
					break;
				}
			}

			if (deviceComboBox.SelectedIndex == -1)
				deviceComboBox.SelectedIndex = 0;		// Select "Default"
		}



		/********************************************************************/
		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		/********************************************************************/
		public void WriteSettings()
		{
			if (deviceComboBox.SelectedIndex != -1)
				settings.OutputDevice = (string)((KryptonListItem)deviceComboBox.SelectedItem).Tag;

			// Save the settings to disk
			settings.Settings.SaveSettings();

			// Check to see if music is playing and if so, tell the main CoreAudio agent to switch endpoint
			if (EventWaitHandle.TryOpenExisting("NostalgicPlayer_CoreAudio", out EventWaitHandle handle))
				handle.Set();
		}



		/********************************************************************/
		/// <summary>
		/// Will restore real-time values
		/// </summary>
		/********************************************************************/
		public void CancelSettings()
		{
		}
		#endregion
	}
}
