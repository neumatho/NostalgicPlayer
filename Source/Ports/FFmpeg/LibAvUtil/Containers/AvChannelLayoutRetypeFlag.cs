/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvChannelLayoutRetypeFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// The conversion must be lossless
		/// </summary>
		Lossless = 1 << 0,

		/// <summary>
		/// The specified retype target order is ignored and the simplest possible
		/// (canonical) order is used for which the input layout can be losslessy
		/// represented
		/// </summary>
		Canonical = 1 << 1
	}
}
