/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.MikMod.LibMikMod
{
	/// <summary>
	/// Interface to driver methods that the player calls
	/// </summary>
	internal interface IDriver
	{
		/// <summary>
		/// Stops the channel
		/// </summary>
		void VoiceStopInternal(sbyte voice);

		/// <summary>
		/// Returns true if the voice doesn't play anymore
		/// </summary>
		bool VoiceStoppedInternal(sbyte voice);

		/// <summary>
		/// Starts to play the sample
		/// </summary>
		void VoicePlayInternal(sbyte voice, Sample s, uint start);

		/// <summary>
		/// Changes the volume in the channel
		/// </summary>
		void VoiceSetVolumeInternal(sbyte voice, ushort vol);

		/// <summary>
		/// Changes the panning in the channel
		/// </summary>
		void VoiceSetPanningInternal(sbyte voice, uint pan);

		/// <summary>
		/// Changes the frequency in the channel
		/// </summary>
		void VoiceSetFrequencyInternal(sbyte voice, uint frq);
	}
}
