/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type can act as a visual, which can show what is played.
	/// You also need to implement the IAgentGuiDisplay interface
	/// </summary>
	public interface IChannelChangeVisualAgent : IVisualAgent
	{
		/// <summary>
		/// Is called when initializing the visual agent. The array contains
		/// all the frequencies for each note per sample
		/// </summary>
		void SetNoteFrequencies(uint[][] noteFrequencies);

		/// <summary>
		/// Tell the visual about changes of the channels
		/// </summary>
		void ChannelsChanged(ChannelChanged[] channelChanged);
	}
}
