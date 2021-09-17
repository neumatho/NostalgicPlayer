/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	internal interface ISidMixer : ISidUnknown
	{
		private static readonly IId iid = new IId(0xc4438750, 0x06ec, 0x11db, 0x9cd8, 0x0800, 0x200c9a66);

		/// <summary>
		/// Return an unique ID for this implementation
		/// </summary>
		static IId IId()
		{
			return iid;
		}

		/// <summary></summary>
		void Mute(byte num, bool enable);

		/// <summary></summary>
		void Volume(byte num, byte level);

		/// <summary></summary>
		void Gain(sbyte percent);
	}
}
