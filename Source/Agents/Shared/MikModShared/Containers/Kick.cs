/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
	/// <summary>
	/// Kick flags (KICK_)
	/// </summary>
	public enum Kick : byte
	{
		/// <summary></summary>
		Absent = 0,
		/// <summary></summary>
		Note = 1,
		/// <summary></summary>
		KeyOff = 2,
		/// <summary></summary>
		Env = 4
	}
}
