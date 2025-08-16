/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Library.Sound.Mixer.Containers
{
	/// <summary>
	/// Different flags used when playing a sample
	/// </summary>
	[Flags]
	internal enum SampleFlag
	{
		None = 0,

		_16Bits = 0x0001,
		Stereo = 0x0002,

		Bidi = 0x0200,
		Reverse = 0x0400
	}
}
