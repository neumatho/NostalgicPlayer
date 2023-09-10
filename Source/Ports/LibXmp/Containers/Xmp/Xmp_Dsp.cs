/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum Xmp_Dsp
	{
		/// <summary>
		/// Lowpass filter effect
		/// </summary>
		LowPass = (1 << 0),

		/// <summary>
		/// 
		/// </summary>
		All = LowPass
	}
}
