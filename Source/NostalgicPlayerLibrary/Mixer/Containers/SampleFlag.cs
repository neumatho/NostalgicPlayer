﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Mixer.Containers
{
	/// <summary>
	/// Different flags used when playing a sample
	/// </summary>
	[Flags]
	internal enum SampleFlag
	{
		None = 0,

		// Sample format (loading and in-memory) flags
		_16Bits = 0x0001,

		// General playback flags
		Loop = 0x0100,
		Bidi = 0x0200,
		Reverse = 0x0400,
		ChangePosition = 0x1000,
		Release = 0x2000
	}
}
