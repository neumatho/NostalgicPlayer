/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Simple Pseudo Random Number Generator
	///
	/// This is a implementation of SFC64, a 64-bit PRNG by Chris Doty-Humphrey.
	///
	/// This Generator is much faster (0m1.872s) than 64bit KISS (0m3.823s) and PCG-XSH-RR-64/32 (0m2.700s)
	/// And passes testu01 and practrand test suits
	/// </summary>
	public static class Sfc64
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t FF_Sfc64_Get(FFSfc64 s)
		{
			uint64_t tmp = s.A + s.B + s.Counter++;
			s.A = s.B ^ (s.B >> 11);
			s.B = s.C + (s.C << 3);		// This is multiply by 9
			s.C = ((s.C << 24) | (s.C >> 40)) + tmp;

			return tmp;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sfc64 with up to 3 seeds
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FF_Sfc64_Init(FFSfc64 s, uint64_t seedA, uint64_t seedB, uint64_t seedC, c_int rounds)
		{
			s.A = seedA;
			s.B = seedB;
			s.C = seedC;
			s.Counter = 1;

			while (rounds-- != 0)
				FF_Sfc64_Get(s);
		}
	}
}
