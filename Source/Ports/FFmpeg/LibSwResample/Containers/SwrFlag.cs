/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum SwrFlag
	{
		/// <summary>
		/// Force resampling even if equal sample rate
		/// </summary>
		Resample = 1
	}
}
