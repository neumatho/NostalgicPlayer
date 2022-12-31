/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Flags
{
	/// <summary>
	/// Different flags indicating what the visual wants
	/// </summary>
	[Flags]
	public enum DisplayFlag
	{
		/// <summary>
		/// Nothing
		/// </summary>
		None = 0,

		/// <summary>
		/// Indicate that the window has a static size
		/// </summary>
		StaticWindow = 0x0001
	}
}
