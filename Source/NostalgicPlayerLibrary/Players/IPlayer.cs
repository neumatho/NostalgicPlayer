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
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Loaders;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
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
		bool StartPlaying(Loader loader, out string errorMessage, MixerConfiguration newMixerConfiguration = null);

		/// <summary>
		/// Will stop the playing
		/// </summary>
		void StopPlaying(bool stopOutputAgent = true);

		/// <summary>
		/// Will pause the playing
		/// </summary>
		void PausePlaying();

		/// <summary>
		/// Will resume the playing
		/// </summary>
		void ResumePlaying();

		/// <summary>
		/// Will set the master volume
		/// </summary>
		void SetMasterVolume(int volume);

		/// <summary>
		/// Will change the mixer settings that can be change real-time
		/// </summary>
		void ChangeMixerSettings(MixerConfiguration mixerConfiguration);

		/// <summary>
		/// Return all the static information about the module
		/// </summary>
		ModuleInfoStatic StaticModuleInformation { get; }

		/// <summary>
		/// Return all the information about the module which changes while
		/// playing
		/// </summary>
		ModuleInfoFloating PlayingModuleInformation { get; }

		/// <summary>
		/// Event called when the player reached the end
		/// </summary>
		event EventHandler EndReached;

		/// <summary>
		/// Event called when the player change some of the module information
		/// </summary>
		event ModuleInfoChangedEventHandler ModuleInfoChanged;
	}
}
