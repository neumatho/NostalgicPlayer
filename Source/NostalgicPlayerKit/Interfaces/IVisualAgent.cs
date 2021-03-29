/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type can act as a visual, which can show what is played.
	/// You also need to implement the IAgentGuiDisplay interface
	///
	/// Do not derive directly from this interface, but use either ChannelChange
	/// or SampleData interface instead
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
	}
}
