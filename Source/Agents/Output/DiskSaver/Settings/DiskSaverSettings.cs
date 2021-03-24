/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Output.DiskSaver.Settings
{
	/// <summary>
	/// This class holds all the settings
	/// </summary>
	internal class DiskSaverSettings
	{
		/// <summary>
		/// The different output types
		/// </summary>
		public enum OutType
		{
			/// <summary>
			/// Mono
			/// </summary>
			Mono,

			/// <summary>
			/// Stereo
			/// </summary>
			Stereo
		}

		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DiskSaverSettings()
		{
			settings = new Kit.Utility.Settings("DiskSaver");
			settings.LoadSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Return the main settings object
		/// </summary>
		/********************************************************************/
		public Kit.Utility.Settings Settings => settings;



		/********************************************************************/
		/// <summary>
		/// Where to store the output
		/// </summary>
		/********************************************************************/
		public string DiskPath
		{
			get
			{
				string path = settings.GetStringEntry("General", "DiskPath");
				if (string.IsNullOrEmpty(path))
					path = Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic);

				return path;
			}

			set => settings.SetStringEntry("General", "DiskPath", value);
		}



		/********************************************************************/
		/// <summary>
		/// Output size
		/// </summary>
		/********************************************************************/
		public int OutputSize
		{
			get => settings.GetIntEntry("General", "OutputSize", 16);

			set => settings.SetIntEntry("General", "OutputSize", value);
		}



		/********************************************************************/
		/// <summary>
		/// Output type
		/// </summary>
		/********************************************************************/
		public OutType OutputType
		{
			get
			{
				if (Enum.TryParse(settings.GetStringEntry("General", "OutputType"), out OutType type))
					return type;

				return OutType.Stereo;
			}

			set => settings.SetStringEntry("General", "OutputType", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Output frequency
		/// </summary>
		/********************************************************************/
		public int OutputFrequency
		{
			get => settings.GetIntEntry("General", "OutputFrequency", 44100);

			set => settings.SetIntEntry("General", "OutputFrequency", value);
		}



		/********************************************************************/
		/// <summary>
		/// Output format
		/// </summary>
		/********************************************************************/
		public Guid OutputFormat
		{
			get
			{
				if (Guid.TryParse(settings.GetStringEntry("General", "OutputFormat"), out Guid g))
					return g;

				return Guid.Empty;
			}

			set => settings.SetStringEntry("General", "OutputFormat", value.ToString("D"));
		}



		/********************************************************************/
		/// <summary>
		/// Output agent
		/// </summary>
		/********************************************************************/
		public Guid OutputAgent
		{
			get
			{
				if (Guid.TryParse(settings.GetStringEntry("General", "OutputAgent"), out Guid g))
					return g;

				return Guid.Empty;
			}

			set => settings.SetStringEntry("General", "OutputAgent", value.ToString("D"));
		}
	}
}
