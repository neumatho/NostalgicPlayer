/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase
{
	/// <summary>
	/// 
	/// </summary>
	internal static class SampleFormatTraits
	{
		/********************************************************************/
		/// <summary>
		/// TNE: The original asks SampleFormatTraits‹TSample› for a
		/// SampleFormat, and then that format for IsInt() and
		/// GetBitsPerSample(). Only the dither depth needs the lookup, so
		/// the whole chain is collapsed into this one call
		/// </summary>
		/********************************************************************/
		public static c_int GetDitherBits<TSample>() where TSample : unmanaged
		{
			// Only integer formats are dithered
			if ((typeof(TSample) == typeof(c_float)) || (typeof(TSample) == typeof(c_double)))
				return 0;

			return Marshal.SizeOf<TSample>() * 8;
		}
	}
}
