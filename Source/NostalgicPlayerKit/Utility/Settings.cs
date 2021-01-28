/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// This class helps to read and write user settings
	/// </summary>
	public class Settings : IDisposable
	{
		private enum LineType
		{
			Comment,
			Section,
			Entry
		}

		private class LineInfo
		{
			public LineType Type;
			public string Line;
		}

		private readonly string component;

		private ReaderWriterLockSlim listLock;
		private List<LineInfo> lineList;

		private bool changed;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Settings(string component)
		{
			// Initialize member variables
			this.component = component;
			changed = false;

			// Create synchronize and list instances
			listLock = new ReaderWriterLockSlim();
			lineList = new List<LineInfo>();
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			listLock.Dispose();
			listLock = null;

			lineList = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will load the setting file into memory
		/// </summary>
		/********************************************************************/
		public void LoadSettings()
		{
			// Find the settings file
			string fullPath = SettingsFileName;

			// Lock the settings
			listLock.EnterWriteLock();

			try
			{
				// The settings is not changed, because we will load new ones
				changed = false;

				// Empty any previous file data
				lineList.Clear();

				// Open the file if exists
				if (File.Exists(fullPath))
				{
					using (StreamReader sr = new StreamReader(fullPath, Encoding.UTF8))
					{
						// Read one line at the time into memory
						while (!sr.EndOfStream)
						{
							LineInfo lineInfo = new LineInfo();

							// Read the line
							lineInfo.Line = sr.ReadLine();

							// Skip empty lines
							if (string.IsNullOrEmpty(lineInfo.Line))
								continue;

							// Find out the type of the line
							switch (lineInfo.Line[0])
							{
								case ';':
								{
									// Comment
									lineInfo.Type = LineType.Comment;
									break;
								}

								case '[':
								{
									// Section
									lineInfo.Type = LineType.Section;

									// Remove section marks
									lineInfo.Line = lineInfo.Line.Substring(1, lineInfo.Line.Length - 2);
									break;
								}

								default:
								{
									// Entry, but check to see if it's valid
									if (lineInfo.Line.IndexOf('=') == -1)
										continue;

									lineInfo.Type = LineType.Entry;
									break;
								}
							}

							// Add the line to the list
							lineList.Add(lineInfo);
						}
					}
				}
			}
			finally
			{
				listLock.ExitWriteLock();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will save the settings in memory back to the file
		/// </summary>
		/********************************************************************/
		public void SaveSettings()
		{
			// Find the settings file
			string fullPath = SettingsFileName;

			// Create the directory if not exists
			string directory = Path.GetDirectoryName(fullPath);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			// Lock the list for reading
			listLock.EnterReadLock();

			try
			{
				// Well, we do only need to save if the settings has been changed
				if (changed)
				{
					// Open the file
					using (StreamWriter sw = new StreamWriter(fullPath, false, Encoding.UTF8))
					{
						bool firstSection = true;

						foreach (LineInfo lineInfo in lineList)
						{
							// If the type is a section, we need to write an empty
							// line before it, except if it is the first section
							if (lineInfo.Type == LineType.Section)
							{
								if (!firstSection)
								{
									// Write an empty line
									sw.WriteLine();
								}
								else
									firstSection = false;

								// Write the section name
								sw.WriteLine($"[{lineInfo.Line}]");
							}
							else
							{
								// Write the line itself
								sw.WriteLine(lineInfo.Line);
							}
						}
					}

					// The settings has been saved, reset the change variable
					changed = false;
				}
			}
			finally
			{
				listLock.ExitReadLock();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find the entry in the settings. If it couldn't be
		/// found, the default value is returned
		/// </summary>
		/********************************************************************/
		public string GetStringEntry(string section, string entry, string defaultValue = "")
		{
			// Start to lock the list
			listLock.EnterReadLock();

			try
			{
				string value = defaultValue;

				// Now find the section
				int index = FindSection(section);
				if (index != -1)
				{
					// Found it, now find the entry
					index = FindEntry(index + 1, entry, out _);
					if (index != -1)
					{
						// Got it, now extract the value
						LineInfo lineInfo = lineList[index];
						value = lineInfo.Line.Substring(lineInfo.Line.IndexOf('=') + 1);
					}
				}

				return value;
			}
			finally
			{
				listLock.ExitReadLock();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to read the entry with a specific index in the settings.
		/// If it couldn't be read, the default value is returned
		/// </summary>
		/********************************************************************/
		public string GetStringEntry(string section, int entryNum, out string entryName, string defaultValue = "")
		{
			// Start to lock the list
			listLock.EnterReadLock();

			try
			{
				string value = defaultValue;
				entryName = string.Empty;

				// Now find the section
				int index = FindSection(section);
				if (index != -1)
				{
					// Found it, now find the entry
					index = FindEntryByNumber(index + 1, entryNum, out int insertPos);
					if (index != -1)
					{
						// Got it, now extract the value
						LineInfo lineInfo = lineList[index];
						int valPos = lineInfo.Line.IndexOf('=');
						entryName = lineInfo.Line.Substring(0, valPos);
						value = lineInfo.Line.Substring(valPos + 1);
					}
				}

				return value;
			}
			finally
			{
				listLock.ExitReadLock();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find the entry in the settings. If it couldn't be
		/// found, the default value is returned
		/// </summary>
		/********************************************************************/
		public int GetIntEntry(string section, string entry, int defaultValue = 0)
		{
			// Use the string read function
			string value = GetStringEntry(section, entry);
			if (string.IsNullOrEmpty(value))
				return defaultValue;

			if (int.TryParse(value, out int i))
				return i;

			return defaultValue;
		}



		/********************************************************************/
		/// <summary>
		/// Will try to find the entry in the settings. If it couldn't be
		/// found, the default value is returned
		/// </summary>
		/********************************************************************/
		public bool GetBoolEntry(string section, string entry, bool defaultValue = false)
		{
			// Use the string read function
			string value = GetStringEntry(section, entry);
			if (string.IsNullOrEmpty(value))
				return defaultValue;

			if (bool.TryParse(value, out bool b))
				return b;

			return defaultValue;
		}



		/********************************************************************/
		/// <summary>
		/// Will store the entry in the settings. If it already exists, it
		/// will be overwritten
		/// </summary>
		/********************************************************************/
		public void SetStringEntry(string section, string entry, string value)
		{
			// Start to lock the list
			listLock.EnterWriteLock();

			try
			{
				// Now find the section
				int index = FindSection(section);
				if (index != -1)
				{
					// Found the section. Now see if the entry exists in the section
					index = FindEntry(index + 1, entry, out int insertPos);
					if (index != -1)
					{
						// Got it, now overwrite the value
						lineList[index].Line = $"{entry}={value}";
					}
					else
					{
						// The entry couldn't be found, so add a new one to the section
						lineList.Insert(insertPos, new LineInfo { Type = LineType.Entry, Line = $"{entry}={value}" });
					}
				}
				else
				{
					// The section couldn't be found, so we create a new one
					lineList.Add(new LineInfo { Type = LineType.Section, Line = section });

					// Then add the entry
					lineList.Add(new LineInfo { Type = LineType.Entry, Line = $"{entry}={value}" });
				}

				// Settings has been changed
				changed = true;
			}
			finally
			{
				listLock.ExitWriteLock();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will store the entry in the settings. If it already exists, it
		/// will be overwritten
		/// </summary>
		/********************************************************************/
		public void SetIntEntry(string section, string entry, int value)
		{
			// Use the string write function to write the number
			SetStringEntry(section, entry, value.ToString(CultureInfo.InvariantCulture));
		}



		/********************************************************************/
		/// <summary>
		/// Will store the entry in the settings. If it already exists, it
		/// will be overwritten
		/// </summary>
		/********************************************************************/
		public void SetBoolEntry(string section, string entry, bool value)
		{
			// Use the string write function to write the number
			SetStringEntry(section, entry, value.ToString(CultureInfo.InvariantCulture));
		}



		/********************************************************************/
		/// <summary>
		/// Will remove an entry from the section given. If the entry
		/// couldn't be found, nothing is done
		/// </summary>
		/********************************************************************/
		public bool RemoveEntry(string section, string entry)
		{
			bool result = false;

			// Start to lock the list
			listLock.EnterWriteLock();

			try
			{
				// Now find the section
				int sectionIndex = FindSection(section);
				if (sectionIndex != -1)
				{
					// Found the section. Now see if the entry exists in the section
					int entryIndex = FindEntry(sectionIndex + 1, entry, out int insertPos);
					if (entryIndex != -1)
					{
						// Got it, now remove it
						lineList.RemoveAt(entryIndex);
						result = true;

						// Was it the last entry in the section?
						if (((sectionIndex + 1) == lineList.Count) || ((sectionIndex + 1) == entryIndex) && (lineList[entryIndex].Type == LineType.Section))
						{
							// Yes, remove the section
							lineList.RemoveAt(sectionIndex);
						}

						// Settings has been changed
						changed = true;
					}
				}
			}
			finally
			{
				listLock.ExitWriteLock();
			}

			return result;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns the full path to the settings file
		/// </summary>
		/********************************************************************/
		private string SettingsFileName
		{
			get
			{
				string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Polycode\NostalgicPlayer");
				return Path.Combine(directory, component + ".ini");
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will search from the top after the section given. If found, the
		/// list index where the section is stored is returned, else -1 will
		/// be returned
		/// </summary>
		/********************************************************************/
		private int FindSection(string section)
		{
			for (int i = 0, count = lineList.Count; i < count; i++)
			{
				LineInfo lineInfo = lineList[i];

				if ((lineInfo.Type == LineType.Section) && (lineInfo.Line == section))
				{
					// Found the section
					return i;
				}
			}

			// The section could not be found
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Will search from the index given after the entry. If found, the
		/// list index where the entry is stored is returned, else -1 will
		/// be returned
		/// </summary>
		/********************************************************************/
		private int FindEntry(int startIndex, string entry, out int insertPos)
		{
			int count = lineList.Count;
			for (int i = startIndex; i < count; i++)
			{
				LineInfo lineInfo = lineList[i];

				if (lineInfo.Type == LineType.Section)
				{
					// A new section is found, which mean the entry is not
					// stored in the previous section
					insertPos = i;
					return -1;
				}

				if (lineInfo.Type == LineType.Entry)
				{
					// Check the entry name
					int valPos = lineList[i].Line.IndexOf('=');
					if (lineInfo.Line.Substring(0, valPos).Trim() == entry)
					{
						// Found the entry
						insertPos = i;
						return i;
					}
				}
			}

			// The section could not be found
			insertPos = count;
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Will search from the index given after the entry. If found, the
		/// list index where the entry is stored is returned, else -1 will
		/// be returned
		/// </summary>
		/********************************************************************/
		private int FindEntryByNumber(int startIndex, int entryNum, out int insertPos)
		{
			int i;
			int count = lineList.Count;

			for (i = 0; i < entryNum; i++)
			{
				// Did we reach the end of the file
				if ((startIndex + i) >= count)
				{
					insertPos = count;
					return -1;
				}

				LineInfo lineInfo = lineList[i];

				if (lineInfo.Type == LineType.Section)
				{
					// A new section is found, which mean the entry is not
					// stored in the previous section
					insertPos = startIndex + i;
					return -1;
				}

				if (lineInfo.Type == LineType.Comment)
				{
					// Skip comments
					entryNum++;
				}
			}

			// Entry found or end of file reached
			if ((startIndex + i) >= count)
			{
				// EOF reached
				insertPos = count;
				return -1;
			}

			insertPos = startIndex + i;

			return insertPos;
		}
		#endregion
	}
}
