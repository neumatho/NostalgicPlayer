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
	/// Flags for ProcessCmd
	/// </summary>
	[Flags]
	public enum ProcessFlags
	{
		OldStyle = 1,		// Behave as old scream tracker
		It,					// Behave ad impulse tracker
		Scream				// Enforce scream tracker specific limits
	}
}
