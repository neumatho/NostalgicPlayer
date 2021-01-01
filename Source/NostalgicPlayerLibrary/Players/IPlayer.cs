/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.NostalgicPlayerLibrary.Players
{
	/// <summary>
	/// Common interface to different player implementations
	/// </summary>
	public interface IPlayer
	{
		/// <summary>
		/// Will initialize the player
		/// </summary>
		bool InitPlayer(PlayerConfiguration playerConfiguration, out string errorMessage);

		/// <summary>
		/// Will cleanup the player
		/// </summary>
		void CleanupPlayer();

		/// <summary>
		/// Will select the song you want to play
		/// </summary>
		void SelectSong(int songNumber);

		/// <summary>
		/// Will start playing the music
		/// </summary>
		void StartPlaying();

		/// <summary>
		/// Will stop the playing
		/// </summary>
		void StopPlaying();

		/// <summary>
		/// Return the name of the module
		/// </summary>
		string ModuleName { get; }

		/// <summary>
		/// Return the name of the author
		/// </summary>
		string Author { get; }

		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		int Channels { get; }

		/// <summary>
		/// Return the total time of the current song
		/// </summary>
		TimeSpan TotalTime { get; }

		/// <summary>
		/// Return the format of the module
		/// </summary>
		string ModuleFormat { get; }

		/// <summary>
		/// Return the name of the player
		/// </summary>
		string PlayerName { get; }

		/// <summary>
		/// Return the size of the module
		/// </summary>
		long ModuleSize { get; }

		/// <summary>
		/// Return the module information list of the current song
		/// </summary>
		string[] ModuleInformation { get; }
	}
}
