/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;

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
		bool InitPlayer(out string errorMessage);

		/// <summary>
		/// Cleanup the player
		/// </summary>
		void CleanupPlayer();

		/// <summary>
		/// Initializes the current song
		/// </summary>
		bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage);

		/// <summary>
		/// Cleanup the current song
		/// </summary>
		void CleanupSound();

		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		DurationInfo[] CalculateDuration();

		/// <summary>
		/// Is only called if BufferMode is set in the SupportFlags. It tells
		/// your player what frequency the NostalgicPlayer mixer is using.
		/// You can use it if you want or you can use your own output
		/// frequency
		/// </summary>
		void SetOutputFrequency(uint mixerFrequency);

		/// <summary>
		/// This is the main player method
		/// </summary>
		void Play();

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
		/// Return the current position of the song
		/// </summary>
		int GetSongPosition();

		/// <summary>
		/// Set a new position of the song
		/// </summary>
		void SetSongPosition(int position, PositionInfo positionInfo);

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
		/// Holds all the virtual channel instances used to play the samples
		/// </summary>
		IChannel[] VirtualChannels { get; set; }

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
