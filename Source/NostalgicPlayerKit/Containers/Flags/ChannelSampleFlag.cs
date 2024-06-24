/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Flags
{
	/// <summary>
	/// Sample in a channel flags
	/// </summary>
	[Flags]
	public enum ChannelSampleFlag : uint
	{
		/// <summary>
		/// No flags
		/// </summary>
		None = 0,

		/// <summary>
		/// Set this if the sample is 16 bit
		/// </summary>
		_16Bit = 0x00000001,

		/// <summary>
		/// Set this if the sample is in stereo
		/// </summary>
		Stereo = 0x00000002,

		/// <summary>
		/// Set this to play the sample backwards
		/// </summary>
		Backwards = 0x00000010,

		/// <summary>
		/// Set this together with the loop for ping-pong loop
		/// </summary>
		PingPong = 0x00000100,
	}
}
