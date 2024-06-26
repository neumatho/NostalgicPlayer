﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers
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
		Surround = 0x0100,

		/// <summary>
		/// Interpolation is enabled
		/// </summary>
		Interpolation = 0x200
	}
}
