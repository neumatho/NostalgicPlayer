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
	/// Unknown interface
	/// </summary>
	internal interface ISidUnknown
	{
		private static readonly IId iid = new IId(0xa595fcc4, 0xa138, 0x449a, 0x9711, 0x4ea5, 0xbb301d2a);

		/// <summary>
		/// Return an unique ID for this implementation
		/// </summary>
		static IId IId()
		{
			return iid;
		}

		/// <summary></summary>
		ISidUnknown IUnknown();

		/// <summary></summary>
		bool IQuery(IId iid, out object implementation);
	}
}
