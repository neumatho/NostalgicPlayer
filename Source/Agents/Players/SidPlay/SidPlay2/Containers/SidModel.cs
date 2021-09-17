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
	/// Different SID models
	/// </summary>
	[Flags]
	internal enum SidModel
	{
		Unknown = 0x00,
		_6581 = 0x01,
		_8580 = 0x02,
		Any = _6581 | _8580
	}
}
