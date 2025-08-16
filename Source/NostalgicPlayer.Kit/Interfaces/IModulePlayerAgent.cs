/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;

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
		/// Return a string containing a warning string. If there is no
		/// warning, an empty string is returned
		/// </summary>
		string GetWarning();

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
		bool InitSound(int songNumber, out string errorMessage);

		/// <summary>
		/// Cleanup the current song
		/// </summary>
		void CleanupSound();

		/// <summary>
		/// Is only called if BufferMode is set in the SupportFlags. It tells
		/// your player what frequency the NostalgicPlayer mixer is using.
		/// You can use it if you want or you can use your own output
		/// frequency, but if you also using BufferDirect, you need to use
		/// this frequency and number of channels
		/// </summary>
		void SetOutputFormat(uint mixerFrequency, int channels);

		/// <summary>
		/// Is only called if BufferDirect is set in the SupportFlags. It
		/// tells your player about the different mixer settings you need to
		/// take care of
		/// </summary>
		void ChangeMixerConfiguration(PlayerMixerInfo mixerInfo);

		/// <summary>
		/// This is the main player method
		/// </summary>
		void Play();

		/// <summary>
		/// Return which speakers the player uses
		/// </summary>
		SpeakerFlag SpeakerFlags { get; }

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
		/// Returns all the instruments available in the module. If none,
		/// null is returned
		/// </summary>
		IEnumerable<InstrumentInfo> Instruments { get; }

		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		IEnumerable<SampleInfo> Samples { get; }

		/// <summary>
		/// Holds all the virtual channel instances used to play the samples
		/// </summary>
		IChannel[] VirtualChannels { get; set; }

		/// <summary>
		/// Holds the channels used by visuals. Only needed for players using
		/// buffer mode if possible
		/// </summary>
		ChannelChanged[] VisualChannels { get; }

		/// <summary>
		/// Return an effect master instance if the player adds extra mixer
		/// effects to the output
		/// </summary>
		IEffectMaster EffectMaster { get; }

		/// <summary>
		/// Return the current playing frequency
		/// </summary>
		float PlayingFrequency { get; }

		/// <summary>
		/// Return the current state of the Amiga filter
		/// </summary>
		bool AmigaFilter { get; }

		/// <summary>
		/// Event called when the player change sub-song
		/// </summary>
		public event SubSongChangedEventHandler SubSongChanged;
	}
}
