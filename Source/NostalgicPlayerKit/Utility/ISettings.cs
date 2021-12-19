using System;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Implementation of loading of settings
	/// </summary>
	public interface ISettings
	{
		/// <summary>
		/// Will load the settings for the given component into memory
		/// </summary>
		public void LoadSettings(string component);

		/// <summary>
		/// Will save the settings in memory back
		/// </summary>
		void SaveSettings();

		/// <summary>
		/// Will delete the settings from memory and disk
		/// </summary>
		void DeleteSettings();

		/// <summary>
		/// Will try to find the entry in the settings. If it couldn't be
		/// found, the default value is returned
		/// </summary>
		string GetStringEntry(string section, string entry, string defaultValue = "");

		/// <summary>
		/// Will try to read the entry with a specific index in the settings.
		/// If it couldn't be read, the default value is returned
		/// </summary>
		string GetStringEntry(string section, int entryNum, out string entryName, string defaultValue = "");

		/// <summary>
		/// Will try to find the entry in the settings. If it couldn't be
		/// found, the default value is returned
		/// </summary>
		int GetIntEntry(string section, string entry, int defaultValue = 0);

		/// <summary>
		/// Will try to find the entry in the settings. If it couldn't be
		/// found, the default value is returned
		/// </summary>
		long GetLongEntry(string section, string entry, long defaultValue = 0);

		/// <summary>
		/// Will try to find the entry in the settings. If it couldn't be
		/// found, the default value is returned
		/// </summary>
		float GetFloatEntry(string section, string entry, float defaultValue = 0.0f);

		/// <summary>
		/// Will try to find the entry in the settings. If it couldn't be
		/// found, the default value is returned
		/// </summary>
		bool GetBoolEntry(string section, string entry, bool defaultValue = false);

		/// <summary>
		/// Will try to find the entry in the settings. If it couldn't be
		/// found, the default value is returned
		/// </summary>
		T GetEnumEntry<T>(string section, string entry, T defaultValue) where T : struct, Enum;

		/// <summary>
		/// Will store the entry in the settings. If it already exists, it
		/// will be overwritten
		/// </summary>
		void SetStringEntry(string section, string entry, string value);

		/// <summary>
		/// Will store the entry in the settings. If it already exists, it
		/// will be overwritten
		/// </summary>
		void SetIntEntry(string section, string entry, int value);

		/// <summary>
		/// Will store the entry in the settings. If it already exists, it
		/// will be overwritten
		/// </summary>
		void SetLongEntry(string section, string entry, long value);

		/// <summary>
		/// Will store the entry in the settings. If it already exists, it
		/// will be overwritten
		/// </summary>
		void SetFloatEntry(string section, string entry, float value);

		/// <summary>
		/// Will store the entry in the settings. If it already exists, it
		/// will be overwritten
		/// </summary>
		void SetBoolEntry(string section, string entry, bool value);

		/// <summary>
		/// Will store the entry in the settings. If it already exists, it
		/// will be overwritten
		/// </summary>
		void SetEnumEntry<T>(string section, string entry, T value) where T : struct, Enum;

		/// <summary>
		/// Will check to see if the entry exists
		/// </summary>
		bool ContainsEntry(string section, string entry);

		/// <summary>
		/// Will remove an entry from the section given. If the entry
		/// couldn't be found, nothing is done
		/// </summary>
		bool RemoveEntry(string section, string entry);

		/// <summary>
		/// Will remove a whole section in the settings. If the section
		/// couldn't be found, nothing is done
		/// </summary>
		void RemoveSection(string section);
	}
}
