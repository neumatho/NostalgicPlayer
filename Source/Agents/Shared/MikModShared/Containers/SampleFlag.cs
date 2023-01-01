/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
	/// <summary>
	/// Sample flags (SF_)
	/// </summary>
	[Flags]
	public enum SampleFlag : ushort
	{
		/// <summary></summary>
		None = 0,

		// Sample format (loading and in-memory) flags

		/// <summary></summary>
		_16Bits = 0x0001,
		/// <summary></summary>
		Stereo = 0x0002,
		/// <summary></summary>
		Signed = 0x0004,
		/// <summary></summary>
		BigEndian = 0x0008,
		/// <summary></summary>
		Delta = 0x0010,
		/// <summary></summary>
		ItPacked = 0x0020,
		/// <summary></summary>
		Adpcm4 = 0x0040,

		/// <summary></summary>
		FormatMask = 0x007f,

		// General playback flags

		/// <summary></summary>
		Loop = 0x0100,
		/// <summary></summary>
		Bidi = 0x0200,
		/// <summary></summary>
		Reverse = 0x0400,
		/// <summary></summary>
		Sustain = 0x0800,

		/// <summary></summary>
		PlaybackMask = 0x0c00,

		// Module-only playback flags

		/// <summary></summary>
		OwnPan = 0x1000,
		/// <summary></summary>
		UstLoop = 0x2000,

		/// <summary></summary>
		ExtraPlaybackMask = 0x3000
	}
}
