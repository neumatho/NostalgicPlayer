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

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the main window settings
	/// </summary>
	public class MainWindowSettings
	{
		/// <summary>
		/// The different time formats
		/// </summary>
		public enum TimeFormat
		{
			/// <summary></summary>
			Elapsed,
			/// <summary></summary>
			Remaining
		}

		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MainWindowSettings(Kit.Utility.Settings windowSettings)
		{
			settings = windowSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Time format
		/// </summary>
		/********************************************************************/
		public TimeFormat Time
		{
			get
			{
				string tempStr = settings.GetStringEntry("General", "TimeFormat");
				if (!Enum.TryParse(tempStr, out TimeFormat timeFormat))
					timeFormat = TimeFormat.Elapsed;

				return timeFormat;
			}

			set => settings.SetStringEntry("General", "TimeFormat", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Time format
		/// </summary>
		/********************************************************************/
		public int MasterVolume
		{
			get => settings.GetIntEntry("General", "MasterVolume", 256);

			set => settings.SetIntEntry("General", "MasterVolume", value);
		}



		/********************************************************************/
		/// <summary>
		/// Open module information window
		/// </summary>
		/********************************************************************/
		public bool OpenModuleInformationWindow
		{
			get => settings.GetBoolEntry("Window", "InfoOpenWindow");

			set => settings.SetBoolEntry("Window", "InfoOpenWindow", value);
		}



		/********************************************************************/
		/// <summary>
		/// Open sample information window
		/// </summary>
		/********************************************************************/
		public bool OpenSampleInformationWindow
		{
			get => settings.GetBoolEntry("Window", "SampleOpenWindow");

			set => settings.SetBoolEntry("Window", "SampleOpenWindow", value);
		}



		/********************************************************************/
		/// <summary>
		/// Open agent windows
		/// </summary>
		/********************************************************************/
		public Guid[] OpenAgentWindows
		{
			get
			{
				List<Guid> result = new List<Guid>();

				for (int i = 0; ; i++)
				{
					try
					{
						// Try to read the entry
						string value = settings.GetStringEntry("Agent Window", i, out string id);
						if (string.IsNullOrEmpty(value))
							break;

						// Parse the entry value
						if (bool.TryParse(value, out bool open) && open)
							result.Add(Guid.Parse(id));
					}
					catch (Exception)
					{
						// Ignore exception
						;
					}
				}

				return result.ToArray();
			}

			set
			{
				settings.RemoveSection("Agent Window");

				foreach (Guid id in value)
					settings.SetBoolEntry("Agent Window", id.ToString("D"), true);
			}
		}
	}
}
