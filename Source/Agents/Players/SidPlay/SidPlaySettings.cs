/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay
{
	/// <summary>
	/// This class holds all the settings
	/// </summary>
	public class SidPlaySettings
	{
		/// <summary></summary>
		public const float DefaultFs = 400.0f;
		/// <summary></summary>
		public const float DefaultFm = 60.0f;
		/// <summary></summary>
		public const float DefaultFt = 0.05f;

		/// <summary>
		/// Different CIA models
		/// </summary>
		public enum CiaModelType
		{
			/// <summary></summary>
			Mos6526,
			/// <summary></summary>
			Mos8521,
			/// <summary></summary>
			Mos6526_W4485
		}

		/// <summary>
		/// Different clock speeds
		/// </summary>
		public enum ClockType
		{
			/// <summary></summary>
			Pal,
			/// <summary></summary>
			Ntsc,
			/// <summary></summary>
			NtscOld,
			/// <summary></summary>
			Drean,
			/// <summary></summary>
			PalM
		}

		/// <summary>
		/// Different options for the clock speed
		/// </summary>
		public enum ClockOptionType
		{
			/// <summary></summary>
			NotKnown,
			/// <summary></summary>
			Always
		}

		/// <summary>
		/// Different SID models
		/// </summary>
		public enum SidModelType
		{
			/// <summary></summary>
			Mos6581,
			/// <summary></summary>
			Mos8580
		}

		/// <summary>
		/// Different options for the SID model
		/// </summary>
		public enum SidModelOptionType
		{
			/// <summary></summary>
			NotKnown,
			/// <summary></summary>
			Always
		}

		/// <summary>
		/// Supported mixer types
		/// </summary>
		public enum MixerType
		{
			/// <summary></summary>
			Interpolate,
			/// <summary></summary>
			ResampleInterpolate
		}

		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidPlaySettings()
		{
			settings = DependencyInjection.Container.GetInstance<ISettings>();
			settings.LoadSettings("SidPlay");

			// Remove obsolete settings
			settings.RemoveEntry("Emulator", "MemoryModel");
			settings.RemoveEntry("Filter", "FilterOption");
			settings.RemoveEntry("Filter", "FilterFs");
			settings.RemoveEntry("Filter", "FilterFm");
			settings.RemoveEntry("Filter", "FilterFt");

			if (settings.ContainsEntry("Filter", "Enabled"))
			{
				FilterEnabled = settings.GetBoolEntry("Filter", "Enabled");
				settings.RemoveEntry("Filter", "Enabled");
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the main settings object
		/// </summary>
		/********************************************************************/
		public ISettings Settings => settings;



		/********************************************************************/
		/// <summary>
		/// Which CIA model to use
		/// </summary>
		/********************************************************************/
		public CiaModelType CiaModel
		{
			get => Enum.Parse<CiaModelType>(settings.GetStringEntry("Emulator", "CiaModel", CiaModelType.Mos6526.ToString()));

			set => settings.SetStringEntry("Emulator", "CiaModel", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Which clock speed to use
		/// </summary>
		/********************************************************************/
		public ClockType ClockSpeed
		{
			get => Enum.Parse<ClockType>(settings.GetStringEntry("Emulator", "ClockSpeed", ClockType.Pal.ToString()));

			set => settings.SetStringEntry("Emulator", "ClockSpeed", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Which clock speed option to use
		/// </summary>
		/********************************************************************/
		public ClockOptionType ClockSpeedOption
		{
			get => Enum.Parse<ClockOptionType>(settings.GetStringEntry("Emulator", "ClockSpeedOption", ClockOptionType.NotKnown.ToString()));

			set => settings.SetStringEntry("Emulator", "ClockSpeedOption", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Which SID model to use
		/// </summary>
		/********************************************************************/
		public SidModelType SidModel
		{
			get => Enum.Parse<SidModelType>(settings.GetStringEntry("Emulator", "SidModel", SidModelType.Mos6581.ToString()));

			set => settings.SetStringEntry("Emulator", "SidModel", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Which SID model option to use
		/// </summary>
		/********************************************************************/
		public SidModelOptionType SidModelOption
		{
			get => Enum.Parse<SidModelOptionType>(settings.GetStringEntry("Emulator", "SidModelOption", SidModelOptionType.NotKnown.ToString()));

			set => settings.SetStringEntry("Emulator", "SidModelOption", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Tells if filter is enabled or not
		/// </summary>
		/********************************************************************/
		public bool FilterEnabled
		{
			get => settings.GetBoolEntry("Options", "Filter", true);

			set => settings.SetBoolEntry("Options", "Filter", value);
		}



		/********************************************************************/
		/// <summary>
		/// Tells if digiboost is enabled or not
		/// </summary>
		/********************************************************************/
		public bool DigiBoostEnabled
		{
			get => settings.GetBoolEntry("Options", "DigiBoost", false);

			set => settings.SetBoolEntry("Options", "DigiBoost", value);
		}



		/********************************************************************/
		/// <summary>
		/// Which mixer to use
		/// </summary>
		/********************************************************************/
		public MixerType Mixer
		{
			get => Enum.Parse<MixerType>(settings.GetStringEntry("Options", "Mixer", MixerType.ResampleInterpolate.ToString()));

			set => settings.SetStringEntry("Options", "Mixer", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Full path to the HVSC directory
		/// </summary>
		/********************************************************************/
		public string HvscPath
		{
			get => settings.GetStringEntry("HVSC", "Path");

			set => settings.SetStringEntry("HVSC", "Path", value);
		}



		/********************************************************************/
		/// <summary>
		/// Tells if STIL lookup is enabled or not
		/// </summary>
		/********************************************************************/
		public bool StilEnabled
		{
			get => settings.GetBoolEntry("HVSC", "Stil", true);

			set => settings.SetBoolEntry("HVSC", "Stil", value);
		}



		/********************************************************************/
		/// <summary>
		/// Tells if bug list lookup is enabled or not
		/// </summary>
		/********************************************************************/
		public bool BugListEnabled
		{
			get => settings.GetBoolEntry("HVSC", "BugList", true);

			set => settings.SetBoolEntry("HVSC", "BugList", value);
		}



		/********************************************************************/
		/// <summary>
		/// Tells if song length lookup is enabled or not
		/// </summary>
		/********************************************************************/
		public bool SongLengthEnabled
		{
			get => settings.GetBoolEntry("HVSC", "SongLength", true);

			set => settings.SetBoolEntry("HVSC", "SongLength", value);
		}
	}
}
