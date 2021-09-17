/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Imp
{
	/// <summary>
	/// Interface to CoEmulation class
	/// </summary>
	internal interface ICoEmulation : ICoComponent
	{
		/// <summary></summary>
		ISidUnknown Builder();

		/// <summary></summary>
		void Clock(Sid2Clock clk);

		/// <summary></summary>
		void Gain(sbyte percent);

		/// <summary>
		/// Set optimization level
		/// </summary>
		void Optimization(byte level);

		/// <summary></summary>
		void Reset(byte volume);
	}
}
