/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

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
		/// Different environment options
		/// </summary>
		public enum Environment
		{
			/// <summary></summary>
			PlaySid,
			/// <summary></summary>
			Transparent,
			/// <summary></summary>
			FullBank,
			/// <summary></summary>
			Real
		}

		/// <summary>
		/// Different clock speeds
		/// </summary>
		public enum Clock
		{
			/// <summary></summary>
			Pal,
			/// <summary></summary>
			Ntsc
		}

		/// <summary>
		/// Different options for the clock speed
		/// </summary>
		public enum ClockOption
		{
			/// <summary></summary>
			NotKnown,
			/// <summary></summary>
			Always
		}

		/// <summary>
		/// Different SID models
		/// </summary>
		public enum Model
		{
			/// <summary></summary>
			Mos6581,
			/// <summary></summary>
			Mos8580
		}

		/// <summary>
		/// Different options for the SID model
		/// </summary>
		public enum ModelOption
		{
			/// <summary></summary>
			NotKnown,
			/// <summary></summary>
			Always
		}

		/// <summary>
		/// Different filter options
		/// </summary>
		public enum FilterOption
		{
			/// <summary></summary>
			ModelSpecific,
			/// <summary></summary>
			Custom
		}

		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidPlaySettings()
		{
			settings = new Kit.Utility.Settings("SidPlay");
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
		/// Which memory model to use
		/// </summary>
		/********************************************************************/
		public Environment MemoryModel
		{
			get => Enum.Parse<Environment>(settings.GetStringEntry("Emulator", "MemoryModel", Environment.Real.ToString()));

			set => settings.SetStringEntry("Emulator", "MemoryModel", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Which clock speed to use
		/// </summary>
		/********************************************************************/
		public Clock ClockSpeed
		{
			get => Enum.Parse<Clock>(settings.GetStringEntry("Emulator", "ClockSpeed", Clock.Pal.ToString()));

			set => settings.SetStringEntry("Emulator", "ClockSpeed", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Which clock speed option to use
		/// </summary>
		/********************************************************************/
		public ClockOption ClockSpeedOption
		{
			get => Enum.Parse<ClockOption>(settings.GetStringEntry("Emulator", "ClockSpeedOption", ClockOption.NotKnown.ToString()));

			set => settings.SetStringEntry("Emulator", "ClockSpeedOption", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Which SID model to use
		/// </summary>
		/********************************************************************/
		public Model SidModel
		{
			get => Enum.Parse<Model>(settings.GetStringEntry("Emulator", "SidModel", Model.Mos6581.ToString()));

			set => settings.SetStringEntry("Emulator", "SidModel", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Which SID model option to use
		/// </summary>
		/********************************************************************/
		public ModelOption SidModelOption
		{
			get => Enum.Parse<ModelOption>(settings.GetStringEntry("Emulator", "SidModelOption", ModelOption.NotKnown.ToString()));

			set => settings.SetStringEntry("Emulator", "SidModelOption", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Tells if filter is enabled or not
		/// </summary>
		/********************************************************************/
		public bool FilterEnabled
		{
			get => settings.GetBoolEntry("Filter", "Enabled", true);

			set => settings.SetBoolEntry("Filter", "Enabled", value);
		}



		/********************************************************************/
		/// <summary>
		/// Which filter option to use
		/// </summary>
		/********************************************************************/
		public FilterOption Filter
		{
			get => Enum.Parse<FilterOption>(settings.GetStringEntry("Filter", "FilterOption", FilterOption.ModelSpecific.ToString()));

			set => settings.SetStringEntry("Filter", "FilterOption", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Tells filter parameter 1
		/// </summary>
		/********************************************************************/
		public float FilterFs
		{
			get => settings.GetFloatEntry("Filter", "FilterFs", DefaultFs);

			set => settings.SetFloatEntry("Filter", "FilterFs", value);
		}



		/********************************************************************/
		/// <summary>
		/// Tells filter parameter 2
		/// </summary>
		/********************************************************************/
		public float FilterFm
		{
			get => settings.GetFloatEntry("Filter", "FilterFm", DefaultFm);

			set => settings.SetFloatEntry("Filter", "FilterFm", value);
		}



		/********************************************************************/
		/// <summary>
		/// Tells filter parameter 1
		/// </summary>
		/********************************************************************/
		public float FilterFt
		{
			get => settings.GetFloatEntry("Filter", "FilterFt", DefaultFt);

			set => settings.SetFloatEntry("Filter", "FilterFt", value);
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
