/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers
{
	/// <summary>
	/// Different clock speeds
	/// </summary>
	[Flags]
	internal enum Clock
	{
		Unknown = 0x00,
		Pal = 0x01,
		Ntsc = 0x02,
		Any = Pal | Ntsc
	}
}
