/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type can play samples
	/// </summary>
	public interface ISamplePlayerAgent : IPlayerAgent
	{
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		SamplePlayerSupportFlag SupportFlags { get; }

		/// <summary>
		/// Will load the header information from the file
		/// </summary>
		AgentResult LoadHeaderInfo(ModuleStream stream, out string errorMessage);

		/// <summary>
		/// Initializes the player
		/// </summary>
		bool InitPlayer(ModuleStream stream);

		/// <summary>
		/// Cleanup the player
		/// </summary>
		void CleanupPlayer();

		/// <summary>
		/// Initializes the player to start the sample from start
		/// </summary>
		void InitSound();

		/// <summary>
		/// Cleanup the current song
		/// </summary>
		void CleanupSound();

		/// <summary>
		/// Will load and decode a data block and store it in the buffer
		/// given
		/// </summary>
		int LoadDataBlock(int[] outputBuffer, int count);

		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		int ChannelCount { get; }

		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		int Frequency { get; }

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
		TimeSpan GetPositionTimeTable(out TimeSpan[] positionTimes);

		/// <summary>
		/// Event called when the player change position
		/// </summary>
		event EventHandler PositionChanged;
	}
}
