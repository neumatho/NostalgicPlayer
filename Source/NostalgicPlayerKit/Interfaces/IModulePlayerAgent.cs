﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Mixer;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type can play modules
	/// </summary>
	public interface IModulePlayerAgent : IPlayerAgent
	{
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		ModulePlayerSupportFlag SupportFlags { get; }

		/// <summary>
		/// Will load the file into memory
		/// </summary>
		AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage);

		/// <summary>
		/// Initializes the player
		/// </summary>
		bool InitPlayer();

		/// <summary>
		/// Cleanup the player
		/// </summary>
		void CleanupPlayer();

		/// <summary>
		/// Initializes the current song
		/// </summary>
		void InitSound(int songNumber);

		/// <summary>
		/// Cleanup the current song
		/// </summary>
		void CleanupSound();

		/// <summary>
		/// Return the number of channels the module want to reserve
		/// </summary>
		int VirtualChannelCount { get; }

		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		int ModuleChannelCount { get; }

		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		SubSongInfo SubSongs { get; }

		/// <summary>
		/// Return the length of the current song
		/// </summary>
		int SongLength { get; }

		/// <summary>
		/// Holds the current position of the song
		/// </summary>
		int SongPosition { get; set; }

		/// <summary>
		/// Calculates the position time for each position
		/// </summary>
		TimeSpan GetPositionTimeTable(int songNumber, out TimeSpan[] positionTimes);

		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		bool GetInformationString(int line, out string description, out string value);

		/// <summary>
		/// Returns all the instruments available in the module. If none,
		/// null is returned
		/// </summary>
		InstrumentInfo[] Instruments { get; }

		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		SampleInfo[] Samples { get; }

		/// <summary>
		/// Return the total size of all the extra files loaded
		/// </summary>
		long ExtraFilesSizes { get; }

		/// <summary>
		/// Holds all the virtual channel instances used to play the samples
		/// </summary>
		Channel[] VirtualChannels { get; set; }

		/// <summary>
		/// Return the current playing frequency
		/// </summary>
		float PlayingFrequency { get; }

		/// <summary>
		/// Return the current state of the Amiga filter
		/// </summary>
		bool AmigaFilter { get; }

		/// <summary>
		/// Event called when the player change position
		/// </summary>
		event EventHandler PositionChanged;
	}
}
