/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.MikMod.Containers
{
	/// <summary>
	/// Vibrato flags (VIB_)
	/// </summary>
	[Flags]
	internal enum VibratoFlags
	{
		None = 0,

		/// <summary>
		/// MOD vibrato is not applied on tick 0
		/// </summary>
		PtBugs = 0x01,

		/// <summary>
		/// Increment LFO position on tick 0
		/// </summary>
		Tick0 = 0x02
	}
}
