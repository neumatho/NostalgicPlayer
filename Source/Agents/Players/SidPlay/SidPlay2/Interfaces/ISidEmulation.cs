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
	internal interface ISidEmulation : ISidComponent
	{
		private static readonly IId iid = new IId(0x82c01032, 0x5d8c, 0x447a, 0x89fa, 0x0599, 0x0990b766);

		/// <summary>
		/// Return an unique ID for this implementation
		/// </summary>
		static IId IId()
		{
			return iid;
		}

		/// <summary></summary>
		ISidUnknown Builder();

		/// <summary></summary>
		void Clock(Sid2Clock clk);

		/// <summary>
		/// Set optimization level
		/// </summary>
		void Optimization(byte level);

		/// <summary></summary>
		void Reset(byte volume);

		/// <summary></summary>
		int Output(byte bits);
	}
}
