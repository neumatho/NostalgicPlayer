/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
	/// <summary>
	/// Envelope flags (EF_)
	/// </summary>
	[Flags]
	public enum EnvelopeFlag : byte
	{
		/// <summary></summary>
		None = 0,

		/// <summary></summary>
		On = 1,
		/// <summary></summary>
		Sustain = 2,
		/// <summary></summary>
		Loop = 4,
		/// <summary></summary>
		VolEnv = 8
	}
}
