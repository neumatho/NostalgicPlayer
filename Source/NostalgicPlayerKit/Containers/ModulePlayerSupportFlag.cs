/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Different flags indicating what a module player supports
	/// </summary>
	[Flags]
	public enum ModulePlayerSupportFlag
	{
		/// <summary>
		/// Nothing
		/// </summary>
		None = 0,

		/// <summary>
		/// Set this if your player can change to a certain position
		/// </summary>
		SetPosition = 0x0001,

		/// <summary>
		/// If this flag is set, the player is switched to buffer mode,
		/// which means your Play() method will only be called, when a
		/// new buffer needs to be set
		/// </summary>
		BufferMode = 0x1000
	}
}
