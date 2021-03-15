/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Output.DiskSaver.Settings
{
	/// <summary>
	/// The disk saver settings
	/// </summary>
	internal partial class SettingsControl : UserControl, ISettingsControl
	{
		private static readonly int[] frequencyTable = { 8268, 11025, 22050, 33075, 44100, 48000 };

		private DiskSaverSettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsControl(AgentInfo[] outputAgents, AgentInfo[] sampleConverterAgents)
		{
			InitializeComponent();

			// Add "None" to pass through
			passThroughComboBox.Items.Add(new KryptonListItem(Resources.IDS_SETTINGS_PASSTHROUGH_NONE));

			// Add the output agents to the pass through list
			foreach (AgentInfo agentInfo in outputAgents)
			{
				KryptonListItem listItem = new KryptonListItem(agentInfo.TypeName);
				listItem.Tag = agentInfo;

				passThroughComboBox.Items.Add(listItem);
			}

			// Add the sample converters to the format list
			foreach (AgentInfo agentInfo in sampleConverterAgents)
			{
				KryptonListItem listItem = new KryptonListItem(agentInfo.TypeName);
				listItem.Tag = agentInfo;

				formatComboBox.Items.Add(listItem);
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
			settings = new DiskSaverSettings();

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
			pathTextBox.Text = settings.DiskPath;

			switch (settings.OutputSize)
			{
				case 8:
				{
					eigthBitRadioButton.Checked = true;
					break;
				}

				case 16:
				{
					sixthteenBitRadioButton.Checked = true;
					break;
				}

				case 32:
				{
					thirtytwoRadioButton.Checked = true;
					break;
				}
			}

			switch (settings.OutputType)
			{
				case DiskSaverSettings.OutType.Mono:
				{
					monoRadioButton.Checked = true;
					break;
				}

				case DiskSaverSettings.OutType.Stereo:
				{
					stereoRadioButton.Checked = true;
					break;
				}
			}

			int freq = settings.OutputFrequency;

			for (int i = 0, cnt = frequencyTable.Length; i < cnt; i++)
			{
				if (frequencyTable[i] > freq)
				{
					frequencyTrackBar.Value = i != 0 ? i - 1 : 0;
					break;
				}
			}

			for (int i = 0, cnt = formatComboBox.Items.Count; i < cnt; i++)
			{
				KryptonListItem listItem = (KryptonListItem)formatComboBox.Items[i];
				if ((listItem.Tag is AgentInfo agentInfo) && (agentInfo.TypeId == settings.OutputFormat))
				{
					formatComboBox.SelectedIndex = i;
					break;
				}
			}

			for (int i = 0, cnt = passThroughComboBox.Items.Count; i < cnt; i++)
			{
				KryptonListItem listItem = (KryptonListItem)passThroughComboBox.Items[i];
				if ((listItem.Tag is AgentInfo agentInfo) && (agentInfo.TypeId == settings.OutputAgent))
				{
					passThroughComboBox.SelectedIndex = i;
					break;
				}
			}

			if (passThroughComboBox.SelectedIndex == -1)
				passThroughComboBox.SelectedIndex = 0;		// Select "None"
		}



		/********************************************************************/
		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		/********************************************************************/
		public void WriteSettings()
		{
			settings.DiskPath = pathTextBox.Text;

			settings.OutputSize = eigthBitRadioButton.Checked ? 8 : sixthteenBitRadioButton.Checked ? 16 : thirtytwoRadioButton.Checked ? 32 : 16;
			settings.OutputType = monoRadioButton.Checked ? DiskSaverSettings.OutType.Mono : DiskSaverSettings.OutType.Stereo;
			settings.OutputFrequency = frequencyTable[frequencyTrackBar.Value];

			if (formatComboBox.SelectedIndex != -1)
				settings.OutputFormat = ((AgentInfo)((KryptonListItem)formatComboBox.SelectedItem).Tag).TypeId;

			if (passThroughComboBox.SelectedIndex != -1)
				settings.OutputAgent = passThroughComboBox.SelectedIndex == 0 ? Guid.Empty : ((AgentInfo)((KryptonListItem)passThroughComboBox.SelectedItem).Tag).TypeId;

			// Save the settings to disk
			settings.Settings.SaveSettings();
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

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the user clicked the path button
		/// </summary>
		/********************************************************************/
		private void PathButton_Click(object sender, EventArgs e)
		{
			string newDirectory = SelectDirectory(pathTextBox.Text);
			if (!string.IsNullOrEmpty(newDirectory))
				pathTextBox.Text = newDirectory;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the output format
		/// </summary>
		/********************************************************************/
		private void FormatComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (((KryptonListItem)formatComboBox.SelectedItem).Tag is AgentInfo agentInfo)
			{
				ISampleSaverAgent saverAgent = (ISampleSaverAgent)agentInfo.Agent.CreateInstance(agentInfo.TypeId);
				SampleSaverSupportFlag supportFlag = saverAgent.SaverSupportFlags;

				eigthBitRadioButton.Enabled = (supportFlag & SampleSaverSupportFlag.Support8Bit) != 0;
				sixthteenBitRadioButton.Enabled = (supportFlag & SampleSaverSupportFlag.Support16Bit) != 0;
				thirtytwoRadioButton.Enabled = (supportFlag & SampleSaverSupportFlag.Support32Bit) != 0;

				monoRadioButton.Enabled = (supportFlag & SampleSaverSupportFlag.SupportMono) != 0;
				stereoRadioButton.Enabled = (supportFlag & SampleSaverSupportFlag.SupportStereo) != 0;

				if (!thirtytwoRadioButton.Enabled && thirtytwoRadioButton.Checked)
				{
					if (sixthteenBitRadioButton.Enabled)
						sixthteenBitRadioButton.Checked = true;
					else
						eigthBitRadioButton.Checked = true;
				}

				if (!sixthteenBitRadioButton.Enabled && sixthteenBitRadioButton.Checked)
				{
					if (thirtytwoRadioButton.Enabled)
						thirtytwoRadioButton.Checked = true;
					else
						eigthBitRadioButton.Checked = true;
				}

				if (!eigthBitRadioButton.Enabled && eigthBitRadioButton.Checked)
				{
					if (sixthteenBitRadioButton.Enabled)
						sixthteenBitRadioButton.Checked = true;
					else
						thirtytwoRadioButton.Checked = true;
				}

				if (!stereoRadioButton.Enabled && stereoRadioButton.Checked)
					monoRadioButton.Checked = true;

				if (!monoRadioButton.Enabled && monoRadioButton.Checked)
					stereoRadioButton.Checked = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the pass-through agent
		/// </summary>
		/********************************************************************/
		private void PassThroughComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (((KryptonListItem)passThroughComboBox.SelectedItem).Tag != null)
			{
				bitPanel.Enabled = false;
				channelPanel.Enabled = false;
				frequencyPanel.Enabled = false;
			}
			else
			{
				bitPanel.Enabled = true;
				channelPanel.Enabled = true;
				frequencyPanel.Enabled = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user change the frequency
		/// </summary>
		/********************************************************************/
		private void FrequencyTrackBar_ValueChanged(object sender, EventArgs e)
		{
			frequencyValueLabel.Text = frequencyTable[frequencyTrackBar.Value].ToString();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Show a directory chooser and return the path. Null if no path
		/// has been selected
		/// </summary>
		/********************************************************************/
		private string SelectDirectory(string startDirectory)
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				dialog.SelectedPath = startDirectory;

				if (dialog.ShowDialog() == DialogResult.OK)
					return dialog.SelectedPath;
			}

			return null;
		}
		#endregion
	}
}
