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
	/// Standard IC interface functions
	/// </summary>
	internal interface ISidComponent : ISidUnknown
	{
		private static readonly IId iid = new IId(0xa9f9bf8b, 0xd0c2, 0x4dfa, 0x8b8a, 0xf0dd, 0xd7c8b05b);

		/// <summary>
		/// Return an unique ID for this implementation
		/// </summary>
		static IId IId()
		{
			return iid;
		}

		/// <summary></summary>
		void Reset();

		/// <summary></summary>
		byte Read(byte addr);

		/// <summary></summary>
		void Write(byte addr, byte data);
	}
}
