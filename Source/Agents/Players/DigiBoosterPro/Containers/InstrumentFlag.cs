/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/

using System;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Different flags for instruments
	/// </summary>
	[Flags]
	internal enum InstrumentFlag : uint16_t
	{
		No_Loop = 0,
		Forward_Loop = 1,
		PingPong_Loop = 2,
		Loop_Mask = 3
	}
}
