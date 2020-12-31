/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.RetroPlayer.RetroPlayerLibrary.Mixer.Containers
{
	/// <summary>
	/// Different modes the mixer can be set in
	/// </summary>
	[Flags]
	internal enum MixerMode : uint
	{
		/// <summary>
		/// No flags has been set
		/// </summary>
		None = 0,

		/// <summary>
		/// Output is in stereo
		/// </summary>
		Stereo = 0x0002,

		/// <summary>
		/// Surround is enabled
		/// </summary>
		Surround = 0x0100
	}
}
