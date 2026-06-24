/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp.Resample
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Limiter
	{
		/// <summary></summary>
		public const int32_t threshold = 28000;

		/********************************************************************/
		/// <summary>
		/// Soft Clipping into 16 bit range [-32768,32767]
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int16_t SoftClip(int32_t x)
		{
			return (int16_t)(SoftClipImpl(x));
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int32_t Clipper(int32_t x, int32_t m)
		{
			if (x < threshold)
				return x;

			double max_val = m;
			double t = threshold / max_val;
			double a = 1.0 - t;
			double b = 1.0 / a;

			double value = (x - threshold) / max_val;
			value = a * CMath.tanh(b * value);

			return (int32_t)(threshold + (value * max_val));
		}



		/********************************************************************/
		/// <summary>
		/// Soft clipping implementation, splitted for test
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int SoftClipImpl(int x)
		{
			return x < 0 ? -Clipper(-x, 32768) : Clipper(x, 32767);
		}
		#endregion
	}
}
