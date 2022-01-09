/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
	/// <summary>
	/// Key flags (KEY_)
	/// </summary>
	[Flags]
	public enum KeyFlag : byte
	{
		/// <summary></summary>
		Kick = 0,
		/// <summary></summary>
		Off = 1,
		/// <summary></summary>
		Fade = 2,
		/// <summary></summary>
		Kill = Off | Fade
	}
}
