/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Manage the current settings
	/// </summary>
	public class SettingsService : ISettingsService, IDisposable
	{
		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsService(ISettingsFactory settingsFactory)
		{
			settings = settingsFactory.CreateSettings();

			settings.LoadSettings("Settings");
			FixSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Save the settings
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			SaveSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Save the settings
		/// </summary>
		/********************************************************************/
		public void SaveSettings()
		{
			settings.SaveSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Return the settings implementation
		/// </summary>
		/********************************************************************/
		public ISettings Settings => settings;

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will fix the settings if needed
		/// </summary>
		/********************************************************************/
		private void FixSettings()
		{
			int version = settings.GetIntEntry("General", "Version", 1);

			if (version == 1)
			{
				ConvertSettingsToVersion2();
				version++;
			}

			if (version == 2)
			{
				ConvertSettingsToVersion3();
				version++;
			}

			if (version == 3)
			{
				ConvertSettingsToVersion4();
				version++;
			}

			settings.SetIntEntry("General", "Version", version);
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the settings from version 1 to version 2
		/// </summary>
		/********************************************************************/
		private void ConvertSettingsToVersion2()
		{
			// Move some of the options to the modules section
			bool boolValue = settings.GetBoolEntry("Options", "DoubleBuffering", false);
			settings.SetBoolEntry("Modules", "DoubleBuffering", boolValue);

			int intValue = settings.GetIntEntry("Options", "DoubleBufferingEarlyLoad", 2);
			settings.SetIntEntry("Modules", "DoubleBufferingEarlyLoad", intValue);

			ModuleSettings.ModuleErrorAction enum1Value = settings.GetEnumEntry("Options", "ModuleError", ModuleSettings.ModuleErrorAction.ShowError);
			settings.SetEnumEntry("Modules", "ModuleError", enum1Value);

			boolValue = settings.GetBoolEntry("Options", "NeverEnding", false);
			settings.SetBoolEntry("Modules", "NeverEnding", boolValue);

			intValue = settings.GetIntEntry("Options", "NeverEndingTimeout", 180);
			settings.SetIntEntry("Modules", "NeverEndingTimeout", intValue);

			ModuleSettings.ModuleListEndAction enum2Value = settings.GetEnumEntry("Options", "ModuleListEnd", ModuleSettings.ModuleListEndAction.JumpToStart);
			settings.SetEnumEntry("Modules", "ModuleListEnd", enum2Value);

			settings.RemoveEntry("Options", "DoubleBuffering");
			settings.RemoveEntry("Options", "DoubleBufferingEarlyLoad");
			settings.RemoveEntry("Options", "ModuleError");
			settings.RemoveEntry("Options", "NeverEnding");
			settings.RemoveEntry("Options", "NeverEndingTimeout");
			settings.RemoveEntry("Options", "ModuleListEnd");
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the settings from version 2 to version 3
		/// </summary>
		/********************************************************************/
		private void ConvertSettingsToVersion3()
		{
			bool boolValue = settings.GetBoolEntry("Sound", "Surround");
			settings.SetEnumEntry("Sound", "SurroundMode", boolValue ? SurroundMode.DolbyProLogic : SurroundMode.None);

			settings.RemoveEntry("Sound", "Surround");
		}



		/********************************************************************/
		/// <summary>
		/// Will convert the settings from version 3 to version 4
		/// </summary>
		/********************************************************************/
		private void ConvertSettingsToVersion4()
		{
			bool boolValue = settings.GetBoolEntry("Sound", "Interpolation");
			settings.SetEnumEntry("Sound", "InterpolationMode", boolValue ? InterpolationMode.Always : InterpolationMode.None);

			settings.RemoveEntry("Sound", "Interpolation");
		}
		#endregion
	}
}
