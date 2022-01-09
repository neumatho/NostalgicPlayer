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
	/// IT vibrato flags (ITVIB_)
	/// </summary>
	[Flags]
	internal enum ItVibratoFlags
	{
		None = 0,

		Fine = 0x01,
		Old = 0x02
	}
}
