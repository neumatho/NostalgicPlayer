/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Handles all the loading and playing of modules
	/// </summary>
	public interface IModuleHandlerService
	{
		/// <summary>
		/// Event called for each second the module has played
		/// </summary>
		event ClockUpdatedEventHandler ClockUpdated;

		/// <summary>
		/// Event called when the player change position
		/// </summary>
		event EventHandler PositionChanged;

		/// <summary>
		/// Event called when the player change sub-song
		/// </summary>
		event SubSongChangedEventHandler SubSongChanged;

		/// <summary>
		/// Event called when the player reached the end
		/// </summary>
		event EventHandler EndReached;

		/// <summary>
		/// Event called when the player change some of the module information
		/// </summary>
		event ModuleInfoChangedEventHandler ModuleInfoChanged;

		/// <summary>
		/// Event called if the player fails while playing
		/// </summary>
		event PlayerFailedEventHandler PlayerFailed;

		/// <summary>
		/// Initialize and start the module handler thread
		/// </summary>
		void Initialize(int startVolume);

		/// <summary>
		/// Close the currently active output agent
		/// </summary>
		void CloseOutputAgent();

		/// <summary>
		/// Tells if the module has been loaded or not
		/// </summary>
		bool IsModuleLoaded { get; }

		/// <summary>
		/// Tells if the double buffering module has been loaded or not
		/// </summary>
		bool IsDoubleBufferingModuleLoaded { get; }

		/// <summary>
		/// Tells if the module is playing or not at the moment
		/// </summary>
		bool IsPlaying { get; }

		/// <summary>
		/// Return the output agent in use if playing
		/// </summary>
		AgentInfo OutputAgentInfo { get; }

		/// <summary>
		/// Return all the static module information
		/// </summary>
		ModuleInfoStatic StaticModuleInformation { get; }

		/// <summary>
		/// Return all the information about the module which changes while
		/// playing
		/// </summary>
		ModuleInfoFloating PlayingModuleInformation { get; }

		/// <summary>
		/// Will load and play the module at the index given
		/// </summary>
		bool LoadAndPlayModule(ModuleListItem listItem, int subSong, int startPos);

		/// <summary>
		/// Load and/or initialize module
		/// </summary>
		bool LoadAndInitModule(ModuleListItem listItem, int? subSong = null, int? startPos = null, bool showError = true);

		/// <summary>
		/// Will start playing the given module
		/// </summary>
		bool PlayModule(ModuleListItem listItem);

		/// <summary>
		/// Will stop and free the playing module if any
		/// </summary>
		void StopAndFreeModule();

		/// <summary>
		/// Free all extra loaded modules (from double buffering)
		/// </summary>
		void FreeExtraModules();

		/// <summary>
		/// Will free all loaded modules
		/// </summary>
		void FreeAllModules();

		/// <summary>
		/// Will start to play the song given
		/// </summary>
		bool StartSong(ModuleListItem listItem, int newSong);

		/// <summary>
		/// Will tell the player to change to the position given
		/// </summary>
		void SetSongPosition(int newPosition);

		/// <summary>
		/// Will remember the mute status
		/// </summary>
		void SetMuteStatus(bool muted);

		/// <summary>
		/// Will tell the mixer to change the volume
		/// </summary>
		void SetVolume(int newVolume);

		/// <summary>
		/// Will change the mixer settings that can be change real-time
		/// </summary>
		void ChangeMixerSettings(MixerConfiguration newMixerConfiguration);

		/// <summary>
		/// Will pause the player
		/// </summary>
		void PausePlaying();

		/// <summary>
		/// Will resume the player
		/// </summary>
		void ResumePlaying();

		/// <summary>
		/// Return the enable status for all channels
		/// </summary>
		bool[] GetEnabledChannels();

		/// <summary>
		/// Set the enable status for a given range of channels
		/// </summary>
		void EnableChannels(bool enabled, int startChannel, int stopChannel = -1);
	}
}
