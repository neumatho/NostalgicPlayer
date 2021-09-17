/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
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
			switch (settings.MemoryModel)
			{
				case SidPlay.SidPlaySettings.Environment.PlaySid:
				{
					playSidRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.Environment.Transparent:
				{
					transparentRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.Environment.FullBank:
				{
					fullBankRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.Environment.Real:
				{
					realC64RadioButton.Checked = true;
					break;
				}
			}

			switch (settings.ClockSpeed)
			{
				case SidPlay.SidPlaySettings.Clock.Pal:
				{
					palRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.Clock.Ntsc:
				{
					ntscRadioButton.Checked = true;
					break;
				}
			}

			switch (settings.ClockSpeedOption)
			{
				case SidPlay.SidPlaySettings.ClockOption.NotKnown:
				{
					clockNotKnownRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.ClockOption.Always:
				{
					clockAlwaysRadioButton.Checked = true;
					break;
				}
			}

			switch (settings.SidModel)
			{
				case SidPlay.SidPlaySettings.Model.Mos6581:
				{
					model6581RadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.Model.Mos8580:
				{
					model8580RadioButton.Checked = true;
					break;
				}
			}

			switch (settings.SidModelOption)
			{
				case SidPlay.SidPlaySettings.ModelOption.NotKnown:
				{
					modelNotKnownRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.ModelOption.Always:
				{
					modelAlwaysRadioButton.Checked = true;
					break;
				}
			}

			enableFilterCheckBox.Checked = settings.FilterEnabled;

			switch (settings.Filter)
			{
				case SidPlay.SidPlaySettings.FilterOption.ModelSpecific:
				{
					filterModelRadioButton.Checked = true;
					break;
				}

				case SidPlay.SidPlaySettings.FilterOption.Custom:
				{
					filterCustomRadioButton.Checked = true;
					break;
				}
			}

			SetFilterValues(settings.FilterFs, settings.FilterFm, settings.FilterFt);

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
			if (playSidRadioButton.Checked)
				settings.MemoryModel = SidPlay.SidPlaySettings.Environment.PlaySid;
			else if (transparentRadioButton.Checked)
				settings.MemoryModel = SidPlay.SidPlaySettings.Environment.Transparent;
			else if (fullBankRadioButton.Checked)
				settings.MemoryModel = SidPlay.SidPlaySettings.Environment.FullBank;
			else if (realC64RadioButton.Checked)
				settings.MemoryModel = SidPlay.SidPlaySettings.Environment.Real;

			if (palRadioButton.Checked)
				settings.ClockSpeed = SidPlay.SidPlaySettings.Clock.Pal;
			else if (ntscRadioButton.Checked)
				settings.ClockSpeed = SidPlay.SidPlaySettings.Clock.Ntsc;

			if (clockNotKnownRadioButton.Checked)
				settings.ClockSpeedOption = SidPlay.SidPlaySettings.ClockOption.NotKnown;
			else if (clockAlwaysRadioButton.Checked)
				settings.ClockSpeedOption = SidPlay.SidPlaySettings.ClockOption.Always;

			if (model6581RadioButton.Checked)
				settings.SidModel = SidPlay.SidPlaySettings.Model.Mos6581;
			else if (model8580RadioButton.Checked)
				settings.SidModel = SidPlay.SidPlaySettings.Model.Mos8580;

			if (modelNotKnownRadioButton.Checked)
				settings.SidModelOption = SidPlay.SidPlaySettings.ModelOption.NotKnown;
			else if (modelAlwaysRadioButton.Checked)
				settings.SidModelOption = SidPlay.SidPlaySettings.ModelOption.Always;

			settings.FilterEnabled = enableFilterCheckBox.Checked;

			if (filterModelRadioButton.Checked)
				settings.Filter = SidPlay.SidPlaySettings.FilterOption.ModelSpecific;
			else if (filterCustomRadioButton.Checked)
				settings.Filter = SidPlay.SidPlaySettings.FilterOption.Custom;

			GetFilterValues(out float fs, out float fm, out float ft);
			settings.FilterFs = fs;
			settings.FilterFm = fm;
			settings.FilterFt = ft;

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
		/// Is called when the filter emulation is enabled/disabled
		/// </summary>
		/********************************************************************/
		private void EnableFilterCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			filterPanel.Enabled = enableFilterCheckBox.Checked;
			EnableFilterAdjustmentPanel();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the model radio button is changed
		/// </summary>
		/********************************************************************/
		private void FilterModelRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			EnableFilterAdjustmentPanel();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the custom radio button is changed
		/// </summary>
		/********************************************************************/
		private void FilterCustomRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			EnableFilterAdjustmentPanel();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the FS filter parameter is changed
		/// </summary>
		/********************************************************************/
		private void FilterFsTrackBar_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateCurve();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the FM filter parameter is changed
		/// </summary>
		/********************************************************************/
		private void FilterFmTrackBar_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateCurve();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the FT filter parameter is changed
		/// </summary>
		/********************************************************************/
		private void FilterFtTrackBar_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateCurve();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user want to reset the filter
		/// </summary>
		/********************************************************************/
		private void FilterResetButton_Click(object sender, System.EventArgs e)
		{
			SetFilterValues(SidPlay.SidPlaySettings.DefaultFs, SidPlay.SidPlaySettings.DefaultFm, SidPlay.SidPlaySettings.DefaultFt);
		}



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
		/// Enable/disable the adjustment panel
		/// </summary>
		/********************************************************************/
		private void EnableFilterAdjustmentPanel()
		{
			filterAdjustmentPanel.Enabled = enableFilterCheckBox.Checked && filterCustomRadioButton.Checked;
		}



		/********************************************************************/
		/// <summary>
		/// Set the given parameters in the controls
		/// </summary>
		/********************************************************************/
		private void SetFilterValues(float fs, float fm, float ft)
		{
			filterFsTrackBar.Value = (int)(filterFsTrackBar.Maximum - fs + filterFsTrackBar.Minimum);
			filterFmTrackBar.Value = (int)fm;
			filterFtTrackBar.Value = (int)(filterFtTrackBar.Maximum - (ft * 100) + filterFtTrackBar.Minimum);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the filter parameters
		/// </summary>
		/********************************************************************/
		private void GetFilterValues(out float fs, out float fm, out float ft)
		{
			fs = filterFsTrackBar.Maximum - filterFsTrackBar.Value + filterFsTrackBar.Minimum;
			fm = filterFmTrackBar.Value;
			ft = (filterFtTrackBar.Maximum - filterFtTrackBar.Value + filterFtTrackBar.Minimum) / 100.0f;
		}



		/********************************************************************/
		/// <summary>
		/// Redraw the curve
		/// </summary>
		/********************************************************************/
		private void UpdateCurve()
		{
			GetFilterValues(out float fs, out float fm, out float ft);
			filterControl.Update(fs, fm, ft);
		}



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
