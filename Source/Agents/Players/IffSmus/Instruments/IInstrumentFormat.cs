/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// All instrument formats derive from this interface
	/// </summary>
	internal interface IInstrumentFormat
	{
		/// <summary>
		/// Load the instrument
		/// </summary>
		bool Load(ModuleStream instrumentStream, PlayerFileInfo fileInfo, string instrumentPath, string instrumentFileName, List<Instrument> instruments, out string errorMessage);

		/// <summary>
		/// Initialize the instrument
		/// </summary>
		void Setup(GlobalInfo globalInfo, GlobalPlayingInfo playingInfo, VoiceInfo voice, IChannel channel, int channelNumber);

		/// <summary>
		/// Play the initialized instrument
		/// </summary>
		void Play(GlobalPlayingInfo playingInfo, VoiceInfo voice, IChannel channel, int channelNumber);

		/// <summary>
		/// Return sample information
		/// </summary>
		SampleInfo GetSampleInfo(GlobalInfo globalInfo);
	}
}
