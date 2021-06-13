/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Used to tell visual agents about a channel change
	/// </summary>
	public class ChannelChanged
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ChannelChanged(IChannel[] virtualChannels, ChannelFlags[] flags)
		{
			VirtualChannels = virtualChannels;
			Flags = flags;
		}



		/********************************************************************/
		/// <summary>
		/// An array with a channel structure for each channel
		/// </summary>
		/********************************************************************/
		public IChannel[] VirtualChannels
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// An array with the channel flags. Use these instead of those
		/// found in the Channel structure, because they may have been
		/// cleared
		/// </summary>
		/********************************************************************/
		public ChannelFlags[] Flags
		{
			get;
		}
	}
}
