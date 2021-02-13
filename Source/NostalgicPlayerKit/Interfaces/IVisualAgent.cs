/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type can act as a visual, which can show what is played.
	/// You also need to implement the IAgentGuiDisplay interface
	/// </summary>
	public interface IVisualAgent : IAgentWorker
	{
		/// <summary>
		/// Initializes the visual
		/// </summary>
		void InitVisual(int channels);

		/// <summary>
		/// Cleanup the visual
		/// </summary>
		void CleanupVisual();

		/// <summary>
		/// Tell the visual about a channel change
		/// </summary>
		void ChannelChange(ChannelChanged channelChanged);
	}
}
