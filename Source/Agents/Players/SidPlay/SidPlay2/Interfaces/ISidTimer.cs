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
	/// </summary>
	internal interface ISidTimer : ISidUnknown
	{
		private static readonly IId iid = new IId(0xba2f0dd8, 0xdafb, 0x4aea, 0xb09a, 0x8aa9, 0xd335b36b);

		/// <summary>
		/// Return an unique ID for this implementation
		/// </summary>
		static IId IId()
		{
			return iid;
		}

		/// <summary></summary>
		uint Mileage();

		/// <summary></summary>
		uint TimeBase();

		/// <summary></summary>
		uint Time();
	}
}
