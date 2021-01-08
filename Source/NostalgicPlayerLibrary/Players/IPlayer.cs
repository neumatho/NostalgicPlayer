/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
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
		/// Will start playing the music
		/// </summary>
		void StartPlaying(MixerConfiguration newMixerConfiguration = null);

		/// <summary>
		/// Will stop the playing
		/// </summary>
		void StopPlaying();

		/// <summary>
		/// Return all the static information about the module
		/// </summary>
		ModuleInfoStatic StaticModuleInformation { get; }

		/// <summary>
		/// Return all the information about the module which changes while
		/// playing
		/// </summary>
		ModuleInfoFloating PlayingModuleInformation { get; }
	}
}
