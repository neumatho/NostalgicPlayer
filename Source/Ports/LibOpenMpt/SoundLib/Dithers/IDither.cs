/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers
{
	/// <summary>
	/// Common interface implemented by the per-sample dither algorithms.
	/// </summary>
	internal interface IDither<TPrng>
	{
		/// <summary>
		/// 
		/// </summary>
		static abstract TPrng Prng_Init<TRd_Result>(IEngine_Traits<TRd_Result> rd) where TRd_Result : IBinaryInteger<TRd_Result>, IUnsignedNumber<TRd_Result>;

		/// <summary>
		/// 
		/// </summary>
		MixSampleInt Process(uint32 targetbits, MixSampleInt sample, TPrng prng);
	}
}
