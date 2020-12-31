/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.RetroPlayer.RetroPlayerKit.Containers
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
		SetPosition = 0x0001
	}
}
