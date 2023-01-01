/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlaySettings
{
	/// <summary>
	/// The SidPlay settings
	/// </summary>
	internal partial class SettingsControl : UserControl, ISettingsControl
	{
		private SidPlay.SidPlaySettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsControl()
		{
			InitializeComponent();

			// Add items in mixer combo control
			mixerComboBox.Items.AddRange(new object[]
			{
				Resources.IDS_SETTINGS_OPTIONS_MIXER_INTERPOLATE,
				Resources.IDS_SETTINGS_OPTIONS_MIXER_RESAMPLE
			});
		}

		#region ISettingsControl implementation
		/********************************************************************/
		/// <summary>
		/// Return the user control holding the settings
		/// </summary>
		/********************************************************************/
		public UserControl GetUserControl()
		{
			settings = new SidPlay.SidPlaySettings();

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
			switch (settings.CiaModel)
			{
				case SidPlay.SidPlaySettings.CiaModelType.Mos6526:
				{
					model6526RadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.CiaModelType.Mos8521:
				{
					model8521RadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.CiaModelType.Mos6526_W4485:
				{
					model6526w4485RadioButton.Checked = true;
					break;
				}
			}

			switch (settings.ClockSpeed)
			{
				case SidPlay.SidPlaySettings.ClockType.Pal:
				{
					palRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.ClockType.Ntsc:
				{
					ntscRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.ClockType.NtscOld:
				{
					ntscOldRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.ClockType.Drean:
				{
					dreanRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.ClockType.PalM:
				{
					palmRadioButton.Checked = true;
					break;
				}
			}

			switch (settings.ClockSpeedOption)
			{
				case SidPlay.SidPlaySettings.ClockOptionType.NotKnown:
				{
					clockNotKnownRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.ClockOptionType.Always:
				{
					clockAlwaysRadioButton.Checked = true;
					break;
				}
			}

			switch (settings.SidModel)
			{
				case SidPlay.SidPlaySettings.SidModelType.Mos6581:
				{
					sid6581RadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.SidModelType.Mos8580:
				{
					sid8580RadioButton.Checked = true;
					break;
				}
			}

			switch (settings.SidModelOption)
			{
				case SidPlay.SidPlaySettings.SidModelOptionType.NotKnown:
				{
					sidNotKnownRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.SidModelOptionType.Always:
				{
					sidAlwaysRadioButton.Checked = true;
					break;
				}
			}

			enableFilterCheckBox.Checked = settings.FilterEnabled;
			digiboostCheckBox.Checked = settings.DigiBoostEnabled;
			mixerComboBox.SelectedIndex = (int)settings.Mixer;

			hvscvPathTextBox.Text = settings.HvscPath;
			stilCheckBox.Checked = settings.StilEnabled;
			bugListCheckBox.Checked = settings.BugListEnabled;
			songLengthCheckBox.Checked = settings.SongLengthEnabled;
		}



		/********************************************************************/
		/// <summary>
		/// Will read the data from the controls and store them in the
		/// settings
		/// </summary>
		/********************************************************************/
		public void WriteSettings()
		{
			if (model6526RadioButton.Checked)
				settings.CiaModel = SidPlay.SidPlaySettings.CiaModelType.Mos6526;
			else if (model8521RadioButton.Checked)
				settings.CiaModel = SidPlay.SidPlaySettings.CiaModelType.Mos8521;
			else if (model6526w4485RadioButton.Checked)
				settings.CiaModel = SidPlay.SidPlaySettings.CiaModelType.Mos6526_W4485;

			if (palRadioButton.Checked)
				settings.ClockSpeed = SidPlay.SidPlaySettings.ClockType.Pal;
			else if (ntscRadioButton.Checked)
				settings.ClockSpeed = SidPlay.SidPlaySettings.ClockType.Ntsc;
			else if (ntscOldRadioButton.Checked)
				settings.ClockSpeed = SidPlay.SidPlaySettings.ClockType.NtscOld;
			else if (dreanRadioButton.Checked)
				settings.ClockSpeed = SidPlay.SidPlaySettings.ClockType.Drean;
			else if (palmRadioButton.Checked)
				settings.ClockSpeed = SidPlay.SidPlaySettings.ClockType.PalM;

			if (clockNotKnownRadioButton.Checked)
				settings.ClockSpeedOption = SidPlay.SidPlaySettings.ClockOptionType.NotKnown;
			else if (clockAlwaysRadioButton.Checked)
				settings.ClockSpeedOption = SidPlay.SidPlaySettings.ClockOptionType.Always;

			if (sid6581RadioButton.Checked)
				settings.SidModel = SidPlay.SidPlaySettings.SidModelType.Mos6581;
			else if (sid8580RadioButton.Checked)
				settings.SidModel = SidPlay.SidPlaySettings.SidModelType.Mos8580;

			if (sidNotKnownRadioButton.Checked)
				settings.SidModelOption = SidPlay.SidPlaySettings.SidModelOptionType.NotKnown;
			else if (sidAlwaysRadioButton.Checked)
				settings.SidModelOption = SidPlay.SidPlaySettings.SidModelOptionType.Always;

			settings.FilterEnabled = enableFilterCheckBox.Checked;
			settings.DigiBoostEnabled = digiboostCheckBox.Checked;
			settings.Mixer = (SidPlay.SidPlaySettings.MixerType)mixerComboBox.SelectedIndex;

			settings.HvscPath = hvscvPathTextBox.Text;
			settings.StilEnabled = stilCheckBox.Checked;
			settings.BugListEnabled = bugListCheckBox.Checked;
			settings.SongLengthEnabled = songLengthCheckBox.Checked;

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
		/// Is called when the user click the directory button
		/// </summary>
		/********************************************************************/
		private void HvscPathButton_Click(object sender, System.EventArgs e)
		{
			string newDirectory = SelectDirectory(hvscvPathTextBox.Text);
			if (!string.IsNullOrEmpty(newDirectory))
				hvscvPathTextBox.Text = newDirectory;
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
